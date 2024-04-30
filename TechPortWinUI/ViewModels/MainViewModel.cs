using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using TechPortWinUI.Commands;
using TechPortWinUI.Views;

namespace TechPortWinUI.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    public ICommand ShowWindowCommand
    {
        get; set;
    }

    public MainViewModel()
    {
        ShowWindowCommand = new RelayCommand(ShowWindow, CanShowWindow);
    }

    private bool CanShowWindow(object obj)
    {
        return true;
    }

    private void ShowWindow(object obj)
    {
    }
}
