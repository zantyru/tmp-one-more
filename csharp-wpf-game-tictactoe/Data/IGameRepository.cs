namespace Data
{
    /// <summary>
    /// Абстракция репозитория игровых сессий.
    /// Реализация может быть заменена на любую другую СУБД.
    /// </summary>
    public interface IGameRepository
    {
        Task InitializeAsync(CancellationToken ct = default);
        Task<long> CreateSessionAsync(GameMode mode, CancellationToken ct = default);
        Task SaveMoveAsync(long sessionId, int moveNumber, int row, int col, string player, CancellationToken ct = default);
        Task FinishSessionAsync(long sessionId, string? winner, bool isDraw, CancellationToken ct = default);
        Task<IReadOnlyList<GameSession>> GetSessionsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<GameMove>> GetMovesAsync(long sessionId, CancellationToken ct = default);
    }
}
