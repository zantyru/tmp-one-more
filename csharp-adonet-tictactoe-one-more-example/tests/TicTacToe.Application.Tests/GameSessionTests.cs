using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;
using Xunit;


namespace TicTacToe.Application.Tests;


public sealed class GameSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ApplyMove_AlternatesMarks()
    {
        var session = GameSession.Start(GameMode.HumanVsHuman, Now);

        Assert.True(session.ApplyMove(0, Now).Accepted);
        Assert.Equal(Mark.O, session.CurrentMark);
        Assert.True(session.ApplyMove(1, Now).Accepted);
        Assert.Equal(Mark.X, session.CurrentMark);
    }

    [Fact]
    public void ApplyMove_FinishingMove_SetsEndedAt()
    {
        var session = GameSession.Start(GameMode.HumanVsHuman, Now);

        session.ApplyMove(0, Now); // X
        session.ApplyMove(3, Now); // O
        session.ApplyMove(1, Now); // X
        session.ApplyMove(4, Now); // O

        var outcome = session.ApplyMove(2, Now); // X выигрывает

        Assert.True(outcome.Accepted);
        Assert.Equal(SessionResult.XWins, session.Result);
        Assert.True(session.IsFinished);
        Assert.Equal(Now, session.EndedAt);
    }

    [Fact]
    public void ApplyMove_AfterFinish_IsRejected()
    {
        var session = GameSession.Start(GameMode.HumanVsHuman, Now);

        session.ApplyMove(0, Now);
        session.ApplyMove(3, Now);
        session.ApplyMove(1, Now);
        session.ApplyMove(4, Now);
        session.ApplyMove(2, Now);

        var outcome = session.ApplyMove(8, Now);

        Assert.False(outcome.Accepted);
    }

    [Fact]
    public void Abort_OnlyForInProgressSession()
    {
        var session = GameSession.Start(GameMode.HumanVsHuman, Now);

        Assert.True(session.Abort(Now).Accepted);
        Assert.False(session.Abort(Now).Accepted);
        Assert.Equal(SessionResult.Aborted, session.Result);
    }

    [Fact]
    public void Snapshot_RoundTripsThroughRestore()
    {
        var session = GameSession.Start(GameMode.HumanVsComputer, Now);

        session.ApplyMove(0, Now);
        session.ApplyMove(4, Now);

        var restored = GameSession.Restore(session.ToSnapshot());

        Assert.Equal(session.Id, restored.Id);
        Assert.Equal(session.Mode, restored.Mode);
        Assert.Equal(session.CurrentMark, restored.CurrentMark);
        Assert.Equal(session.Board.ToStateString(), restored.Board.ToStateString());
        Assert.Equal(session.MovesCount, restored.MovesCount);
    }

    [Fact]
    public void Restore_RejectsSnapshotWithMismatchedMovesCount()
    {
        var snapshot = new GameSessionSnapshot(
            Id: Guid.NewGuid(),
            StartedAt: Now,
            EndedAt: null,
            Result: SessionResult.InProgress,
            Mode: GameMode.HumanVsHuman,
            BoardState: "X        ",
            CurrentMark: Mark.O,
            MovesCount: 3
        );

        Assert.Throws<InvalidOperationException>(() => GameSession.Restore(snapshot));
    }

    [Fact]
    public void IsComputerTurn_RespectsModeAndMark()
    {
        var humanVsComputer = GameSession.Start(GameMode.HumanVsComputer, Now);

        Assert.False(humanVsComputer.IsComputerTurn); // X - человек
        humanVsComputer.ApplyMove(0, Now);
        Assert.True(humanVsComputer.IsComputerTurn);  // O - компьютер

        var computerVsComputer = GameSession.Start(GameMode.ComputerVsComputer, Now);

        Assert.True(computerVsComputer.IsComputerTurn);
    }
}
