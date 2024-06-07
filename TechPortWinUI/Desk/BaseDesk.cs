using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace TechPortWinUI.Desk
{
    public abstract class BaseDesk : IDesk, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly BluetoothLEDevice _device;
        protected BluetoothLEDevice Device { get => _device; }

        private short _minHeight;
        public short MinHeight
        {
            get => _minHeight;
            set
            {
                _minHeight = value;
                OnPropertyChanged();
            }
        }

        private short _maxHeight;
        public short MaxHeight
        {
            get => _maxHeight;
            set
            {
                _maxHeight = value;
                OnPropertyChanged();
            }
        }

        private short _height;
        public short Height
        { 
            get => _height;
            protected set
            {
                _height = value;
                OnPropertyChanged();
            }
        }

        private short _speed;
        public short Speed
        {
            get => _speed;
            protected set
            {
                _speed = value;
                OnPropertyChanged();
            }
        }

        private MovementStatus _movementStatus;
        public MovementStatus MovementStatus
        {
            get => _movementStatus;
            protected set
            {
                _movementStatus = value;
                OnPropertyChanged();
            }
        }

        public BluetoothLEAppearance Appearance { get => Device.Appearance; }

        public ulong BluetoothAddress { get => Device.BluetoothAddress; }

        public BluetoothAddressType BluetoothAddressType { get => Device.BluetoothAddressType; }

        public BluetoothDeviceId BluetoothDeviceId { get => Device.BluetoothDeviceId; }
            
        public BluetoothConnectionStatus BluetoothConnectionStatus { get => Device.ConnectionStatus; }

        public DeviceAccessInformation DeviceAccessInformation { get => Device.DeviceAccessInformation; }

        public string DeviceId { get => Device.DeviceId; }

        public DeviceInformation DeviceInformation { get => Device.DeviceInformation; }

        public string Name { get => Device.Name; }

        public bool WasSecureConnectionUsedForPairing { get => Device.WasSecureConnectionUsedForPairing; }

        protected BaseDesk(
            BluetoothLEDevice device, short minHeight = short.MinValue, short maxHeight = short.MinValue)
        {
            _device = device;
            _minHeight = minHeight;
            _maxHeight = maxHeight;

            device.ConnectionStatusChanged += ConnectionStatusChanged;
            device.NameChanged += NameChanged;

            _ = OnObjectChange();
        }

        private async Task OnObjectChange()
        {
            await Task.Run(() => Thread.Sleep(4000));
        }

        #region Notification
        private void ConnectionStatusChanged(BluetoothLEDevice bluetoothLEDevice, object o)
        {
            OnPropertyChanged(nameof(ConnectionStatusChanged));
        }

        private void NameChanged(BluetoothLEDevice bluetoothLEDevice, object o)
        {
            OnPropertyChanged(nameof(BluetoothConnectionStatus));
        }
        #endregion

        private static Task<IDesk> CreateAsync(string deskId) => throw new NotImplementedException();
        
        public abstract Task<GattCommunicationStatus> MoveUpAsync();
        public abstract Task<GattCommunicationStatus> MoveDownAsync();
        public abstract void MoveToHeightAsync(short targetHeight);
        public abstract Task<GattCommunicationStatus> StopMovingAsync();

        public void UpdateProperty()
        {
            OnPropertyChanged(nameof(MinHeight));
            OnPropertyChanged(nameof(MaxHeight));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Speed));
            OnPropertyChanged(nameof(MovementStatus));
            OnPropertyChanged(nameof(Appearance));
            OnPropertyChanged(nameof(BluetoothAddress));
            OnPropertyChanged(nameof(BluetoothAddressType));
            OnPropertyChanged(nameof(BluetoothDeviceId));
            OnPropertyChanged(nameof(BluetoothConnectionStatus));
            OnPropertyChanged(nameof(DeviceAccessInformation));
            OnPropertyChanged(nameof(DeviceId));
            OnPropertyChanged(nameof(DeviceInformation));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(WasSecureConnectionUsedForPairing));
        }
    }
}
