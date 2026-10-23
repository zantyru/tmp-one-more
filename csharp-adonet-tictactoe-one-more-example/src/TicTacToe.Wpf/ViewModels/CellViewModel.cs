using TicTacToe.Wpf.Infrastructure;


namespace TicTacToe.Wpf.ViewModels;


public sealed class CellViewModel : ObservableObject
{
    private string _display = string.Empty;
    private bool _isEnabled;

    public CellViewModel(int index)
        => Index = index;

    public int Index { get; }

    public string Display
    {
        get => _display;

        set => Set(ref _display, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;

        set => Set(ref _isEnabled, value);
    }
}
