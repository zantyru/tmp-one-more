using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;


namespace TicTacToe.Application;


public interface IGameSessionService
{
    public GameSession? Current { get; }

    public Task<GameSession?> TryResumeAsync(CancellationToken cancellationToken = default);

    public Task<GameSession> StartNewAsync(GameMode mode, CancellationToken cancellationToken = default);

    public Task<MoveOutcome> MakeHumanMoveAsync(int cellIndex, CancellationToken cancellationToken = default);

    public Task<MoveOutcome> MakeComputerMoveAsync(CancellationToken cancellationToken = default);

    public Task<MoveOutcome> AbortAsync(CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<GameSessionSnapshot>> GetHistoryAsync(int take, CancellationToken cancellationToken = default);
}
