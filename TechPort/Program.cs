using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using TechPort.Helpers;
using TechPort.Settings;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Console = TechPort.Helpers.Console;

namespace TechPort
{
    internal class Program
    {
        private static readonly List<GattCharacteristic> G_characteristics = new List<GattCharacteristic>();

        const double MIN_HEIGHT = 0.62; //Minimum desk height in meters
        const double MAX_HEIGHT = 1.27; //Maximum desk height in meters
        const int RETRYCOUNT = 3; //Number of retry on bluetooth connect

        const string UUID_HEIGHT = "99fa0021-338a-1024-8a49-009c0215f78a";
        const string UUID_COMMAND = "99fa0002-338a-1024-8a49-009c0215f78a";
        const string UUID_REFERENCE_INPUT = "99fa0031-338a-1024-8a49-009c0215f78a";
        const string UUID_ADV_SVC = "99fa0001-338a-1024-8a49-009c0215f78a";
        const string UUID_DPG = "99fa0011-338a-1024-8a49-009c0215f78a";

        static readonly byte[] COMMAND_REFERENCE_INPUT_STOP = new byte[] { 0x01, 0x80 };
        static readonly byte[] COMMAND_UP = new byte[] { 0x47, 0x00 };
        static readonly byte[] COMMAND_DOWN = new byte[] { 0x46, 0x00 };
        static readonly byte[] COMMAND_STOP = new byte[] { 0xFF, 0x00 };
        static readonly byte[] COMMAND_WAKEUP = new byte[] { 0xFE, 0x00 };

        static async Task Main(string[] args)
        {
            System.Console.Title = "TechPort";

            #region Settings
            string json = JsonConvert.SerializeObject(new DeskSettings(), Formatting.Indented);
            string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}deskSettings.json";
            Console.WriteLine($"Settings file: {filePath}");
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
           
            Bluetooth bluetooth = new Bluetooth();
            var bluetoothLeDevice = await bluetooth.GetBluetoothLEDeviceAsync("Bureau a Victor");

            GattDeviceServicesResult services = await bluetoothLeDevice.GetGattServicesAsync();

            Console.WriteLine(services.Status, InformationType.Success);

            if (services.Status == GattCommunicationStatus.Success)
            {
                foreach (var service in services.Services)
                {
                    Console.WriteLine(service.Uuid);
                    Console.NewLine(0);

                    GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync(BluetoothCacheMode.Cached);

                    if (characteristicResult.Status == GattCommunicationStatus.Success)
                    {
                        var characteristics = characteristicResult.Characteristics;
                        for (int i = 0; i < characteristics.Count; i++)
                        {
                            var characteristic = characteristics[i];
                            G_characteristics.Add(characteristic);

                            GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

                            Console.Write($"  * ", InformationType.AdditionalInformation);
                            Console.Write($"{characteristic.Uuid}    ", InformationType.Information);

                            if (properties.HasFlag(GattCharacteristicProperties.Read))
                            {
                                Console.Write("* Reading ");
                            }
                            else Console.Write("*         ");

                            if (properties.HasFlag(GattCharacteristicProperties.Write))
                            {
                                // This characteristic supports writing to it.
                                Console.Write(" Writing ");
                            }
                            else Console.Write("         ");

                            if (properties.HasFlag(GattCharacteristicProperties.Notify))
                            {
                                GattCommunicationStatus status = await
                                    characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                                if (status == GattCommunicationStatus.Success)
                                    characteristic.ValueChanged += Characteristic_ValueChanged;

                                Console.Write(" Notify *");
                            }
                            else Console.Write("        *");

                            Console.NewLine();
                        }
                    }
                }
                Console.NewLine(3);
            }
            bluetoothLeDevice.Dispose();
            Console.WriteLine("\nPress any key to exit");
            Console.WriteLine("Waiting...");
            System.Console.ReadKey();
            Environment.Exit(0);
            #endregion
        }

        #region Characteristic CallBack
        private static void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            System.Console.BackgroundColor = ConsoleColor.Red;
            switch (sender.Uuid.ToString())
            {
                case UUID_HEIGHT:
                    System.Console.BackgroundColor = ConsoleColor.Black;
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
            Console.WriteLine($"time: {DateTime.Now.ToString("ss.fff")}");

            short speed = (short)(data >> 16);
            UInt32 height = data & (UInt32)0xFFFF;
            Console.WriteLine($"data: 0x{data:X8}\theight: {height}   :   {height / 100 + (uint)(MIN_HEIGHT * 100)} cm\tspeed: {speed}");
        }
        #endregion

        #region HotKeyManagerCallBack
        static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            if (e.Modifiers == KeyModifiers.Alt)
            {
                if (e.Key == Keys.Up)
                {
                    Console.WriteLine("UP!", InformationType.Success);
                    MoveUP();
                }
                else if (e.Key == Keys.Down)
                {
                    Console.WriteLine("DOWN!", InformationType.Success);
                    MoveDown();
                }
                else if (e.Key == Keys.Left)
                {
                    Console.WriteLine("GetDeskHeight!", InformationType.Success);
                    Task a = GetDeskHeight();
                }
            }
        }
        #endregion

        #region Command
        static private void MoveUP()
        {
            _ = MoveDesk(COMMAND_UP);
        }
        
        static private void MoveDown()
        {
            _ = MoveDesk(COMMAND_DOWN);
        }
        
        static private async Task MoveDesk(byte[] command)
        {
            var c = GetCommandCharacteristic(UUID_COMMAND);
            Console.WriteLine("Characteristic found   UUID : " + c.Uuid.ToString());

            var writer = new DataWriter();
            // WriteByte used for simplicity. Other common functions - WriteInt16 and WriteSingle
            writer.WriteBytes(command);
            //Thread.Sleep(600);
            Console.WriteLine("STATUS GOOD", InformationType.Success);
            GattCommunicationStatus result = await c.WriteValueAsync(writer.DetachBuffer());
            if (result == GattCommunicationStatus.Success)
            {
                Console.WriteLine("Successfully wrote to device", InformationType.Success);
                // Successfully wrote to device
            }
            else
                Console.WriteLine("Failed to wrote to device", InformationType.Error);
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
                Console.WriteLine("Successfully wrote to device", InformationType.Success);
                // Successfully wrote to device
            }
            else
                Console.WriteLine("Failed to wrote to device", InformationType.Error);
        }
        
        private static GattCharacteristic GetCommandCharacteristic(string command)
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
                var c = GetCommandCharacteristic(UUID_HEIGHT);

                var result = await c.ReadValueAsync(BluetoothCacheMode.Uncached);
                Console.WriteLine($"result : {result}");
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var reader = DataReader.FromBuffer(result.Value);
                    byte[] input = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(input);
                    // Utilize the data as needed

                    Console.WriteLine(input.GetType(), InformationType.Success);


                    for (int i = 0; i < input.Length; i++)
                    {

                        Console.Write($"{input[i].ToString("X4")}");
                    }
                    Console.NewLine();
                }
                else
                    Console.WriteLine("ERROR", InformationType.Error);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception : {e.Message}");
            }
        }
        #endregion
    }
}
