namespace Core
{
    /// <summary>
    /// Состояние одной клетки на игровом поле.
    /// </summary>
    public enum CellState
    {
        Empty = GameConstants.EmptyCellValue,
        X = GameConstants.PlayerXValue,
        O = GameConstants.PlayerOValue
    }
}
