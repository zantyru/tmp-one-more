using TicTacToe.Gameplay;
using Xunit;


namespace TicTacToe.Gameplay.Tests;


public sealed class SimpleComputerStrategyTests
{
    private static SimpleComputerStrategy CreateStrategy()
        => new(new Random(42));

    [Fact]
    public void ChooseMove_TakesWinningMove()
    {
        // X занимает 0 и 1 - победный ход в клетку 2.
        var board = Board.FromStateString("XX       ");
        var move = CreateStrategy().ChooseMove(board, Mark.X);

        Assert.Equal(2, move);
    }

    [Fact]
    public void ChooseMove_BlocksOpponentWinningMove()
    {
        // O вот-вот выиграет (0 и 1) - X обязан заблокировать клетку 2.
        var board = Board.FromStateString("OO       ");
        var move = CreateStrategy().ChooseMove(board, Mark.X);

        Assert.Equal(2, move);
    }

    [Fact]
    public void ChooseMove_PrefersOwnWinOverBlock()
    {
        // X может выиграть в клетке 2; O угрожает выигрышем в клетке 5.
        var board = Board.FromStateString("XX OO    ");
        var move = CreateStrategy().ChooseMove(board, Mark.X);

        Assert.Equal(2, move);
    }

    [Fact]
    public void ChooseMove_BlocksO_WhenNoOwnWin()
    {
        // У X нет выигрыша, у O - диагональ 0,4; блокируем клетку 8.
        var board = Board.FromStateString("OX  O    ");
        var move = CreateStrategy().ChooseMove(board, Mark.X);

        Assert.Equal(8, move);
    }

    [Fact]
    public void ChooseMove_FallsBackToEmptyCell()
    {
        var board = new Board();
        var move = CreateStrategy().ChooseMove(board, Mark.X);

        Assert.InRange(move, 0, Board.CellCount - 1);
        Assert.True(board.IsEmpty(move));
    }

    [Fact]
    public void ChooseMove_RejectsEmptyMark()
    {
        var board = new Board();

        Assert.Throws<ArgumentException>(() => CreateStrategy().ChooseMove(board, Mark.None));
    }

    [Fact]
    public void ChooseMove_ThrowsWhenBoardIsFull()
    {
        var board = Board.FromStateString("XOXXOOOXX");

        Assert.Throws<InvalidOperationException>(() => CreateStrategy().ChooseMove(board, Mark.X));
    }
}
