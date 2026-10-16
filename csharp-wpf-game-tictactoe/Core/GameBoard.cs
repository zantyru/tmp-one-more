namespace Core
{
    /// <summary>
    /// Игровое поле 3×3. Не зависит от БД и UI.
    /// </summary>
    public class GameBoard
    {
        private readonly CellState[,] _cells;

        public GameBoard()
        {
            _cells = new CellState[GameConstants.BoardSize, GameConstants.BoardSize];
            Reset();
        }

        /// <summary>Сбросить поле в начальное состояние.</summary>
        public void Reset()
        {
            for (int row = 0; row < GameConstants.BoardSize; row++)
                for (int col = 0; col < GameConstants.BoardSize; col++)
                    _cells[row, col] = CellState.Empty;
        }

        /// <summary>Получить состояние клетки.</summary>
        public CellState GetCell(int row, int col)
        {
            ValidateCoordinates(row, col);
            return _cells[row, col];
        }

        /// <summary>Установить состояние клетки. Возвращает false, если клетка занята.</summary>
        public bool SetCell(int row, int col, CellState state)
        {
            ValidateCoordinates(row, col);
            if (_cells[row, col] != CellState.Empty)
                return false;

            _cells[row, col] = state;
            return true;
        }

        /// <summary>Проверить, заполнено ли поле полностью.</summary>
        public bool IsFull()
        {
            for (int row = 0; row < GameConstants.BoardSize; row++)
                for (int col = 0; col < GameConstants.BoardSize; col++)
                    if (_cells[row, col] == CellState.Empty)
                        return false;
            return true;
        }

        /// <summary>Получить все пустые клетки в виде списка (row, col).</summary>
        public IReadOnlyList<(int Row, int Col)> GetEmptyCells()
        {
            var result = new List<(int, int)>();
            for (int row = 0; row < GameConstants.BoardSize; row++)
                for (int col = 0; col < GameConstants.BoardSize; col++)
                    if (_cells[row, col] == CellState.Empty)
                        result.Add((row, col));
            return result;
        }

        /// <summary>Проверить, выиграл ли указанный игрок.</summary>
        public bool IsWinner(CellState player)
        {
            if (player == CellState.Empty) return false;

            // Строки
            for (int row = 0; row < GameConstants.BoardSize; row++)
            {
                bool allMatch = true;
                for (int col = 0; col < GameConstants.BoardSize; col++)
                    if (_cells[row, col] != player) { allMatch = false; break; }
                if (allMatch) return true;
            }

            // Столбцы
            for (int col = 0; col < GameConstants.BoardSize; col++)
            {
                bool allMatch = true;
                for (int row = 0; row < GameConstants.BoardSize; row++)
                    if (_cells[row, col] != player) { allMatch = false; break; }
                if (allMatch) return true;
            }

            // Главная диагональ
            bool diag1 = true;
            for (int i = 0; i < GameConstants.BoardSize; i++)
                if (_cells[i, i] != player) { diag1 = false; break; }

            // Побочная диагональ
            bool diag2 = true;
            for (int i = 0; i < GameConstants.BoardSize; i++)
                if (_cells[i, GameConstants.BoardSize - 1 - i] != player) { diag2 = false; break; }

            return diag1 || diag2;
        }

        private static void ValidateCoordinates(int row, int col)
        {
            if (row < 0 || row >= GameConstants.BoardSize)
                throw new ArgumentOutOfRangeException(nameof(row));

            if (col < 0 || col >= GameConstants.BoardSize)
                throw new ArgumentOutOfRangeException(nameof(col));
        }
    }
}
