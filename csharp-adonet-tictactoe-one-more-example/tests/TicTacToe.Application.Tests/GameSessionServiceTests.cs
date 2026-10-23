using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;
using Xunit;


namespace TicTacToe.Application.Tests;


public sealed class GameSessionServiceTests
{
    private static (GameSessionService Service, FakeGameSessionRepository Repository, FixedTimeProvider Time) Create()
    {
        var repository = new FakeGameSessionRepository();
        var time = new FixedTimeProvider(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var service = new GameSessionService(repository, new SimpleComputerStrategy(new Random(1)), time);

        return (service, repository, time);
    }

    [Fact]
    public async Task StartNewAsync_InsertsInProgressSession()
    {
        var (service, repository, _) = Create();
        var session = await service.StartNewAsync(GameMode.HumanVsHuman);
        var stored = Assert.Single(repository.Items);

        Assert.Equal(session.Id, stored.Id);
        Assert.Equal(SessionResult.InProgress, stored.Result);
        Assert.Equal(Mark.X, stored.CurrentMark);
        Assert.Equal(new string(' ', Board.CellCount), stored.BoardState);
        Assert.Null(stored.EndedAt);
    }

    [Fact]
    public async Task TryResumeAsync_ReturnsUnfinishedSession()
    {
        var (service, _, _) = Create();
        var started = await service.StartNewAsync(GameMode.HumanVsHuman);
        await service.MakeHumanMoveAsync(4);

        var resumed = await service.TryResumeAsync();

        Assert.NotNull(resumed);
        Assert.Equal(started.Id, resumed!.Id);
        Assert.Equal(Mark.X, resumed.Board[4]);
        Assert.Equal(Mark.O, resumed.CurrentMark);
    }

    [Fact]
    public async Task TryResumeAsync_AbortsCorruptSnapshot()
    {
        var (service, repository, _) = Create();

        await repository.InsertAsync(new GameSessionSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            null,
            SessionResult.InProgress,
            GameMode.HumanVsHuman,
            "XXXXXXXXX",
            Mark.X,
            Board.CellCount)
        );

        var resumed = await service.TryResumeAsync();

        Assert.Null(resumed);
        Assert.Equal(SessionResult.Aborted, repository.Items[0].Result);
        Assert.NotNull(repository.Items[0].EndedAt);
    }

    [Fact]
    public async Task MakeHumanMoveAsync_PersistsBoardAfterEachMove()
    {
        var (service, repository, _) = Create();
        await service.StartNewAsync(GameMode.HumanVsHuman);

        var outcome = await service.MakeHumanMoveAsync(4);

        Assert.True(outcome.Accepted);

        var stored = Assert.Single(repository.Items);

        Assert.Equal('X', stored.BoardState[4]);
        Assert.Equal(Mark.O, stored.CurrentMark);
        Assert.Equal(1, stored.MovesCount);
    }

    [Fact]
    public async Task MakeHumanMoveAsync_RejectsOccupiedCell()
    {
        var (service, _, _) = Create();

        await service.StartNewAsync(GameMode.HumanVsHuman);
        await service.MakeHumanMoveAsync(0);

        var outcome = await service.MakeHumanMoveAsync(0);

        Assert.False(outcome.Accepted);
        Assert.NotNull(outcome.Error);
    }

    [Fact]
    public async Task MakeHumanMoveAsync_RejectsMoveWhenComputerTurn()
    {
        var (service, _, _) = Create();

        await service.StartNewAsync(GameMode.HumanVsComputer);
        await service.MakeHumanMoveAsync(0);

        var outcome = await service.MakeHumanMoveAsync(1);

        Assert.False(outcome.Accepted);
    }

    [Fact]
    public async Task MakeComputerMoveAsync_PlacesMarkWhenComputerTurn()
    {
        var (service, repository, _) = Create();

        await service.StartNewAsync(GameMode.HumanVsComputer);
        await service.MakeHumanMoveAsync(0);

        var outcome = await service.MakeComputerMoveAsync();

        Assert.True(outcome.Accepted);

        var stored = Assert.Single(repository.Items);

        Assert.Equal(1, stored.BoardState.Count(c => c == 'O'));
        Assert.Equal(2, stored.BoardState.Count(c => c != ' '));
    }

    [Fact]
    public async Task AbortAsync_MarksSessionAborted()
    {
        var (service, repository, _) = Create();

        await service.StartNewAsync(GameMode.HumanVsHuman);

        var outcome = await service.AbortAsync();

        Assert.True(outcome.Accepted);
        Assert.Equal(SessionResult.Aborted, repository.Items[0].Result);
        Assert.NotNull(repository.Items[0].EndedAt);
    }

    [Fact]
    public async Task StartNewAsync_AbortsPreviousUnfinishedSession()
    {
        var (service, repository, _) = Create();

        await service.StartNewAsync(GameMode.HumanVsHuman);
        await service.StartNewAsync(GameMode.HumanVsComputer);

        Assert.Equal(2, repository.Items.Count);
        Assert.Equal(SessionResult.Aborted, repository.Items[0].Result);
        Assert.Equal(SessionResult.InProgress, repository.Items[1].Result);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsNewestFirst()
    {
        var (service, _, time) = Create();

        await service.StartNewAsync(GameMode.HumanVsHuman);
        await service.AbortAsync();

        time.Advance(TimeSpan.FromMinutes(1));
        await service.StartNewAsync(GameMode.HumanVsComputer);

        var history = await service.GetHistoryAsync(10);

        Assert.Equal(2, history.Count);
        Assert.True(history[0].StartedAt >= history[1].StartedAt);
    }
}
