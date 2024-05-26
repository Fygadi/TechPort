using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace TechPortWinUI.Helpers;

//reference https://learn.microsoft.com/en-us/windows/uwp/devices-sensors/gatt-client
public class Bluetooth
{
    static readonly string aqsAllBluetoothLEDevices = BluetoothLEDevice.GetDeviceSelectorFromAppearance(
            BluetoothLEAppearance.FromParts(
                BluetoothLEAppearanceCategories.Uncategorized,
                BluetoothLEAppearanceSubcategories.Generic));
    static readonly string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", };
    static DeviceWatcher deviceWatcher = null;

    DeviceInformation deviceInformation = null;
    string deviceName = "";

    bool enumerationCompleted = false;

    public async Task<BluetoothLEDevice> GetBluetoothLEDeviceAsync(string deviceName)
    {
        this.deviceName = deviceName;
        
        deviceWatcher =
            DeviceInformation.CreateWatcher(
                aqsAllBluetoothLEDevices,
                requestedProperties,
                DeviceInformationKind.AssociationEndpoint);

        // Register event handlers before starting the watcher.
        // Added, Updated and Removed are required to get all nearby devices
        deviceWatcher.Added += DeviceWatcher_Added;
        deviceWatcher.Updated += DeviceWatcher_Updated;
        deviceWatcher.Removed += DeviceWatcher_Removed;

        // EnumerationCompleted and Stopped are optional to implement.
        deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
        deviceWatcher.Stopped += DeviceWatcher_Stopped;

        deviceWatcher.Start();

        while (deviceInformation == null || enumerationCompleted)
        {
            Thread.Sleep(100);
        }

        BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceInformation.Id);

        return bluetoothLeDevice;
    }

    #region DeviceWatcher CallBack
    private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
    {
        if (!string.IsNullOrEmpty(args.Name))
        {
        }

        if (args.Name.ToLower() == deviceName.ToLower())
        {
            deviceInformation = args;

            // Once device found Stop the watcher.
            deviceWatcher.Stop();
        }
    }

    private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
    {
    }

    private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
    {
    }

    private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
    {
    }

    private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
    {
        enumerationCompleted = true;

        // Once device found Stop the watcher.
        deviceWatcher.Stop();
    }
    #endregion
}