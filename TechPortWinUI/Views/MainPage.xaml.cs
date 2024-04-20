using Microsoft.UI.Xaml.Controls;

using TechPortWinUI.ViewModels;

namespace TechPortWinUI.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
