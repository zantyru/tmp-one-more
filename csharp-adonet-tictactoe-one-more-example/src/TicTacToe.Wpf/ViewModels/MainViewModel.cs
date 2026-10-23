using System.Collections.ObjectModel;
using TicTacToe.Application;
using TicTacToe.Gameplay;
using TicTacToe.Wpf.Infrastructure;


namespace TicTacToe.Wpf.ViewModels;


public sealed class MainViewModel : ObservableObject
{
    private readonly IGameSessionService _service;

    private ModeOption? _selectedMode;
    private string _statusText = "Загрузка…";
    private bool _isBusy;

    public MainViewModel(IGameSessionService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));

        Cells = new ObservableCollection<CellViewModel>(
            Enumerable
            .Range(0, Board.CellCount)
            .Select(index => new CellViewModel(index))
        );

        Modes = new[]
        {
            new ModeOption(GameMode.HumanVsHuman, "Человек против человека"),
            new ModeOption(GameMode.HumanVsComputer, "Человек против компьютера"),
            new ModeOption(GameMode.ComputerVsComputer, "Компьютер против компьютера")
        };

        _selectedMode = Modes[0];

        CellCommand = new AsyncRelayCommand(
            parameter => CellAsync(parameter is int index ? index : -1),
            _ => CanHumanMove()
        );

        NewGameCommand = new AsyncRelayCommand(
            _ => NewGameAsync(),
            _ => !IsBusy
        );

        AbortCommand = new AsyncRelayCommand(
            _ => AbortAsync(),
            _ => _service.Current is { IsFinished: false }
        );

        CellCommand.ExecutionFailed += OnCommandFailed;
        NewGameCommand.ExecutionFailed += OnCommandFailed;
        AbortCommand.ExecutionFailed += OnCommandFailed;
    }

    public ObservableCollection<CellViewModel> Cells { get; }

    public IReadOnlyList<ModeOption> Modes { get; }

    public ObservableCollection<string> History { get; } = new();

    public AsyncRelayCommand CellCommand { get; }

    public AsyncRelayCommand NewGameCommand { get; }

    public AsyncRelayCommand AbortCommand { get; }

    public ModeOption? SelectedMode
    {
        get => _selectedMode;

        set => Set(ref _selectedMode, value);
    }

    public string StatusText
    {
        get => _statusText;

        private set => Set(ref _statusText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;

        private set
        {
            if (Set(ref _isBusy, value))
            {
                RaiseCommands();
            }
        }
    }

    public async Task InitializeAsync()
    {
        var session = await _service.TryResumeAsync();

        if (session is null)
        {
            session = await _service.StartNewAsync(SelectedMode?.Mode ?? GameMode.HumanVsHuman);
        }
        else
        {
            var option = Modes.FirstOrDefault(mode => mode.Mode == session.Mode);

            if (option is not null)
            {
                SelectedMode = option;
            }
        }

        Sync();
        await RefreshHistoryAsync();

        _ = RunComputerMovesAsync();
    }

    private async Task CellAsync(int index)
    {
        if (index < 0)
        {
            return;
        }

        var outcome = await _service.MakeHumanMoveAsync(index);

        if (!outcome.Accepted)
        {
            StatusText = outcome.Error ?? "Ход не принят.";
        }
        else
        {
            Sync();
            await RunComputerMovesAsync();

            if (_service.Current is { IsFinished: true })
            {
                await RefreshHistoryAsync();
            }
        }
    }

    private async Task NewGameAsync()
    {
        await _service.StartNewAsync(SelectedMode?.Mode ?? GameMode.HumanVsHuman);
        Sync();
        await RefreshHistoryAsync();
        await RunComputerMovesAsync();
    }

    private async Task AbortAsync()
    {
        var outcome = await _service.AbortAsync();

        if (!outcome.Accepted)
        {
            StatusText = outcome.Error ?? "Не удалось прервать сессию.";
        }
        else
        {
            Sync();
            await RefreshHistoryAsync();
        }
    }

    private async Task RunComputerMovesAsync()
    {
        var shouldRun = _service.Current is { IsFinished: false, IsComputerTurn: true };

        if (!shouldRun)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Sync();

            while (_service.Current is { IsFinished: false, IsComputerTurn: true })
            {
                await Task.Delay(350);

                var outcome = await _service.MakeComputerMoveAsync();

                if (!outcome.Accepted)
                {
                    StatusText = outcome.Error ?? "Компьютер не смог сходить.";
                    break;
                }

                Sync();
            }
        }
        catch (Exception ex)
        {
            StatusText = "Ошибка: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
            Sync();
        }

        await RefreshHistoryAsync();
    }

    private void Sync()
    {
        var session = _service.Current;

        if (session is null)
        {
            StatusText = "Сессия не начата.";
            RaiseCommands();
        }
        else
        {
            var canPlay = !session.IsFinished && !session.IsComputerTurn && !IsBusy;

            for (var index = 0; index < Cells.Count; index++)
            {
                var mark = session.Board[index];
                Cells[index].Display = mark == Mark.None ? string.Empty : mark.ToChar().ToString();
                Cells[index].IsEnabled = canPlay && session.Board.IsEmpty(index);
            }

            StatusText = BuildStatus(session);
            RaiseCommands();
        }
    }

    private bool CanHumanMove()
    {
        var session = _service.Current;
        var result = !IsBusy && session is { IsFinished: false } && !session.IsComputerTurn;

        return result;
    }

    private async Task RefreshHistoryAsync()
    {
        var items = await _service.GetHistoryAsync(10);

        History.Clear();

        foreach (var item in items)
        {
            var ended = item.EndedAt?.ToString("HH:mm:ss") ?? "—";

            History.Add(
                $"{item.StartedAt:yyyy-MM-dd HH:mm} -> {ended} | " +
                $"{DescribeResult(item.Result)} | {DescribeMode(item.Mode)} | {item.MovesCount}"
            );
        }
    }

    private void RaiseCommands()
    {
        CellCommand.RaiseCanExecuteChanged();
        NewGameCommand.RaiseCanExecuteChanged();
        AbortCommand.RaiseCanExecuteChanged();
    }

    private void OnCommandFailed(Exception exception)
        => StatusText = "Ошибка: " + exception.Message;

    private static string BuildStatus(GameSession session)
    {
        string result;

        if (session.IsFinished)
        {
            result = $"{DescribeResult(session.Result)}. Ходов: {session.MovesCount}.";
        }
        else
        {
            var who = session.IsComputerTurn ? "компьютер" : "вы";

            result = $"Ход {session.CurrentMark.ToChar()} — {who}. " +
                     $"Ходов: {session.MovesCount}. Режим: {DescribeMode(session.Mode)}.";
        }

        return result;
    }

    private static string DescribeMode(GameMode mode)
        => mode switch
        {
            GameMode.HumanVsHuman => "человек против человека",
            GameMode.HumanVsComputer => "человек против компьютера",
            GameMode.ComputerVsComputer => "компьютер против компьютера",
            _ => mode.ToString()
        };

    private static string DescribeResult(SessionResult result)
        => result switch
        {
            SessionResult.InProgress => "в процессе",
            SessionResult.XWins => "победили крестики",
            SessionResult.OWins => "победили нолики",
            SessionResult.Draw => "ничья",
            SessionResult.Aborted => "прервана",
            _ => result.ToString()
        };
}
