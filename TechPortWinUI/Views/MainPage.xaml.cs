using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using TechPortWinUI.ViewModels;
using Windows.Devices.Enumeration;

namespace TechPortWinUI.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        //Custumise TitleBar
        var window = App.MainWindow;
        window.ExtendsContentIntoTitleBar = true;
        window.IsMaximizable = false;
        window.IsResizable = false;

        App.MainWindow.SetTitleBarBackgroundColors(Windows.UI.Color.FromArgb(255,033,043,130));

        DataContext = ViewModel;
    }

    //Show a prompt to select a bluetooth LE device
    private async Task DevicePickerExecutionAsync()
    {
        var devicePicker = new DevicePicker();

        // Get the current window's HWND by passing in the Window object
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(devicePicker, hwnd);

        var deviceFilter = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"";
        devicePicker.Filter.SupportedDeviceSelectors.Add(deviceFilter);

        // Define the DeviceInformationKind
        var deviceKind = DeviceInformationKind.AssociationEndpoint;

        await devicePicker.PickSingleDeviceAsync(new Windows.Foundation.Rect(400, 400, 800, 600));
    }
}
