namespace TransportFramework.Data.Packets
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// The Win-USB packet.
    /// </summary>
    public sealed class WinUsbPacket : PacketBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WinUsbPacket"/> class.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="dataLength">
        /// The data length.
        /// </param>
        public WinUsbPacket(IEnumerable<byte> data, int dataLength)
            : base(data)
        {
            this.DataLength = dataLength;
        }

        /// <summary>
        /// Gets or sets the data length.
        /// </summary>
        public override int DataLength { get; protected set; }

        /// <summary>
        /// The to string. 
        /// </summary>
        /// <param name="hex">
        /// The hex.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public new string ToString(bool hex)
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