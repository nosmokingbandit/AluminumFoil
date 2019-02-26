using System.IO;
using System.Linq;
using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
 
namespace AluminumFoil
{
    public class PFS0
    {
        public string FilePath { get; set; }
        public ObservableCollection<PFS0File> Contents { get; } = new ObservableCollection<PFS0File>();

        public readonly string BaseName;
        public readonly long Size;
                
        private readonly byte[] MAGIC = new byte[0x4] { 80, 70, 83, 48 }; // "PFS0"
        private readonly string[] Suffixes = new string[] { "B", "KB", "MB", "GB", "TB" };

        private const uint SUPERBLOCKLEN = 0x10;
        private const uint FILENTRYLEN = 0x18;

        public PFS0(string fp)
            // Constructor reads metadata from NSP header and fills class fields
            // fp: path to NSP file
        {
            FilePath = fp;
            FileInfo fi = new FileInfo(FilePath);
            BaseName = fi.Name;
            Size = fi.Length;

            using (BinaryReader reader = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
            {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                byte[] magic = reader.ReadBytes(0x4);

                if (!magic.SequenceEqual(MAGIC))
                {
                    throw new NotSupportedException(string.Format("Invalid NSP Header; Wanted 'PFS0' got '{0}'", magic.AsString()));
                }

                uint fileCount = BitConverter.ToUInt32(reader.ReadBytes(0x4), 0);
                uint fileEntryTableLen = FILENTRYLEN * fileCount;

                uint stringTableLen = BitConverter.ToUInt32(reader.ReadBytes(0x4), 0);

                reader.ReadBytes(0x4); // null reserved section of pfs0 header

                reader.BaseStream.Seek(SUPERBLOCKLEN + fileEntryTableLen, SeekOrigin.Begin);
                byte[] stringTableBuffer = reader.ReadBytes((int)stringTableLen);

                for (var i = 0; i < fileCount; i++)
                {
                    reader.BaseStream.Seek(SUPERBLOCKLEN + FILENTRYLEN * i, SeekOrigin.Begin);
                    PFS0File file = new PFS0File();

                    file.Offset = SUPERBLOCKLEN + fileEntryTableLen + stringTableLen + BitConverter.ToUInt64(reader.ReadBytes(0x8), 0);
                    file.Size = BitConverter.ToUInt64(reader.ReadBytes(0x8), 0);

                    uint nameOffset = BitConverter.ToUInt32(reader.ReadBytes(0x4), 0);

                    uint nameLen = 0;

                    while (stringTableBuffer[nameOffset + nameLen] != 0x0)
                    {
                        nameLen++;
                    }

                    file.Name = stringTableBuffer.SubArray((int)nameOffset, (int)nameLen).AsString();
                    
                    var s = file.Size;
                    var suff = 0;
                    while(s / 1024 > 0)
                    {
                        s /= 1024;
                        suff++;
                    }
                    file.HumanSize = s.ToString() + Suffixes[suff];

                    Contents.Add(file);
                }
            }
        }

        // TODO read chunksize from config
        public IEnumerable<byte[]> ReadFile(int ind, int ChunkSize = 0x100000)
            // Generator to read file from NSP
            // ind: index of file defined by nca header
            // ChunkSize: size of read chunk in bytes; default 0x100000 (1mb)
            // Yields byte[] of size ChunkSize or remaining data (whichever is smaller)
        {
            PFS0File file = Contents[ind];

            using (BinaryReader reader = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
            {
                reader.BaseStream.Seek((long)file.Offset, SeekOrigin.Begin);
                var remaining = (int)file.Size;
                
                while (remaining > 0)
                {
                    if (ChunkSize > remaining)
                    {
                        ChunkSize = remaining;
                    }
                    byte[] chunk = reader.ReadBytes(ChunkSize);
                    remaining -= ChunkSize;
                    yield return chunk;
                }
            }
        }
    }

    // TODO Can this be watched for changes without inheriting from ReactiveUI
    // and calling Notify events? It would be nice to have the PFS0 file not
    // using reactiveui and have the viewmodel take care of everything.
    public class PFS0File : INotifyPropertyChanged
    {
        public string Name { get; set; }    // Name of content eg 123456789.nca
        public ulong Offset { get; set; }   // Start of file in nsp counting from byte-0x0
        public ulong Size { get; set; }     // Size of the file in bytes
        private ulong _Transferred;
        public ulong Transferred {
            get => _Transferred;
            set
            {
                _Transferred = value;
                NotifyPropertyChanged("Transferred");
            }
        }

        public string HumanSize { get; set; }
        public bool Finished { get; set; }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}