namespace TicTacToe.Persistence.Abstractions;

/// <summary>
/// Хранилище игровых сессий. Сервисный слой зависит только от этого интерфейса,
/// поэтому замена SQLite на другую СУБД не затрагивает логику игры.
/// </summary>
public interface IGameSessionRepository
{
    /// <summary>Возвращает незавершённую сессию, если она есть.</summary>
    public Task<GameSessionSnapshot?> FindUnfinishedAsync(CancellationToken cancellationToken = default);

    public Task<GameSessionSnapshot?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    public Task InsertAsync(GameSessionSnapshot snapshot, CancellationToken cancellationToken = default);

    public Task UpdateAsync(GameSessionSnapshot snapshot, CancellationToken cancellationToken = default);

    /// <summary>Последние сессии (завершённые, прерванные и текущая) для экрана истории.</summary>
    public Task<IReadOnlyList<GameSessionSnapshot>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
}
