﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace TechPort.Helpers
{
    public class BluetoothService
    {
        public static async Task<BluetoothLEDevice> GetBluetoothLEDeviceFromIdAsync(string deviceId)
        {
            // Get the BluetoothLEDevice object associated with the device ID
            BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceId);

            return bluetoothLeDevice;
        }

        public static async Task<List<GattCharacteristic>> GetGattCharacteristicsAsync(BluetoothLEDevice bluetoothLEDevice)
        {
            List<GattCharacteristic> characteristicsList = new List<GattCharacteristic> ();

            GattDeviceServicesResult services = await bluetoothLEDevice.GetGattServicesAsync();
            if (services.Status != GattCommunicationStatus.Success) return null;

            foreach (var service in services.Services)
            {
                GattCharacteristicsResult characteristics = await service.GetCharacteristicsAsync();
                if (characteristics.Status != GattCommunicationStatus.Success) continue;

                characteristicsList.AddRange(characteristics.Characteristics);
            }

            return characteristicsList;
        }

        public static async Task<byte[]> ReadAsync(GattCharacteristic selectedCharacteristic)
        {
            byte[] raw = null;
            GattReadResult result = await selectedCharacteristic.ReadValueAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                DataReader reader = DataReader.FromBuffer(result.Value);
                raw = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(raw);

                // TODO: Use the IPAddress.NetworkToHostOrder();
            }

            return raw;
        }

        public static async Task<bool> WriteAsync(GattCharacteristic selectedCharacteristic, byte[] byteValue)
        {
            // TODO: Implement the logic.
            var writer = new DataWriter();
            // WriteByte used for simplicity. Other common functions - WriteInt16 and WriteSingle
            writer.WriteBytes(byteValue);

            GattCommunicationStatus result = await selectedCharacteristic.WriteValueAsync(writer.DetachBuffer());
            return (result == GattCommunicationStatus.Success);
        }
    }
}