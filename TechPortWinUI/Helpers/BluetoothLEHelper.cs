using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace TechPortWinUI.Helpers;

public static class BluetoothLEHelper
{
    public static async Task<BluetoothLEDevice> GetBluetoothLEDeviceFromIdAsync(string deviceId) => await BluetoothLEDevice.FromIdAsync(deviceId);
    public static List<GattCharacteristic> GetGattCharacteristics(BluetoothLEDevice bluetoothLEDevice)
    {
        var characteristics = new List<GattCharacteristic>();

        foreach (var service in bluetoothLEDevice.GattServices)
            characteristics.AddRange(service.GetAllCharacteristics());
        return characteristics;
    }
    public static GattCharacteristic GetCharacteristicFromUuid(List<GattCharacteristic> gattCharacteristics, string command)
    {
        var c = gattCharacteristics.FirstOrDefault(characteristic => characteristic.Uuid.ToString() == command);
        if (c == null)
            //TODO: Handle exception case
            throw new NotImplementedException("characterisitcs not found");

        return c;
    }
    public static async Task<(byte[]?, GattCommunicationStatus)> ReadAsync(List<GattCharacteristic> gattCharacteristics, string c)
    {
        GattCharacteristic characteristic = GetCharacteristicFromUuid(gattCharacteristics, c);

        byte[]? raw = null;
        var result = await characteristic.ReadValueAsync();
        if (result.Status == GattCommunicationStatus.Success)
        {
            var reader = DataReader.FromBuffer(result.Value);
            raw = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(raw);

            // TODO: Use the IPAddress.NetworkToHostOrder(); (not sure)
        }

        return (raw, result.Status);
    }    
    public static async Task<GattCommunicationStatus> WriteAsync(List<GattCharacteristic> gattCharacteristics, string c, byte[] byteValue)
    {
        GattCharacteristic characteristic = GetCharacteristicFromUuid(gattCharacteristics, c);
        DataWriter writer = new();
        writer.WriteBytes(byteValue);
        return await characteristic.WriteValueAsync(writer.DetachBuffer());
    }
    public static async Task<GattCommunicationStatus> SubscribeToCharacteristicNotificationsAsync(GattCharacteristic characteristic, TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> CharacteristicValueChangedCallBack)
    {
        GattCommunicationStatus communicationStatus =
            await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
        if (communicationStatus == GattCommunicationStatus.Success)
            characteristic.ValueChanged += CharacteristicValueChangedCallBack;
        return communicationStatus;
    }
}