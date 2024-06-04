using CommunityToolkit.Mvvm.ComponentModel;

namespace TechPortWinUI.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    #region Fields
    private readonly PresetsViewModel _presetsViewModel = new ();
    private readonly DeskViewModel _deskViewModel = new ();
    #endregion

    #region Property
    public PresetsViewModel PresetsViewModel { get => _presetsViewModel; }
    public DeskViewModel DeskViewModel { get => _deskViewModel; }
    #endregion


    #region Default constructor
    public MainViewModel() { }
    #endregion
}