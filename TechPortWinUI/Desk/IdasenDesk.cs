using CommunityToolkit.Mvvm.ComponentModel;
using System.Net;
using TechPortWinUI.Helpers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace TechPortWinUI.Desk;
public class IdasenDesk : BaseDesk, IDesk
{
    #region Field
    //short.MaxValue means it has reach the desired height
    private short _targetHeight = short.MaxValue;

    private readonly Dictionary<GattDeviceService, List<GattCharacteristic>> _gattServiceAndCharacteristics;
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
    private IdasenDesk(BluetoothLEDevice device,
                       Dictionary<GattDeviceService,
                       List<GattCharacteristic>> gattServiceAndCharacteristics): base(
                           device,
                           0,
                           5192)
    {
        //Initialise variable
        _gattServiceAndCharacteristics = gattServiceAndCharacteristics;

        //Register to device event
        Device.ConnectionStatusChanged += ConnectionStatusChanged;
        Device.GattServicesChanged += GattServicesChanged;
        Device.NameChanged += NameChanged;

        //Register to characteristics notification
        GetCharacteristicByGuid(_UUID_HEIGHT).ValueChanged += CharacteristicHeight_ValueChangedAsync;
        GetCharacteristicByGuid(_UUID_DPG).ValueChanged += CharacteristicDpg_ValueChanged;
    }

    //use a createAsync method since the constructor cant use an async
    //TODO make sure there isn't 2 desk with the same id (use a static list)
    public static async Task<IdasenDesk> CreateAsync(string deskId)
    {
        var device = await BluetoothLeHelper.ConnectDeviceAsync(deskId);
        var gattServicesAndCharacteristics = await BluetoothLeHelper.GetServicesAndCharacteristicsFromDevice(device);

        var idasenDesk = new IdasenDesk(device, gattServicesAndCharacteristics);
        (short speed, short height) = await idasenDesk.GetSpeedAndHeightAsync();

        //TODO FIX THIS ITS CAUSING AN UPDATE
        idasenDesk.Speed = speed;
        idasenDesk.Height = height;

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
        (Speed, Height) = DataToSpeedAndHeightConverter(data);

        // Check if the current height of the desk is within an acceptable proximity to the desired height
        if (Math.Abs(Height - _targetHeight) <= 10)
        {
            // If the difference between the current height and the target height is within a small threshold (e.g., 10 units),
            // set the target height to short.MaxValue to indicate that the desk has reached the desired height and does not need to move further.
            _targetHeight = short.MaxValue;

            if (await StopMovingAsync() != GattCommunicationStatus.Success)
                throw new NotImplementedException();
        }
    }
    private void CharacteristicDpg_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Command methods
    public override async  Task<GattCommunicationStatus> MoveUpAsync() => await WriteAsync(_UUID_COMMAND, _COMMAND_UP);
    public override async Task<GattCommunicationStatus> MoveDownAsync() => await WriteAsync(_UUID_COMMAND, _COMMAND_DOWN);
    public override async Task<GattCommunicationStatus> StopMovingAsync() => await WriteAsync(_UUID_COMMAND, _COMMAND_STOP);
    public override async void MoveToHeightAsync(short targetHeight)
    {
        //TODO: Check for Condition and avoid multiple instance
        _targetHeight = targetHeight;

        while (_targetHeight != short.MaxValue)
        {
            if (Height < _targetHeight)
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
        Device.Dispose();
    }
    #endregion
}