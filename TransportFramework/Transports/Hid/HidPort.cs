namespace TransportFramework.Transports.Hid
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Win32.SafeHandles;

    using TransportFramework.Data.Exceptions;
    using TransportFramework.Helpers;
    using TransportFramework.Transports.Base;
    using TransportFramework.Transports.Base.Native;

    /// <summary>
    /// The hid port.
    /// </summary>
    public class HidPort : IDisposable
    {
        /// <summary>
        /// The device stream.
        /// </summary>
        private FileStream deviceStream;

        /// <summary>
        /// The connected.
        /// </summary>
        private bool isOpen;

        /// <summary>
        /// The read buffer size.
        /// </summary>
        private int readBufferSize = 4095;

        /// <summary>
        /// The read buffer.
        /// </summary>
        private byte[] readBuffer;

        /// <summary>
        /// The current index.
        /// </summary>
        private uint currentReadIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="HidPort"/> class.
        /// </summary>
        /// <param name="devicePath">
        /// The device path.
        /// </param>
        public HidPort(string devicePath)
        {
            this.DevicePath = devicePath;
            this.State = ConnectedStates.Unknown;
            this.HidInformation = new HidInformation();
        }

        #region Events

        /// <summary>
        /// The device state changed.
        /// </summary>
        public event EventHandler<ConnectedStateChangedEventArgs> DeviceStateChanged;

        /// <summary>
        /// The data received.
        /// </summary>
        public event EventHandler<EventArgs> DataReceived;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether we are connected to a device.
        /// </summary>
        public bool IsOpen
        {
            get => this.isOpen;

            private set
            {
                if (this.isOpen == value)
                {
                    return;
                }

                this.isOpen = value;
                this.DeviceStateChanged.RaiseEvent(this, new ConnectedStateChangedEventArgs(this.isOpen, this.State));
            }
        }

        /// <summary>
        /// Gets or sets the device path.
        /// </summary>
        public string DevicePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the read buffer size.
        /// </summary>
        public int ReadBufferSize
        {
            get => this.readBufferSize;

            set
            {
                if (this.HidInformation != null && this.HidInformation.OutputReportLength > value)
                {
                    value = this.HidInformation.OutputReportLength;
                }

                this.readBufferSize = value;
                this.readBuffer = new byte[value];
            }
        }

        /// <summary>
        /// Gets the bytes to read.
        /// </summary>
        public uint BytesToRead
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the hid serial number.
        /// </summary>
        public string HidSerialNumber
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the hid product.
        /// </summary>
        public string HidProduct { get; set; }

        /// <summary>
        /// Gets the device information.
        /// </summary>
        public HidInformation HidInformation
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets The Connected state.
        /// </summary>
        internal ConnectedStates State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets The device safe handle.
        /// </summary>
        protected internal SafeFileHandle DeviceHandle
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get hid devices.
        /// </summary>
        /// <returns>
        /// The IEnumerable of device information.
        /// </returns>
        public static IEnumerable<HidInformation> GetHidDevices()
        {
            return NativeMethods.GetHidObjects();
        }

        /// <summary>
        /// Opens the device if a valid Device is selected.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// Will throw an exception if the device path is empty.
        /// </exception>
        public bool Open()
        {
            if (this.IsOpen)
            {
                throw new Exception("Device is Already Open");
            }

            if (string.IsNullOrEmpty(this.DevicePath))
            {
                throw new ArgumentNullException(this.DevicePath, "Device Path is Not Set");
            }

            this.State = ConnectedStates.Disconnected;

            this.BytesToRead = this.currentReadIndex = 0;

            var isReadOnly = false;

            // try to open the device. 
            this.DeviceHandle = NativeMethods.OpenDevice(this.DevicePath, ref isReadOnly);

            // We don't have a Valid Device.
            if (this.DeviceHandle.IsInvalid)
            {
                return this.IsOpen = false;
            }

            var info = new HidInformation { IsReadOnly = isReadOnly };

            NativeMethods.HidD_GetPreparsedData(this.DeviceHandle, out var preparsedData);
            NativeMethods.HidP_GetCaps(preparsedData, out var capabilities);
            NativeMethods.HidD_GetAttributes(this.DeviceHandle, out var attributes);
            NativeMethods.ReadDeviceCapabilities(capabilities, preparsedData, ref info);

            info.Capabilities = capabilities;
            info.DevicePath = this.DevicePath;
            info.HidAttributes = attributes;

            NativeMethods.HidD_FreePreparsedData(ref preparsedData);

            // If we have a valid handle we can then connect the the HID device.
            if (this.DeviceHandle.IsInvalid || this.DeviceHandle.IsClosed)
            {
                return this.IsOpen = false;
            }

            // set our variable to the passed value.
            this.HidInformation = info;

            // verify our buffer is large enough for the device.
            this.ReadBufferSize = this.readBufferSize;

            // Initialize some device properties.
            this.Initialize();

            // Get the Hid Serial Number.
            this.ReadProductAndSerial();

            // Update our current connected state.
            this.State = this.isOpen ? ConnectedStates.Connected : ConnectedStates.Failed;

            // return our current connected state.
            return this.IsOpen;
        }

        /// <summary>
        /// Close the connection to the device.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            if (!this.isOpen)
            {
                return;
            }

            this.BytesToRead = this.currentReadIndex = 0;

            if (this.deviceStream != null)
            {
                this.deviceStream.Dispose();
                this.deviceStream = null;
            }

            if (!this.DeviceHandle.IsClosed || !this.DeviceHandle.IsInvalid)
            {
                this.DeviceHandle.Dispose();
            }

            this.IsOpen = false;
        }

        /// <summary>
        /// The read.
        /// </summary>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="length">
        /// The length to read.
        /// </param>
        /// <returns>
        /// The byte array read.
        /// </returns>
        /// <exception cref="Exception">
        /// If we are not connected will throw an exception.
        /// </exception>
        public byte[] Read(uint offset, uint length)
        {
            if (!this.IsOpen)
            {
                throw new NotConnectedException();
            }

            if (length < 1)
            {
                return null;
            }

            var response = new List<byte>((int)length);

            for (var index = (offset - this.BytesToRead) + this.currentReadIndex; response.Count < length; index++)
            {
                response.Add(this.readBuffer[index % this.ReadBufferSize]);
            }

            this.BytesToRead -= length;

            return response.ToArray();
        }

        /// <summary>
        /// The read all bytes
        /// </summary>
        /// <returns>
        /// The byte array read.
        /// </returns>
        public byte[] ReadAllBytes()
        {
            return this.Read(0, this.BytesToRead);
        }

        /// <summary>
        /// The write enumerable of bytes.
        /// </summary>
        /// <param name="data">
        /// The data to write.
        /// </param>
        public void Write(IEnumerable<byte> data)
        {
            var array = data as byte[] ?? data.ToArray();
            this.Write(array, 0, array.Length);
        }

        /// <summary>
        /// Writes to a connected device.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public void Write(byte[] data)
        {
            this.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes to a connected device.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <exception cref="Exception">
        /// If we are not connected will throw an exception.
        /// </exception>
        public void Write(byte[] data, int offset, int length)
        {
            if (!this.IsOpen || this.HidInformation.IsReadOnly)
            {
                return;
                /*
                                throw new Exception("Device not Connected To");
                */
            }

            try
            {
                // write the data to the HID Device Stream.
                // and wait for the write to complete.
                this.deviceStream.Write(data, offset, length);

                // Force a flush to push the current bytes to the device.
                this.deviceStream.Flush();
            }
            catch
            {
                // Failed connection.
                this.State = ConnectedStates.Failed;

                // we are no longer open.
                this.Close();
            }
        }

        /// <summary>
        /// The Get feature.
        /// </summary>
        /// <param name="data">
        /// The feature report to Request.
        /// </param>
        /// <returns>
        /// The Response from the Get Feature Request as an IList<byte></byte>
        /// </returns>
        public IList<byte> GetFeature(byte[] data)
        {
            Array.Resize(ref data, this.HidInformation.Capabilities.FeatureReportByteLength);
            return NativeMethods.HidD_GetFeature(this.DeviceHandle, data, data.Length) ? data : new byte[0];
        }

        /// <summary>
        /// The set report.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool SetOutputReport(byte[] data)
        {
            Array.Resize(ref data, this.HidInformation.Capabilities.OutputReportByteLength);
            return NativeMethods.HidD_SetOutputReport(this.DeviceHandle, data, data.Length);
        }

        /// <summary>
        /// The Get input report.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The buffer of bytes.
        /// </returns>
        public byte[] GetInputReport(params byte[] data)
        {
            Array.Resize(ref data, this.HidInformation.Capabilities.InputReportByteLength);
            return NativeMethods.HidD_GetInputReport(this.DeviceHandle, data, data.Length) ? data : new byte[0];
        }

        /// <summary>
        /// The Set feature.
        /// </summary>
        /// <param name="data">
        /// The The feature report to Set.
        /// </param>
        /// <returns>
        /// The Response from the Get Feature Request as an IList<byte></byte>
        /// </returns>
        public IList<byte> SetFeature(byte[] data)
        {
            Array.Resize(ref data, this.HidInformation.Capabilities.FeatureReportByteLength);
            return NativeMethods.HidD_SetFeature(this.DeviceHandle, data, data.Length) ? data : new byte[0];
        }

        /// <summary>
        /// The initialize creates our reader thread etc.
        /// </summary>
        private void Initialize()
        {
            // Enable the device stream so we can write/read to and from the device async.
            this.deviceStream = new FileStream(this.DeviceHandle, FileAccess.ReadWrite, this.HidInformation.InputReportLength, true);

            // Set Connected to true and start the reading Thread.
            this.IsOpen = true;

            // Start the reading Task.
            Task.Factory.StartNew(this.DeviceReader, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// The read Hid serial.
        /// </summary>
        /// <param name="size">
        /// The size.
        /// </param>
        private void ReadProductAndSerial(int size = 250)
        {
            var buffer = new byte[size];

            if (NativeMethods.HidD_GetSerialNumberString(this.DeviceHandle, buffer, buffer.Length))
            {
                this.HidSerialNumber = Encoding.Unicode.GetString(buffer).TrimEnd('\0');
            }

            if (NativeMethods.HidD_GetProductString(this.DeviceHandle, buffer, buffer.Length))
            {
                this.HidProduct = Encoding.Unicode.GetString(buffer).TrimEnd('\0');
            }
        }

        #endregion

        #region Reading Task

        /// <summary>
        /// Reads the Hid devices available Data
        /// </summary>
        /// <returns>Byte[] of Read Data</returns>
        private IEnumerable<byte> ReadFromDevice()
        {
            // Create a report Buffer size is based on the Input Report Length.
            var report = new byte[this.HidInformation.InputReportLength];

            try
            {
                if (this.currentReadIndex + report.Length >= this.readBuffer.Length)
                {
                    this.currentReadIndex = 0;
                }

                // try to read the report.
                var read = (uint)this.deviceStream.Read(this.readBuffer, (int)this.currentReadIndex, report.Length);

                this.currentReadIndex += read;
                this.BytesToRead += read;
            }
            catch
            {
                this.State = ConnectedStates.Failed;

                // not connected to the device anymore?
                report = null;
            }

            // return the report.
            return report;
        }

        /// <summary>
        /// Thread that runs to read data from the device.
        /// </summary>
        private void DeviceReader()
        {
            while (this.IsOpen)
            {
                // Get any Data available from the device.
                var packet = this.ReadFromDevice();

                // if we receive a null packet we are no longer connected to the device 
                // and we should stop the reader thread. 
                if (packet == null)
                {
                    this.Close();
                    return;
                }

                this.DataReceived.RaiseEvent(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}