namespace TransportFramework.Helpers.UsbDetection
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    using Transports.Base.Native;

    /// <summary>
    /// The USB change state.
    /// </summary>
    public enum UsbStates
    {
        /// <summary>
        /// The arrived.
        /// </summary>
        Arrived = 0,

        /// <summary>
        /// The removed.
        /// </summary>
        Removed = 1
    }

    /// <summary>
    /// The device notification.
    /// </summary>
    public class DeviceNotification
    {
        /// <summary>
        /// The all USB class id.
        /// </summary>
        private const string AllUsbClassId = "88BAE032-5A81-49f0-BC3D-A4FF138216D6";

        /// <summary>
        /// The current state.
        /// </summary>
        private static readonly UsbChangeEventArgs CurrentState = new UsbChangeEventArgs();

        /// <summary>
        /// The windowHandle.
        /// </summary>
        private static IntPtr handle = IntPtr.Zero;

        /// <summary>
        /// Gets or sets the device change event.
        /// This event will be triggered when a device is plugged into the USB port on
        /// the computer. And it is completely enumerated by windows and ready for use.
        /// </summary>
        public static EventHandler<UsbChangeEventArgs> UsbChangedEvent
        {
            get;
            set;
        }

        /// <summary>
        /// The unsubscribe from USB events.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool UnsubscribeFromUsbEvents()
        {
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            var response = NativeMethods.UnregisterDeviceNotification(handle);

            handle = IntPtr.Zero;

            return response;
        }

        /// <summary>
        /// This method will filter the messages that are passed for USB device change messages only. 
        /// And parse them and take the appropriate action 
        /// </summary>
        /// <param name="msg">
        /// The Message.
        ///  </param>
        /// <param name="windowParam">
        /// The w parameter.
        /// </param>
        /// <param name="pointerParam">
        /// The pointer parameter.
        /// </param>
        public static void ParseMessages(int msg, IntPtr windowParam, IntPtr pointerParam)
        {
            // we got a device change message! A USB device was inserted or removed
            if (!msg.Equals(NativeMethods.WmDeviceChange) ||
                UsbChangedEvent == null ||
                windowParam == IntPtr.Zero ||
                pointerParam == IntPtr.Zero)
            {
                return;
            }

            var di =
                (NativeMethods.DevBroadcastDeviceInterface)
                Marshal.PtrToStructure(pointerParam, typeof(NativeMethods.DevBroadcastDeviceInterface));

            if (string.IsNullOrEmpty(di.Path))
            {
                return;
            }

            if (di.ClassGuid.Equals(NativeMethods.WinUsbGuid))
            {
                // WinUSB Device.
                // force the information to always be non null.
                CurrentState.Information =
                   UsbHelpers.GetWinUsbAddresses(di.Path, UsbHelpers.FilterTypes.DeviceName)
                       .FirstOrDefault()
                       .Information ?? new HidInformation();
            }
            else
            {
                // Non WinUSB device.
                // force the information to always be non null.
                CurrentState.Information =
                    UsbHelpers.GetHidAddresses(di.Path, UsbHelpers.FilterTypes.DeviceName).FirstOrDefault().Information
                    ?? new HidInformation();
            }

            CurrentState.Path = di.Path;

            // Check the window parameter to see if a device was inserted or removed
            switch (windowParam.ToInt64())
            {
                // inserted
                case NativeMethods.DeviceArrival:
                    CurrentState.State = UsbStates.Arrived;
                    break;

                // removed
                case NativeMethods.DeviceRemoveComplete:
                    CurrentState.State = UsbStates.Removed;

                    break;

                default:
                    return;
            }

            UsbChangedEvent.RaiseEvent(null, CurrentState);
        }

        /// <summary>
        /// The Handle register.
        /// </summary>
        /// <param name="windowHandle">
        /// The WPF window handle.
        /// </param>
        /// <param name="msg">
        /// The Message.
        /// </param>
        /// <param name="windowParam">
        /// The window parameter.
        /// </param>
        /// <param name="pointerParam">
        /// The pointer parameter.
        /// </param>
        /// <param name="handled">
        /// The handled.
        /// </param>
        /// <returns>
        /// The Handle.
        /// </returns>
        public static IntPtr RegisterHandle(IntPtr windowHandle, int msg, IntPtr windowParam, IntPtr pointerParam, ref bool handled)
        {
            if (pointerParam == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            ParseMessages(msg, windowParam, pointerParam);
            handled = false;

            return IntPtr.Zero;
        }

        /// <summary>
        /// The register for USB events.
        /// </summary>
        /// <param name="pointer">
        /// The pointer.
        /// </param>
        /// <param name="deviceClass">
        /// The device Class.
        /// </param>
        /// <exception cref="Exception">
        /// Cannot get a valid Handle.
        /// </exception>
        public static void RegisterHandle(IntPtr pointer, Guid? deviceClass = null)
        {
            var deviceInterface = new NativeMethods.DevBroadcastDeviceInterface
            {
                Size = Marshal.SizeOf(typeof(NativeMethods.DevBroadcastDeviceInterface)),
                DeviceType = NativeMethods.DeviceInterface,
                ClassGuid = deviceClass ?? new Guid(AllUsbClassId),
            };

            var buffer = Marshal.AllocHGlobal(deviceInterface.Size);

            Marshal.StructureToPtr(deviceInterface, buffer, true);

            handle = NativeMethods.RegisterDeviceNotification(
                pointer,
                buffer,
                NativeMethods.DeviceNotifyWindowHandle | NativeMethods.DeviceNotifyAllInterfaceClasses);

            Marshal.FreeHGlobal(buffer);

            if (handle == IntPtr.Zero)
            {
                throw new Exception("Error registering for Notification");
            }
        }
    }
}
