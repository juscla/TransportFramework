namespace TransportFramework.Transports.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Data.Packets;
    using Helpers;
    using Parsers.Base;

    /// <summary>
    /// The transport base.
    /// </summary>
    public abstract class TransportBase : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransportBase"/> class.
        /// </summary>
        protected TransportBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportBase"/> class.
        /// </summary>
        /// <param name="devicePath">
        /// The device path.
        /// </param>
        protected TransportBase(string devicePath)
        {
            this.DevicePath = devicePath;
        }

        #region Events

        /// <summary>
        /// The sent event handler.
        /// Occurs anytime a packet is sent to the device.
        /// </summary>
        public virtual event EventHandler<PacketBase> Sent;

        /// <summary>
        /// The received event handler
        /// Occurs anytime a packet is received from the device.
        /// </summary>
        public virtual event EventHandler<PacketBase> Received;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Name of this connection type
        /// </summary>
        public string Name => this.GetType().Name.ToLower();

        /// <summary>
        /// Gets or sets a value indicating whether or not the device is connected and able to communicate
        /// </summary>
        public abstract bool IsConnected
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the device path for connection
        /// </summary>
        public string DevicePath
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the List of the Parsers that the Transport has
        /// </summary>
        public IList<Parser> Parsers { get; } = new List<Parser>(10);

        #endregion

        /// <summary>
        /// The add Parser.
        /// </summary>
        /// <param name="parser">
        /// The system.
        /// </param>
        /// <param name="autoSubscribe">
        /// The auto Subscribe.
        /// </param>
        public virtual void AddParser(Parser parser, bool autoSubscribe = true)
        {
            if (parser == null)
            {
                return;
            }

            if (autoSubscribe)
            {
                // tie the parser to the tranport.
                parser.SendDevice += this.Write;
            }

            this.Parsers.Add(parser);
        }

        /// <summary>
        /// The remove subsystem.
        /// </summary>
        /// <param name="parser">
        /// The parser.
        /// </param>
        /// <param name="dispose">
        /// The dispose.
        /// </param>
        public virtual void RemoveParser(Parser parser, bool dispose = true)
        {
            if (parser == null || !this.Parsers.Contains(parser))
            {
                return;
            }

            // remove the subsystem subscription.
            parser.SendDevice = null;

            lock (this.Parsers)
            {
                // remove the parser from our list.
                this.Parsers.Remove(parser);
            }

            if (dispose)
            {
                parser.Dispose();
            }
        }

        /// <summary>
        /// The add parsers.
        /// </summary>
        /// <param name="add">
        /// The parsers to add
        /// </param>
        /// <param name="autoSubscribe">
        /// The auto Subscribe.
        /// </param>
        public virtual void AddParsers(IEnumerable<Parser> add, bool autoSubscribe = true)
        {
            foreach (var item in add)
            {
                this.AddParser(item, autoSubscribe);
            }
        }

        /// <summary>
        /// The remove parsers.
        /// </summary>
        /// <param name="remove">
        /// The parsers to remove
        /// </param>
        /// <param name="dispose">
        /// The dispose.
        /// </param>
        public virtual void RemoveParsers(IEnumerable<Parser> remove, bool dispose = true)
        {
            foreach (var item in remove)
            {
                this.RemoveParser(item, dispose);
            }
        }

        /// <summary>
        /// Connect to the device
        /// </summary>
        /// <returns>TRUE: Connection made.   FALSE: Connection Failed!</returns>
        public abstract bool Connect();

        /// <summary>
        /// Disconnect from the Transport.  
        /// Should close all ports it can
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public abstract bool Disconnect();

        /// <summary>
        /// The write.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public abstract void Write(params byte[] data);

        /// <summary>
        /// The write raw.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public abstract void WriteRaw(IEnumerable<byte> data);

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="cleanManaged">
        /// The clean managed.
        /// </param>
        protected virtual void Dispose(bool cleanManaged)
        {
            foreach (var s in this.Parsers)
            {
                s.Dispose();
            }

            this.Disconnect();
        }

        /// <summary>
        /// The notify Parsers.
        /// </summary>
        /// <param name="packet">
        /// The packet.
        /// </param>
        protected void NotifyParsers(PacketBase packet)
        {
            // check if we have any payload to throw away empty packets.
            if (packet.Data.Length < 1 || packet.DataLength < 1)
            {
                return;
            }

            // pull the payload out of the packet. 
            var payload = packet.Data.Take(packet.DataLength).ToArray();
            
            // read the current parsers.
            var current = this.Parsers.Where(x => !x.IsWriteOnly).ToList();

            // notify all modules that are not Write Only of the new data.
            Parallel.ForEach(current, s => s.Received(payload));
        }

        /// <summary>
        /// The on sent.
        /// </summary>
        /// <param name="e">
        /// The event source.
        /// </param>
        /// <typeparam name="TEventArgs">
        /// The Event Type
        /// </typeparam>
        protected virtual void OnSent<TEventArgs>(TEventArgs e) where TEventArgs : EventArgs
        {
            this.Sent.RaiseEvent(this, e);
        }

        /// <summary>
        /// The on received.
        /// </summary>
        /// <param name="e">
        /// The event source.
        /// </param>
        /// <typeparam name="TEventArgs">
        /// The Event Type
        /// </typeparam>
        protected virtual void OnReceived<TEventArgs>(TEventArgs e) where TEventArgs : EventArgs
        {
            this.Received.RaiseEvent(this, e);
        }
    }
}
