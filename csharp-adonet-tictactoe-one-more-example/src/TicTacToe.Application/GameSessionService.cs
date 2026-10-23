using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;


namespace TicTacToe.Application;


public sealed class GameSessionService : IGameSessionService, IDisposable
{
    private readonly IGameSessionRepository _repository;
    private readonly IComputerStrategy _strategy;
    private readonly TimeProvider _time;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private GameSession? _current;
    private bool _disposed;

    public GameSessionService(
        IGameSessionRepository repository,
        IComputerStrategy strategy,
        TimeProvider? time = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _time = time ?? TimeProvider.System;
    }

    public GameSession? Current
        => _current;

    public async Task<GameSession?> TryResumeAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            GameSession? result = _current;

            if (result is null || result.IsFinished)
            {
                var snapshot = await _repository.FindUnfinishedAsync(cancellationToken);

                if (snapshot is null)
                {
                    result = null;
                }
                else
                {
                    try
                    {
                        result = GameSession.Restore(snapshot);
                        _current = result;
                    }
                    catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
                    {
                        await _repository.UpdateAsync(
                            snapshot with {
                                Result = SessionResult.Aborted,
                                EndedAt = _time.GetUtcNow()
                            },
                            cancellationToken
                        );

                        result = null;
                    }
                }
            }

            return result;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<GameSession> StartNewAsync(GameMode mode, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var now = _time.GetUtcNow();

            if (_current is { IsFinished: false })
            {
                _current.Abort(now);
                await _repository.UpdateAsync(_current.ToSnapshot(), cancellationToken);
            }
            else
            {
                var unfinished = await _repository.FindUnfinishedAsync(cancellationToken);

                if (unfinished is not null)
                {
                    await _repository.UpdateAsync(
                        unfinished with {
                            Result = SessionResult.Aborted,
                            EndedAt = now
                        },
                        cancellationToken
                    );
                }
            }

            var session = GameSession.Start(mode, now);
            await _repository.InsertAsync(session.ToSnapshot(), cancellationToken);

            _current = session;

            return session;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<MoveOutcome> MakeHumanMoveAsync(int cellIndex, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var session = _current;

            if (session is null)
            {
                return MoveOutcome.Fail("Сессия не начата.");
            }

            if (session.IsComputerTurn)
            {
                return MoveOutcome.Fail("Сейчас ход компьютера.");
            }

            var outcome = session.ApplyMove(cellIndex, _time.GetUtcNow());

            if (outcome.Accepted)
            {
                await _repository.UpdateAsync(session.ToSnapshot(), cancellationToken);
            }

            return outcome;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<MoveOutcome> MakeComputerMoveAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var session = _current;

            if (session is null)
            {
                return MoveOutcome.Fail("Сессия не начата.");
            }

            if (!session.IsComputerTurn)
            {
                return MoveOutcome.Fail("Сейчас не ход компьютера.");
            }

            var cell = _strategy.ChooseMove(session.Board, session.CurrentMark);
            var outcome = session.ApplyMove(cell, _time.GetUtcNow());

            if (outcome.Accepted)
            {
                await _repository.UpdateAsync(session.ToSnapshot(), cancellationToken);
            }

            return outcome;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<MoveOutcome> AbortAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var session = _current;

            if (session is null)
            {
                return MoveOutcome.Fail("Сессия не начата.");
            }

            var outcome = session.Abort(_time.GetUtcNow());

            if (outcome.Accepted)
            {
                await _repository.UpdateAsync(session.ToSnapshot(), cancellationToken);
            }

            return outcome;
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task<IReadOnlyList<GameSessionSnapshot>> GetHistoryAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Количество записей должно быть положительным.");
        }

        return _repository.GetRecentAsync(take, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _gate.Dispose();
    }
}
