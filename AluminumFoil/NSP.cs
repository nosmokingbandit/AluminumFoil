using System.IO;
using System.Linq;
using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ReactiveUI;

namespace AluminumFoil.NSP
{
    public class PFS0 : ReactiveObject
    {

        public string FilePath { get; set; }
        public ObservableCollection<PFS0File> Contents { get; } = new ObservableCollection<PFS0File>();

        private string _BaseName;
        public string BaseName
        {
            get => _BaseName;
            set
            {
                this.RaiseAndSetIfChanged(ref _BaseName, value);
            }
        }

        public string HumanSize { get; set; }
        private ulong _Size;
        public ulong Size {
            get => _Size;
            set
            {
                _Size = value;
                HumanSize = value.HumanSize();
            }
        }
        private ulong _Transferred;
        public ulong Transferred {
            get => _Transferred;
            set
            {
                this.RaiseAndSetIfChanged(ref _Transferred, value);
            }
        }

        private readonly byte[] PFS0MAGIC = new byte[0x4] { 80, 70, 83, 48 }; // "PFS0"
        private const int FileEntryLen = 0x18;
        private readonly long DataOffset;
        private readonly byte[] StringTable;

        public PFS0(string fp)
        // Constructor reads metadata from NSP header and fills class fields
        // fp: path to NSP file
        {
            FilePath = fp;
            FileInfo fi = new FileInfo(FilePath);
            BaseName = fi.Name;
            Size = (ulong)fi.Length;

            using (BinaryReader reader = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
            {
                reader.BaseStream.Seek(0, 0);

                byte[] magic = reader.ReadBytes(0x4);
                if (!magic.SequenceEqual(PFS0MAGIC))
                {
                    throw new NotSupportedException("Invalid NSP header; Wanted PFS0 got " + magic.AsString());
                }

                uint fileCount = reader.ReadUInt32();
                uint stringTableLen = reader.ReadUInt32();
                reader.ReadBytes(0x4); // Null/reserved area
                byte[] fileEntryTable = reader.ReadBytes((int)fileCount * FileEntryLen);
                StringTable = reader.ReadBytes((int)stringTableLen);
                DataOffset = reader.BaseStream.Position;

                for (var i = 0; i < fileCount; i++)
                {
                    Contents.Add(NewPFS0File(reader, i));
                }
            }
        }

        private PFS0File NewPFS0File(BinaryReader reader, int fileNum)
        {
            PFS0File file = new PFS0File();

            reader.BaseStream.Seek(0x10 + FileEntryLen * fileNum, SeekOrigin.Begin);

            file.Offset = reader.ReadUInt64();
            file.Size = reader.ReadUInt64();
            uint nameOffset = reader.ReadUInt32();

            uint nameLen = 0;

            while (StringTable[nameOffset + nameLen] != 0x0)
            {
                nameLen++;
            }

            file.Name = StringTable.SubArray((int)nameOffset, (int)nameLen).AsString();
            file.HumanSize = file.Size.HumanSize();

            return file;
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

    public class PFS0File : INotifyPropertyChanged
    {
        public string Name { get; set; }    // Name of content eg 123456789.nca
        public ulong Offset { get; set; }   // Start of file in nsp counting from byte-0x0
        public ulong Size { get; set; }     // Size of the file in bytes
        private ulong _Transferred;
        public ulong Transferred
        {
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