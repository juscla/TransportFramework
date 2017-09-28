namespace TransportFramework.Data.Interfaces
{
    using Delegates;

    /// <summary>
    /// The Feature Parser interface.
    /// </summary>
    public interface IFeatureParser
    {
        /// <summary>
        ///  Gets or sets the get feature callback.
        /// </summary>
        GetFeature GetFeature { get; set; }

        /// <summary>
        ///  Gets or sets the set feature callback.
        /// </summary>
        SetFeature SetFeature { get; set; }
    }
}