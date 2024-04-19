using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace TechPort.Helpers
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

            System.Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("BluetoothLE Watcher Start");
            System.Console.ForegroundColor = ConsoleColor.White;
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
                System.Console.WriteLine("Found : {0,-25} {1,5}", args.Name, args.Id);
            }

            if (args.Name.ToLower() == deviceName.ToLower())
            {
                deviceInformation = args;

                // Once device found Stop the watcher.
                System.Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("BluetoothLE Watcher stoped");
                System.Console.ForegroundColor = ConsoleColor.White;
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
            Console.WriteLine($"DeviceWatcher Enumeration completed", InformationType.Success);
            enumerationCompleted = true;

            // Once device found Stop the watcher.
            System.Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("BluetoothLE Watcher stoped");
            System.Console.ForegroundColor = ConsoleColor.White;
            deviceWatcher.Stop();
        }
        #endregion

        public async Task GetGattCharacteristicsFromBluetoothLEDeviceAsync()
        {
            /*
            GattDeviceServicesResult services = await bluetoothLeDevice.GetGattServicesAsync();

            Console.ConsoleWriteLine(services.Status, InformationType.Success);

            if (services.Status == GattCommunicationStatus.Success)
            {
                foreach (var service in services.Services)
                {
                    ConsoleWriteLine(service.Uuid);
                    ConsoleNewLine(0);

                    GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync(BluetoothCacheMode.Cached);

                    if (characteristicResult.Status == GattCommunicationStatus.Success)
                    {
                        var characteristics = characteristicResult.Characteristics;
                        for (int i = 0; i < characteristics.Count; i++)
                        {
                            var characteristic = characteristics[i];
                            G_characteristics.Add(characteristic);

                            GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

                            ConsoleWrite($"  * ", InformationType.AdditionalInformation);
                            ConsoleWrite($"{characteristic.Uuid}    ", InformationType.Information);

                            if (properties.HasFlag(GattCharacteristicProperties.Read))
                            {
                                ConsoleWrite("* Reading ");
                            }
                            else ConsoleWrite("*         ");

                            if (properties.HasFlag(GattCharacteristicProperties.Write))
                            {
                                // This characteristic supports writing to it.
                                ConsoleWrite(" Writing ");
                            }
                            else ConsoleWrite("         ");

                            if (properties.HasFlag(GattCharacteristicProperties.Notify))
                            {
                                //GattCommunicationStatus status = await
                                //    characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                                //if (status == GattCommunicationStatus.Success)
                                //    characteristic.ValueChanged += Characteristic_ValueChanged;

                                ConsoleWrite(" Notify *");
                            }
                            else ConsoleWrite("        *");

                            ConsoleNewLine();
                        }
                    }
                }
                ConsoleNewLine(3);
            }
            */
        }
    }
}