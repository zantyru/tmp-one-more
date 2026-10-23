namespace TicTacToe.Gameplay;

/// <summary>
/// Стратегия выбора хода компьютерным игроком.
/// Реализация не знает, кто её вызвал — сервис сессии, консоль или тест.
/// </summary>
public interface IComputerStrategy
{
    /// <summary>Выбирает индекс свободной клетки для хода указанной меткой.</summary>
    public int ChooseMove(Board board, Mark mark);
}
