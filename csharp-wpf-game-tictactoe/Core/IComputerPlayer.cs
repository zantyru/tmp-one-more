namespace Core
{
    /// <summary>
    /// Интерфейс компьютерного игрока.
    /// </summary>
    public interface IComputerPlayer
    {
        Task<(int Row, int Col)> ChooseMoveAsync(GameBoard board, CellState mySymbol, CancellationToken ct = default);
    }
}
