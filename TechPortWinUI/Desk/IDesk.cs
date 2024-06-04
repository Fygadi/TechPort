using TechPortWinUI.Desk;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using TechPortWinUI.Models;

namespace TechPort.Desk;
/// <summary>
/// Represent a Desk
/// </summary>
public interface IDesk
{
    short MinHeight { get; set; }
    short MaxHeight { get; set; }
    short Height { get; }
    short Speed { get; }
    MovementStatus CurrentMovementStatus { get; }

    BluetoothLEAppearance Appearance { get;}
    ulong BluetoothAddress { get; }
    BluetoothAddressType BluetoothAddressType { get; }
    BluetoothDeviceId BluetoothDeviceId { get; }
    BluetoothConnectionStatus ConnectionStatus { get; }
    DeviceAccessInformation DeviceAccessInformation { get; }
    string DeviceId { get; }
    DeviceInformation DeviceInformation { get; }
    //public IReadOnlyList<GattDeviceService> GattServices{ get; }
    string Name { get; }
    bool WasSecureConnectionUsedForPairing{ get; }

    static Task<IDesk>? CreateAsync(string deskId) => throw new NotImplementedException();
    Task<GattCommunicationStatus> MoveUpAsync();
    Task<GattCommunicationStatus> MoveDownAsync();
    Task<GattCommunicationStatus> StopAsync();
    void MoveToHeightAsync(short targetHeight);
}