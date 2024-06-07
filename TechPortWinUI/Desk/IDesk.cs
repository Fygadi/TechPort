using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using TechPortWinUI.Helpers;

namespace TechPortWinUI.Desk;
/// <summary>
/// Represent a Desk
/// </summary>
public partial interface IDesk
{
    public event PropertyChangedEventHandler? PropertyChanged;
    short MinHeight { get; set; }
    short MaxHeight { get; set; }
    short Height { get; }
    short Speed { get; }
    MovementStatus MovementStatus { get; }

    BluetoothLEAppearance Appearance { get;}
    ulong BluetoothAddress { get; }
    BluetoothAddressType BluetoothAddressType { get; }
    BluetoothDeviceId BluetoothDeviceId { get; }
    BluetoothConnectionStatus BluetoothConnectionStatus { get; }
    DeviceAccessInformation DeviceAccessInformation { get; }
    string DeviceId { get; }
    DeviceInformation DeviceInformation { get; }
    //public IReadOnlyList<GattDeviceService> GattServices{ get; }
    string Name { get; }
    bool WasSecureConnectionUsedForPairing{ get; }

    static async Task<IdasenDesk> CreateAsync(string deskId) => throw new NotImplementedException();
    Task<GattCommunicationStatus> MoveUpAsync();
    Task<GattCommunicationStatus> MoveDownAsync();
    Task<GattCommunicationStatus> StopMovingAsync();
    void MoveToHeightAsync(short targetHeight);

    void UpdateProperty();
}