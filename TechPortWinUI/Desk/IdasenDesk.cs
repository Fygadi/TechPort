using System.Net;
using TechPortWinUI.Desk;
using TechPortWinUI.Helpers;
using TechPortWinUI.ViewModels;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using static System.Net.Mime.MediaTypeNames;

namespace TechPort.Desk;
public class IdasenDesk : IDesk
{
    #region Field
    readonly BluetoothLEDevice _device;

    private short _minHeight = 620;
    private short _maxHeight = 1270;
    private short _height;
    private short _speed = 0;
    private readonly MovementStatus _currentMovementStatus = new();

    //short.MaxValue means it has reach the desired height
    private short _targetHeight = short.MaxValue;

    private readonly Dictionary<GattDeviceService, List<GattCharacteristic>> _gattServiceAndCharacteristics;
    #endregion
    
    #region Property
    public short MinHeight { get => _minHeight; set => _minHeight = value; }
    public short MaxHeight { get => _maxHeight; set => _maxHeight = value; }
    public short Height { get => _height; }
    public short Speed { get => _speed; }
    public MovementStatus CurrentMovementStatus { get => _currentMovementStatus;}

    public BluetoothLEAppearance Appearance { get => _device.Appearance; }
    public ulong BluetoothAddress { get => _device.BluetoothAddress; }
    public BluetoothAddressType BluetoothAddressType { get => _device.BluetoothAddressType; }
    public BluetoothDeviceId BluetoothDeviceId { get => _device.BluetoothDeviceId; }
    public BluetoothConnectionStatus ConnectionStatus { get => _device.ConnectionStatus; }
    public DeviceAccessInformation DeviceAccessInformation { get => _device.DeviceAccessInformation; }
    public string DeviceId { get => _device.DeviceId; }
    public DeviceInformation DeviceInformation { get => _device.DeviceInformation; }
    public IReadOnlyList<GattDeviceService> GattServices => throw new NotImplementedException();
    public string Name { get => _device.Name;}
    public bool WasSecureConnectionUsedForPairing { get => _device.WasSecureConnectionUsedForPairing; }
    #endregion

    #region Characteristics
    /// <summary>
    /// UUID representing the characteristic for the height of the desk.
    /// </summary>
    private static readonly Guid _UUID_HEIGHT = Guid.Parse("99fa0021-338a-1024-8a49-009c0215f78a");
    /// <summary>
    /// UUID representing the characteristic to write commands to control the desk (move up, move down, stop).
    /// </summary>
    private static readonly Guid _UUID_COMMAND = Guid.Parse("99fa0002-338a-1024-8a49-009c0215f78a");
    /// <summary>
    /// UUID representing the characteristic for setting presets or receiving preset notifications.
    /// </summary>
    private static readonly Guid _UUID_DPG = Guid.Parse("99fa0011-338a-1024-8a49-009c0215f78a");
    #endregion
    
    #region Command
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

    #region Constructor
    private IdasenDesk(BluetoothLEDevice device, Dictionary<GattDeviceService, List<GattCharacteristic>> gattServiceAndCharacteristics)
    {
        //Initialise variable
        this._device = device;
        _gattServiceAndCharacteristics = gattServiceAndCharacteristics;

        //Register to device event
        _device.ConnectionStatusChanged += ConnectionStatusChanged;
        _device.GattServicesChanged += GattServicesChanged;
        _device.NameChanged += NameChanged;

        //Register to characteristics notification
        GetCharacteristicByGuid(_UUID_HEIGHT).ValueChanged += CharacteristicHeight_ValueChangedAsync;
        GetCharacteristicByGuid(_UUID_DPG).ValueChanged += CharacteristicDpg_ValueChanged;
    }

    //use a createAsync method since the constructor cant use an async
    //TODO make sure there isn't 2 desk with the same id (use a static list)
    public static async Task<IdasenDesk?> CreateAsync(string deskId)
    {
        var device = await BluetoothLeHelper.ConnectDeviceAsync(deskId);
        var gattServicesAndCharacteristics = await BluetoothLeHelper.GetServicesAndCharacteristicsFromDevice(device);

        var idasenDesk = new IdasenDesk(device, gattServicesAndCharacteristics);
        (short speed, short height) = await idasenDesk.GetSpeedAndHeightAsync();

        idasenDesk._speed = speed;
        idasenDesk._height = height;

        return idasenDesk;
    }
    #endregion

    #region Notification
    private void ConnectionStatusChanged(BluetoothLEDevice bluetoothLEDevice, object o)
    {
        //TODO: Implement this method
        //throw new NotImplementedException();
    }
    private void GattServicesChanged(BluetoothLEDevice bluetoothLEDevice, object o)
    {
        //TODO: Implement this method
        throw new NotImplementedException();
    }
    private void NameChanged(BluetoothLEDevice bluetoothLEDevice, object o)
    {
        //TODO: Implement this method
        throw new NotImplementedException();
    }

    private async void CharacteristicHeight_ValueChangedAsync(GattCharacteristic sender, GattValueChangedEventArgs args)
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

            if (await StopAsync() != GattCommunicationStatus.Success)
                throw new NotImplementedException();
        }
    }
    private void CharacteristicDpg_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Command methods
    public async Task<GattCommunicationStatus> MoveUpAsync() => await WriteAsync(_UUID_COMMAND, _COMMAND_UP);
    public async Task<GattCommunicationStatus> MoveDownAsync() => await WriteAsync(_UUID_COMMAND, _COMMAND_DOWN);
    public async Task<GattCommunicationStatus> StopAsync() => await WriteAsync(_UUID_COMMAND, _COMMAND_STOP);
    public async void MoveToHeightAsync(short targetHeight)
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
        //TODO: Add return statement
    }

    //This method must be call when unregistred from characteristic _UUID_HEIGHT
    private async Task<(short, short)> GetSpeedAndHeightAsync()
    {
        var (data, status) = await ReadAsync(_UUID_HEIGHT);
        if (status != GattCommunicationStatus.Success)
            throw new NotImplementedException();

        return DataToSpeedAndHeightConverter(BitConverter.ToUInt32(data));
    }
    #endregion

    #region Data converter
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

    #region Helper
    private async Task<GattCommunicationStatus> WriteAsync(Guid characteristic, byte[] data)
    {
        var c = GetCharacteristicByGuid(characteristic);
        if (c == null)
        {
            //TODO: Implement exception
            throw new ArgumentNullException(nameof(c));
        }

        return await BluetoothLeHelper.WriteAsync(c, data);
    }
    private async Task<(byte[], GattCommunicationStatus)> ReadAsync(Guid characteristic)
    {
        var c = GetCharacteristicByGuid(characteristic);
        if (c == null)
            throw new ArgumentNullException(nameof(c));

        return await BluetoothLeHelper.ReadAsync(c);
    }
    private GattCharacteristic GetCharacteristicByGuid(Guid characteristicUuid)
    {
        foreach (var listGattCharacteristics in _gattServiceAndCharacteristics.Values)
        {
            foreach (var gattCharacteristic in listGattCharacteristics)
            {
                if (gattCharacteristic.Uuid == characteristicUuid)
                    return gattCharacteristic;
            }
        }
        throw new NotImplementedException();
    }
    #endregion

    #region Destructor
    /// <summary>
    /// Take care of disposing the resources associated with the Device (DeskInstance).
    /// </summary>
    ~IdasenDesk()
    {
        _device.Dispose();
    }
    #endregion
}