using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace TechPortWinUI.Helpers;

public static class BluetoothLeHelper
{
    public static async Task<BluetoothLEDevice> ConnectDeviceAsync(string Id)
    {
        // Note: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
        return await BluetoothLEDevice.FromIdAsync(Id);
    }
    public static async Task<BluetoothLEDevice> ConnectDeviceAsync(DeviceInformation deviceInfo) => await ConnectDeviceAsync(deviceInfo.Id);
    public static void DisconnectDevice(BluetoothLEDevice bluetoothLeDevice)
    {
        bluetoothLeDevice.Dispose();
    }

    public static async Task<(byte[]?, GattCommunicationStatus)> ReadAsync(GattCharacteristic characteristic)
    {
        if (characteristic == null)
            throw new ArgumentNullException(nameof(characteristic));

        GattReadResult result = await characteristic.ReadValueAsync();

        if (result.Status != GattCommunicationStatus.Success)
            return (null, result.Status);

        var reader = DataReader.FromBuffer(result.Value);
        byte[] data = new byte[reader.UnconsumedBufferLength];
        reader.ReadBytes(data);

        return (data, result.Status);
    }
    public static async Task<GattCommunicationStatus> WriteAsync(GattCharacteristic characteristic, byte[] data)
    {
        if (characteristic == null)
            throw new ArgumentNullException(nameof(characteristic));

        var writer = new DataWriter();
        // WriteByte used for simplicity. Other common functions - WriteInt16 and WriteSingle
        writer.WriteBytes(data);

        GattCommunicationStatus result = await characteristic.WriteValueAsync(writer.DetachBuffer());
        return result;
    }

    private static async Task<(IReadOnlyList<GattDeviceService>?, GattCommunicationStatus)> GetServicesFromDevice(BluetoothLEDevice bluetoothLeDevice)
    {
        GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync();

        if (result.Status != GattCommunicationStatus.Success)
            return (null, result.Status);

        return (result.Services, result.Status);
    }
    private static async Task<(IReadOnlyList<GattCharacteristic>?, GattCommunicationStatus)> GetCharacteristricsFromService(GattDeviceService gattDeviceService)
    {
        GattCharacteristicsResult result = await gattDeviceService.GetCharacteristicsAsync();

        if (result.Status != GattCommunicationStatus.Success)
            return (null, result.Status);

        return (result.Characteristics, result.Status);
    }
    /// <summary>
    /// return a list of characteristics or an empty Dictionary if no characteristics found
    /// </summary>
    /// <param name="bluetoothLeDevice"></param>
    /// <returns></returns>
    public static async Task<Dictionary<GattDeviceService, List<GattCharacteristic>>> GetServicesAndCharacteristicsFromDevice(BluetoothLEDevice bluetoothLeDevice)
    {
        Dictionary<GattDeviceService, List<GattCharacteristic>> gattServiceCharacteristics = new();
        var servicesResult = await GetServicesFromDevice(bluetoothLeDevice);

        if (servicesResult.Item2 == GattCommunicationStatus.Success && servicesResult.Item1 != null)
        {
            foreach (var service in servicesResult.Item1)
            {
                var characteristics = await GetCharacteristricsFromService(service);
                if (characteristics.Item2 == GattCommunicationStatus.Success && characteristics.Item1 != null)
                    gattServiceCharacteristics.Add(service, new List<GattCharacteristic>(characteristics.Item1));
                else
                    gattServiceCharacteristics.Add(service, new List<GattCharacteristic>());
            }
        }

        return gattServiceCharacteristics;
    }

    /// <summary>
    /// Subscribe to a characteristic notification
    /// </summary>
    /// <param name="gattCharacteristic">The characteristic to subscribe for the notification</param>
    /// <param name="callbackAction">The call back method for the notificaiton</param>
    /// <returns></returns>
    public static bool SubscribeToCharacteristics(GattCharacteristic gattCharacteristic, TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> callbackAction)
    {
        if (gattCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
        {
            gattCharacteristic.ValueChanged += callbackAction;
            return true;
        }

        else return false;
    }

    //might be usefull for checking if bluetooth is turn on or off
    private async static Task<bool> CheckPeripheralRoleSupportAsync()
    {
        // BT_Code: New for Creator's Update - Bluetooth adapter has properties of the local BT radio.
        var localAdapter = await BluetoothAdapter.GetDefaultAsync();

        if (localAdapter != null)
        {
            return localAdapter.IsPeripheralRoleSupported;
        }
        else
        {
            // Bluetooth is not turned on 
            return false;
        }
    }
}