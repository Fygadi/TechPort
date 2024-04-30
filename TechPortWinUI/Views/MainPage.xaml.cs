using System.Diagnostics;
using System.Drawing;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using TechPortWinUI.ViewModels;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Storage.Pickers;
using Windows.UI.WindowManagement;
using AppWindow = Windows.UI.WindowManagement.AppWindow;

namespace TechPortWinUI.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    { get; }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        //Custumise TitleBar
        //Extends content into titleBar
        Window window = App.MainWindow;
        window.ExtendsContentIntoTitleBar = true;

        window.SetIsMaximizable(false);
        window.SetIsResizable(false);
        window.SetTitleBarBackgroundColors(Windows.UI.Color.FromArgb(255,033,043,130));
    }

    private void Button_Bluetooth_Click(object sender, RoutedEventArgs e)
    {
        test();
    }

    private void test()
    {
        // Create a new instance of your window
        var newWindow = new WindowEx();

        // Create a content for the window
        var textBlock = new TextBlock();
        textBlock.Text = "Hello, WinUI 3 Window!";
        newWindow.Content = textBlock;

        // Set the size and title of the window
        newWindow.Width = 400;
        newWindow.Height = 200;
        newWindow.Title = "New WinUI 3 Window";

        // Show the window
        newWindow.Activate();
    }

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
