using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TechPortWinUI.Models;

namespace TechPortWinUI.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    #region Private members
    public DeskViewModel _deskViewModel { get; set; }
    #endregion

    #region Default constructor
    public MainViewModel() { }
    #endregion
}