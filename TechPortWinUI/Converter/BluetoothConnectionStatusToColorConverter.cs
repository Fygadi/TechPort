using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.Devices.Bluetooth;
using Windows.UI;

namespace TechPortWinUI.Converter
{
    internal class BluetoothConnectionStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {


            SolidColorBrush solidBrushColor = new()
            {
                Color = Color.FromArgb(255, 128, 27, 28)
            };

            if (value is BluetoothConnectionStatus.Connected)
                solidBrushColor.Color = Color.FromArgb(255, 108, 203, 95);

            return solidBrushColor;
            //Red "#801b1c"
            //Green "#6ccb5f"
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
