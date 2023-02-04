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
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WifiBluetoothRSSI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private WiFiAdapter wifiAdapter;
        public MainPage()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // NOTE:    RequestAccessAsync MUST be called by UI thread at least once
            var access = await WiFiAdapter.RequestAccessAsync();

            if (access != WiFiAccessStatus.Allowed)
            {
                // NOTE:    if found to be Denied, add to Capabilities in Package.appxmanifest the following:
                //          <DeviceCapability Name="wiFiControl"/> 
                ResultsLog.Text += "Access to RequestAccessAsync is denied\n";
            }
            else
            {
                var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());

                if (result.Count >= 1)
                {
                    wifiAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);
                    ResultsLog.Text += "Adapter found with AdapterID: " + wifiAdapter.NetworkAdapter.NetworkAdapterId + "\n";
                    ResultsLog.Text += "NetworkId: " + wifiAdapter.NetworkAdapter.NetworkItem.NetworkId + "\n";
                }
                else
                {
                    ResultsLog.Text += "No adapters found...";
                }
            }
        }
        // List all networks
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await wifiAdapter.ScanAsync();
            DisplayNetworkReport(wifiAdapter.NetworkReport);
        }

        private async void DisplayNetworkReport(WiFiNetworkReport report)
        {
            foreach (var network in report.AvailableNetworks)
            {

                ResultsLog.Text += "SSID: " + network.Ssid + " | MAC: " + network.Bssid + "\n";
                ResultsLog.Text += "RSSI: " + network.NetworkRssiInDecibelMilliwatts + " dBm\n\n";
            }
        }

        // find rssi using ssid
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string strSsid = SearchNetworkBox.Text;
            await wifiAdapter.ScanAsync();
            double dRssi = GetWifiRssiSsid(strSsid, wifiAdapter.NetworkReport);

            if (!dRssi.Equals(double.NaN))
            {
                ResultsLog.Text += "SSID: " + strSsid + " | RSSI: " + dRssi + " dBm\n";
            }
            else
            {
                ResultsLog.Text += "Specified SSID: " + strSsid + " was not detected\n";
            }

        }
        // find rssi using mac address
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string strMac = SearchNetworkBox.Text;

            await wifiAdapter.ScanAsync();
            double dRssi = GetWifiRssiMac(strMac, wifiAdapter.NetworkReport);

            if (!dRssi.Equals(double.NaN))
            {
                ResultsLog.Text += "MAC Address: " + strMac + " | RSSI: " + dRssi + " dBm\n";
            }
            else
            {
                ResultsLog.Text += "Specified MAC Address: " + strMac + " was not detected\n";
            }
        }

        public double GetWifiRssiSsid(string ssid, WiFiNetworkReport report)
        {
            foreach (var network in report.AvailableNetworks)
            {
                if(network.Ssid.Trim().ToUpper() == ssid.Trim().ToUpper())
                {
                    return network.NetworkRssiInDecibelMilliwatts;
                }
            }
            return double.NaN;
        }

        public double GetWifiRssiMac(string mac, WiFiNetworkReport report)
        {
            mac = mac.Replace("-", "").Replace(".", "").Replace(":", "").Replace(" ", "");
            foreach (var network in report.AvailableNetworks)
            {
                string bssid = network.Bssid.Replace("-", "").Replace(".", "").Replace(":", "").Replace(" ", "");
                if (bssid.Trim().ToUpper() == mac.Trim().ToUpper())
                {
                    return network.NetworkRssiInDecibelMilliwatts;
                }
            }
            return double.NaN;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
