namespace TransportFramework.Parsers.Remote
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using TransportFramework.Data.Delegates;
    using TransportFramework.Data.Interfaces;
    using TransportFramework.Helpers;
    using TransportFramework.Parsers.Base;
    using TransportFramework.Transports.Hid;
    using TransportFramework.Transports.Network;

    /// <summary>
    /// The remote.
    /// </summary>
    public class Remote : ControllableParser, IFeatureParser, IGetSetReportParser
    {
        /// <summary>
        /// The port number.
        /// </summary>
        public const int PortNumber = 888;

        /// <summary>
        /// The heart beat message.
        /// </summary>
        public const string HeartBeatMessage = "ServerActive";

        /// <summary>
        /// The offset.
        /// </summary>
        private const int Offset = 2;

        /// <summary>
        /// The server.
        /// </summary>
        private readonly TcpListener server = new TcpListener(IPAddress.Any, PortNumber);

        /// <summary>
        /// The clients.
        /// </summary>
        private readonly List<TcpClient> clients = new List<TcpClient>(10);

        /// <summary>
        /// The packets to send.
        /// </summary>
        private readonly ConcurrentQueue<byte[]> packetsToSend = new ConcurrentQueue<byte[]>();

        /// <summary>
        /// The heart beat on.
        /// </summary>
        private bool heartBeatOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="Remote"/> class.
        /// </summary>
        /// <param name="device">
        /// The device.
        /// </param>
        public Remote(HidTransportBase device = null) : base(device)
        {
            this.HeartBeatTime = 5000;
            this.BufferSize = 62;
            this.Ip =
                Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
        }

        /// <summary>
        /// Gets or sets the buffer size.
        /// </summary>
        public int BufferSize { get; protected set; }

        /// <summary>
        /// Gets the IP address of the server.
        /// </summary>
        public IPAddress Ip { get; }

        /// <summary>
        /// Gets or sets the get feature.
        /// </summary>
        public GetFeature GetFeature { get; set; }

        /// <summary>
        /// Gets or sets the set feature.
        /// </summary>
        public SetFeature SetFeature { get; set; }

        /// <summary>
        /// Gets or sets the get report.
        /// </summary>
        public GetInputReport GetReport { get; set; }

        /// <summary>
        /// Gets or sets the set report.
        /// </summary>
        public SetOutputReport SetReport { get; set; }

        /// <summary>
        /// Gets or sets the heart beat time.
        /// </summary>
        public int HeartBeatTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// The received.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public override void Received(byte[] data)
        {
            // append a blade message tag to the data.
            // add the length to the packet.
            var send = new byte[Math.Max(data.Length, this.BufferSize)];

            var offset = 0;
            send[offset++] = NetworkTransport.RemoteMessageTypes.ClientHid.ToByte();
            send[offset++] = (byte)data.Length;

            Buffer.BlockCopy(data, 0, send, offset, data.Length);

            this.NotifyClients(send);
        }

        /// <summary>
        /// The Send Message to all clients.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void SendMessage(string message)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            // append a Server message tag to the data.
            var temp = Encoding.ASCII.GetBytes(message);
            var data = new List<byte> { NetworkTransport.RemoteMessageTypes.Server.ToByte(), (byte)temp.Length };
            data.AddRange(temp);

            this.NotifyClients(data.ToArray());
        }

        /// <summary>
        /// The Send Message to a specific client.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        public void SendMessage(string message, TcpClient client)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            // append a Server message tag to the data.
            var temp = Encoding.ASCII.GetBytes(message);
            var data = new List<byte> { NetworkTransport.RemoteMessageTypes.Server.ToByte(), (byte)temp.Length };
            data.AddRange(temp);

            client.Client.Send(data.ToArray());
        }

        /// <summary>
        /// The start.
        /// </summary>
        public override void Start()
        {
            if (this.IsEnabled)
            {
                return;
            }

            this.server.Start();

            this.IsEnabled = true;

            // start the Sender thread
            // we use a queue to avoid losing any data.
            new Thread(this.Sender) { Priority = ThreadPriority.Highest }.Start();

            Task.Factory.StartNew(this.ListenForClients, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// The stop.
        /// </summary>
        public override void Stop()
        {
            this.IsEnabled = false;
            this.heartBeatOn = false;

            this.server.Stop();

            lock (this.clients)
            {
                this.clients.Clear();
            }
        }

        /// <summary>
        /// The notify clients.
        /// </summary>
        /// <param name="packet">
        /// The packet to add the queue.
        /// </param>
        public void NotifyClients(byte[] packet)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            // force our array to be the correct size.
            Array.Resize(ref packet, Math.Max(packet.Length, this.BufferSize));

            // add the packet to the queue
            this.packetsToSend.Enqueue(packet);
        }

        /// <summary>
        /// The process packet.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        protected virtual void ProcessPacket(IList<byte> data)
        {
            byte[] response;

            var type = data[0].ToEnum<NetworkTransport.RemoteMessageTypes>();
            var length = data[1];

            switch (type)
            {
                case NetworkTransport.RemoteMessageTypes.ClientGetFeature:
                case NetworkTransport.RemoteMessageTypes.ClientSetFeature:
                    var result = type == NetworkTransport.RemoteMessageTypes.ClientGetFeature
                                     ? this.GetFeature(data.Skip(Offset).Take(length).ToArray()).ToArray()
                                     : this.SetFeature(data.Skip(Offset).Take(length).ToArray()).ToArray();

                    if (result.Length < 1)
                    {
                        break;
                    }

                    response = new byte[result.Length + 2];
                    response[0] = data[0];
                    response[1] = (byte)result.Length;

                    Buffer.BlockCopy(result, 0, response, Offset, result.Length);

                    this.NotifyClients(response);
                    break;

                case NetworkTransport.RemoteMessageTypes.ClientGetReport:
                    var r = this.GetReport(data.Skip(Offset).Take(length).ToArray());
                    response = new byte[r.Length + 2];
                    response[0] = data[0];
                    response[1] = (byte)(r.Length - 1);

                    Buffer.BlockCopy(r, 0, response, Offset, response.Length);
                    this.NotifyClients(response);
                    break;

                case NetworkTransport.RemoteMessageTypes.ClientHid:
                    this.SendDevice(data.Skip(Offset).Take(length).ToArray());
                    break;
            }
        }

        /// <summary>
        /// The listen for clients.
        /// </summary>
        private void ListenForClients()
        {
            while (this.IsEnabled)
            {
                try
                {
                    var client = this.server.AcceptTcpClient();

                    lock (this.clients)
                    {
                        this.clients.Add(client);
                    }

                    Task.Run(() => this.ListenToClient(client));

                    if (this.clients.Count == 1)
                    {
                        Task.Factory.StartNew(this.HeartBeat);
                    }

                    this.SendMessage($"Connected to {this.Ip}", client);
                }
                catch
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// The heart beat.
        /// </summary>
        private void HeartBeat()
        {
            if (this.heartBeatOn)
            {
                return;
            }

            this.heartBeatOn = true;

            while (this.heartBeatOn && this.clients.Count > 0)
            {
                this.SendMessage(HeartBeatMessage);
                Thread.Sleep(this.HeartBeatTime);
            }

            this.heartBeatOn = false;
        }

        /// <summary>
        /// The listen to client.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        private void ListenToClient(TcpClient client)
        {
            while (client.Connected)
            {
                try
                {
                    var data = new byte[this.BufferSize];

                    var length = client.Client.Receive(data);

                    if (length < 1)
                    {
                        break;
                    }

                    this.ProcessPacket(data);
                }
                catch
                {
                    // ignored
                }
            }

            lock (this.clients)
            {
                this.clients.Remove(client);
            }
        }

        /// <summary>
        /// The sender.
        /// </summary>
        private void Sender()
        {
            while (this.IsEnabled)
            {
                if (this.packetsToSend.Count < 0)
                {
                    continue;
                }

                byte[] data;

                if (!this.packetsToSend.TryDequeue(out data))
                {
                    continue;
                }

                Parallel.ForEach(
                    this.clients,
                    c =>
                        {
                            try
                            {
                                c.Client.Send(data);
                            }
                            catch
                            {
                                // ignored
                            }
                        });
            }
        }
    }
}
