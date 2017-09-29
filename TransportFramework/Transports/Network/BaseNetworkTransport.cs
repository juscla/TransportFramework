namespace TransportFramework.Transports.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using Base;

    using Data.Packets;

    /// <summary>
    /// The base network Transport.
    /// </summary>
    public class BaseNetworkTransport : TransportBase
    {
        /// <summary>
        /// The port.
        /// </summary>
        private readonly TcpClient port = new TcpClient();

        /// <summary>
        /// The port number.
        /// </summary>
        private readonly int portNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseNetworkTransport"/> class.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="port">
        /// The port.
        /// </param>
        public BaseNetworkTransport(string path, int port)
        {
            this.DevicePath = path;
            this.portNumber = port;
            this.MinimumBufferSize = 1024;
        }

        /// <summary>
        /// Gets or sets a value indicating whether is connected.
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return this.port.Client != null && this.port.Connected;
            }

            protected set
            {
            }
        }

        /// <summary>
        /// Gets or sets the minimum buffer size.
        /// </summary>
        public int MinimumBufferSize { get; protected set; }

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
                throw new Exception("Device Path Cannot be empty");
            }

            var result = this.port.BeginConnect(this.DevicePath, this.portNumber, null, null);
            result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

            if (!this.port.Connected)
            {
                return false;
            }

            Task.Run(() => this.HandleReceive());
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
            this.port.Close();
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
            if (!this.IsConnected)
            {
                return;
            }

            this.port.Client.Send(data.ToArray());
        }

        /// <summary>
        /// The handle packet.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        protected virtual void HandlePacket(IEnumerable<byte> data)
        {
            var packet = new NetworkPacket(data.ToArray());
            this.NotifyParsers(packet);
            this.OnReceived(packet);
        }

        /// <summary>
        /// The handle receive.
        /// </summary>
        private void HandleReceive()
        {
            var data = new byte[this.MinimumBufferSize];

            while (this.IsConnected)
            {
                try
                {
                    var length = this.port.Client.Receive(data, 0, data.Length, SocketFlags.None);

                    if (length < 1)
                    {
                        break;
                    }

                    this.HandlePacket(data.Take(length));
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
