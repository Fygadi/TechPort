using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TechPortWinUI.Models;
using TechPortWinUI.ViewModels;
using Windows.Devices.Enumeration;

namespace TechPortWinUI.Views;

public sealed partial class MainPage : Page
{
    private DeskInstanceModel _deskInstance;
    private DispatcherTimer _MoveUptimer;
    private DispatcherTimer _MoveDowntimer;
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

        #region DeskCommand
        initialize();
        

        // Create and configure the timer MoveUp
        _MoveUptimer = new();
        _MoveUptimer.Interval = TimeSpan.FromMilliseconds(600); // Set the interval to 600 milliseconds
        _MoveUptimer.Tick += MoveUPTimerCallBack; // Set the event handler for the Tick event

        // Create and configure the timer (MoveDown)
        _MoveDowntimer = new();
        _MoveDowntimer.Interval = TimeSpan.FromMilliseconds(600); // Set the interval to 600 milliseconds
        _MoveDowntimer.Tick += MoveDownTimerCallBack; // Set the event handler for the Tick event
        #endregion

        DataContext = new MainViewModel();
    }

    private async void initialize()
    {
        _deskInstance = await DeskInstanceModel.CreateAsync("BluetoothLE#BluetoothLEe8:48:b8:c8:20:00-d1:b7:7f:95:5b:1d");
        //await _deskInstance.MoveToHeight(2500); //25cm

        //await _deskInstance.SetPreset(DeskInstance.Preset._1, 2500);
        //await _deskInstance.SetPreset(DeskInstance.Preset._2, 2500);
        //await _deskInstance.SetPreset(DeskInstance.Preset._3, 2500);
    }

    #region DeskCommand
    #region Move up
    private async void MoveUPTimerCallBack(object sender, object e)
    {
        await _deskInstance.MoveUpAsync();
    }

    private async void MoveUpButton_PointerCanceled(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _MoveUptimer.Stop();
        await _deskInstance.StopAsync();
    }

    private void MoveUpButton_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
       _MoveUptimer.Start();
    }

    private async void MoveUpButton_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _MoveUptimer.Stop();
        await _deskInstance.StopAsync();
    }
    #endregion
    #region Move Down
    private async void MoveDownTimerCallBack(object sender, object e)
    {
        await _deskInstance.MoveDownAsync();
    }

    private async void MoveDownButton_PointerCanceled(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _MoveDowntimer.Stop();
        await _deskInstance.StopAsync();
    }

    private void MoveDownButton_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _MoveDowntimer.Start();
    }

    private async void MoveDownButton_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        _MoveDowntimer.Stop();
        await _deskInstance.StopAsync();
    }
    #endregion
    #endregion

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
