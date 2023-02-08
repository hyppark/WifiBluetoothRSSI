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
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Enumeration;
//using static WiFiBluetoothRSSI.WiFiScanner;

namespace WiFiBluetoothRSSI
{
    public sealed partial class MainPage : Page
    {
        
        
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

            

            //int bluetoothScannerSetupStatus = await BluetoothScanner.SetupBluetoothScanner();
            //BluetoothResultsLog.Text += bluetoothScannerSetupStatus;

            //App.Current.Suspending += BluetoothScanner.App_Suspending;
            //App.Current.Resuming += BluetoothScanner.App_Resuming;

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
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string strSsid = SearchNetworkBox.Text;
            double dRssi = await WiFiScanner.GetWifiRssiSsid(strSsid);

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
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string strMac = SearchNetworkBox.Text;

            double dRssi = await WiFiScanner.GetWifiRssiMac(strMac);

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
        * BLUETOOTH SAMPLE CODE 
        * =========================
        */
        
        /// <summary>
        /// Depreciated. Use Class BluetoothLEScanner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BluetoothScan_Button_Click(object sender, RoutedEventArgs e)
        {
            // NOT IMPLEMENTED ANYMORE
            int bluetoothScannerSetupStatus = await BluetoothScanner.SetupBluetoothScanner();
            BluetoothResultsLog.Text += bluetoothScannerSetupStatus;

            App.Current.Suspending += BluetoothScanner.App_Suspending;
            App.Current.Resuming += BluetoothScanner.App_Resuming;

            BluetoothResultsLog.Text += "Bluetooth Advertisement Watcher Starting...\n";
            BluetoothScanner.Start();

            BluetoothResultsLog.Text += "Watcher status: " + BluetoothScanner.getWatcherStatus() + "\n";
             //open BT advertisement
        }

        private async void SubmitBluetoothMac_Button_Click(object sender, RoutedEventArgs e)
        {
            BluetoothLEScanner bleScanner = new BluetoothLEScanner();
            string macAdr = SearchBluetoothBox.Text;
            double dRssi = await bleScanner.GetBleRssiMac(macAdr);
            
            if (!Double.IsNaN(dRssi))
            {
                Debug.WriteLine(String.Format("MAC: {0} | RSSI: {1}\n", macAdr, dRssi));
                BluetoothResultsLog.Text += String.Format("MAC: {0} | RSSI: {1}\n", macAdr, dRssi);
            }
            else
            {
                Debug.WriteLine(String.Format("MAC: {0} | RSSI: {1}\n", macAdr, dRssi));
                BluetoothResultsLog.Text += String.Format("MAC: {0} not Detected!\n", macAdr);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BluetoothScanner.Stop();
            BluetoothResultsLog.Text += "Bluetooth Advertisement Watcher Stopped...\n";
            BluetoothResultsLog.Text += "Watcher status: " + BluetoothScanner.getWatcherStatus() + "\n";
        }

        
    }
}
