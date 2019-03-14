using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


        public IEnumerable<Tuple<string, string>> InstallNSP(ObservableCollection<AluminumFoil.NSP> NSPs)
        {
            // Installs NSP to Switch via TinFoil
            using (AluminumFoil.Switch NX = new AluminumFoil.Switch())
            {
                // Send NSP List
                yield return new InstallUpdate("Sending NSP names to TinFoil", "installing");
                NX.Write(MAGIC);

                Console.WriteLine("Sending NSP names to TinFoil");
                string concatenatedNames = string.Join("\n", NSPs.Select(n => n.BaseName)); 

                byte[] namesBytes = concatenatedNames.AsBytes();
                NX.Write(BitConverter.GetBytes(Convert.ToUInt32(namesBytes.Length)));
                NX.Write(new byte[0x8] { 0, 0, 0, 0, 0, 0, 0, 0 });
                NX.Write(namesBytes);

                Console.WriteLine("Waiting for response from TinFoil");
                yield return new InstallUpdate("Select NSP on TinFoil", "waiting");

                // Poll Commands
                while (true)
                {
                    byte[] magic = NX.Read(0x4);
                    if (magic.SequenceEqual(new byte[4])){
                        Console.WriteLine("TinFoil seems to be closed, ending communication");
                        break;
                    }

                    if (!magic.SequenceEqual(COMMANDMAGIC))
                    {
                        continue;
                    };

                    byte[] command = NX.Read(0x1C);

                    byte[] cmdType = command.SubArray(0x0, 0x1);
                    uint cmdID = BitConverter.ToUInt32(command, 0x4);
                    ulong payloadSize = BitConverter.ToUInt64(command, 0x8);

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

                        string selectedBaseName = NX.Read(Convert.ToInt32(nameLen)).AsString();

                        AluminumFoil.NSP selectedNSP = NSPs.FirstOrDefault(n => n.BaseName == selectedBaseName);

                        if (selectedNSP == null)
                        {
                            throw new Exception(string.Format("TinFoil requested {0} but this NSP is not opened for installation.", selectedBaseName));
                        }

                        yield return new InstallUpdate(string.Format("Transferring {0} requested bytes to TinFoil", size), "installing");

                        NX.Write(ResponseHeader((uint)CommandIDs.FileRange, size));

                        using (BinaryReader fileReader = new BinaryReader(new FileStream(selectedNSP.FilePath, FileMode.Open)))
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
                                selectedNSP.Transferred += readLen;
                            }
                        }
                    }
                }
            }
        }
    }
}