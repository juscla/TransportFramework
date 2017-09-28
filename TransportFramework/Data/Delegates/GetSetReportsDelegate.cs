namespace TransportFramework.Data.Delegates
{
    /// <summary>
    /// Set Output report Delegate
    /// </summary>
    /// <param name="data">
    /// The Report to Set.
    /// </param>
    /// <returns>
    /// Whether the set was successful or failed. 
    /// </returns>
    public delegate bool SetOutputReport(params byte[] data);

    /// <summary>
    /// Get Input report Delegate
    /// </summary>
    /// <param name="data">
    /// The Report to request [ will auto resize if required ]
    /// </param>
    /// <returns>
    /// The response from the Report [ If empty means this has failed.]
    /// </returns>
    public delegate byte[] GetInputReport(params byte[] data);
}
