using System.Linq;
using System.Collections.Generic;
using System;
using ExtensionMethods;
using System.IO;

namespace TinFoil
{
    using InstallUpdate = Tuple<string, string>;

    public class TinFoil
    {
        private readonly byte[] MAGIC = new byte[] { 84, 85, 76, 48 }; // TUL0
        private readonly byte[] COMMANDMAGIC = new byte[] { 84, 85, 67, 48 }; // TUC0

        private enum CommandIDs : uint { Exit = 0, FileRange = 1, Response = 1 };

        private const int ChunkSize = 0x100000;

        private byte[] ResponseHeader(uint cmdId, ulong size)
        {
            byte[] response = new byte[32];
            COMMANDMAGIC.CopyTo(response, 0x0);
            BitConverter.GetBytes((uint)CommandIDs.Response).CopyTo(response, 0x4);
            BitConverter.GetBytes(cmdId).CopyTo(response, 0x8);
            BitConverter.GetBytes(size).CopyTo(response, 0xC);
            return response;
        }


        public IEnumerable<Tuple<string, string>> InstallNSP(AluminumFoil.NSP.PFS0 nsp)
        {
            // Installs NSP to Switch via TinFoil
            using (AluminumFoil.Switch NX = new AluminumFoil.Switch())
            {
                // Send NSP List (just one though)
                yield return new InstallUpdate("Sending NSP name to TinFoil", "installing");
                NX.Write(MAGIC);
                byte[] nameBytes = nsp.BaseName.AsBytes();
                NX.Write(BitConverter.GetBytes(Convert.ToUInt32(nameBytes.Length)));
                NX.Write(new byte[0x8] { 0, 0, 0, 0, 0, 0, 0, 0 });
                NX.Write(nameBytes);

                yield return new InstallUpdate("Confirm Installation Options on TinFoil", "waiting");

                // Poll Commands
                while (true)
                {
                    byte[] command = NX.Read(0x20);

                    if (!command.SubArray(0x0, 0x4).SequenceEqual(COMMANDMAGIC))
                    {
                        continue;
                    };

                    byte[] cmdType = command.SubArray(0x4, 0x1);
                    uint cmdID = BitConverter.ToUInt32(command, 0x8);
                    ulong payloadSize = BitConverter.ToUInt64(command, 0xC);

                    if (cmdID == (uint)CommandIDs.Exit)
                    {
                        yield return new InstallUpdate("Finished", "finished");
                        break;
                    }
                    else if (cmdID == (uint)CommandIDs.FileRange)
                    {
                        // File Range Command
                        byte[] fileRangeRequest = NX.Read(0x20);

                        ulong size = BitConverter.ToUInt64(fileRangeRequest, 0x0);
                        ulong offset = BitConverter.ToUInt64(fileRangeRequest, 0x8);
                        ulong nameLen = BitConverter.ToUInt64(fileRangeRequest, 0x10);

                        string nspName = BitConverter.ToString(NX.Read(Convert.ToInt32(nameLen)));

                        yield return new InstallUpdate(string.Format("Transferring {0} requested bytes to TinFoil", size), "installing");

                        NX.Write(ResponseHeader((uint)CommandIDs.FileRange, size));

                        using (BinaryReader fileReader = new BinaryReader(new FileStream(nsp.FilePath, FileMode.Open)))
                        {
                            fileReader.BaseStream.Seek((long)offset, SeekOrigin.Begin);

                            ulong bytesRead = 0;

                            while (bytesRead < size)
                            {
                                ulong readLen;
                                if (bytesRead + ChunkSize >= size)
                                {
                                    readLen = size - bytesRead;
                                }
                                else
                                {
                                    readLen = ChunkSize;
                                }

                                NX.Write(fileReader.ReadBytes((int)readLen));
                                bytesRead += readLen;
                                nsp.Transferred += readLen;
                            }
                        }
                    }
                }
            }
        }
    }
}