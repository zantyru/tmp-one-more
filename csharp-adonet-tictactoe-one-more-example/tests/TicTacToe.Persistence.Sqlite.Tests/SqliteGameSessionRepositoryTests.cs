using Microsoft.Data.Sqlite;
using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;
using Xunit;


namespace TicTacToe.Persistence.Sqlite.Tests;


public sealed class SqliteGameSessionRepositoryTests
{
    private static GameSessionSnapshot NewSnapshot(
        DateTimeOffset? startedAt = null,
        SessionResult result = SessionResult.InProgress,
        DateTimeOffset? endedAt = null,
        string boardState = "         ",
        Mark currentMark = Mark.X,
        int movesCount = 0,
        Guid? id = null
    )
        => new(
            id ?? Guid.NewGuid(),
            startedAt ?? new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            endedAt,
            result,
            GameMode.HumanVsHuman,
            boardState,
            currentMark,
            movesCount
        );

    [Fact]
    public async Task InsertAndFindUnfinished_RoundTripsAllFields()
    {
        using var database = new SqliteTestDatabase();

        await database.InitializeAsync();

        var snapshot = NewSnapshot(boardState: "X        ", currentMark: Mark.O, movesCount: 1);

        await database.Repository.InsertAsync(snapshot);

        var found = await database.Repository.FindUnfinishedAsync();

        Assert.NotNull(found);
        Assert.Equal(snapshot.Id, found!.Id);
        Assert.Equal(snapshot.StartedAt, found.StartedAt);
        Assert.Equal(snapshot.BoardState, found.BoardState);
        Assert.Equal(snapshot.CurrentMark, found.CurrentMark);
        Assert.Equal(snapshot.MovesCount, found.MovesCount);
        Assert.Equal(snapshot.Mode, found.Mode);
        Assert.Null(found.EndedAt);
    }

    [Fact]
    public async Task EndedAt_NullRoundTrip()
    {
        using var database = new SqliteTestDatabase();

        await database.InitializeAsync();

        var finished = NewSnapshot(
            result: SessionResult.Aborted,
            endedAt: new DateTimeOffset(2026, 1, 1, 12, 30, 0, TimeSpan.Zero),
            boardState: "XO       ",
            currentMark: Mark.X,
            movesCount: 2
        );

        await database.Repository.InsertAsync(finished);

        var loaded = await database.Repository.GetAsync(finished.Id);

        Assert.NotNull(loaded);
        Assert.Equal(finished.EndedAt, loaded!.EndedAt);
        Assert.Equal(SessionResult.Aborted, loaded.Result);
    }

    [Fact]
    public async Task UniqueIndex_PreventsSecondInProgressSession()
    {
        using var database = new SqliteTestDatabase();

        await database.InitializeAsync();
        await database.Repository.InsertAsync(NewSnapshot());
        await Assert.ThrowsAsync<SqliteException>(
            () => database.Repository.InsertAsync(NewSnapshot())
        );
    }

    [Fact]
    public async Task Update_MissingRow_Throws()
    {
        using var database = new SqliteTestDatabase();

        await database.InitializeAsync();

        var missing = NewSnapshot();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => database.Repository.UpdateAsync(missing)
        );
    }

    [Fact]
    public async Task Update_PersistsEndedAtAndResult()
    {
        using var database = new SqliteTestDatabase();

        await database.InitializeAsync();

        var snapshot = NewSnapshot();

        await database.Repository.InsertAsync(snapshot);

        var endedAt = new DateTimeOffset(2026, 1, 1, 12, 45, 0, TimeSpan.Zero);

        await database.Repository.UpdateAsync(snapshot with
        {
            Result = SessionResult.XWins,
            EndedAt = endedAt,
            BoardState = "XXXOO    ",
            MovesCount = 5
        });

        var loaded = await database.Repository.GetAsync(snapshot.Id);

        Assert.NotNull(loaded);
        Assert.Equal(SessionResult.XWins, loaded!.Result);
        Assert.Equal(endedAt, loaded.EndedAt);
        Assert.Equal("XXXOO    ", loaded.BoardState);
        Assert.Equal(5, loaded.MovesCount);
    }

    [Fact]
    public async Task GetRecent_OrdersByStartedAtDescending()
    {
        using var database = new SqliteTestDatabase();

        await database.InitializeAsync();

        var baseTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        await database.Repository.InsertAsync(
            NewSnapshot(
                startedAt: baseTime,
                result: SessionResult.Draw,
                endedAt: baseTime.AddMinutes(5),
                boardState: "XOXXOOOXX",
                currentMark: Mark.X,
                movesCount: 9
            )
        );
        await database.Repository.InsertAsync(
            NewSnapshot(
                startedAt: baseTime.AddMinutes(10),
                result: SessionResult.Aborted,
                endedAt: baseTime.AddMinutes(11),
                boardState: "X        ",
                currentMark: Mark.O,
                movesCount: 1
            )
        );
        await database.Repository.InsertAsync(
            NewSnapshot(
                startedAt: baseTime.AddMinutes(20)
            )
        );

        var recent = await database.Repository.GetRecentAsync(10);

        Assert.Equal(3, recent.Count);
        Assert.Equal(baseTime.AddMinutes(20), recent[0].StartedAt);
        Assert.Equal(baseTime.AddMinutes(10), recent[1].StartedAt);
        Assert.Equal(baseTime, recent[2].StartedAt);
    }
}
