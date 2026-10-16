namespace Core
{
    /// <summary>
    /// Все константы игрового ядра.
    /// </summary>
    public static class GameConstants
    {
        public const int BoardSize = 3;
        public const int BoardCellCount = BoardSize * BoardSize;
        public const int EmptyCellValue = 0;
        public const int PlayerXValue = 1;
        public const int PlayerOValue = 2;

        public const string PlayerXSymbol = "X";
        public const string PlayerOSymbol = "O";
        public const string EmptySymbol = "";

        /// <summary>Длительность задержки ИИ для визуального эффекта (мс).</summary>
        public const int AiMoveDelayMs = 400;
    }
}
