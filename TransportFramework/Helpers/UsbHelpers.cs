namespace TransportFramework.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Transports.Base.Native;

    /// <summary>
    /// The extensions.
    /// </summary>
    public class UsbHelpers
    {
        /// <summary>
        /// The filter types.
        /// </summary>
        public enum FilterTypes
        {
            /// <summary>
            /// Filter on product id.
            /// </summary>
            ProductId,

            /// <summary>
            /// Filter on vendor id.
            /// </summary>
            VendorId,

            /// <summary>
            /// Filter on device name.
            /// </summary>
            DeviceName
        }

        /// <summary>
        /// The Get Hid Devices Attached.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="filterType">
        /// The type of Filter
        /// </param>
        /// <returns>
        /// The Enumerable of Devices found.
        /// </returns>
        public static IList<UsbDeviceFound> GetHidAddresses(object filter, FilterTypes filterType)
        {
            var devices = NativeMethods.GetHidObjects();

            switch (filterType)
            {
                case FilterTypes.ProductId:
                    var pid = Convert.ToInt16(filter);
                    if (pid > 0)
                    {
                        return devices.Where(x => x.HidAttributes.ProductId == pid)
                            .Select(x => new UsbDeviceFound(x)).ToList();
                    }

                    break;

                case FilterTypes.VendorId:
                    var vid = Convert.ToInt16(filter);
                    if (vid > 0)
                    {
                        return devices.Where(x => x.HidAttributes.VendorId == vid)
                            .Select(x => new UsbDeviceFound(x)).ToList();
                    }

                    break;

                case FilterTypes.DeviceName:
                    return devices.Where(x => x.DevicePath.Contains(filter.ToString()))
                        .Select(x => new UsbDeviceFound(x)).ToList();
            }

            return new UsbDeviceFound[0];
        }

        /// <summary>
        /// The get WIN-USB Devices Attached.
        /// </summary>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <param name="filterType">
        /// The type of Filter
        /// </param>
        /// <returns>
        /// The <see cref="Enumerable"/>.
        /// </returns>
        public static IList<UsbDeviceFound> GetWinUsbAddresses(object filter, FilterTypes filterType)
        {
            var devices = NativeMethods.GetWinUsbObjects();

            switch (filterType)
            {
                case FilterTypes.ProductId:
                    var pid = Convert.ToUInt16(filter);
                    if (pid > 0)
                    {
                        return devices.Where(x => x.ProductId == pid)
                            .Select(x => new UsbDeviceFound(x)).ToList();
                    }

                    break;

                case FilterTypes.VendorId:
                    var vid = Convert.ToUInt16(filter);
                    if (vid > 0)
                    {
                        return devices.Where(x => x.VendorId == vid)
                            .Select(x => new UsbDeviceFound(x)).ToList();
                    }

                    break;

                case FilterTypes.DeviceName:
                    return devices.Where(x => x.DevicePath.ToLower().Contains(filter.ToString().ToLower()))
                        .Select(x => new UsbDeviceFound(x)).ToList();
            }

            return new UsbDeviceFound[0];
        }

        /// <summary>
        /// The hid devices.
        /// </summary>
        public struct UsbDeviceFound
        {
            /// <summary>
            /// The path.
            /// </summary>
            private string path;

            /// <summary>
            /// Initializes a new instance of the <see cref="UsbDeviceFound"/> structure. 
            /// </summary>
            /// <param name="information">
            /// The information about the device.
            /// </param>
            internal UsbDeviceFound(HidInformation information)
                : this()
            {
                this.path = string.Empty;
                this.Information = information;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="UsbDeviceFound"/> struct.
            /// </summary>
            /// <param name="devicePath">
            /// The device path.
            /// </param>
            internal UsbDeviceFound(string devicePath)
                : this()
            {
                this.path = devicePath;
                this.Information = null;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="UsbDeviceFound"/> struct.
            /// </summary>
            /// <param name="descriptor">
            /// The descriptor.
            /// </param>
            internal UsbDeviceFound(UsbDeviceDescriptor descriptor)
                : this()
            {
                this.path = descriptor.DevicePath;
                this.Information = new HidInformation
                {
                    HidAttributes = new HidAttributes
                    {
                        ProductId = descriptor.ProductId,
                        VendorId = descriptor.VendorId,
                    },
                    DevicePath = descriptor.DevicePath
                };
            }

            /// <summary>
            /// Gets the information.
            /// </summary>
            public HidInformation Information
            {
                get;
            }

            /// <summary>
            /// Gets or sets the device path.
            /// </summary>
            public string DevicePath
            {
                get => this.Information == null ? this.path : this.Information.DevicePath;

                set => this.path = value;
            }

            /// <summary>
            /// The to string.
            /// </summary>
            /// <returns>
            /// The <see cref="string"/>.
            /// </returns>
            public override string ToString()
            {
                return this.DevicePath;
            }
        }
    }
}
