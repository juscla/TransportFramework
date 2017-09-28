namespace TransportFramework.Transports.Serial
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Data.Packets;

    using Helpers;

    using Microsoft.Win32;

    using Transports.Base;

    /// <summary>
    /// The serial Transport.
    /// </summary>
    public class SerialTransport : TransportBase
    {
        /// <summary>
        /// The state.
        /// </summary>
        private ConnectedStates state = ConnectedStates.Unknown;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialTransport"/> class.
        /// </summary>
        /// <param name="portName">
        /// The port name.
        /// </param>
        /// <param name="baud">
        /// The baud.
        /// </param>
        public SerialTransport(string portName, int baud = 9600)
        {
            this.Port = new SerialPort
             {
                 BaudRate = baud,
                 Parity = Parity.None,
                 DataBits = 8,
                 StopBits = StopBits.One,
                 DtrEnable = true,
                 RtsEnable = true,
                 PortName = this.DevicePath = portName
             };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialTransport"/> class.
        /// </summary>
        /// <param name="port">
        /// The port.
        /// </param>
        public SerialTransport(SerialPort port)
        {
            this.Port = port;
            this.DevicePath = port.PortName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialTransport"/> class.
        /// </summary>
        /// <param name="port">
        /// The port.
        /// </param>
        /// <param name="baud">
        /// The baud.
        /// </param>
        public SerialTransport(int port, int baud = 9600) : this("COM" + port, baud)
        {
        }

        #region Events

        /// <summary>
        /// Gets or sets the device state changed.
        /// </summary>
        public event EventHandler<ConnectedStateChangedEventArgs> DeviceStateChanged;

        #endregion

        /// <summary>
        /// Gets or sets the baud rate.
        /// </summary>
        public int BaudRate
        {
            get => this.Port.BaudRate;
            set => this.Port.BaudRate = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether is connected.
        /// </summary>
        public override bool IsConnected
        {
            get => this.Port.IsOpen;

            protected set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        public SerialPort Port { get; protected set; }

        /// <summary>
        /// Gets or sets The Connected state.
        /// </summary>
        internal ConnectedStates State
        {
            get => this.state;

            set
            {
                if (this.state == value)
                {
                    return;
                }

                this.state = value;

                this.DeviceStateChanged.RaiseEvent(
                    this,
                    new ConnectedStateChangedEventArgs(this.state == ConnectedStates.Connected, this.state));
            }
        }

        /// <summary>
        /// The get ports.
        /// </summary>
        /// <param name="vendorId">
        /// The vendor id.
        /// </param>
        /// <returns>
        /// The table of COM ports Vendor Id and Product Id associated with the COM Port.
        /// </returns>
        public static Dictionary<string, string> GetPorts(ushort vendorId = 0x0403)
        {
            var comports = new Dictionary<string, string>();

            var rx = new Regex($"^vid_{vendorId:X4}", RegexOptions.IgnoreCase);

            var rk2 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");

            foreach (var s3 in rk2.GetSubKeyNames())
            {
                var rk3 = rk2.OpenSubKey(s3);
                foreach (var s in rk3.GetSubKeyNames())
                {
                    if (!rx.Match(s).Success)
                    {
                        continue;
                    }

                    var rk4 = rk3.OpenSubKey(s);
                    foreach (var s2 in rk4.GetSubKeyNames())
                    {
                        var rk5 = rk4.OpenSubKey(s2);
                        var location = (string)rk5.GetValue("FriendlyName");
                        if (string.IsNullOrEmpty(location))
                        {
                            continue;
                        }

                        var port = location.Substring(location.IndexOf('(') + 1).TrimEnd(')');
                        if (port != string.Empty)
                        {
                            comports.Add(rk5.Name.Substring(rk5.Name.IndexOf("VID")), port);
                        }
                    }
                }
            }

            return comports.OrderBy(x => x.Value).ToDictionary(x => x.Key.Replace("\\", "#"), x => x.Value);
        }

        /// <summary>
        /// The get devices.
        /// </summary>
        /// <param name="vid">
        ///     The Vendor Id.
        /// </param>
        /// <param name="pid">
        ///     The Product Id.
        /// </param>
        /// <returns>
        /// The List of devices found.
        /// </returns>
        public static IEnumerable<string> GetListOfDevices(ushort vid, ushort pid)
        {
            return
                GetPorts(vid)
                    .Where(x => x.Key.Contains(pid.ToString("X4")))
                    .Select(x => x.Value)
                    .Intersect(GetListOfDevices());
        }

        /// <summary>
        /// The get list of Serial Port devices currently Attached.
        /// </summary>
        /// <returns>
        /// The List of device paths. 
        /// </returns>
        public static IEnumerable<string> GetListOfDevices()
        {
            var response = new Dictionary<string, string>();

            using (var reg = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM"))
            {
                if (reg == null)
                {
                    return new string[0];
                }

                foreach (var s in reg.GetValueNames())
                {
                    response.Add(s, reg.GetValue(s).ToString());
                }
            }

            return response.Where(x => x.Key.ToUpper().Contains("VCP") || x.Key.ToUpper().Contains("USBSER")).Select(x => x.Value).OrderBy(x => x);
        }

        /// <summary>
        /// The open.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Open()
        {
            return this.Connect();
        }

        /// <summary>
        /// The connect.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Connect()
        {
            if (!this.IsConnected)
            {
                this.Port.Open();

                this.Port.DiscardInBuffer();

                this.Port.DiscardOutBuffer();

                this.State = this.IsConnected ? ConnectedStates.Connected : ConnectedStates.Failed;
            }

            if (this.State != ConnectedStates.Connected)
            {
                return false;
            }

            this.Port.DataReceived += this.ReceivedData;

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
            this.Port.DtrEnable = false;
            this.Port.RtsEnable = false;

            this.Port.DiscardInBuffer();
            this.Port.DiscardOutBuffer();

            this.Port.DataReceived -= this.ReceivedData;

            this.Port.Dispose();

            return this.IsConnected;
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
            var raw = data.ToArray();

            this.OnSent(new SerialPacket(raw));

            try
            {
                this.Port.Write(raw, 0, raw.Length);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// The received data.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected virtual void ReceivedData(object sender, SerialDataReceivedEventArgs e)
        {
            var packet = this.ReadAllBytes();

            // notify all subsystems of the data.
            this.NotifyParsers(packet);

            this.OnReceived(packet);
        }

        /// <summary>
        /// The read all bytes
        /// </summary>
        /// <returns>
        /// The byte array read.
        /// </returns>
        private SerialPacket ReadAllBytes()
        {
            var response = new byte[this.Port.BytesToRead];
            var length = this.Port.Read(response, 0, response.Length);
            return new SerialPacket(response.Take(length).ToList());
        }
    }
}
