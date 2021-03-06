﻿namespace TransportFramework.Transports.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Data.Exceptions;
    using Data.Interfaces;
    using Data.Packets;
    using Helpers;
    using Parsers.Base;

    /// <summary>
    /// The network transport.
    /// </summary>
    public class NetworkTransport : BaseNetworkTransport
    {
        /// <summary>
        /// The feature.
        /// </summary>
        private byte[] feature = new byte[0];

        /// <summary>
        /// The report.
        /// </summary>
        private byte[] report = new byte[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkTransport"/> class.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        public NetworkTransport(string path, int port = 888) : base(path, port)
        {
            this.MinimumBufferSize = 63;
        }

        /// <summary>
        /// The messages from the server.
        /// From the server to determine if we are still connected to a server.
        /// Or relay information to Clients.
        /// </summary>
        public event EventHandler<string> Messages;

        /// <summary>
        /// The Remote message types.
        /// </summary>
        public enum RemoteMessageTypes
        {
            /// <summary>
            /// The server.
            /// </summary>
            Server,

            /// <summary>
            /// The Client Hid.
            /// </summary>
            ClientHid,

            /// <summary>
            /// The client get feature.
            /// </summary>
            ClientGetFeature,

            /// <summary>
            /// The client set feature.
            /// </summary>
            ClientSetFeature,

            /// <summary>
            /// The client get report.
            /// </summary>
            ClientGetReport,

            /// <summary>
            /// The client set report.
            /// </summary>
            ClientSetReport,
        }

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        public override void Write(params byte[] packet)
        {
            var data = new List<byte> { RemoteMessageTypes.ClientHid.ToByte(), (byte)packet.Length };
            data.AddRange(packet);
            this.WriteRaw(data);
        }

        /// <summary>
        /// The Get or Set Feature for the Device.
        /// </summary>
        /// <param name="packet">
        /// The packet to send.
        /// </param>
        /// <returns>
        /// The byte array response.
        /// </returns>
        public virtual byte[] GetFeature(params byte[] packet)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            this.feature = new byte[0];
            var toSend = new List<byte> { RemoteMessageTypes.ClientGetFeature.ToByte(), (byte)packet.Length };
            toSend.AddRange(packet);

            this.Wait(() => this.WriteRaw(toSend), () => this.feature.Length == 0, 0, 2000);
            return this.feature;
        }

        /// <summary>
        /// The Set Feature for the Device.
        /// </summary>
        /// <param name="packet">
        /// The packet to send.
        /// </param>
        /// <returns>
        /// The device responded
        /// </returns>
        public virtual byte[] SetFeature(params byte[] packet)
        {
            if (!this.IsConnected)
            {
                throw new NotConnectedException();
            }

            this.feature = new byte[0];
            var toSend = new List<byte> { RemoteMessageTypes.ClientSetFeature.ToByte(), (byte)packet.Length };
            toSend.AddRange(packet);

            this.Wait(() => this.WriteRaw(toSend), () => this.feature.Length == 0, 0, 2000);
            return this.feature;
        }
        
        /// <summary>
        /// The set output report.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool SetOutputReport(byte[] data)
        {
            this.report = new byte[0];
            var toSend = new List<byte> { RemoteMessageTypes.ClientSetReport.ToByte(), (byte)data.Length };
            toSend.AddRange(data);

            this.Wait(() => this.WriteRaw(toSend), () => this.report.Length == 0, 0, 2000);
            return this.report.Length > 0;
        }

        /// <summary>
        /// The get input report.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The Buffer of bytes.
        /// </returns>
        public byte[] GetInputReport(params byte[] data)
        {
            this.report = new byte[0];
            var toSend = new List<byte> { RemoteMessageTypes.ClientGetReport.ToByte(), (byte)data.Length };
            toSend.AddRange(data);

            this.Wait(() => this.WriteRaw(toSend), () => this.report.Length == 0, 0, 2000);
            return this.report;
        }

        /// <summary>
        /// The add subsystem.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        /// <param name="autoSubscribe">
        /// The auto Subscribe.
        /// </param>
        public override void AddParser(Parser system, bool autoSubscribe = true)
        {
            var feat = system as IFeatureParser;

            if (feat != null && feat.GetFeature == null)
            {
                feat.GetFeature += this.GetFeature;
                feat.SetFeature += this.SetFeature;
            }

            var report = system as IGetSetReportParser;

            if (report != null)
            {
                report.GetReport += this.GetInputReport;
                report.SetReport += this.SetOutputReport;
            }

            base.AddParser(system, autoSubscribe);
        }

        /// <summary>
        /// The handle packet.
        /// </summary>
        /// <param name="raw">
        /// The raw.
        /// </param>
        protected override void HandlePacket(IEnumerable<byte> raw)
        {
            var data = raw.ToList();
            var length = data[1];

            if (data.Count < 3)
            {
                return;
            }

            var converted = data.Skip(2).Take(length).ToArray();

            // skip the first byte as it is our flag byte.
            switch (data[0].ToEnum<RemoteMessageTypes>())
            {
                case RemoteMessageTypes.Server:
                    this.Messages.RaiseEventClass(this, Encoding.ASCII.GetString(converted));
                    break;

                case RemoteMessageTypes.ClientHid:
                    var packet = new HidPacket(converted);
                    this.NotifyParsers(packet);

                    // notify any raw data listeners.
                    this.OnReceived(packet);
                    break;

                case RemoteMessageTypes.ClientGetFeature:
                case RemoteMessageTypes.ClientSetFeature:
                    this.feature = converted;
                    break;

                case RemoteMessageTypes.ClientGetReport:
                case RemoteMessageTypes.ClientSetReport:
                    this.report = converted;
                    break;
            }
        }
    }
}
