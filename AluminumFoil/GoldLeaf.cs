using System.Linq;
using System.Collections.Generic;
using System;
using ExtensionMethods;
using System.IO;

using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

namespace GoldLeaf
{
    using InstallUpdate = Tuple<string, string>;

    public class GoldLeaf
    {
        private const int VID = 0x057E;
        private const int PID = 0x3000;
        private const int TIMEOUT = 0;

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

        private int Write(UsbEndpointWriter Writer, byte[] payload)
        {
            Writer.Write(payload, TIMEOUT, out int txLen);
            return txLen;
        }

        private byte[] Read(UsbEndpointReader Reader, int ReadLen)
        {
            var readBuffer = new byte[ReadLen];
            Reader.Read(readBuffer, TIMEOUT, out int txLen);
            return readBuffer;
        }

        public IEnumerable<Tuple<string, string>> InstallNSP(AluminumFoil.NSP.PFS0 nsp)
        {
            // Installs NSP to Switch via GoldLeaf
            int txLen;

            using (UsbContext context = new UsbContext())
            {
                var usbDeviceCollection = context.List();
                IUsbDevice NX = usbDeviceCollection.FirstOrDefault(d => d.ProductId == PID && d.VendorId == VID);

                if (NX == null)
                {
                    throw new Exception("Unable to find Switch. Ensure Switch is connected and GoldLeaf's USB Install is open.");
                }

                NX.Open();
                NX.ClaimInterface(NX.Configs[0].Interfaces[0].Number);

                var Writer = NX.OpenEndpointWriter(WriteEndpointID.Ep01);
                var Reader = NX.OpenEndpointReader(ReadEndpointID.Ep01);

                Write(Writer, commandHeaders["ConnectionRequest"]);

                bool finished = false;
                while (!finished)
                {
                    byte[] resp = Read(Reader, 0x8);

                    string CommandName = RespType(resp);

                    switch (CommandName)
                    {
                        case "ConnectionResponse":
                            yield return new InstallUpdate("Sending NSP name to GoldLeaf", "installing");
                            Write(Writer, commandHeaders["NSPName"]);

                            var nameBytes = nsp.BaseName.AsBytes();

                            Write(Writer, BitConverter.GetBytes(Convert.ToUInt32(nameBytes.Length)));
                            Write(Writer, nameBytes);

                            yield return new InstallUpdate("Confirm Installation Options on GoldLeaf", "waiting");

                            break;

                        case "Start":
                            yield return new InstallUpdate("Sending NSP Metadata", "installing");

                            Write(Writer, commandHeaders["NSPData"]);
                            Write(Writer, BitConverter.GetBytes(Convert.ToUInt32(nsp.Contents.Count)));

                            foreach (var file in nsp.Contents)
                            {
                                // uint [4] Name len 
                                Write(Writer, BitConverter.GetBytes(Convert.ToUInt32(file.Name.Length)));

                                // str [*] Name
                                Write(Writer, file.Name.AsBytes());

                                // ulong [8] File offset 
                                Write(Writer, BitConverter.GetBytes(file.Offset));

                                // ulong [8] File size
                                Write(Writer, BitConverter.GetBytes(file.Size));
                            }

                            break;

                        case "NSPContent":
                            byte[] indBytes = Read(Reader, 0x4);

                            // can only index observablecolleciton with int, which is kind of dumb...
                            var idx = (int)BitConverter.ToUInt32(indBytes, 0);

                            yield return new InstallUpdate("Installing " + nsp.Contents[idx].Name, "installing");

                            foreach (var chunk in nsp.ReadFile(idx))
                            {
                                txLen = Write(Writer, chunk);
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

                            Write(Writer, ticketFile);

                            break;

                        case "Finish":
                            yield return new InstallUpdate("Finished", "finished");
                            finished = true;
                            break;
                        default:
                            yield return new InstallUpdate(string.Format("Unknown request from GoldLeaf: {0}", resp.AsString()), "alert");
                            break;
                    }
                }
                NX.Dispose();
            }
        }
    }
}