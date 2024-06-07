using CommunityToolkit.Mvvm.ComponentModel;

namespace TechPortWinUI.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly DeskViewModel _deskViewModel = new ();
    public DeskViewModel DeskViewModel { get => _deskViewModel; }

    private readonly PresetsViewModel _presetsViewModel = new ();
    public PresetsViewModel PresetsViewModel { get => _presetsViewModel; }

    public MainViewModel() { }
}