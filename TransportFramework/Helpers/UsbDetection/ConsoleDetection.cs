namespace TransportFramework.Helpers.UsbDetection
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    /// <summary>
    /// The change fix.
    /// </summary>
    public sealed class ConsoleDetection : NativeWindow
    {
        /// <summary>
        /// The quick edit mask.
        /// </summary>
        public const uint QuickEditMask = 0x40;

        /// <summary>
        /// The Console Detection instance.
        /// </summary>
        private static ConsoleDetection instance;

        /// <summary>
        /// Prevents a default instance of the <see cref="ConsoleDetection"/> class from being created.
        /// </summary>
        private ConsoleDetection()
        {
            if (instance != null)
            {
                return;
            }

            instance = this;

            // Prevent window getting visible
            this.CreateHandle(new CreateParams());
            DeviceNotification.RegisterHandle(this.Handle);
        }
        
        /// <summary>
        /// The USB changed event.
        /// </summary>
        public static event EventHandler<UsbChangeEventArgs> UsbChangedEvent
        {
            add => DeviceNotification.UsbChangedEvent += value;

            remove
            {
                DeviceNotification.UsbChangedEvent -= value;
                DeviceNotification.UsbChangedEvent = null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is valid.
        /// </summary>
        internal static bool IsValid => instance != null && instance.Handle != IntPtr.Zero;

        /// <summary>
        /// The start.
        /// </summary>
        public static void Start()
        {
            // force disposal just in case.
            Stop();

            Task.Run(() =>
                    {
                        instance = new ConsoleDetection();
                        Application.Run();
                    });
        }

        /// <summary>
        /// The Stop To disable USB Notification from a DLL or Console Application.
        /// </summary>
        public static void Stop()
        {
            if (instance != null)
            {
                DeviceNotification.UnsubscribeFromUsbEvents();
                instance.DestroyHandle();
                instance = null;
            }

            Application.Exit();
        }

        /// <summary>
        /// The toggle quick edit.
        /// </summary>
        /// <param name="enabled">
        /// If Enabled the mouse can tend to set focus on the Console window causing output to stop.
        /// If Disabled will turn off all quick edit features removing this focus issue.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool ToggleQuickEdit(bool enabled)
        {
            var handle = GetStdHandle(-10);
            uint mode = 0;

            if (!GetConsoleMode(handle, ref mode))
            {
                return false;
            }

            if (enabled)
            {
                mode |= QuickEditMask;
            }
            else
            {
                mode &= ~QuickEditMask;
            }

            return SetConsoleMode(handle, mode);
        }

        /// <summary>
        /// The window message processor.
        /// </summary>
        /// <param name="m">
        /// The message passed back.
        /// </param>
        protected override void WndProc(ref Message m)
        {
            DeviceNotification.ParseMessages(m.Msg, m.WParam, m.LParam);
            base.WndProc(ref m);
        }

        [DllImport("Kernel32", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr console, ref uint mode);

        [DllImport("Kernel32", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr console, uint mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int handle);
    }
}