using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;


namespace TicTacToe.Application.Tests;


public sealed class FakeGameSessionRepository : IGameSessionRepository
{
    private readonly List<GameSessionSnapshot> _items = new();

    public IReadOnlyList<GameSessionSnapshot> Items
        => _items;

    public Task<GameSessionSnapshot?> FindUnfinishedAsync(CancellationToken cancellationToken = default)
    {
        var unfinished =
            _items
            .Where(item => item.Result == SessionResult.InProgress)
            .OrderByDescending(item => item.StartedAt)
            .FirstOrDefault();

        return Task.FromResult(unfinished);
    }

    public Task<GameSessionSnapshot?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.FirstOrDefault(item => item.Id == id));

    public Task InsertAsync(GameSessionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        if (_items.Any(item => item.Id == snapshot.Id))
        {
            throw new InvalidOperationException("Сессия с таким Id уже существует.");
        }

        _items.Add(snapshot);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(GameSessionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var index = _items.FindIndex(item => item.Id == snapshot.Id);

        if (index < 0)
        {
            throw new InvalidOperationException("Сессия не найдена.");
        }

        _items[index] = snapshot;

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<GameSessionSnapshot>> GetRecentAsync(
        int take,
        CancellationToken cancellationToken = default
    )
        => Task.FromResult<IReadOnlyList<GameSessionSnapshot>>(
            _items.OrderByDescending(item => item.StartedAt).Take(take).ToList()
        );
}
