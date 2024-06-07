using System.ComponentModel;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace TechPortWinUI.Desk
{
    internal class DefaultDesk : IDesk
    {
        public event PropertyChangedEventHandler? PropertyChanged;

#pragma warning disable CS8603 // Possible null reference return.
        public short MinHeight { get => -1; set { } }
        public short MaxHeight { get => -1; set { } }

        public short Height => -1;

        public short Speed => -1;

        public MovementStatus MovementStatus => MovementStatus.Idle;

        public BluetoothLEAppearance Appearance => null;

        public ulong BluetoothAddress => 0;

        public BluetoothAddressType BluetoothAddressType => BluetoothAddressType.Public;

        public BluetoothDeviceId BluetoothDeviceId => null;

        public BluetoothConnectionStatus BluetoothConnectionStatus => BluetoothConnectionStatus.Disconnected;

        public DeviceAccessInformation DeviceAccessInformation => null;

        public string DeviceId => "NA";

        public DeviceInformation DeviceInformation => null;

        public string Name => "NA";

        public bool WasSecureConnectionUsedForPairing => false;

        public Task<GattCommunicationStatus> MoveDownAsync()
        {
            throw new NotImplementedException();
        }

        public void MoveToHeightAsync(short targetHeight)
        {
            throw new NotImplementedException();
        }

        public Task<GattCommunicationStatus> MoveUpAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GattCommunicationStatus> StopMovingAsync()
        {
            throw new NotImplementedException();
        }

        public void UpdateProperty() { }
    }
}
