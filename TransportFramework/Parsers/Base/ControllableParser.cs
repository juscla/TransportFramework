using TransportFramework.Transports.Hid;

namespace TransportFramework.Parsers.Base
{
    using TransportFramework.Transports.Base;

    /// <summary>
    /// The controllable parser.
    /// </summary>
    public abstract class ControllableParser : Parser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllableParser"/> class.
        /// </summary>
        /// <param name="device">
        /// The device.
        /// </param>
        protected ControllableParser(TransportBase device = null) : base(device)
        {
        }

        /// <summary>
        /// The start.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// The stop.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// The dispose.
        /// </summary>
        public override void Dispose()
        {
            this.Stop();
            base.Dispose();
        }
    }
}
