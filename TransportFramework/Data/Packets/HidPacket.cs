namespace TransportFramework.Data.Packets
{
    using System.Collections.Generic;

    /// <summary>
    /// The hid packet.
    /// </summary>
    public class HidPacket : PacketBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HidPacket"/> class.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        public HidPacket(IEnumerable<byte> data, TypeOfPacket type = TypeOfPacket.Rx)
            : base(data)
        {
            this.Type = type;
            this.DataLength = this.Data.Length;
        }

        /// <summary>
        /// Gets or sets the data length.
        /// </summary>
        public override sealed int DataLength { get; protected set; }
    }
}