using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// custom namespaces
using Windows.Storage.Streams;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
//using static WiFiBluetoothRSSI.WiFiScanner;

namespace WiFiBluetoothRSSI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        private BluetoothLEAdvertisementWatcher watcher;
        public MainPage()
        {
            this.InitializeComponent();
            
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            int wifiScannerSetupStatus = await WiFiScanner.SetupWifiScanner();
            
            // Optional status printing to verify status
            if (wifiScannerSetupStatus == 1)
            {
                WifiResultsLog.Text += "Adapter found with AdapterID: " + WiFiScanner.wifiAdapter.NetworkAdapter.NetworkAdapterId + "\n";
                WifiResultsLog.Text += "NetworkId: " + WiFiScanner.wifiAdapter.NetworkAdapter.NetworkItem.NetworkId + "\n";
            }
            else if (wifiScannerSetupStatus == 0)
            {
                WifiResultsLog.Text += "No adapters found...\n";
            }
            else if (wifiScannerSetupStatus == -1)
            {
                WifiResultsLog.Text += "Access to RequestAccessAsync is denied\n";
            }

            SetupBluetoothScanner();
        }

        /* 
         * =========================
         * WIFI SAMPLE CODE SECTION
         * =========================
         */
        // SAMPLE: List all WiFi networks
        private async void WifiScan_Button_Click(object sender, RoutedEventArgs e)
        {
            WiFiNetworkReport report = await WiFiScanner.getWifiNetworkReport();
            foreach (var network in report.AvailableNetworks)
            {
                WifiResultsLog.Text += "SSID: " + network.Ssid + " | MAC: " + network.Bssid + "\n";
                WifiResultsLog.Text += "RSSI: " + network.NetworkRssiInDecibelMilliwatts + " dBm\n\n";
            }
        }

        // SAMPLE: find rssi using WiFi ssid
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string strSsid = SearchNetworkBox.Text;
            double dRssi = WiFiScanner.GetWifiRssiSsid(strSsid);

            if (!dRssi.Equals(double.NaN))
            {
                WifiResultsLog.Text += "SSID: " + strSsid + " | RSSI: " + dRssi + " dBm\n";
            }
            else
            {
                WifiResultsLog.Text += "Specified SSID: " + strSsid + " was not detected\n";
            }

        }
        // SAMPLE: find rssi using WiFi mac address
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string strMac = SearchNetworkBox.Text;

            double dRssi = WiFiScanner.GetWifiRssiMac(strMac);

            if (!dRssi.Equals(double.NaN))
            {
                WifiResultsLog.Text += "MAC Address: " + strMac + " | RSSI: " + dRssi + " dBm\n";
            }
            else
            {
                WifiResultsLog.Text += "Specified MAC Address: " + strMac + " was not detected\n";
            }
        }

        /* 
        * =========================
        * BLUETOOTH SECTION (To Transfer to standalone class)
        * =========================
        */
        private void SetupBluetoothScanner()
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

            App.Current.Suspending += App_Suspending;
            App.Current.Resuming += App_Resuming;
        }
        
        private void App_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            watcher.Stop();
            // Unregister handlers to prevent memory leak
            watcher.Received -= OnAdvertisementReceived;
            watcher.Stopped -= OnAdvertisementWatcherStopped;
        }

        private void App_Resuming(object sender, object e)
        {
            watcher.Received += OnAdvertisementReceived;
            watcher.Stopped += OnAdvertisementWatcherStopped;
        }

        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
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

            if(manufacturerSections.Count > 0)
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
            
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                BluetoothResultsLog.Text += string.Format("[{0}]: Adversitement type={1},\nRSSI={2},\nDevice name={3},\nmanufacturerData=[{4}]\n\n",
                    timestamp.ToString("HH\\:mm\\:ss\\.fff"),
                    advertisementType.ToString(),
                    rssi.ToString(),
                    localName,
                    manufacturerDataString);
            });
            
        }

        // Do when advertisement stopped
        private async void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                BluetoothResultsLog.Text += "Watcher stopped \n";
            });
        }
        
        

        private void BluetoothScan_Button_Click(object sender, RoutedEventArgs e)
        {
            BluetoothResultsLog.Text += "Bluetooth Advertisement Watcher Starting...\n";
            watcher.Start();
            BluetoothResultsLog.Text += "Watcher status: " + watcher.Status + "\n";
            //open BT advertisement
        }

        private void SubmitBluetoothMac_Button_Click(object sender, RoutedEventArgs e)
        {
        }

        //private short getBluetoothRssiMac(string mac, )
        //{
        //    // start watcher
        //    // if mac match
        //    //      return rssi
        //}
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            watcher.Stop();
            BluetoothResultsLog.Text += "Bluetooth Advertisement Watcher Stopped...\n";
            BluetoothResultsLog.Text += "Watcher status: " + watcher.Status + "\n";
        }

        
    }
}
