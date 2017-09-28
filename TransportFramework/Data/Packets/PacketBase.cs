namespace TransportFramework.Data.Packets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The packet base.
    /// </summary>
    public abstract class PacketBase : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PacketBase"/> class.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        protected PacketBase(IEnumerable<byte> data)
        {
            this.Timestamp = DateTime.Now;
            this.Data = data.ToArray();
        }

        /// <summary>
        /// The types of packet.
        /// </summary>
        public enum TypeOfPacket
        {
            /// <summary>
            /// The Received.
            /// </summary>
            Rx,

            /// <summary>
            /// The Sent.
            /// </summary>
            Tx,
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public TypeOfPacket Type
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public byte[] Data
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the data length.
        /// </summary>
        public abstract int DataLength
        {
            get;
            protected set;
        }

        /// <summary>
        /// The to string. 
        /// </summary>
        /// <param name="hex">
        /// The hex.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ToString(bool hex)
        {
            var builder = new StringBuilder($"{this.Timestamp:hh:mm:ss.fff} \t {this.Type}: ");

            var format = hex ? "X2" : "00";

            builder.Append(this.DataLength.ToString(format) + " - ");

            for (var index = 0; index < this.DataLength; index++)
            {
                builder.Append(this.Data[index].ToString(format) + ' ');
            }

            return builder.ToString().Trim();
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return this.ToString(true);
        }
    }
}