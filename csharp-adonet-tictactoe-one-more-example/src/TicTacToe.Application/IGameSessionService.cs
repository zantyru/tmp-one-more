using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;

namespace TicTacToe.Application;

/// <summary>Сценарии работы с игровой сессией, которыми пользуются оба UI.</summary>
public interface IGameSessionService
{
    /// <summary>Текущая сессия в памяти (если она уже загружена или начата).</summary>
    public GameSession? Current { get; }

    /// <summary>Пытается восстановить незавершённую сессию. Возвращает null, если её нет.</summary>
    public Task<GameSession?> TryResumeAsync(CancellationToken cancellationToken = default);

    /// <summary>Начинает новую сессию, предварительно прерывая текущую незавершённую.</summary>
    public Task<GameSession> StartNewAsync(GameMode mode, CancellationToken cancellationToken = default);

    public Task<MoveOutcome> MakeHumanMoveAsync(int cellIndex, CancellationToken cancellationToken = default);

    public Task<MoveOutcome> MakeComputerMoveAsync(CancellationToken cancellationToken = default);

    public Task<MoveOutcome> AbortAsync(CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<GameSessionSnapshot>> GetHistoryAsync(int take, CancellationToken cancellationToken = default);
}
