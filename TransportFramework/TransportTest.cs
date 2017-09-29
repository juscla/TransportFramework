#if EXE
namespace TransportFramework
{
    using System.Linq;

    using TransportFramework.Helpers;
    using TransportFramework.Parsers.Remote;
    using TransportFramework.Transports.Hid;

    /// <summary>
    /// The trasport test.
    /// </summary>
    public class TransportTest
    {
        public static void Main()
        {
            var trackpadPath = UsbHelpers.GetHidAddresses(0x45e, UsbHelpers.FilterTypes.VendorId)
                .FirstOrDefault(x => x.Information.Usage == 0x5).DevicePath;

            byte[] featureHid = new byte[2];

            using (var hidDevice = new HidTransportBase(trackpadPath))
            {
                if (hidDevice.Connect())
                {
                    var remove = new Remote(hidDevice);
                    
                }
            }
        }
    }
}
#endif