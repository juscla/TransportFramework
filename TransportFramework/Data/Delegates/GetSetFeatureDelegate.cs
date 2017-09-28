namespace TransportFramework.Data.Delegates
{
    /// <summary>
    /// The get feature.
    /// </summary>
    /// <param name="packet">
    /// The packet.
    /// </param>
    /// <returns>
    /// The byte array response.
    /// </returns>
    public delegate byte[] GetFeature(params byte[] packet);

    /// <summary>
    /// The set feature.
    /// </summary>
    /// <param name="packet">
    /// The packet.
    /// </param>
    /// <returns>
    /// The byte array response.
    /// </returns>
    public delegate byte[] SetFeature(params byte[] packet);
}