using System.IO;
using System.Linq;
using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
 
namespace AluminumFoil.NSP
{
    public class PFS0
    {

        public string FilePath { get; set; }
        public ObservableCollection<PFS0File> Contents { get; } = new ObservableCollection<PFS0File>();

        public readonly string BaseName;
        public readonly long Size;
                
        private readonly byte[] PFS0MAGIC = new byte[0x4] { 80, 70, 83, 48 }; // "PFS0"
        private readonly string[] Suffixes = new string[] { "B", "KB", "MB", "GB", "TB" };

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
            Size = fi.Length;

            using (BinaryReader reader = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
            {
                reader.BaseStream.Seek(0, 0);


                byte[] magic = reader.ReadBytes(0x4);
                uint fileCount = reader.ReadUInt32();
                uint stringTableLen = reader.ReadUInt32();
                reader.ReadBytes(0x4); // Null/reserved area
                byte[] fileEntryTable = reader.ReadBytes((int)fileCount * FileEntryLen);
                StringTable = reader.ReadBytes((int)stringTableLen);
                DataOffset = reader.BaseStream.Position;

                if (!magic.SequenceEqual(PFS0MAGIC))
                {
                    throw new NotSupportedException("Invalid NSP header; Wanted 'PFS0' got " + magic.AsString());
                }

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

            var s = file.Size;
            var suff = 0;
            while (s / 1024 > 0)
            {
                s /= 1024;
                suff++;
            }
            file.HumanSize = s.ToString() + Suffixes[suff];

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
}