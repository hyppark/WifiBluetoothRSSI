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
using Windows.Devices.WiFi;
using System.Diagnostics;
//using static WiFiBluetoothRSSI.WiFiScanner;

namespace WiFiBluetoothRSSI
{
    public sealed partial class MainPage : Page
    {
        private BluetoothLEScanner bleScanner;
        private WiFiScanner wifiScanner;

        public MainPage()
        {
            this.InitializeComponent();
            
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            wifiScanner = new WiFiScanner();
            bleScanner = new BluetoothLEScanner();
            
            // Must call at least once before scanning
            int wifiScannerSetupStatus = await wifiScanner.SetupWifiScanner();

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

        }
        /* 
         * =========================
         * WIFI SAMPLE CODE SECTION
         * =========================
         */
        // SAMPLE: List all WiFi networks
        private async void WifiScan_Button_Click(object sender, RoutedEventArgs e)
        {
            WiFiNetworkReport report = await wifiScanner.getWifiNetworkReport();
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
            double dRssi = await wifiScanner.GetWifiRssiGivenSsid(strSsid);

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

            double dRssi = await wifiScanner.GetWifiRssiGivenMac(strMac);

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
        
        // SAMPLE: find Bluetooth LE RSSI using MAC Address
        private async void SubmitBluetoothMac_Button_Click(object sender, RoutedEventArgs e)
        {
            string macAdr = SearchBluetoothBox.Text;
            //bleScanner = new BluetoothLEScanner();
            BluetoothResultsLog.Text += String.Format("Scanning... This operation may take up to 30 seconds.\n");

            double dRssi = await bleScanner.GetBleRssiGivenMac(macAdr);

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

        // cancel button that can be used to terminate GetBleRssiMac
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            bleScanner.BCancelled = true;
        }
    }
}
