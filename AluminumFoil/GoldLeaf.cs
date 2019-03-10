using System.Linq;
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

        public IEnumerable<Tuple<string, string>> InstallNSP(AluminumFoil.NSP.PFS0 nsp)
        {
            // Installs NSP to Switch via GoldLeaf
            int txLen;

            using (AluminumFoil.Switch NX = new AluminumFoil.Switch())
            {
                NX.Write(commandHeaders["ConnectionRequest"]);

                bool finished = false;
                while (!finished)
                {
                    byte[] resp = NX.Read(0x8);

                    string CommandName = RespType(resp);

                    switch (CommandName)
                    {
                        case "ConnectionResponse":
                            yield return new InstallUpdate("Sending NSP name to GoldLeaf", "installing");
                            NX.Write(commandHeaders["NSPName"]);

                            var nameBytes = nsp.BaseName.AsBytes();

                            NX.Write(BitConverter.GetBytes(Convert.ToUInt32(nameBytes.Length)));
                            NX.Write(nameBytes);

                            yield return new InstallUpdate("Confirm Installation Options on GoldLeaf", "waiting");

                            break;

                        case "Start":
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

                            // can only index observablecolleciton with int, which is kind of dumb...
                            var idx = (int)BitConverter.ToUInt32(indBytes, 0);

                            yield return new InstallUpdate("Installing " + nsp.Contents[idx].Name, "installing");

                            foreach (var chunk in nsp.ReadFile(idx))
                            {
                                txLen = NX.Write(chunk);
                                nsp.Contents[idx].Transferred += Convert.ToUInt64(chunk.Length);
                            }

                            nsp.Contents[idx].Finished = true;

                            break;

                        case "NSPTicket":

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
                                throw new IndexOutOfRangeException("Ticket file not found in NSP header.");
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
                            yield return new InstallUpdate("Finished", "finished");
                            finished = true;
                            break;
                        default:
                            yield return new InstallUpdate(string.Format("Unknown request from GoldLeaf: {0}", resp.AsString()), "alert");
                            finished = true;
                            break;
                    }
                }
            }
        }
    }
}