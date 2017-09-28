namespace TransportFramework.Transports.Base.Native
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    #region enums

    /// <summary>
    /// The hid report type.
    /// </summary>
    public enum HidReportType
    {
        /// <summary>
        /// The input.
        /// </summary>
        Input,

        /// <summary>
        /// The output.
        /// </summary>
        Output,

        /// <summary>
        /// The feature.
        /// </summary>
        Feature,
    }

    /// <summary>
    /// The digital interfaces.
    /// </summary>
    [Flags]
    internal enum DigitalInterfaces
    {
        /// <summary>
        /// The present.
        /// </summary>
        Present = 0x02,

        /// <summary>
        /// The all classes.
        /// </summary>
        AllClasses = 0x04,

        /// <summary>
        /// The device interface.
        /// </summary>
        DeviceInterface = 0x10
    }

    #endregion

    #region Structures.
    /// <summary>
    /// The hid caps.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),]
    public struct HidCaps
    {
        public ushort Usage;
        public ushort UsagePage;
        public ushort InputReportByteLength;
        public ushort OutputReportByteLength;
        public ushort FeatureReportByteLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public ushort[] Reserved;

        public ushort NumberLinkCollectionNodes;
        public ushort NumberInputButtonCaps;
        public ushort NumberInputValueCaps;
        public ushort NumberInputDataIndices;
        public ushort NumberOutputButtonCaps;
        public ushort NumberOutputValueCaps;
        public ushort NumberOutputDataIndices;
        public ushort NumberFeatureButtonCaps;
        public ushort NumberFeatureValueCaps;
        public ushort NumberFeatureDataIndices;
    }

    /// <summary>
    /// The hid range.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),]
    public struct HidRange
    {
        public short UsageMin;
        public short UsageMax;
        public short StringMin;
        public short StringMax;
        public short DesignatorMin;
        public short DesignatorMax;
        public short DataIndexMin;
        public short DataIndexMax;
    }

    /// <summary>
    /// The hid not range.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),]
    public struct HidNotRange
    {
        public short Usage;
        public short Reserved1;
        public short StringIndex;
        public short Reserved2;
        public short DesignatorIndex;
        public short Reserved3;
        public short DataIndex;
        public short Reserved4;
    }

    /// <summary>
    /// The hid value caps.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),]
    public struct HidValueCaps
    {
        [FieldOffset(0)]
        public ushort UsagePage;
        [FieldOffset(2)]
        public byte ReportID;
        [FieldOffset(3), MarshalAs(UnmanagedType.U1)]
        public bool IsAlias;
        [FieldOffset(4)]
        public ushort BitField;
        [FieldOffset(6)]
        public ushort LinkCollection;
        [FieldOffset(8)]
        public ushort LinkUsage;
        [FieldOffset(10)]
        public ushort LinkUsagePage;
        [FieldOffset(12), MarshalAs(UnmanagedType.U1)]
        public bool IsRange;
        [FieldOffset(13), MarshalAs(UnmanagedType.U1)]
        public bool IsStringRange;
        [FieldOffset(14), MarshalAs(UnmanagedType.U1)]
        public bool IsDesignatorRange;
        [FieldOffset(15), MarshalAs(UnmanagedType.U1)]
        public bool IsAbsolute;
        [FieldOffset(16), MarshalAs(UnmanagedType.U1)]
        public bool HasNull;
        [FieldOffset(17)]
        public byte Reserved;
        [FieldOffset(18)]
        public short BitSize;
        [FieldOffset(20)]
        public short ReportCount;
        [FieldOffset(22)]
        public ushort Reserved2a;
        [FieldOffset(24)]
        public ushort Reserved2b;
        [FieldOffset(26)]
        public ushort Reserved2c;
        [FieldOffset(28)]
        public ushort Reserved2d;
        [FieldOffset(30)]
        public ushort Reserved2e;
        [FieldOffset(32)]
        public int UnitsExp;
        [FieldOffset(36)]
        public int Units;
        [FieldOffset(40)]
        public int LogicalMin;
        [FieldOffset(44)]
        public int LogicalMax;
        [FieldOffset(48)]
        public int PhysicalMin;
        [FieldOffset(52)]
        public int PhysicalMax;

        [FieldOffset(56)]
        public HidRange Range;
        [FieldOffset(56)]
        public HidNotRange NotRange;
    }

    [StructLayout(LayoutKind.Explicit)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here."),]
    public struct HidButtonCaps
    {
        [FieldOffset(0)]
        public short UsagePage;
        [FieldOffset(2)]
        public byte ReportID;
        [FieldOffset(3), MarshalAs(UnmanagedType.U1)]
        public bool IsAlias;
        [FieldOffset(4)]
        public short BitField;
        [FieldOffset(6)]
        public short LinkCollection;
        [FieldOffset(8)]
        public short LinkUsage;
        [FieldOffset(10)]
        public short LinkUsagePage;
        [FieldOffset(12), MarshalAs(UnmanagedType.U1)]
        public bool IsRange;
        [FieldOffset(13), MarshalAs(UnmanagedType.U1)]
        public bool IsStringRange;
        [FieldOffset(14), MarshalAs(UnmanagedType.U1)]
        public bool IsDesignatorRange;
        [FieldOffset(15), MarshalAs(UnmanagedType.U1)]
        public bool IsAbsolute;
        [FieldOffset(16), MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public int[] Reserved;

        [FieldOffset(56)]
        public HidRange Range;
        [FieldOffset(56)]
        public HidNotRange NotRange;
    }

    /// <summary>
    /// The hid attributes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HidAttributes
    {
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the vendor id.
        /// </summary>
        public ushort VendorId { get; set; }

        /// <summary>
        /// Gets or sets the product id.
        /// </summary>
        public ushort ProductId { get; set; }

        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        public ushort VersionNumber { get; set; }
    }

    #endregion

    /// <summary>
    /// The native methods.
    /// </summary>
    internal static partial class NativeMethods
    {
        #region Constants

        /// <summary>
        /// The status success.
        /// </summary>
        public const int StatusSuccess = (0x0 << 28) | (0x11 << 16) | 0;

        /// <summary>
        /// The overlapped.
        /// </summary>
        public const int Overlapped = 0x40000000;

        /// <summary>
        /// The generic read.
        /// </summary>
        public const int GenericRead = (int)-0x80000000;

        /// <summary>
        /// The generic write.
        /// </summary>
        public const int GenericWrite = 0x40000000;

        /// <summary>
        /// The file share read.
        /// </summary>
        public const int FileShareRead = 0x00000001;

        /// <summary>
        /// The file share write.
        /// </summary>
        public const int FileShareWrite = 0x00000002;

        /// <summary>
        /// The open existing.
        /// </summary>
        public const int OpenExisting = 3;

        /// <summary>
        /// Windows msg to indicate a change usb bus
        /// </summary>
        internal const int WmDeviceChange = 0x0219;

        /// <summary>
        /// WParam for above : A device was inserted
        /// </summary>
        internal const int DeviceArrival = 0x8000;

        /// <summary>
        /// WParam for above : A device was removed
        /// </summary>
        internal const int DeviceRemoveComplete = 0x8004;

        /// <summary>
        /// Used when registering for device insert/remove messages 
        /// </summary>
        internal const int DeviceNotifyWindowHandle = 0;

        /// <summary>
        /// Used when registering for device insert/remove messages : specifies the type of device
        /// </summary>
        internal const int DeviceInterface = 0x05;

        /// <summary>
        /// Used when registering for device insert/remove messages : we're giving the API call a window windowHandle
        /// </summary>
        internal const int DeviceNotifyAllInterfaceClasses = 4;

        #endregion

        #region APIs
        /// <summary>
        /// Get all Hid devices currently connected to the PC. 
        /// </summary>
        /// <returns>
        /// The Enumerable of Hid Devices.
        /// </returns>
        internal static IEnumerable<string> GetHidPaths()
        {
            Guid gHid;
            HidD_GetHidGuid(out gHid);

            var hInfoSet = SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, DigitalInterfaces.DeviceInterface | DigitalInterfaces.Present);

            var data = new SpDeviceInterfaceData
            {
                Size = Marshal.SizeOf(typeof(SpDeviceInterfaceData))
            };

            int index = 0;
            var paths = new List<string>();

            while (SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, index, out data))
            {
                paths.Add(GetDevicePaths(ref hInfoSet, ref data));
                index++;
            }

            SetupDiDestroyDeviceInfoList(hInfoSet);
            return paths;
        }

        /// <summary>
        /// Get all Hid devices currently connected to the PC. 
        /// </summary>
        /// <returns>
        /// The Enumerable of Hid Devices.
        /// </returns>
        internal static IEnumerable<HidInformation> GetHidObjects()
        {
            var response = new List<HidInformation>();

            // Iterate through all items found and connect to the first one that we 
            // Gain a valid windowHandle from.
            foreach (var hidItem in GetHidPaths())
            {
                bool isReadOnly = false;

                var deviceHandle = OpenDevice(hidItem, ref isReadOnly);

                // We don't have a Valid Device.
                if (deviceHandle.IsInvalid)
                {
                    deviceHandle.Dispose();
                    continue;
                }

                var deviceInformation = new HidInformation { DevicePath = hidItem, IsReadOnly = isReadOnly };

                IntPtr parsedData;

                if (!HidD_GetPreparsedData(deviceHandle, out parsedData))
                {
                    HidD_FreePreparsedData(ref parsedData);
                    continue;
                }

                try
                {
                    HidAttributes temp;
                    HidD_GetAttributes(deviceHandle, out temp);

                    deviceInformation.HidAttributes = temp;

                    HidCaps caps;
                    HidP_GetCaps(parsedData, out caps);

                    deviceInformation.Capabilities = caps;

                    ReadDeviceCapabilities(caps, parsedData, ref deviceInformation);

                    response.Add(deviceInformation);
                }
                catch
                {
                    Console.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()).Message);
                }
                finally
                {
                    HidD_FreePreparsedData(ref parsedData);
                    deviceHandle.Dispose();
                }
            }

            return response;
        }

        /// <summary>
        /// The open device.
        /// </summary>
        /// <param name="deviceName">
        /// The device name.
        /// </param>
        /// <param name="isReadOnly">
        /// The is Read Only.
        /// </param>
        /// <returns>
        /// The <see cref="SafeFileHandle"/>.
        /// </returns>
        internal static SafeFileHandle OpenDevice(string deviceName, ref bool isReadOnly)
        {
            var handle = CreateFile(
                deviceName,
                GenericRead | GenericWrite,
                FileShareWrite | FileShareRead,
                IntPtr.Zero,
                OpenExisting,
                Overlapped,
                IntPtr.Zero);

            if (handle.IsInvalid)
            {
                isReadOnly = true;
                handle = OpenDeviceRead(deviceName);
            }

            return handle;
        }

        /// <summary>
        /// The open device.
        /// </summary>
        /// <param name="deviceName">
        /// The device name.
        /// </param>
        /// <returns>
        /// The <see cref="SafeFileHandle"/>.
        /// </returns>
        private static SafeFileHandle OpenDeviceRead(string deviceName)
        {
            return CreateFile(
                deviceName,
                0,
                FileShareWrite | FileShareRead,
                IntPtr.Zero,
                OpenExisting,
                Overlapped,
                IntPtr.Zero);
        }

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool HidD_SetOutputReport(SafeFileHandle hFile, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] reportBuffer, int reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool HidD_GetInputReport(SafeFileHandle hFile, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] reportBuffer, int reportBufferLength);

        // Gets a handle to a buffer with information about the HID's reports
        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern bool HidD_GetPreparsedData(SafeFileHandle hFile, out IntPtr lpData);

        // Gets frees of open HID device
        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern bool HidD_FreePreparsedData(ref IntPtr pData);

        // Gets capabilities of open HID device
        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern int HidP_GetCaps(IntPtr lpData, out HidCaps oCaps);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern int HidP_GetValueCaps(HidReportType reportType, [In, Out] HidValueCaps[] valueCaps, ref ushort valueCapsLength, IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern int HidP_GetValueCaps(HidReportType reportType, ref IntPtr valueCaps, ref ushort valueCapsLength, IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern int HidP_GetButtonCaps(HidReportType reportType, [In, Out] HidButtonCaps[] buttonCaps, ref ushort buttonCapsLength, IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern bool HidD_GetAttributes(SafeFileHandle hFile, out HidAttributes attributes);

        // Get GUID  for HID device class
        [DllImport("hid.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern void HidD_GetHidGuid(out Guid guid);

        [DllImport("hid.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.I1)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern bool HidD_SetFeature(SafeFileHandle hFile, byte[] lpReportBuffer, int reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern bool HidD_GetFeature(SafeFileHandle hFile, byte[] reportBuffer, int reportBufferLength);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal static extern SafeFileHandle CreateFile(
            string fileName,
            int desiredAccess,
            int shareMode,
            IntPtr securityAttributes,
            int creationDisposition,
            int flagsAndAttributes,
            IntPtr templateFile);

        /// <summary>
        /// The get device paths.
        /// </summary>
        /// <param name="pointer">
        /// The pointer.
        /// </param>
        /// <param name="oInterface">
        /// The o interface.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        internal static string GetDevicePaths(ref IntPtr pointer, ref SpDeviceInterfaceData oInterface)
        {
            uint requiredSize = 0;
            SetupDiGetDeviceInterfaceDetail(pointer, ref oInterface, IntPtr.Zero, 0, ref requiredSize, IntPtr.Zero);

            var detail = new DeviceInterfaceDetailData
            {
                Size = Marshal.SizeOf(typeof(IntPtr)) == 8 ? 8 : 5
            };

            SetupDiGetDeviceInterfaceDetail(pointer, ref oInterface, ref detail, requiredSize, ref requiredSize, IntPtr.Zero);

            var response = detail.DevicePath.Clone().ToString();
            detail = default(DeviceInterfaceDetailData);

            return response;
        }

        // Get additional details of a connected device
        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr lpDeviceInfoSet, ref SpDeviceInterfaceData oInterfaceData, IntPtr lpDeviceInterfaceDetailData, uint nDeviceInterfaceDetailDataSize, ref uint nRequiredSize, IntPtr lpDeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr lpDeviceInfoSet, ref SpDeviceInterfaceData oInterfaceData, ref DeviceInterfaceDetailData oDetailData, uint nDeviceInterfaceDetailDataSize, ref uint nRequiredSize, IntPtr lpDeviceInfoData);

        [DllImport("SetupApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr hFile,
            ref SpDeviceInterfaceData deviceInterfaceData,
            [In, Out]
            byte[] deviceInterfaceDetailData,
            int deviceInterfaceDetailDataSize,
            out int requiredSize,
            IntPtr deviceInfoData);

        // Frees memory allocated in connected devices
        [DllImport("SetupApi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [DllImport("SetupApi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, int memberIndex, ref SpDeviceInterfaceData deviceInterfaceData);

        // Gets details of a connected device.
        [DllImport("SetupApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr deviceInfoSet,
            uint deviceInfoData,
            ref Guid interfaceClassGuid,
            int memberIndex,
            out SpDeviceInterfaceData deviceInterfaceData);

        // Get all connected devices by class
        [DllImport("SetupApi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, [MarshalAs(UnmanagedType.LPWStr)]string enumerator, IntPtr handle, DigitalInterfaces flags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr RegisterDeviceNotification(IntPtr pointer, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnregisterDeviceNotification(IntPtr hHandle);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool HidD_GetProductString(SafeFileHandle hDevice, byte[] buffer, int length);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool HidD_GetSerialNumberString(SafeFileHandle hDevice, byte[] buffer, int length);

        /// <summary>
        /// The read capabilities.
        /// </summary>
        /// <param name="caps">
        /// The caps.
        /// </param>
        /// <param name="pd">
        /// The pd.
        /// </param>
        /// <param name="hidInformation">
        /// The Device Information.
        /// </param>
        internal static void ReadDeviceCapabilities(HidCaps caps, IntPtr pd, ref HidInformation hidInformation)
        {
            if (caps.NumberOutputValueCaps > 0)
            {
                hidInformation.OutputValues.ValueCaps = new HidValueCaps[caps.NumberOutputValueCaps];
                HidP_GetValueCaps(
                    HidReportType.Output,
                    hidInformation.OutputValues.ValueCaps,
                    ref caps.NumberOutputValueCaps,
                    pd);
            }

            if (caps.NumberOutputButtonCaps > 0)
            {
                hidInformation.OutputValues.ValueButtons = new HidButtonCaps[caps.NumberOutputButtonCaps];
                HidP_GetButtonCaps(
                    HidReportType.Output,
                    hidInformation.OutputValues.ValueButtons,
                    ref caps.NumberOutputButtonCaps,
                    pd);
            }

            if (caps.NumberInputValueCaps > 0)
            {
                hidInformation.InputValues.ValueCaps = new HidValueCaps[caps.NumberInputValueCaps];
                HidP_GetValueCaps(
                    HidReportType.Input,
                    hidInformation.InputValues.ValueCaps,
                    ref caps.NumberInputValueCaps,
                    pd);
            }

            if (caps.NumberInputButtonCaps > 0)
            {
                hidInformation.InputValues.ValueButtons = new HidButtonCaps[caps.NumberInputButtonCaps];
                HidP_GetButtonCaps(
                    HidReportType.Input,
                    hidInformation.InputValues.ValueButtons,
                    ref caps.NumberInputButtonCaps,
                    pd);
            }

            if (caps.NumberFeatureValueCaps > 0)
            {
                hidInformation.FeatureValues.ValueCaps = new HidValueCaps[caps.NumberFeatureValueCaps];
                HidP_GetValueCaps(
                    HidReportType.Feature,
                    hidInformation.FeatureValues.ValueCaps,
                    ref caps.NumberFeatureValueCaps,
                    pd);
            }

            if (caps.NumberFeatureButtonCaps <= 0)
            {
                return;
            }

            hidInformation.FeatureValues.ValueButtons = new HidButtonCaps[caps.NumberFeatureButtonCaps];
            HidP_GetButtonCaps(
                HidReportType.Feature,
                hidInformation.FeatureValues.ValueButtons,
                ref caps.NumberFeatureButtonCaps,
                pd);
        }

        #endregion

        #region Structures

        /// <summary>
        /// The broadcast device interface.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct DevBroadcastDeviceInterface
        {
            /// <summary>
            /// The size.
            /// </summary>
            public int Size;

            /// <summary>
            /// The device type.
            /// </summary>
            public int DeviceType;

            /// <summary>
            /// The reserved.
            /// </summary>
            public int Reserved;

            /// <summary>
            /// The class guid.
            /// </summary>
            public Guid ClassGuid;

            /// <summary>
            /// The path.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string Path;
        }

        /// <summary>
        /// The sp device interface data.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal struct SpDeviceInterfaceData
        {
            internal int Size;
            internal Guid InterfaceClassGuid;
            internal int Flags;
            internal IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
        internal struct DeviceInterfaceDetailData
        {
            public int Size;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string DevicePath;
        }
        #endregion
    }
}