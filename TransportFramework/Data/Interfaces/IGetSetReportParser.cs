namespace TransportFramework.Data.Interfaces
{
    using Delegates;

    /// <summary>
    /// The GetSetReportSubSystem interface.
    /// </summary>
    public interface IGetSetReportParser
    {
        /// <summary>
        /// Gets or sets the get report.
        /// </summary>
        GetInputReport GetReport { get; set; }

        /// <summary>
        /// Gets or sets the set report.
        /// </summary>
        SetOutputReport SetReport { get; set; }
    }
}
