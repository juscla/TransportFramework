namespace TransportFramework.Data.Exceptions
{
    /// <summary>
    /// The not connected exception.
    /// </summary>
    public class NotConnectedException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotConnectedException"/> class.
        /// </summary>
        public NotConnectedException() : base("Not Connected to a Device")
        {
        }
    }
}