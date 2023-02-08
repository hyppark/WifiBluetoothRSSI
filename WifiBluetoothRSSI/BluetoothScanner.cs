using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using Windows.Devices.Enumeration;
namespace WiFiBluetoothRSSI
{
    /// <summary>
    /// This class was used to test the features of BluetoothLEAdvertisement
    /// Better method of finding all BLE devices could be through Device.Enumeration
    /// so this class will not be used anymore.
    /// </summary>
    class BluetoothScanner
    {
        private static BluetoothAdapter bluetoothAdapter;
        private static BluetoothLEAdvertisementWatcher watcher;

        public static async Task<int> SetupBluetoothScanner()
        {
            // NOTE:    if found to be Denied, add to Capabilities in Package.appxmanifest the following:
            //          <DeviceCapability Name="bluetooth"/>

            watcher = new BluetoothLEAdvertisementWatcher();
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(2000);

            // Attach handler 
            watcher.Received += OnAdvertisementReceived;
            // stopping due to various conditions
            // such as Bluetooth turning off or calling Stop method
            watcher.Stopped += OnAdvertisementWatcherStopped;
            



            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(BluetoothAdapter.GetDeviceSelector());
            if (devices.Count >= 1)
            {
                return 1;
            }
            else
            {
                return 0;
            }
            //bluetoothAdapter.
        }

        public static void Start()
        {
            watcher.Start();
        }
        public static void Stop()
        {
            watcher.Stop();
        }
        public static string getWatcherStatus()
        {
            return watcher.Status.ToString();

        }
        public static void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            watcher.Stop();
            // Unregister handlers to prevent memory leak
            watcher.Received -= OnAdvertisementReceived;
            watcher.Stopped -= OnAdvertisementWatcherStopped;
        }

        public static void App_Resuming(object sender, object e)
        {
            watcher.Received += OnAdvertisementReceived;
            watcher.Stopped += OnAdvertisementWatcherStopped;
        }

        private static async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            DateTimeOffset timestamp = eventArgs.Timestamp;

            // Type of advertisement
            BluetoothLEAdvertisementType advertisementType = eventArgs.AdvertisementType;

            // received signal strength indicator (RSSI)
            Int16 rssi = eventArgs.RawSignalStrengthInDBm;

            // name of advertising device. May be blank
            string localName = eventArgs.Advertisement.LocalName;

            // get first from manufacturer-specific sections
            string manufacturerDataString = "";
            var manufacturerSections = eventArgs.Advertisement.ManufacturerData;

            if (manufacturerSections.Count > 0)
            {
                var manufacturerData = manufacturerSections[0];
                var dataLength = new byte[manufacturerData.Data.Length];
                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                {
                    reader.ReadBytes(dataLength);
                }
                // get company ID + raw data in hex format
                manufacturerDataString = string.Format("0x{0}: {1}",
                    manufacturerData.CompanyId.ToString("X"),
                    BitConverter.ToString(dataLength));
            }

            // Print results

            //await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            //    BluetoothResultsLog.Text += string.Format("[{0}]: Adversitement type={1},\nRSSI={2},\nDevice name={3},\nmanufacturerData=[{4}]\n\n",
            //        timestamp.ToString("HH\\:mm\\:ss\\.fff"),
            //        advertisementType.ToString(),
            //        rssi.ToString(),
            //        localName,
            //        manufacturerDataString);
            //});

        }

        // Do when advertisement stopped
        private static async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            //await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            //    BluetoothResultsLog.Text += "Watcher stopped \n";
            //});
        }
    }
}
