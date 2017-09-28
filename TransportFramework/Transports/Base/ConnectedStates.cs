namespace TransportFramework.Transports.Base
{
    /// <summary>
    /// The device states.
    /// </summary>
    public enum ConnectedStates
    {
        /// <summary>
        /// The unknown.
        /// </summary>
        Unknown, 

        /// <summary>
        /// Connected to device.
        /// </summary>
        Connected,

        /// <summary>
        /// Disconnected from the device..
        /// </summary>
        Disconnected,

        /// <summary>
        /// Connection Failed.
        /// </summary>
        Failed,
    }
}