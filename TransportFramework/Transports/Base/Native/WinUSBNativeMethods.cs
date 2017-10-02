namespace TransportFramework.Transports.Base.Native
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    /// <summary>
    /// The usb pipe type.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public enum PipeType
    {
        Control,
        Isochronous,
        Bulk,
        Interrupt,
    }

    /// <summary>
    /// The usb device descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public struct UsbDeviceDescriptor
    {
        public byte Length;
        public byte DescriptorType;
        public ushort BcdUSB;
        public byte DeviceClass;
        public byte DeviceSubClass;
        public byte DeviceProtocol;
        public byte MaxPacketSize0;
        public ushort VendorId;
        public ushort ProductId;
        public ushort BcdDevice;
        public byte Manufacturer;
        public byte Product;
        public byte SerialNumber;
        public byte NumConfigurations;

        public string DevicePath { get; set; }
    }

    /// <summary>
    /// The usb interface descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public struct UsbInterfaceDescriptor
    {
        public byte Length;
        public byte DescriptorType;
        public byte InterfaceNumber;
        public byte AlternateSetting;
        public byte NumEndpoints;
        public byte InterfaceClass;
        public byte InterfaceSubClass;
        public byte InterfaceProtocol;
        public byte Interface;
    }

    /// <summary>
    /// The win usb pipe information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public struct UsbPipeInformation
    {
        public PipeType Type;
        public byte Id;
        public ushort MaximumSize;
        public byte Interval;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public struct WinUsbSetupPacket
    {
        public byte RequestType;
        public byte Request;
        public ushort Value;
        public ushort Index;
        public ushort Length;
    }

    /// <summary>
    /// The native methods.
    /// </summary>
    internal static partial class NativeMethods
    {
        /// <summary>
        /// The normal.
        /// </summary>
        public const int Normal = 0x80;

        /// <summary>
        /// The WINUSB GUID.
        /// </summary>
        public static readonly Guid WinUsbGuid = new Guid("{a5dcbf10-6530-11d2-901f-00c04fb951ed}");

        /// <summary>
        /// Get all Hid devices currently connected to the PC. 
        /// </summary>
        /// <returns>
        /// The Enumerable of Hid Devices.
        /// </returns>
        public static IEnumerable<string> GetWinUsbPaths()
        {
            var guid = WinUsbGuid;

            var hInfoSet = SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, DigitalInterfaces.DeviceInterface | DigitalInterfaces.Present);

            var data = new SpDeviceInterfaceData
            {
                Size = Marshal.SizeOf(typeof(SpDeviceInterfaceData))
            };

            var index = 0;

            var paths = new List<string>();
            while (SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref guid, index, out data))
            {
                paths.Add(GetDevicePaths(ref hInfoSet, ref data));
                index++;
            }

            SetupDiDestroyDeviceInfoList(hInfoSet);
            return paths;
        }

        /// <summary>
        /// Get all WIN-USB devices currently connected to the PC. 
        /// </summary>
        /// <returns>
        /// The Enumerable of WinUsb Devices.
        /// </returns>
        internal static IEnumerable<UsbDeviceDescriptor> GetWinUsbObjects()
        {
            var paths = GetWinUsbPaths();

            var response = new List<UsbDeviceDescriptor>();

            // Iterate through all items found and connect to the first one that we 
            // Gain a valid windowHandle from.
            foreach (var path in paths)
            {
                bool isReadOnly = false;
                var deviceHandle = NativeMethods.OpenDevice(path, ref isReadOnly);

                // We don't have a Valid Device.
                if (deviceHandle.IsInvalid)
                {
                    deviceHandle.Dispose();
                    continue;
                }

                var usbHandle = IntPtr.Zero;
                if (WinUsb_Initialize(deviceHandle, ref usbHandle))
                {
                    var descriptor = GetDeviceDescriptor(usbHandle, path);
                    response.Add(descriptor);
                    WinUsb_Free(usbHandle);
                }

                deviceHandle.Dispose();
            }

            return response;
        }

        /// <summary>
        /// The get device descriptor.
        /// </summary>
        /// <param name="handle">
        /// The handle.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// The <see cref="UsbDeviceDescriptor"/>.
        /// </returns>
        internal static UsbDeviceDescriptor GetDeviceDescriptor(IntPtr handle, string path)
        {
            var deviceDesc = new UsbDeviceDescriptor
            {
                DevicePath = path
            };

            uint transfered;
            WinUsb_GetDescriptor(handle, 0x01, 0, 0, out deviceDesc, (uint)Marshal.SizeOf(deviceDesc), out transfered);
            return deviceDesc;
        }

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_Free(IntPtr interfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_Initialize(SafeFileHandle deviceHandle, ref IntPtr interfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_QueryInterfaceSettings(IntPtr interfaceHandle, byte alternateInterfaceNumber, out UsbInterfaceDescriptor usbAltInterfaceDescriptor);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_QueryPipe(IntPtr interfaceHandle, byte alternateInterfaceNumber, byte pipeIndex, out UsbPipeInformation pipeInformation);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_ReadPipe(IntPtr interfaceHandle, byte pipeId, byte[] buffer, int bufferLength, out int lengthTransferred, IntPtr overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_WritePipe(IntPtr interfaceHandle, byte pipeId, byte[] buffer, uint bufferLength, out uint lengthTransferred, IntPtr overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_GetAssociatedInterface(IntPtr interfaceHandle, byte associatedInterfaceIndex, out IntPtr associatedinterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_GetDescriptor(IntPtr interfaceHandle, byte descriptorType, byte index, ushort languageId, out UsbDeviceDescriptor deviceDesc, uint bufferLength, out uint lengthTransfered);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_GetPipePolicy(IntPtr interfaceHandle, uint pipeId, uint policyType, ref uint length, out uint result);

        [DllImport("winusb.dll", SetLastError = true)]
        internal static extern bool WinUsb_ControlTransfer(IntPtr interfaceHandle, WinUsbSetupPacket setupPacket, byte[] buffer, uint length, ref uint lengthTransferred, IntPtr overlapped);
    }
}
