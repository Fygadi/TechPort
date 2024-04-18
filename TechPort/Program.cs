using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TechPort.Helpers;
using TechPort.Settings;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace TechPort
{
    internal class Program
    {
        private static readonly List<GattCharacteristic> G_characteristics = new List<GattCharacteristic>();

        static double MIN_HEIGHT = 0.62; //Minimum desk height in meters
        static double MAX_HEIGHT = 1.27; //Maximum desk height in meters
        static int RETTYCOUNT = 3; //Number of retry on bluetooth connect

        //Custom service
        const string UUID_HEIGHT = "99fa0021-338a-1024-8a49-009c0215f78a";
        const string UUID_COMMAND = "99fa0002-338a-1024-8a49-009c0215f78a";
        const string UUID_REFERENCE_INPUT = "99fa0031-338a-1024-8a49-009c0215f78a";
        const string UUID_ADV_SVC = "99fa0001-338a-1024-8a49-009c0215f78a";
        const string UUID_DPG = "99fa0011-338a-1024-8a49-009c0215f78a";

        #region General services
        //General services
        const string UUID_DEVICE_NAME = "00002a00-0000-1000-8000-00805f9b34fb";
        const string UUID_APPEARANCE = "00002a01-0000-1000-8000-00805f9b34fb";
        const string UUID_PERIPHERAL_PREFERRED_CONNECTION_PARAMETERS = "00002a04-0000-1000-8000-0080D5f9b34fb";
        const string UUID_CENTRAL_ADDRESS_RESOLUTION = "00002aa6-0000-1000-8000-00805f9b34fb";
        const string UUID_SERVICE_CHANGED = "00002a05-0000-1000-8000-00805f9b34fb";
        #endregion

        //Command
        static readonly byte[] COMMAND_REFERENCE_INPUT_STOP = new byte[] { 0x01, 0x80 };
        static readonly byte[] COMMAND_UP = new byte[] { 0x47, 0x00 };
        static readonly byte[] COMMAND_DOWN = new byte[] { 0x46, 0x00 };
        static readonly byte[] COMMAND_STOP = new byte[] { 0xFF, 0x00 };
        static readonly byte[] COMMAND_WAKEUP = new byte[] { 0xFE, 0x00 };

        static DeviceWatcher deviceWatcher = null;
        static DeviceInformation device = null;

        static async Task Main(string[] args)
        {
            Console.Title = "TechPort";

            #region Settings
            string json = JsonConvert.SerializeObject(new DeskSettings(), Formatting.Indented);
            string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}deskSettings.json";
            ConsoleWriteLine($"Settings file: {filePath}");
            File.WriteAllText(filePath, json);
            #endregion

            #region HotKeyManager
            //Used comme from https://stackoverflow.com/questions/3654787/global-hotkey-in-console-application
            HotKeyManager.RegisterHotKey(Keys.Up, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.Down, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.Left, KeyModifiers.Alt);
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyManager_HotKeyPressed);
            #endregion

            #region bluetooth
            string aqsAllBluetoothLEDevices = BluetoothLEDevice.GetDeviceSelectorFromAppearance(
                BluetoothLEAppearance.FromParts(
                    BluetoothLEAppearanceCategories.Uncategorized,
                    BluetoothLEAppearanceSubcategories.Generic));

            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", };

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

            // Start the watcher.
            ConsoleWriteLine("BluetoothLE Watcher started", InformationType.Success);
            deviceWatcher.Start();

            //wait until the device is found
            while (device == null)
                Thread.Sleep(200);

            ConsoleWriteLine("Connecting...");
            BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(device.Id);
            GattDeviceServicesResult services = await bluetoothLeDevice.GetGattServicesAsync();

            ConsoleWriteLine(services.Status, InformationType.Success);

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
                bluetoothLeDevice.Dispose();
            }
            Console.WriteLine("\nPress any key to exit");
            Console.WriteLine("Waiting...");
            Console.ReadKey();
            Environment.Exit(0);
            #endregion
        }

        #region DeviceWatcher CallBack
        private static void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            //Console.ForegroundColor = ConsoleColor.Green;
            if (!string.IsNullOrEmpty(args.Name))
                Console.WriteLine("{0,-25} {1,5}", args.Name, args.Id);

            if (args.Name == "Bureau a Victor")
            {
                device = args;

                // Stop the watcher.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("BluetoothLE Watcher stoped");
                Console.ForegroundColor = ConsoleColor.White;
                deviceWatcher.Stop();
            }
        }

        private static void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine($"Removed: {args.Id}");
        }

        private static void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine($"\nDeviceWatcher Stopped\n");
        }

        private static void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            //Console.ForegroundColor = ConsoleColor.Blue;
            //Console.WriteLine($"Updated: {args.Id}");
        }

        private static void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine($"\nDeviceWatcher Enumeration completed\n");
        }
        #endregion

        #region Characteristic CallBack
        private static void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            switch (sender.Uuid.ToString())
            {
                case UUID_HEIGHT:
                    Console.BackgroundColor = ConsoleColor.Black;
                    DataReader reader = DataReader.FromBuffer(args.CharacteristicValue);
                    UInt32 data = (UInt32)IPAddress.NetworkToHostOrder((int)reader.ReadUInt32());
                    HeightOnChanged(data);
                    break;
                case UUID_COMMAND:
                    Console.WriteLine($"UUID_COMMAND {sender.Uuid}");
                    break;
                case UUID_REFERENCE_INPUT:
                    Console.WriteLine($"UUID_REFERENCE_INPUT {sender.Uuid}");
                    break;
                case UUID_ADV_SVC:
                    Console.WriteLine($"UUID_ADV_SVC {sender.Uuid}");
                    break;
                case UUID_DPG:
                    Console.WriteLine($"UUID_DPG {sender.Uuid}");
                    break;
            }
        }

        private static void HeightOnChanged(UInt32 data)
        {
            ConsoleWriteLine($"time: {DateTime.Now.ToString("ss.fff")}");

            short speed = (short)(data >> 16);
            UInt32 height = data & (UInt32)0xFFFF;
            Console.WriteLine($"data: 0x{data:X8}\theight: {height}   :   {height / 100 + (uint)(MIN_HEIGHT * 100)} cm\tspeed: {speed}");
        }
        #endregion

        #region Utilities
        static private void ConsoleWrite(object value, InformationType color = InformationType.DefaultColor)
        {
            Console.ForegroundColor = (ConsoleColor)color;
            Console.Write(value.ToString());
            Console.ResetColor();
        }
        static private void ConsoleWriteLine(object value, InformationType color = InformationType.DefaultColor)
        {
            Console.ForegroundColor = (ConsoleColor)color;
            Console.WriteLine(value.ToString());
            Console.ResetColor();
        }
        static private void ConsoleNewLine(int i = 1)
        {
            for (; i > 0; i--)
                Console.WriteLine();
        }
        private enum InformationType
        {
            Information = ConsoleColor.Cyan,
            AdditionalInformation = ConsoleColor.Gray,
            Warning = ConsoleColor.Yellow,
            Error = ConsoleColor.Red,
            Success = ConsoleColor.Green,
            DefaultColor = ConsoleColor.White
        }
        #endregion

        #region HotKeyManagerCallBack
        static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            if (e.Modifiers == KeyModifiers.Alt)
            {
                if (e.Key == Keys.Up)
                {
                    ConsoleWriteLine("UP!", InformationType.Success);
                    MoveDesk(COMMAND_UP);
                }
                else if (e.Key == Keys.Down)
                {
                    ConsoleWriteLine("DOWN!", InformationType.Success);
                    _ = MoveDown();
                }
                else if (e.Key == Keys.Left)
                {
                    ConsoleWriteLine("GetDeskHeight!", InformationType.Success);
                    Task a = GetDeskHeight();
                }
            }
        }
        #endregion

        #region Command
        static private async Task MoveUP()
        {
            MoveDesk(COMMAND_UP);
        }
        static private async Task MoveDown()
        {
            _ = MoveDesk(COMMAND_DOWN);
        }
        static private async Task MoveDesk(byte[] command)
        {
            var c = GetCommmandCharacterisic(UUID_COMMAND);
            ConsoleWriteLine("Characteristic found   UUID : " + c.Uuid.ToString());

            var writer = new DataWriter();
            // WriteByte used for simplicity. Other common functions - WriteInt16 and WriteSingle
            writer.WriteBytes(command);
            //Thread.Sleep(600);
            ConsoleWriteLine("STATUS GOOD", InformationType.Success);
            GattCommunicationStatus result = await c.WriteValueAsync(writer.DetachBuffer());
            if (result == GattCommunicationStatus.Success)
            {
                ConsoleWriteLine("Successfully wrote to device", InformationType.Success);
                // Successfully wrote to device
            }
            else
                ConsoleWriteLine("Failed to wrote to device", InformationType.Error);
        }

        static private async Task sendCommand(byte[] command)
        {
            GattCharacteristic c = G_characteristics.FirstOrDefault(characteristic => characteristic.Uuid.ToString() == UUID_COMMAND);
            if (c == null)
                new NotImplementedException("characterisitcs not found");

            var writer = new DataWriter();
            // WriteByte used for simplicity. Other common functions - WriteInt16 and WriteSingle
            writer.WriteBytes(command);
            GattCommunicationStatus result = await c.WriteValueAsync(writer.DetachBuffer());
            if (result == GattCommunicationStatus.Success)
            {
                ConsoleWriteLine("Successfully wrote to device", InformationType.Success);
                // Successfully wrote to device
            }
            else
                ConsoleWriteLine("Failed to wrote to device", InformationType.Error);
        }

        static private GattCharacteristic GetCommmandCharacterisic(string command)
        {
            GattCharacteristic c = G_characteristics.FirstOrDefault(characteristic => characteristic.Uuid.ToString() == command);
            if (c == null)
                throw new NotImplementedException("characterisitcs not found");

            return c;
        }

        static private async Task GetDeskHeight()
        {
            try
            {
                //var c = GetCommmandCharacterisic("00002aa6-0000-1000-8000-00805f9b34fb");
                var c = GetCommmandCharacterisic(UUID_HEIGHT);

                var result = await c.ReadValueAsync(BluetoothCacheMode.Uncached);
                ConsoleWriteLine($"result : {result}");
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var reader = DataReader.FromBuffer(result.Value);
                    byte[] input = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(input);
                    // Utilize the data as needed

                    ConsoleWriteLine(input.GetType(), InformationType.Success);


                    for (int i = 0; i < input.Length; i++)
                    {

                        Console.Write($"{input[i].ToString("X4")}");
                    }
                    ConsoleNewLine();
                }
                else
                    ConsoleWriteLine("ERROR", InformationType.Error);
            }
            catch (Exception e)
            {
                ConsoleWriteLine($"Exception : {e.Message}");
            }
        }
        #endregion
    }
}
