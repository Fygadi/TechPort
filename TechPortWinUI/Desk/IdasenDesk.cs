using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TechPortWinUI.Helpers;
using TechPortWinUI.Models;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using Windows.Storage.Streams;

namespace TechPort.Desk
{
    internal class IdasenDesk : IDesk
    {
        public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public short MinHeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public short MaxHeight { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public short Height { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public short Speed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TechPortWinUI.Desk.MovementStatus CurrentMovementStatus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<TechPortWinUI.Desk.Preset> PresetList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //public async void MoveUpAsync()
        //{
        //    WriteAsync(_UUID_COMMAND, _COMMAND_UP);
        //}

        //public async void MoveDownAsync()
        //{
        //    WriteAsync(_UUID_COMMAND, _COMMAND_DOWN);
        //    throw new NotImplementedException();
        //}

        //public async void MoveToHeightAsync()
        //{
        //    throw new NotImplementedException();
        //}

        #region Constants
        /// <summary>
        /// Represent the minimum desk height in meters
        /// </summary>
        private const double _MIN_HEIGHT = 0.62;
        /// <summary>
        /// Represent the maximum desk height in meters
        /// </summary>
        private const double _MAX_HEIGHT = 1.27;

        #region Desk characteristics
        /// <summary>
        /// UUID representing the characteristic for the height of the desk.
        /// </summary>
        private const string _UUID_HEIGHT = "99fa0021-338a-1024-8a49-009c0215f78a";
        /// <summary>
        /// UUID representing the characteristic to write commands to control the desk (move up, move down, stop).
        /// </summary>
        private const string _UUID_COMMAND = "99fa0002-338a-1024-8a49-009c0215f78a";
        /// <summary>
        /// UUID representing the characteristic for setting presets or receiving preset notifications.
        /// </summary>
        private const string _UUID_DPG = "99fa0011-338a-1024-8a49-009c0215f78a";
        #endregion

        #region Desk command
        //Note Command must be send with the characteristic _UUID_COMMAND

        /// <summary>
        /// Represents the command for moving up.
        /// </summary>
        private static readonly byte[] _COMMAND_UP = { 0x47, 0x00 };
        /// <summary>
        /// Represents the command for moving down.
        /// </summary>
        private static readonly byte[] _COMMAND_DOWN = { 0x46, 0x00 };
        /// <summary>
        /// Represents the command to stop movement.
        /// </summary>
        private static readonly byte[] _COMMAND_STOP = { 0xFF, 0x00 };
        #endregion
        #endregion

        #region Private members
        private readonly BluetoothLEDevice? _bluetoothLEDevice = null;
        private readonly List<GattCharacteristic> _gattCharacteristics;

        private string _name;
        private short _speed;
        private short _height;
        private List<uint> _presets;

        /// <summary>
        /// Used link the MoveUpAsync and MoveDownAsync method with
        /// the desk notification beeing received by the HeightAndSpeed_ValueChanged method
        /// </summary>
        private short _targetHeight = short.MaxValue;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor for creating a DeskInstance object with a Bluetooth LE device.
        /// </summary>
        /// <param name="bluetoothLEDevice">The Bluetooth LE device associated with the DeskInstance.</param>
        private IdasenDesk(BluetoothLEDevice bluetoothLEDevice)
        {
            _name = bluetoothLEDevice.Name;
            bluetoothLEDevice.NameChanged += (d, o) => _name = d.Name;

            _gattCharacteristics = BluetoothLEHelper.GetGattCharacteristics(bluetoothLEDevice);
        }

        /// <summary>
        /// Creates a new DeskInstance asynchronously based on the specified device ID.
        /// </summary>
        /// <param name="deviceId">The ID of the Bluetooth LE device.</param>
        /// <returns>A Task representing the asynchronous operation, yielding the created DeskInstance.</returns>
        public static async Task<IdasenDesk> CreateAsync(string deviceId)
        {
            var device = await BluetoothLEHelper.GetBluetoothLEDeviceFromIdAsync(deviceId);

            //var characteristics = await GetGattCharacteristicsAsync(device);

            var bLEDeskControl = new IdasenDesk(device);
            var result = await bLEDeskControl.GetPresetsAsync();
            bLEDeskControl._presets = result.Item1;

            (bLEDeskControl._speed, bLEDeskControl._height) = await bLEDeskControl.GetSpeedAndHeightAsync();
            await BluetoothLEHelper.SubscribeToCharacteristicNotificationsAsync(BluetoothLEHelper.GetCharacteristicFromUuid(bLEDeskControl._gattCharacteristics, _UUID_HEIGHT), bLEDeskControl.HeightAndSpeed_ValueChanged);
            await BluetoothLEHelper.SubscribeToCharacteristicNotificationsAsync(BluetoothLEHelper.GetCharacteristicFromUuid(bLEDeskControl._gattCharacteristics, _UUID_DPG), bLEDeskControl.Preset_ValueChanged);

            return bLEDeskControl;
        }
        #endregion

        #region public Method
        /// <summary>
        /// Moves the desk to the specified target height asynchronously.
        /// </summary>
        /// <param name="targetHeight">The target height to which the desk should be moved.</param>
        /// <returns>A boolean indicating whether the desk has successfully moved to the target height.</returns>
        public async Task<bool> MoveToHeight(short targetHeight)
        {
            //TODO: Check for Condition and avoid multiple instance
            _targetHeight = targetHeight;

            while (_targetHeight != short.MaxValue)
            {
                if (_height < _targetHeight)
                    await MoveUpAsync();
                else
                    await MoveDownAsync();
                Thread.Sleep(500);
            }
            //TODO: Fix return statement
            return true;
        }
        /// <summary>
        /// Sends the command to the desk to move up asynchronously.
        /// </summary>
        /// <returns>The communication status of the write operation.</returns>
        /// <remarks>
        /// This method must be called approximately every 500 milliseconds to maintain constant movement of the desk upwards.
        /// </remarks>
        public async Task<GattCommunicationStatus> MoveUpAsync() => await WriteAsync(_UUID_COMMAND, _COMMAND_UP);
        /// <summary>
        /// Sends the command to the desk to move down asynchronously.
        /// </summary>
        /// <returns>The communication status of the write operation.</returns>
        /// <remarks>
        /// This method must be called approximately every 500 milliseconds to maintain constant movement of the desk downwards.
        /// </remarks>
        public async Task<GattCommunicationStatus> MoveDownAsync() => await WriteAsync(_UUID_COMMAND, _COMMAND_DOWN);
        /// <summary>
        /// Sends the command to the desk to stop movement asynchronously.
        /// </summary>
        /// <returns>The communication status of the write operation.</returns>
        public async Task<GattCommunicationStatus> StopAsync() => await WriteAsync(_UUID_COMMAND, _COMMAND_STOP);
        /// <summary>
        /// Sets a preset with the specified height asynchronously.
        /// </summary>
        /// <param name="preset">The preset to set.</param>
        /// <param name="height">The height to associate with the preset.</param>
        /// <returns>The communication status of the write operation.</returns>
        /// <remarks>
        /// This method is not implemented yet. It needs to be fixed and properly implemented.
        /// An example usage and any additional notes should be added once the method is fixed.
        /// </remarks>
        public async Task<GattCommunicationStatus> SetPreset(Preset preset, short height)
        {
            throw new NotImplementedException();
            //TODO: Fix this method and add example for comment

            byte[] heightBytes = BitConverter.GetBytes(height);
            byte[] value = { 0x7f, 0x8, (byte)preset, heightBytes[0], heightBytes[1] };
            return await WriteAsync(_UUID_DPG, value);
        }
        #endregion

        #region Notification CallBack
        /// <summary>
        /// Event handler for handling changes in height and speed received from the desk.
        /// </summary>
        /// <param name="sender">The GATT characteristic sending the notification.</param>
        /// <param name="args">Event arguments containing the characteristic value.</param>
        private async void HeightAndSpeed_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            DataReader reader = DataReader.FromBuffer(args.CharacteristicValue);
            uint data = (uint)IPAddress.NetworkToHostOrder((int)reader.ReadUInt32());
            (_speed, _height) = DataToSpeedAndHeightConverter(data);

            // Check if the current height of the desk is within an acceptable proximity to the desired height
            if (Math.Abs(_height - _targetHeight) <= 10)
            {
                // If the difference between the current height and the target height is within a small threshold (e.g., 10 units),
                // set the target height to short.MaxValue to indicate that the desk has reached the desired height and does not need to move further.
                _targetHeight = short.MaxValue;

                await StopAsync();
            }
        }
        /// <summary>
        /// Event handler for handling changes in preset values received from the desk.
        /// </summary>
        /// <param name="sender">The GATT characteristic sending the notification.</param>
        /// <param name="args">Event arguments containing the characteristic value.</param>
        private void Preset_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            //TODO: Implement the logic.
            Debug.WriteLine(args.CharacteristicValue);
            DataReader reader = DataReader.FromBuffer(args.CharacteristicValue);
            uint length = reader.UnconsumedBufferLength;
            uint data = (uint)IPAddress.NetworkToHostOrder((int)reader.ReadByte());
        }
        #endregion

        #region private helper method
        /// <summary>
        /// Reads data asynchronously from the specified characteristic UUID.
        /// </summary>
        /// <param name="c">The UUID of the characteristic to read from.</param>
        /// <returns>A tuple containing the read data and the communication status.</returns>
        public async Task<(byte[]?, GattCommunicationStatus)> ReadAsync(string c)
        {
            GattCharacteristic characteristic = BluetoothLEHelper.GetCharacteristicFromUuid(_gattCharacteristics, c);

            byte[]? data = null;
            var result = await characteristic.ReadValueAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                var reader = DataReader.FromBuffer(result.Value);
                data = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(data);

                // TODO: Use the IPAddress.NetworkToHostOrder(); (not sure)
            }

            return (data, result.Status);
        }
        /// <summary>
        /// Writes data asynchronously to the specified characteristic UUID.
        /// </summary>
        /// <param name="c">The UUID of the characteristic to write to.</param>
        /// <param name="byteValue">The byte array to write.</param>
        /// <returns>The communication status of the write operation.</returns>
        public async Task<GattCommunicationStatus> WriteAsync(string c, byte[] byteValue)
        {
            GattCharacteristic characteristic = BluetoothLEHelper.GetCharacteristicFromUuid(_gattCharacteristics, c);
            DataWriter writer = new();
            writer.WriteBytes(byteValue);
            return await characteristic.WriteValueAsync(writer.DetachBuffer());
        }

        /// <summary>
        /// Retrieves presets asynchronously from the Bluetooth Low Energy desk.
        /// </summary>
        /// <returns>A tuple containing the list of presets and the communication status.</returns>
        private async Task<(List<uint>, GattCommunicationStatus)> GetPresetsAsync()
        {
            var (value, gattCommunicationStatus) = await BluetoothLEHelper.ReadAsync(_gattCharacteristics, _UUID_DPG);
            return (new List<uint>(), gattCommunicationStatus);
        }
        /// <summary>
        /// Retrieves speed and height asynchronously from the Bluetooth Low Energy desk.
        /// </summary>
        /// <returns>A tuple containing the speed and height values.</returns>
        private async Task<(short, short)> GetSpeedAndHeightAsync()
        {
            var (data, _) = await ReadAsync(_UUID_HEIGHT);
            return DataToSpeedAndHeightConverter(BitConverter.ToUInt32(data));
        }
        #endregion

        #region data converter
        /// <summary>
        /// Converts a 32-bit unsigned integer data provided by the desk into speed and height.
        /// </summary>
        /// <param name="data">The 32-bit unsigned integer containing speed and height information from the desk.</param>
        /// <returns>A tuple containing the speed and height as short integers.</returns>
        private static (short, short) DataToSpeedAndHeightConverter(uint data)
        {
            short speed = (short)(data >> 16);
            short height = (short)(data & 0xFFFF);
            return (speed, height);
        }
        #endregion

        #region Enum
        /// <summary>
        /// Enum representing the presets 1, 2, and 3 used by the desk, along with their corresponding byte values.
        /// </summary>
        public enum Preset : byte
        {
            /// <summary>
            /// Represents the desk byte value for preset 1.
            /// </summary>
            _1 = 0x9,
            /// <summary>
            /// Represents the desk byte value for preset 2.
            /// </summary>
            _2 = 0xb,
            /// <summary>
            /// Represents the desk byte value for preset 3.
            /// </summary>
            _3 = 0xa,
        }
        #endregion

        #region Destructor
        /// <summary>
        /// Destructor responsible for disposing the Bluetooth resources associated with the DeskInstance.
        /// </summary>
        ~IdasenDesk()
        {
            if (_bluetoothLEDevice != null)
                _bluetoothLEDevice.Dispose();
        }
        #endregion
    }
}
