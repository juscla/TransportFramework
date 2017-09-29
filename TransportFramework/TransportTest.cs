#if EXE
namespace TransportFramework
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using TransportFramework.Helpers;
    using TransportFramework.Parsers.Remote;
    using TransportFramework.Transports.Hid;
    using TransportFramework.Transports.Network;

    /// <summary>
    /// The trasport test.
    /// </summary>
    public class TransportTest
    {
        public static void Main()
        {
            const short VendorId = 0x45e;
            const byte TackpadUsage = 0x05;
            const byte MaxContactFeatureReportId = 0x05;
            const string HostAddress = "localhost";

            // find the Hid device path to the Precision Trackpad. 
            var trackpadPath = UsbHelpers.GetHidAddresses(VendorId, UsbHelpers.FilterTypes.VendorId)
                .FirstOrDefault(x => x.Information.Usage == TackpadUsage).DevicePath;

            if (string.IsNullOrEmpty(trackpadPath))
            {
                Console.WriteLine("Cannot find PTP Trackpad.");
                return;
            }

            // create our buffers to store the repsonses from the feature calls. 
            byte[] featureHid;
            byte[] featureNet = { 1, 2, 3 };

            using (var hid = new HidTransportBase(trackpadPath))
            {
                if (!hid.Connect())
                {
                    Console.WriteLine("Failed to connect to PTP device.");
                    return;
                }

                // get the feature report Over the hid transport. 
                featureHid = hid.GetFeature(MaxContactFeatureReportId);

                // initialize and start our remote parser.
                // this allows remote communications from other clients to 
                // our hid transport. 
                var remote = new Remote(hid);
                remote.Start();

                // initalize our network transport.
                using (var network = new NetworkTransport(HostAddress, Remote.PortNumber))
                {
                    // connect to our runnign remote parser.
                    if (network.Connect())
                    {
                        // get the feature report Over the network transport. 
                        featureNet = network.GetFeature(MaxContactFeatureReportId);
                    }
                }
            }

            // check that the two responses are equal. 
            Console.WriteLine("Test {0}....", featureNet.SequenceEqual(featureHid) ? "Passed" : "Failed");
            Console.ReadKey(true);
        }
    }
}
#endif