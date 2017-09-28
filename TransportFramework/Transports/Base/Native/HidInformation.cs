namespace TransportFramework.Transports.Base.Native
{
    /// <summary>
    /// The hid information.
    /// </summary>
    public class HidInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HidInformation"/> class.
        /// </summary>
        public HidInformation()
        {
            this.OutputValues = new HidDeviceValues();
            this.InputValues = new HidDeviceValues();
            this.FeatureValues = new HidDeviceValues();
            this.HidAttributes = new HidAttributes();
        }

        /// <summary>
        /// Gets the vendor id.
        /// </summary>
        public ushort VendorId => this.HidAttributes.VendorId;

        /// <summary>
        /// Gets the product id.
        /// </summary>
        public ushort ProductId => this.HidAttributes.ProductId;

        /// <summary>
        /// Gets or sets the report id.
        /// </summary>
        public byte ReportId { get; set; }

        /// <summary>
        /// Gets the input report length.
        /// </summary>
        public int InputReportLength => this.Capabilities.InputReportByteLength;

        /// <summary>
        /// Gets the output report length.
        /// </summary>
        public int OutputReportLength => this.Capabilities.OutputReportByteLength;

        /// <summary>
        /// Gets the usage.
        /// </summary>
        public ushort Usage => this.Capabilities.Usage;

        /// <summary>
        /// Gets the usage page.
        /// </summary>
        public ushort UsagePage => this.Capabilities.UsagePage;

        /// <summary>
        /// Gets or sets the Output values.
        /// </summary>
        public HidDeviceValues OutputValues
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the Input values.
        /// </summary>
        public HidDeviceValues InputValues
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the Feature values.
        /// </summary>
        public HidDeviceValues FeatureValues
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the hid attributes.
        /// </summary>
        public HidAttributes HidAttributes
        {
            get;
            protected internal set;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the device is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Gets or sets the device path.
        /// </summary>
        public string DevicePath
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// Gets or sets the capabilities.
        /// </summary>
        public HidCaps Capabilities
        {
            get;
            protected internal set;
        }

        /// <summary>
        /// The hid device values.
        /// </summary>
        public class HidDeviceValues
        {
            /// <summary>
            /// Gets the value caps.
            /// </summary>
            public HidValueCaps[] ValueCaps
            {
                get;
                internal set;
            }

            /// <summary>
            /// Gets the value buttons.
            /// </summary>
            public HidButtonCaps[] ValueButtons
            {
                get;
                internal set;
            }
        }
    }
}