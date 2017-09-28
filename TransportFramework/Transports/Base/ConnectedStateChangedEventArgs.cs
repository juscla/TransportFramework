namespace TransportFramework.Transports.Base
{
    using System;

    /// <summary>
    /// The transport connection state changed event args.
    /// </summary>
    public class ConnectedStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedStateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        internal ConnectedStateChangedEventArgs(bool value, ConnectedStates state)
        {
            this.Connected = value;
            this.State = state;
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public ConnectedStates State
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether connected.
        /// </summary>
        public bool Connected
        {
            get;
        }
    }
}