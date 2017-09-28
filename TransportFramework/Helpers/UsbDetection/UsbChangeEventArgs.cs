namespace TransportFramework.Helpers.UsbDetection
{
    using Transports.Base.Native;

    /// <summary>
    /// The device change event args.
    /// </summary>
    public class UsbChangeEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsbChangeEventArgs"/> class.
        /// </summary>
        internal UsbChangeEventArgs()
        {
            this.Path = string.Empty;
        }

        /// <summary>
        /// Gets or sets the device Path.
        /// </summary>
        public string Path
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public UsbStates State
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the information.
        /// </summary>
        public HidInformation Information
        {
            get; 
            set;
        }
    }
}