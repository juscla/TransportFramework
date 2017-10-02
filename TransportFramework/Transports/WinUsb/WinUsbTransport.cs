namespace TransportFramework.Transports.WinUsb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Data.Packets;

    using Helpers;

    using Microsoft.Win32.SafeHandles;

    using Transports.Base;
    using Transports.Base.Native;

    /// <summary>
    /// The win USB Transport.
    /// </summary>
    public class WinUsbTransport : TransportBase
    {
        /// <summary>
        /// The interface descriptor.
        /// </summary>
        private UsbInterfaceDescriptor interfaceDescriptor;

        /// <summary>
        /// The device handle.
        /// </summary>
        private SafeFileHandle deviceHandle;

        /// <summary>
        /// The WinUSB handle.
        /// </summary>
        private IntPtr winUsbHandle = IntPtr.Zero;

        /// <summary>
        /// The add interfaces.
        /// </summary>
        private IntPtr[] interfaces = new IntPtr[0];

        /// <summary>
        /// The writer.
        /// </summary>
        private WinUsbPipe writer;

        /// <summary>
        /// The reader.
        /// </summary>
        private WinUsbPipe reader;

        /// <summary>
        /// The running flag.
        /// </summary>
        private bool running;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinUsbTransport"/> class. 
        /// </summary>
        /// <param name="devicePathName">
        /// Device path name of the USB device to create
        /// </param>
        public WinUsbTransport(string devicePathName) :
            base(devicePathName)
        {
        }

        /// <summary>
        /// Gets or sets the device state changed.
        /// </summary>
        public event EventHandler<ConnectedStateChangedEventArgs> DeviceStateChanged;

        /// <summary>
        /// Gets or sets a value indicating whether is connected.
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return
                    this.winUsbHandle != IntPtr.Zero &&
                    this.deviceHandle != null &&
                    !this.deviceHandle.IsInvalid;
            }

            protected set
            {
            }
        }

        /// <summary>
        /// Gets the buffer size.
        /// </summary>
        public int BufferSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Device descriptors.
        /// </summary>
        public UsbDeviceDescriptor DeviceDescriptor
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the interface descriptor.
        /// </summary>
        public UsbInterfaceDescriptor InterfaceDescriptor
        {
            get => this.interfaceDescriptor;
            private set => this.interfaceDescriptor = value;
        }

        /// <summary>
        /// The get list of devices.
        /// </summary>
        /// <returns>
        /// The <see cref="Enumerable"/>.
        /// </returns>
        public static IEnumerable<string> GetListOfDevices()
        {
            return UsbHelpers.GetWinUsbAddresses(string.Empty, UsbHelpers.FilterTypes.DeviceName)
                .Select(x => x.DevicePath);
        }

        /// <summary>
        /// The connect.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Connect()
        {
            try
            {
                var isReadOnly = false;
                this.deviceHandle = NativeMethods.OpenDevice(this.DevicePath, ref isReadOnly);

                if (this.deviceHandle.IsInvalid)
                {
                    return false;
                }

                if (this.InitializeDevice())
                {
                    this.SetupInterfaces();
                }
            }
            catch
            {
                // ignored
            }
          
            this.DeviceStateChanged.RaiseEvent(
                this,
                new ConnectedStateChangedEventArgs(
                this.IsConnected,
                this.IsConnected ? ConnectedStates.Connected : ConnectedStates.Failed));

            if (this.IsConnected)
            {
                Task.Factory.StartNew(this.ReadData, TaskCreationOptions.LongRunning);
            }

            return this.IsConnected;
        }

        /// <summary>
        /// The disconnect.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Disconnect()
        {
            this.running = false;

            this.DeviceDescriptor = default(UsbDeviceDescriptor);
            this.InterfaceDescriptor = default(UsbInterfaceDescriptor);

            foreach (var inter in this.interfaces)
            {
                NativeMethods.WinUsb_Free(inter);
            }

            this.interfaces = new IntPtr[0];

            if (this.deviceHandle != null && !this.deviceHandle.IsInvalid)
            {
                this.deviceHandle.Dispose();
                this.deviceHandle = null;
            }

            if (this.winUsbHandle != IntPtr.Zero)
            {
                NativeMethods.WinUsb_Free(this.winUsbHandle);
                this.winUsbHandle = IntPtr.Zero;
            }

            this.DeviceStateChanged.RaiseEvent(this, new ConnectedStateChangedEventArgs(false, ConnectedStates.Disconnected));
            return true;
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        public override void Write(params byte[] packet)
        {
            this.WriteRaw(packet);
        }

        /// <summary>
        /// The write raw.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public override void WriteRaw(IEnumerable<byte> data)
        {
            var d = data.ToArray();
            uint bytesWritten;

            if (NativeMethods.WinUsb_WritePipe(
                this.winUsbHandle,
                this.writer.Id,
                d,
                (uint)d.Length,
                out bytesWritten,
                IntPtr.Zero))
            {
                return;
            }

            this.DeviceStateChanged.RaiseEvent(this, new ConnectedStateChangedEventArgs(false, ConnectedStates.Failed));
            this.Disconnect();
        }

        /// <summary>
        /// The write control.
        /// </summary>
        /// <param name="setup">
        /// The setup.
        /// </param>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        public uint WriteControl(WinUsbSetupPacket setup, byte[] buffer, uint length)
        {
            uint written = 0;
            NativeMethods.WinUsb_ControlTransfer(this.winUsbHandle, setup, buffer, length, ref written, IntPtr.Zero);
            return written;
        }

        /// <summary>
        /// The read.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="uint"/>.
        /// </returns>
        private int Read(byte[] data)
        {
            int bytesRead;
            if (NativeMethods.WinUsb_ReadPipe(
                this.winUsbHandle,
                this.reader.Id,
                data,
                data.Length,
                out bytesRead,
                IntPtr.Zero))
            {
                return bytesRead;
            }

            this.DeviceStateChanged.RaiseEvent(this, new ConnectedStateChangedEventArgs(false, ConnectedStates.Failed));
            this.Disconnect();

            return 0;
        }

        /// <summary>
        /// The read data.
        /// </summary>
        private void ReadData()
        {
            if (this.running)
            {
                return;
            }

            this.running = true;

            while (this.IsConnected && this.running)
            {
                var inData = new byte[this.BufferSize];

                var read = this.Read(inData);

                if (!this.IsConnected)
                {
                    continue;
                }

                // convert to raw packet.
                var packet = new WinUsbPacket(inData, read);

                // Notify all Parsers.
                this.NotifyParsers(packet);

                // notify users of raw packet.
                this.OnReceived(packet);
            }

            this.running = false;
        }

        /// <summary>
        /// The initialize device.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool InitializeDevice()
        {
            if (!NativeMethods.WinUsb_Initialize(this.deviceHandle, ref this.winUsbHandle))
            {
                return false;
            }

            this.DeviceDescriptor = NativeMethods.GetDeviceDescriptor(this.winUsbHandle, this.DevicePath);

            var interfacesTemp = new List<IntPtr>();
            byte index = 0;

            try
            {
                while (true)
                {
                    IntPtr interfaceHandle;

                    if (!NativeMethods.WinUsb_GetAssociatedInterface(this.winUsbHandle, index, out interfaceHandle))
                    {
                        if (Marshal.GetLastWin32Error() == 259)
                        {
                            // no more interfaces.
                            break;
                        }
                    }

                    interfacesTemp.Add(interfaceHandle);
                    index++;
                }
            }
            finally
            {
                this.interfaces = interfacesTemp.ToArray();
            }

            ThreadPool.BindHandle(this.deviceHandle);
            return true;
        }

        /// <summary>
        /// The setup interfaces.
        /// </summary>
        private void SetupInterfaces()
        {
            var count = Math.Max(this.interfaces.Length, 1);

            for (var index = 0; index < count; index++)
            {
                UsbPipeInformation[] pipesInfo;

                this.GetInterfaceInfo(index, out this.interfaceDescriptor, out pipesInfo);

                foreach (var pp in pipesInfo.Select(p => new WinUsbPipe(p)))
                {
                    if (pp.IsOut)
                    {
                        this.writer = pp;
                    }
                    else if (pp.IsIn && this.reader == null)
                    {
                        this.reader = pp;
                        this.BufferSize = pp.Size;
                    }
                }
            }
        }

        /// <summary>
        /// The interface handle.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        private IntPtr InterfaceHandle(int index)
        {
            return index == 0 ? this.winUsbHandle : this.interfaces[index - 1];
        }

        /// <summary>
        /// The get interface info.
        /// </summary>
        /// <param name="interfaceIndex">
        /// The interface index.
        /// </param>
        /// <param name="descriptor">
        /// The descriptor.
        /// </param>
        /// <param name="pipes">
        /// The pipes.
        /// </param>
        private void GetInterfaceInfo(int interfaceIndex, out UsbInterfaceDescriptor descriptor, out UsbPipeInformation[] pipes)
        {
            var pipeList = new List<UsbPipeInformation>();
            var handle = this.InterfaceHandle(interfaceIndex);
            NativeMethods.WinUsb_QueryInterfaceSettings(handle, 0, out descriptor);

            for (byte index = 0; index < descriptor.NumEndpoints; index++)
            {
                UsbPipeInformation pipeInfo;
                NativeMethods.WinUsb_QueryPipe(handle, 0, index, out pipeInfo);
                pipeList.Add(pipeInfo);
            }

            pipes = pipeList.ToArray();
        }

        /// <summary>
        /// The USB pipe.
        /// </summary>
        internal class WinUsbPipe
        {
            /// <summary>
            /// The pipe info.
            /// </summary>
            private readonly UsbPipeInformation info;

            /// <summary>
            /// Initializes a new instance of the <see cref="WinUsbPipe"/> class.
            /// </summary>
            /// <param name="info">
            /// The info.
            /// </param>
            public WinUsbPipe(UsbPipeInformation info)
            {
                this.info = info;
            }

            /// <summary>
            /// Gets the id.
            /// </summary>
            public byte Id => this.info.Id;

            /// <summary>
            /// Gets a value indicating whether is out.
            /// </summary>
            public bool IsOut => (this.Id & 0x80) == 0;

            /// <summary>
            /// Gets a value indicating whether is in.
            /// </summary>
            public bool IsIn => (this.Id & 0x80) != 0;

            /// <summary>
            /// Gets the size.
            /// </summary>
            public int Size => this.info.MaximumSize;

            /// <summary>
            /// Gets the type.
            /// </summary>
            public PipeType Type => this.info.Type;
        }
    }
}
