namespace TransportFramework.Data.Packets
{
    using System.Collections.Generic;

    /// <summary>
    /// The serial packet.
    /// </summary>
    public class SerialPacket : PacketBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerialPacket"/> class.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public SerialPacket(IList<byte> data)
            : base(data)
        {
            this.DataLength = data.Count;
        }

        /// <summary>
        /// Gets or sets the data length.
        /// </summary>
        public override sealed int DataLength { get; protected set; }
    }
}