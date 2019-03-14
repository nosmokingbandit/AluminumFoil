using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using ExtensionMethods;
using System.IO;

namespace GoldLeaf
{
    using InstallUpdate = Tuple<string, string>;

    public class GoldLeaf
    {
        private readonly Dictionary<string, byte[]> commandHeaders = new Dictionary<string, byte[]>()
        {                                      // G   L   U   C  #ID
            { "ConnectionRequest",   new byte[]{ 71, 76, 85, 67, 0, 0, 0, 0 } },
            { "ConnectionResponse",  new byte[]{ 71, 76, 85, 67, 1, 0, 0, 0 } },
            { "NSPName",             new byte[]{ 71, 76, 85, 67, 2, 0, 0, 0 } },
            { "Start",               new byte[]{ 71, 76, 85, 67, 3, 0, 0, 0 } },
            { "NSPData",             new byte[]{ 71, 76, 85, 67, 4, 0, 0, 0 } },
            { "NSPContent",          new byte[]{ 71, 76, 85, 67, 5, 0, 0, 0 } },
            { "NSPTicket",           new byte[]{ 71, 76, 85, 67, 6, 0, 0, 0 } },
            { "Finish",              new byte[]{ 71, 76, 85, 67, 7, 0, 0, 0 } }
        };

        private string RespType(byte[] resp)
        {
            foreach (var pair in commandHeaders)
            {
                if (pair.Value.SequenceEqual(resp))
                {
                    return pair.Key;
                }
            }
            return "";
        }

        public IEnumerable<Tuple<string, string>> InstallNSP(ObservableCollection<AluminumFoil.NSP> NSPs)
        {
            AluminumFoil.NSP nsp = NSPs.First();

            Console.WriteLine(string.Format("Installing {0} via GoldLeaf", nsp.BaseName));
            // Installs NSP to Switch via GoldLeaf
            int txLen;

            using (AluminumFoil.Switch NX = new AluminumFoil.Switch())
            {
                Console.WriteLine("Sending ConnectionRequest");
                NX.Write(commandHeaders["ConnectionRequest"]);

                bool finished = false;
                while (!finished)
                {
                    byte[] resp = NX.Read(0x8);

                    string CommandName = RespType(resp);

                    Console.WriteLine("Recieved command from GoldLeaf: " + CommandName);
                    switch (CommandName)
                    {
                        case "ConnectionResponse":
                            Console.WriteLine("Sending NSP name to GoldLeaf");
                            yield return new InstallUpdate("Sending NSP name to GoldLeaf", "installing");

                            NX.Write(commandHeaders["NSPName"]);

                            var nameBytes = nsp.BaseName.AsBytes();

                            NX.Write(BitConverter.GetBytes(Convert.ToUInt32(nameBytes.Length)));
                            NX.Write(nameBytes);

                            yield return new InstallUpdate("Confirm Installation Options on GoldLeaf", "waiting");

                            break;

                        case "Start":
                            Console.WriteLine("Sending NSP metadata to GoldLeaf");
                            yield return new InstallUpdate("Sending NSP Metadata", "installing");

                            NX.Write(commandHeaders["NSPData"]);
                            NX.Write(BitConverter.GetBytes(Convert.ToUInt32(nsp.Contents.Count)));

                            foreach (var file in nsp.Contents)
                            {
                                // uint [4] Name len 
                                NX.Write(BitConverter.GetBytes(Convert.ToUInt32(file.Name.Length)));

                                // str [*] Name
                                NX.Write(file.Name.AsBytes());

                                // ulong [8] File offset 
                                NX.Write(BitConverter.GetBytes(file.Offset));

                                // ulong [8] File size
                                NX.Write(BitConverter.GetBytes(file.Size));
                            }

                            break;

                        case "NSPContent":
                            byte[] indBytes = NX.Read(0x4);

                            var idx = (int)BitConverter.ToUInt32(indBytes, 0);


                            Console.WriteLine(string.Format("GoldLeaf requested nca {0}: {1}", idx, nsp.Contents[idx].Name));
                            yield return new InstallUpdate("Installing " + nsp.Contents[idx].Name, "installing");

                            foreach (var chunk in nsp.ReadFile(idx))
                            {
                                txLen = NX.Write(chunk);
                                nsp.Transferred += (ulong)chunk.Length;
                            }

                            nsp.Contents[idx].Finished = true;

                            break;

                        case "NSPTicket":
                            Console.WriteLine("Sending NSP ticket");
                            yield return new InstallUpdate("Installing Ticket", "installing");

                            var tikind = -1;

                            for (var i = 0; i < nsp.Contents.Count; i++)
                            {
                                if (nsp.Contents[i].Name.EndsWith("tik"))
                                {
                                    tikind = i;
                                    break;
                                }
                            }

                            if (tikind == -1)
                            {
                                Console.WriteLine("Could not find tik in NSP");
                                Exception exc = new IndexOutOfRangeException("Ticket file not found in NSP header.");
                                exc.Source = nsp.FilePath;
                                throw exc;
                            };

                            byte[] ticketFile = new byte[nsp.Contents[tikind].Size];

                            using (BinaryReader reader = new BinaryReader(new FileStream(nsp.FilePath, FileMode.Open)))
                            {
                                var tikoffset = nsp.Contents[tikind].Offset;
                                reader.BaseStream.Seek(Convert.ToInt64(tikoffset), SeekOrigin.Begin);
                                reader.Read(ticketFile, 0, ticketFile.Length);
                            }

                            NX.Write(ticketFile);

                            break;

                        case "Finish":
                            Console.WriteLine("Finished installing " + nsp.BaseName);
                            yield return new InstallUpdate("Finished", "finished");
                            finished = true;
                            break;
                        default:
                            Console.WriteLine("Unhandled command from GoldLeaf: " + resp.AsString());
                            yield return new InstallUpdate("Unknown request from GoldLeaf: " + resp.AsString(), "alert");
                            finished = true;
                            break;
                    }
                }
            }
        }
    }
}