using TicTacToe.Gameplay;
using Xunit;

namespace TicTacToe.Gameplay.Tests;

public sealed class BoardTests
{
    [Theory]
    [InlineData("XXX      ", SessionResult.XWins)]
    [InlineData("   XXX   ", SessionResult.XWins)]
    [InlineData("      XXX", SessionResult.XWins)]
    [InlineData("X  X  X  ", SessionResult.XWins)]
    [InlineData(" X  X  X ", SessionResult.XWins)]
    [InlineData("  X  X  X", SessionResult.XWins)]
    [InlineData("X   X   X", SessionResult.XWins)]
    [InlineData("  X X X  ", SessionResult.XWins)]
    [InlineData("OOO      ", SessionResult.OWins)]
    [InlineData("O   O   O", SessionResult.OWins)]
    public void Evaluate_DetectsWin(string state, SessionResult expected)
    {
        var board = Board.FromStateString(state);

        Assert.Equal(expected, board.Evaluate());
    }

    [Fact]
    public void Evaluate_DetectsDraw()
    {
        var board = Board.FromStateString("XOXXOOOXX");

        Assert.Equal(SessionResult.Draw, board.Evaluate());
    }

    [Fact]
    public void Evaluate_EmptyBoard_IsInProgress()
    {
        var board = new Board();

        Assert.Equal(SessionResult.InProgress, board.Evaluate());
    }

    [Fact]
    public void TryPlace_RejectsOccupiedCell()
    {
        var board = new Board();
        Assert.True(board.TryPlace(4, Mark.X));

        Assert.False(board.TryPlace(4, Mark.O));
        Assert.Equal(Mark.X, board[4]);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void TryPlace_RejectsIndexOutOfRange(int index)
    {
        var board = new Board();

        Assert.False(board.TryPlace(index, Mark.X));
    }

    [Fact]
    public void TryPlace_RejectsEmptyMark()
    {
        var board = new Board();

        Assert.False(board.TryPlace(0, Mark.None));
    }

    [Fact]
    public void StateString_RoundTrips()
    {
        const string state = "XO X  O X";

        var board = Board.FromStateString(state);

        Assert.Equal(state, board.ToStateString());
        Assert.Equal(5, board.MovesCount);
    }

    [Fact]
    public void FromStateString_RejectsWrongLength()
    {
        Assert.Throws<ArgumentException>(() => Board.FromStateString("XO"));
    }
}
