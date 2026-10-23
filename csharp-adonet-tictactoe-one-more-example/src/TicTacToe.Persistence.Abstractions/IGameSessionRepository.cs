namespace TicTacToe.Persistence.Abstractions;


public interface IGameSessionRepository
{
    public Task<GameSessionSnapshot?> FindUnfinishedAsync(CancellationToken cancellationToken = default);

    public Task<GameSessionSnapshot?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    public Task InsertAsync(GameSessionSnapshot snapshot, CancellationToken cancellationToken = default);

    public Task UpdateAsync(GameSessionSnapshot snapshot, CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<GameSessionSnapshot>> GetRecentAsync(int take, CancellationToken cancellationToken = default);
}
