using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

namespace AluminumFoil
{
    class Switch : IDisposable
    {
        private const int VID = 0x057E;
        private const int PID = 0x3000;
        private const int TIMEOUT = 0;
        private UsbContext LibUsbContext;
        private IUsbDevice NX;
        private UsbEndpointWriter Writer;
        private UsbEndpointReader Reader;

        public int Write(byte[] payload)
        {
            Writer.Write(payload, TIMEOUT, out int txLen);
            return txLen;
        }

        public byte[] Read(int ReadLen)
        {
            var readBuffer = new byte[ReadLen];
            Reader.Read(readBuffer, TIMEOUT, out int txLen);
            return readBuffer;
        }

        public void Dispose()
        {
            LibUsbContext.Dispose();
            NX.Dispose();
        }

        public Switch()
        {
            this.LibUsbContext = new UsbContext();
            var usbDeviceCollection = LibUsbContext.List();
            NX = usbDeviceCollection.FirstOrDefault(d => d.ProductId == PID && d.VendorId == VID);

            if (NX == null)
            {
                throw new Exception("Unable to find Switch. Ensure Switch is connected and GoldLeaf's USB Install is open.");
            }

            NX.Open();
            NX.ClaimInterface(NX.Configs[0].Interfaces[0].Number);

            Writer = NX.OpenEndpointWriter(WriteEndpointID.Ep01);
            Reader = NX.OpenEndpointReader(ReadEndpointID.Ep01);
        }
    }
}
