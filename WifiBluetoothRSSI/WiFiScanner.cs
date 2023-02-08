﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFi;

namespace WiFiBluetoothRSSI
{
    class WiFiScanner
    {
        public static WiFiAdapter wifiAdapter;
        
        /// <summary>
        /// WiFi Scanner Setup must be called by UI/Main thread at least once
        /// </summary>
        public static async Task<int> SetupWifiScanner()
        {
            var access = await WiFiAdapter.RequestAccessAsync();

            // NOTE: if access found to be Denied, add to Capabilities in Package.appxmanifest the following line:
            // <DeviceCapability Name="wiFiControl"/> 
            if (access != WiFiAccessStatus.Allowed)
            {
                return -1;
            }
            else
            {
                var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                
                if (result.Count >= 1)
                {
                    wifiAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Run new scan to detect updated wifi networks, then return network report
        /// can access rssi, ssid, etc. by using: WiFiNetworkReport.AvailableNetwork[0].Ssid
        /// </summary>
        /// <returns>
        /// WiFiNetworkReport containing list of WiFiNetworkReport.AvailableNetworks[0~n]
        /// </returns>
        public static async Task<WiFiNetworkReport> getWifiNetworkReport()
        {
            await wifiAdapter.ScanAsync();
            return wifiAdapter.NetworkReport;
        }

        /// <summary>
        /// Given ssid, Scans available networks for match, returns signal strength in dBm
        /// </summary>
        /// <param name="ssid"></param>
        /// <returns>rssi in dBm if found, else double.NaN</returns>
        public static async Task<double> GetWifiRssiSsid(string ssid)
        {
            WiFiNetworkReport report = await getWifiNetworkReport();
            foreach (var network in report.AvailableNetworks)
            {
                if (network.Ssid.Trim().ToUpper() == ssid.Trim().ToUpper())
                {
                    return network.NetworkRssiInDecibelMilliwatts;
                }
            }
            return double.NaN;
        }

        /// <summary>
        /// Given MAC Address, Scans available networks for match, returns signal strength in dBm
        /// </summary>
        /// <param name="mac"></param>
        /// <returns> rssi in dBm if found, else double.NaN </returns>
        public static async Task<double> GetWifiRssiMac(string macAdr)
        {
            WiFiNetworkReport report = await getWifiNetworkReport();
            macAdr = macAdr.Replace("-", "").Replace(".", "").Replace(":", "").Replace(" ", "");
            foreach (var network in report.AvailableNetworks)
            {
                string bssid = network.Bssid.Replace("-", "").Replace(".", "").Replace(":", "").Replace(" ", "");
                if (bssid.Trim().ToUpper() == macAdr.Trim().ToUpper())
                {
                    return network.NetworkRssiInDecibelMilliwatts;
                }
            }
            return double.NaN;
        }


    }

    
}
