using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace TechPort.Bluetooth
{
    //reference https://learn.microsoft.com/en-us/windows/uwp/devices-sensors/gatt-client
    public class Bluetooth
    {
        static string aqsAllBluetoothLEDevices = BluetoothLEDevice.GetDeviceSelectorFromAppearance(
                BluetoothLEAppearance.FromParts(
                    BluetoothLEAppearanceCategories.Uncategorized,
                    BluetoothLEAppearanceSubcategories.Generic));
        static string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", };
        static DeviceWatcher deviceWatcher = null;

        DeviceInformation deviceInformation = null;
        string deviceName = "";

        bool enumerationCompleted = false;

        public async Task<BluetoothLEDevice> GetBluetoothLEDeviceAsync(string deviceName)
        {
            this.deviceName = deviceName;
            
            deviceWatcher =
                DeviceInformation.CreateWatcher(
                    aqsAllBluetoothLEDevices,
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // EnumerationCompleted and Stopped are optional to implement.
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            deviceWatcher.Start();

            while (deviceInformation == null || enumerationCompleted)
            {
                Thread.Sleep(100);
            }

            BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceInformation.Id);

            return bluetoothLeDevice;
        }

        #region DeviceWatcher CallBack
        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            if (!string.IsNullOrEmpty(args.Name))
            {
                //List all new device
                Console.WriteLine("{0,-25} {1,5}", args.Name, args.Id);
            }

            if (args.Name == deviceName)
            {
                deviceInformation = args;

                // Once device found Stop the watcher.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("BluetoothLE Watcher stoped");
                Console.ForegroundColor = ConsoleColor.White;
                deviceWatcher.Stop();
            }
        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine($"Removed: {args.Id}");
        }

        private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine($"\nDeviceWatcher Stopped\n");
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //Console.ForegroundColor = ConsoleColor.Blue;
            //Console.WriteLine($"Updated: {args.Id}");
        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            Console.WriteLine($"DeviceWatcher Enumeration completed", ConsoleColor.Green);
            enumerationCompleted = true;
        }
        #endregion
    }
}