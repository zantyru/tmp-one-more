namespace Core
{
    /// <summary>
    /// Результат хода.
    /// </summary>
    public class MoveResult
    {
        public bool IsValid { get; init; }
        public CellState Winner { get; init; }
        public bool IsDraw { get; init; }
        public string? ErrorMessage { get; init; }

        public static MoveResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
        public static MoveResult Valid(CellState winner, bool isDraw) => new() { IsValid = true, Winner = winner, IsDraw = isDraw };
    }

    /// <summary>
    /// Движок игры «Крестики-нолики». Не знает о БД и WPF.
    /// </summary>
    public class GameEngine
    {
        private readonly GameBoard _board;
        private readonly IComputerPlayer _computerPlayer;

        public CellState CurrentPlayer { get; private set; }
        public GameBoard Board => _board;
        public bool IsGameOver { get; private set; }

        public GameEngine(IComputerPlayer computerPlayer)
        {
            _board = new GameBoard();
            _computerPlayer = computerPlayer;
            CurrentPlayer = CellState.X;
            IsGameOver = false;
        }

        /// <summary>Начать новую игру.</summary>
        public void NewGame()
        {
            _board.Reset();
            CurrentPlayer = CellState.X;
            IsGameOver = false;
        }

        /// <summary>Сделать ход игрока.</summary>
        public MoveResult MakeMove(int row, int col)
        {
            if (IsGameOver)
                return MoveResult.Invalid("Игра уже завершена.");

            if (!_board.SetCell(row, col, CurrentPlayer))
                return MoveResult.Invalid("Клетка уже занята.");

            return EvaluateAndAdvance();
        }

        /// <summary>Ход компьютера (асинхронно, с задержкой для UX).</summary>
        public async Task<MoveResult> MakeComputerMoveAsync(CancellationToken ct = default)
        {
            if (IsGameOver)
                return MoveResult.Invalid("Игра уже завершена.");

            await Task.Delay(GameConstants.AiMoveDelayMs, ct);

            var (row, col) = await _computerPlayer.ChooseMoveAsync(_board, CurrentPlayer, ct);
            if (!_board.SetCell(row, col, CurrentPlayer))
                return MoveResult.Invalid("Компьютер выбрал занятую клетку.");

            return EvaluateAndAdvance();
        }

        /// <summary>Проверить результат и передать ход.</summary>
        private MoveResult EvaluateAndAdvance()
        {
            if (_board.IsWinner(CurrentPlayer))
            {
                IsGameOver = true;
                return MoveResult.Valid(CurrentPlayer, false);
            }

            if (_board.IsFull())
            {
                IsGameOver = true;
                return MoveResult.Valid(CellState.Empty, true);
            }

            CurrentPlayer = CurrentPlayer == CellState.X ? CellState.O : CellState.X;
            return MoveResult.Valid(CellState.Empty, false);
        }
    }
}
