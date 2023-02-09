using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using System.Diagnostics;

namespace WiFiBluetoothRSSI
{
    class BluetoothLEScanner
    {
        private DeviceWatcher deviceWatcher;
        private double dRssi = double.NaN;
        private string strFindMacAdr = "";
        private bool bMacFound = false;
        private bool bTimedOut = false;
        private bool bCancelled = false;
        public BluetoothLEScanner()
        {

        }
        /// <summary>
        /// Call from UI (e.g. cancel button) to terminate default 30 second enumeration time
        /// </summary>
        public bool BCancelled
        {
            get { return bCancelled; }
            set { bCancelled = value; }
        }

        /// <summary>
        /// Input Bluetooth Low Energy MAC Address to find for match in discovered BLE devices
        /// Maximum search time is 30 seconds. returns double.NaN on timeout
        /// </summary>
        /// <param name="macAdr"></param>
        /// <returns>double RSSI, if MAC cannot be detected, reuturn double.NaN</returns>
        public async Task<double> GetBleRssiGivenMac(string macAdr)
        {
            strFindMacAdr = FormatMacAddress(macAdr);
            // AQS = Advanced Query Syntax
            string aqsFilterString= "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
            string[] requestedProperties = { "System.Devices.Aep.SignalStrength", "System.Devices.Aep.DeviceAddress" };

            deviceWatcher = DeviceInformation.CreateWatcher(
                aqsFilterString,
                requestedProperties,
                DeviceInformationKind.AssociationEndpoint
            );

            // subscribe to events
            deviceWatcher.Added += DeviceWatcher;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            deviceWatcher.Start();

            while (await Task.Run(() => !bMacFound && !bTimedOut && !bCancelled))
            {
                // wait until MAC is found OR Enumeration is Timed Out OR user manually cancels operation
                // TO DO: See if there is better way to implement this without "while (true)"

                // NOTE: Delay needed here.
                // If no delay, CPU usage goes to 20% (Tested on 11th gen i7 mobile)
                await Task.Delay(1);
            }
            if (deviceWatcher.Status != DeviceWatcherStatus.Stopped)
            {
                deviceWatcher.Stop();
            }
            if (bCancelled)
            {
                Debug.WriteLine("Operation cancelled");
            }
            if (bMacFound)
            {
                return dRssi;
            }
            return double.NaN;
        }

        private static string FormatMacAddress(string macAdr)
        {
            macAdr = macAdr.Replace("-", "").Replace(".", "").Replace(":", "").Replace(" ", "").Trim().ToUpper();
            return macAdr;
        }

        private void DeviceWatcher(DeviceWatcher sender, DeviceInformation deviceInfo)
        {

            Debug.WriteLine("Added device: " + deviceInfo.Id);
            string currentDeviceMacAdr = FormatMacAddress(deviceInfo.Properties["System.Devices.Aep.DeviceAddress"].ToString());

            if (currentDeviceMacAdr == strFindMacAdr)
            {
                dRssi = Convert.ToDouble(deviceInfo.Properties["System.Devices.Aep.SignalStrength"]);
                bMacFound = true;
                Debug.WriteLine(String.Format("MAC match found for {0} | RSSI: {1}", strFindMacAdr, dRssi));

            }
        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // timed out. could not find
            Debug.WriteLine("Enumeration Completed for Find MAC");

            if (!bMacFound)
            {
                Debug.WriteLine("Search reached time limit of 30 seconds. Could not find MAC Address");
                bTimedOut = true;
            }
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            Debug.WriteLine("Updated device: " + deviceInfoUpdate.Id);
        }
        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            Debug.WriteLine("Removed device: " + deviceInfoUpdate.Id);
        }
        
        private void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // reset variables so that 1 instance can be reused for many requests. Can also work with 1 new instance per request
            dRssi = double.NaN;
            strFindMacAdr = "";
            bMacFound = false;
            bTimedOut = false;
            bCancelled = false;
            // unsubscribe to events 
            deviceWatcher.Added -= DeviceWatcher;
            deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Updated -= DeviceWatcher_Updated;
            deviceWatcher.Removed -= DeviceWatcher_Removed;
            deviceWatcher.Stopped -= DeviceWatcher_Stopped;
            Debug.WriteLine("Stopped FindMAC DeviceWatcher");
        }
        
    }
}
