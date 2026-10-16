namespace Core
{
    /// <summary>
    /// Простой ИИ: завершает свою цепочку, иначе прерывает цепочку противника, иначе — случайный ход.
    /// Без эвристик и минимакса.
    /// </summary>
    public class SimpleComputerPlayer : IComputerPlayer
    {
        private static readonly Random Rng = new();

        public Task<(int Row, int Col)> ChooseMoveAsync(
            GameBoard board, CellState mySymbol, CancellationToken ct = default)
        {
            // 1. Попытка завершить свою выигрышную цепочку
            var winningMove = FindWinningMove(board, mySymbol);
            if (winningMove.HasValue)
                return Task.FromResult(winningMove.Value);

            // 2. Попытка прервать выигрышную цепочку противника
            var opponentSymbol = mySymbol == CellState.X ? CellState.O : CellState.X;
            var blockingMove = FindWinningMove(board, opponentSymbol);
            if (blockingMove.HasValue)
                return Task.FromResult(blockingMove.Value);

            // 3. Случайный ход
            var emptyCells = board.GetEmptyCells();
            var randomCell = emptyCells[Rng.Next(emptyCells.Count)];
            return Task.FromResult(randomCell);
        }

        /// <summary>
        /// Найти клетку, установка символа в которую приведёт к победе указанного игрока.
        /// </summary>
        private static (int Row, int Col)? FindWinningMove(GameBoard board, CellState symbol)
        {
            foreach (var (row, col) in board.GetEmptyCells())
            {
                // Симулируем ход
                var snapshot = TakeSnapshot(board);
                board.SetCell(row, col, symbol);
                bool isWin = board.IsWinner(symbol);
                RestoreSnapshot(board, snapshot);

                if (isWin)
                    return (row, col);
            }
            return null;
        }

        private static CellState[,] TakeSnapshot(GameBoard board)
        {
            var snapshot = new CellState[GameConstants.BoardSize, GameConstants.BoardSize];
            for (int r = 0; r < GameConstants.BoardSize; r++)
                for (int c = 0; c < GameConstants.BoardSize; c++)
                    snapshot[r, c] = board.GetCell(r, c);
            return snapshot;
        }

        private static void RestoreSnapshot(GameBoard board, CellState[,] snapshot)
        {
            for (int r = 0; r < GameConstants.BoardSize; r++)
                for (int c = 0; c < GameConstants.BoardSize; c++)
                {
                    if (board.GetCell(r, c) != snapshot[r, c])
                        board.SetCell(r, c, snapshot[r, c]);
                }
        }
    }
}
