using Microsoft.UI.Xaml.Data;
using Windows.Devices.Bluetooth;

namespace TechPortWinUI.Converter
{
    internal class BluetoothConnectionStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BluetoothConnectionStatus.Connected)
                return "Connected";
            else
                return "Disconnected";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
