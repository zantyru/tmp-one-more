namespace Data
{
    /// <summary>
    /// Режим игры.
    /// </summary>
    public enum GameMode
    {
        HumanVsHuman,
        HumanVsComputer
    }

    /// <summary>
    /// DTO: сессия игры.
    /// </summary>
    public class GameSession
    {
        public long Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public GameMode GameMode { get; set; }
        public string? Winner { get; set; }
        public bool IsDraw { get; set; }
    }

    /// <summary>
    /// DTO: ход.
    /// </summary>
    public class GameMove
    {
        public long Id { get; set; }
        public long SessionId { get; set; }
        public int MoveNumber { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public string Player { get; set; } = string.Empty;
        public DateTime MadeAt { get; set; }
    }
}
