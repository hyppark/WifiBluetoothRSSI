using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using System.Diagnostics;
//using System.Devices;
namespace WiFiBluetoothRSSI
{
    class BluetoothLEScanner
    {
        private DeviceWatcher deviceWatcher;
        private double dRssi;
        private bool bMacFound = false;
        private bool bTimedOut = false;
        public BluetoothLEScanner()
        {
            //InitializeComponent();
        }

        public async Task<double> GetBleRssiMac(string macAdr)
        {
            // AQS = Advanced Query Syntax
            string aqsFilterString= "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
            string[] requestedProperties = { "System.Devices.Aep.SignalStrength", "System.Devices.Aep.DeviceAddress" };

            deviceWatcher = DeviceInformation.CreateWatcher(
                aqsFilterString,
                requestedProperties,
                DeviceInformationKind.AssociationEndpoint
            );

            deviceWatcher.Added += (sender, e) => DeviceWatcher(sender, e, macAdr);
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            deviceWatcher.Start();
            while (await Task.Run(() => !bMacFound && !bTimedOut))
            {
                // wait until MAC is found OR Enumeration is Timed Out
                // TO DO: See if there is better way to implement
            }
            if (bMacFound)
            {
                return dRssi;
            }
            return double.NaN;
        }
        private void DeviceWatcher(DeviceWatcher sender, DeviceInformation deviceInfo, string macAdr)
        {
            string currentDeviceMacAdr = deviceInfo.Properties["System.Devices.Aep.DeviceAddress"].ToString();
            if (currentDeviceMacAdr == macAdr)
            {
                dRssi = Convert.ToDouble(deviceInfo.Properties["System.Devices.Aep.SignalStrength"]);
                bMacFound = true;
                Debug.WriteLine(String.Format("MAC match found for {0} | RSSI: {1}", macAdr, dRssi));
                // stop watcher
                deviceWatcher.Stop();

            }
        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // timed out. could not find
            Debug.WriteLine("Enumeration Completed for Find MAC");
            if (!bMacFound)
            {
                Debug.WriteLine("Search reached time limit of 30 seconds. Could not find MAC Address");
                deviceWatcher.Stop();
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
        
        public void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            Debug.WriteLine("Stopped FindMAC DeviceWatcher");
        }
        /*

        public void StartWatcher()
        {
            BluetoothLEDevice.GetDeviceSelectorFromPairingState(false);
            BluetoothLEDevice.GetDeviceSelectorFromPairingState(true);
            //BluetoothDevice.GetDeviceSelectorFromPairingState(false);
            //BluetoothDevice.GetDeviceSelectorFromPairingState(true);

            string bluetoothSelector = "System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\""; // bluetoothSelector
            string bluetoothLESelector = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\""; // bluetoothLESelector
            
            string aqsALLBluetoothLEDevices = "";
            //string aqsALLBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
            string[] requestedProperties = { "System.Devices.Aep.SignalStrength", "System.Devices.Aep.DeviceAddress" };
            
            // For DEBUGGING
            int caseNumber = 0;
            switch (caseNumber)
            {
                // Known and working scenario
                // use protocol ID for AQS string
                case 0:
                {
                    aqsALLBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

                    deviceWatcher = DeviceInformation.CreateWatcher(
                        aqsALLBluetoothLEDevices, // Device Type
                        requestedProperties, //request for additional properties
                        DeviceInformationKind.AssociationEndpoint // 
                    );
                    break;
                }

                    // for BLE Device that is of pairingstate = false
                case 1:
                {
                    deviceWatcher = DeviceInformation.CreateWatcher(
                        BluetoothDevice.GetDeviceSelectorFromPairingState(false),
                        requestedProperties, //request for additional properties
                        DeviceInformationKind.AssociationEndpoint // 
                    );
                    break;
                }
                case 2:
                {
                    deviceWatcher = DeviceInformation.CreateWatcher(
                        aqsALLBluetoothLEDevices, // Device Type
                        requestedProperties, //request for additional properties
                        DeviceInformationKind.AssociationEndpoint // 
                    );
                        break;
                }
            }


            deviceWatcher = DeviceInformation.CreateWatcher(
            //BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                aqsALLBluetoothLEDevices, // Device Type
                requestedProperties, //request for additional properties
                DeviceInformationKind.AssociationEndpoint // 
            );

            deviceWatcher.Added += DeviceWatcher_Added;

            // deviceWatcher.Updated required for real time detection
            // if deviceWatcher.Updated is excluded, event watcher will only return events once enumeration is completed
            deviceWatcher.Updated += DeviceWatcher_Updated;

            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            Debug.WriteLine("debug watcher started");
            deviceWatcher.Start();

            GetBleRssiMac("74:69:96:ba:d0:c9");
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            //await Task.Run(() =>
            //{
                Debug.WriteLine(String.Format("Added Id: {0} | Name: {1} | RSSI: {2} | Address: {3}", deviceInfo.Id, deviceInfo.Name, deviceInfo.Properties["System.Devices.Aep.SignalStrength"], deviceInfo.Properties["System.Devices.Aep.DeviceAddress"]));
            //});

            //Debug.WriteLine(String.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));
        }
        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {

        }
        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {

        }
        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            Debug.WriteLine("Enumeration completed");
        }
        private void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {

        }
        */
    }
}
