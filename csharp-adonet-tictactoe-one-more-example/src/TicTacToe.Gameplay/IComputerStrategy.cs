namespace TicTacToe.Gameplay;


public interface IComputerStrategy
{
    public int ChooseMove(Board board, Mark mark);
}
