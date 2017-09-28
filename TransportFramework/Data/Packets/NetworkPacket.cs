namespace TransportFramework.Data.Packets
{
    using System.Collections.Generic;

    /// <summary>
    /// The network packet.
    /// </summary>
    public class NetworkPacket : PacketBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkPacket"/> class.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public NetworkPacket(IList<byte> data) : base(data)
        {
            this.DataLength = data.Count;
        }

        /// <summary>
        /// Gets or sets the data length.
        /// </summary>
        public override sealed int DataLength
        {
            get; protected set;
        }
    }
}
