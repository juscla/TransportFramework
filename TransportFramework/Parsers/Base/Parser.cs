namespace TransportFramework.Parsers.Base
{
    using System;
    using System.Linq;

    using Data.Delegates;

    using TransportFramework.Data.Interfaces;

    using Transports.Base;

    /// <summary>
    /// The sub system.
    /// </summary>
    public abstract class Parser : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="device">
        /// The device.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        protected Parser(TransportBase device = null, string name = null) : this()
        {
            device?.AddParser(this);
            this.Name = string.IsNullOrEmpty(name) ? this.GetType().ToString().ToLower() : name.ToLower();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="Parser"/> class from being created.
        /// </summary>
        private Parser()
        {
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the send device.
        /// </summary>
        public WriteDelegate SendDevice
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the subsystem is a is write only.
        /// </summary>
        public bool IsWriteOnly
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether disposed.
        /// </summary>
        public virtual bool Disposed
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets The report.
        /// </summary>
        protected virtual EventHandler Report
        {
            get;
            set;
        }

        /// <summary>
        /// The received.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        public abstract void Received(byte[] data);

        /// <summary>
        /// The dispose.
        /// </summary>
        public virtual void Dispose()
        {
            var feature = this as IFeatureParser;
            if (feature != null)
            {
                feature.GetFeature = null;
                feature.SetFeature = null;
            }

            var report = this as IGetSetReportParser;

            if (report != null)
            {
                report.GetReport = null;
                report.SetReport = null;
            }

            this.SendDevice = null;
            this.Report = null;

            // remove all event Subscriptions
            foreach (var ev in this.GetType().GetProperties()
                .Where(x => !string.IsNullOrEmpty(x.PropertyType.FullName) && x.PropertyType.FullName.Contains("System.EventHandler")))
            {
                ev.SetValue(this, null);
            }

            this.Disposed = true;
        }
    }
}