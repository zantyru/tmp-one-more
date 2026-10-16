using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Core;
using Data;


namespace UIWPF
{
    /// <summary>
    /// ViewModel главного окна.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly GameEngine _engine;
        private readonly IGameRepository _repository;
        private long _currentSessionId;
        private int _moveNumber;
        private string _statusText = string.Empty;
        private GameMode _selectedMode = GameMode.HumanVsComputer;
        private bool _isComputerTurn;

        public ObservableCollection<CellViewModel> Cells { get; } = new();
        public ObservableCollection<string> HistoryLog { get; } = new();

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public GameMode SelectedMode
        {
            get => _selectedMode;
            set { _selectedMode = value; OnPropertyChanged(); }
        }

        public ICommand NewGameCommand { get; }
        public ICommand CellClickCommand { get; }

        public MainViewModel(IGameRepository repository)
        {
            _repository = repository;
            _engine = new GameEngine(new SimpleComputerPlayer());

            for (int i = 0; i < GameConstants.BoardCellCount; i++)
            {
                int row = i / GameConstants.BoardSize;
                int col = i % GameConstants.BoardSize;
                Cells.Add(new CellViewModel(row, col));
            }

            NewGameCommand = new AsyncRelayCommand(StartNewGameAsync);
            CellClickCommand = new AsyncRelayCommand<CellViewModel>(OnCellClickAsync);

            StatusText = UiConstants.StatusXTurn;
        }

        private async Task StartNewGameAsync()
        {
            _engine.NewGame();
            _moveNumber = 0;
            HistoryLog.Clear();

            foreach (var cell in Cells)
                cell.State = CellState.Empty;

            _currentSessionId = await _repository.CreateSessionAsync(_selectedMode);
            HistoryLog.Add($"Сессия #{_currentSessionId} начата ({_selectedMode}).");

            StatusText = UiConstants.StatusXTurn;
            _isComputerTurn = false;

            // Если компьютер играет за X — сразу делаем ход
            if (_selectedMode == GameMode.HumanVsComputer && _engine.CurrentPlayer == CellState.O)
                await MakeComputerMoveAsync();
        }

        private async Task OnCellClickAsync(CellViewModel cell)
        {
            if (_engine.IsGameOver || _isComputerTurn) return;

            var result = _engine.MakeMove(cell.Row, cell.Col);
            if (!result.IsValid) return;

            cell.State = _engine.Board.GetCell(cell.Row, cell.Col);

            _moveNumber++;
            await _repository.SaveMoveAsync(
                _currentSessionId, _moveNumber, cell.Row, cell.Col,
                _engine.CurrentPlayer == CellState.X ? "X" : "O");

            HistoryLog.Add($"Ход #{_moveNumber}: {_engine.CurrentPlayer} -> ({cell.Row},{cell.Col})");

            await ProcessGameResultAsync(result);
        }

        private async Task MakeComputerMoveAsync()
        {
            _isComputerTurn = true;
            StatusText = UiConstants.StatusComputerTurn;

            var result = await _engine.MakeComputerMoveAsync();

            // Обновляем UI после хода компьютера
            var (row, col) = FindLastMove();
            var cell = Cells.First(c => c.Row == row && c.Col == col);
            cell.State = _engine.Board.GetCell(row, col);

            _moveNumber++;
            await _repository.SaveMoveAsync(
                _currentSessionId, _moveNumber, row, col, "O");

            HistoryLog.Add($"Ход #{_moveNumber}: O (компьютер) -> ({row},{col})");

            _isComputerTurn = false;
            await ProcessGameResultAsync(result);
        }

        private async Task ProcessGameResultAsync(MoveResult result)
        {
            if (result.Winner != CellState.Empty)
            {
                string winnerSymbol = result.Winner == CellState.X
                    ? GameConstants.PlayerXSymbol
                    : GameConstants.PlayerOSymbol;
                StatusText = string.Format(UiConstants.StatusWinFormat, winnerSymbol);
                await _repository.FinishSessionAsync(_currentSessionId, winnerSymbol, false);
                HistoryLog.Add($"Игра завершена. Победитель: {winnerSymbol}");
            }
            else if (result.IsDraw)
            {
                StatusText = UiConstants.StatusDraw;
                await _repository.FinishSessionAsync(_currentSessionId, null, true);
                HistoryLog.Add("Игра завершена. Ничья.");
            }
            else
            {
                StatusText = _engine.CurrentPlayer == CellState.X
                    ? UiConstants.StatusXTurn
                    : UiConstants.StatusOTurn;

                // Ход компьютера
                if (_selectedMode == GameMode.HumanVsComputer && _engine.CurrentPlayer == CellState.O)
                    await MakeComputerMoveAsync();
            }
        }

        private (int Row, int Col) FindLastMove()
        {
            for (int r = 0; r < GameConstants.BoardSize; r++)
                for (int c = 0; c < GameConstants.BoardSize; c++)
                {
                    if (_engine.Board.GetCell(r, c) != CellState.Empty)
                    {
                        // Ищем последнюю изменённую клетку — упрощённо берём последнюю ненустую
                    }
                }
            // Возвращаем последнюю ненустую клетку
            (int, int) last = (-1, -1);
            for (int r = 0; r < GameConstants.BoardSize; r++)
                for (int c = 0; c < GameConstants.BoardSize; c++)
                    if (_engine.Board.GetCell(r, c) != CellState.Empty)
                        last = (r, c);
            return last;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// ViewModel одной клетки.
    /// </summary>
    public class CellViewModel : INotifyPropertyChanged
    {
        private CellState _state;

        public int Row { get; }
        public int Col { get; }

        public CellState State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        public CellViewModel(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
