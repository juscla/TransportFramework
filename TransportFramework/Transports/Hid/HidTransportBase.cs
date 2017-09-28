namespace TransportFramework.Transports.Hid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Data.Exceptions;
    using Data.Interfaces;
    using Data.Packets;
    using Parsers.Base;
    using Transports.Base;
    using Transports.Base.Native;

    /// <summary>
    /// The hid transport base.
    /// </summary>
    public class HidTransportBase : TransportBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HidTransportBase"/> class.
        /// </summary>
        /// <param name="devicePath">
        /// The device path.
        /// </param>
        public HidTransportBase(string devicePath)
        {
            this.DevicePath = devicePath;
        }

        /// <summary>
        /// Gets a value indicating whether a device is connected.
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return this.Port.IsOpen;
            }

            protected set
            {
            }
        }

        /// <summary>
        /// Gets The HID Port.
        /// </summary>
        public HidPort Port { get; private set; } = new HidPort(string.Empty);

        /// <summary>
        /// The get list of devices.
        /// </summary>
        /// <returns>
        /// The List of device paths. 
        /// </returns>
        public static IEnumerable<string> GetListOfDevices()
        {
            return NativeMethods.GetHidPaths();
        }

        /// <summary>
        /// The connect.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Connect()
        {
            if (string.IsNullOrEmpty(this.DevicePath))
            {
                throw new NullReferenceException("The DevicePath Cannot be null");
            }

            if (this.IsConnected)
            {
                return true;
            }

            // create a new HidPort with the Device Path.
            this.Port = new HidPort(this.DevicePath);

            // subscribe to all Data received;
            this.Port.DataReceived += this.OnDataReceived;

            // open the port.
            this.Port.Open();

            // return the status.
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
            this.Port.Dispose();
            return this.IsConnected;
        }

        /// <summary>
        /// The add subsystem.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        /// <param name="autoSubscribe">
        /// The auto subscribe.
        /// </param>
        public override void AddParser(Parser system, bool autoSubscribe = true)
        {
            var report = system as IGetSetReportParser;

            if (report != null)
            {
                report.GetReport += this.GetInputReport;
                report.SetReport += this.SetOutputReport;
            }

            var feature = system as IFeatureParser;

            if (feature != null)
            {
                feature.GetFeature += this.GetFeature;
                feature.SetFeature += this.SetFeature;
            }

            base.AddParser(system, autoSubscribe);
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        public override void Write(params byte[] packet)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            // we have to create a buffer the correct size.
            var data = new byte[this.Port.HidInformation.OutputReportLength];

            packet.CopyTo(data, 0);

            this.WriteRaw(data);

            this.OnSent(new HidPacket(data, PacketBase.TypeOfPacket.Tx));
        }

        /// <summary>
        /// The write raw.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public override void WriteRaw(IEnumerable<byte> data)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            // write the data packet.
            this.Port.Write(data);
        }

        /// <summary>
        /// Gets a specific input report
        /// </summary>
        /// <param name="data">
        /// The report Data to Read into. 
        /// </param>
        /// <returns>
        /// The buffer of bytes.
        /// </returns>
        public virtual byte[] GetInputReport(params byte[] data)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            return this.Port.GetInputReport(data);
        }

        /// <summary>
        /// Sets a specific output report
        /// </summary>
        /// <param name="data">
        /// The report Data to Set. 
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public virtual bool SetOutputReport(params byte[] data)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            return this.Port.SetOutputReport(data);
        }

        /// <summary>
        /// The get feature.
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        /// <returns>
        /// The Buffer of bytes.
        /// </returns>
        public virtual byte[] GetFeature(params byte[] packet)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            var result = this.Port.GetFeature(packet);
            if (result.Count > 0)
            {
                this.OnReceived(new HidPacket(result));
            }

            return result.ToArray();
        }

        /// <summary>
        /// The set feature.
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        /// <returns>
        /// The Buffer of bytes.
        /// </returns>
        public virtual byte[] SetFeature(params byte[] packet)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            var result = this.Port.SetFeature(packet);
            if (result.Count > 0)
            {
                this.OnReceived(new HidPacket(result));
            }

            return result.ToArray();
        }

        /// <summary>
        /// The data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void OnDataReceived(object sender, EventArgs e)
        {
            // remove the first two bytes from the read data.
            var packet = new HidPacket(this.Port.ReadAllBytes());

            // Notify all Subsystems.
            this.NotifyParsers(packet);

            // notify any raw data listeners.
            this.OnReceived(packet);
        }
    }
}
