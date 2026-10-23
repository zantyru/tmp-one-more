namespace TicTacToe.Gameplay;

/// <summary>
/// Простая стратегия компьютерного игрока (не минимакс):
/// 1) если своим ходом можно победить — сделать этот ход;
/// 2) иначе если противник может победить следующим ходом — заблокировать его клетку;
/// 3) иначе выбрать случайную свободную клетку.
/// </summary>
public sealed class SimpleComputerStrategy : IComputerStrategy
{
    private readonly Random _random;

    /// <param name="random">Источник случайности; в тестах удобно передавать <c>new Random(seed)</c>.</param>
    public SimpleComputerStrategy(Random? random = null)
        => _random = random ?? Random.Shared;

    public int ChooseMove(Board board, Mark mark)
    {
        ArgumentNullException.ThrowIfNull(board);

        if (mark == Mark.None)
        {
            throw new ArgumentException("Метка компьютера не может быть пустой.", nameof(mark));
        }

        var move = FindWinningMove(board, mark)
            ?? FindWinningMove(board, mark.Opponent())
            ?? FindRandomMove(board);

        return move;
    }

    private static int? FindWinningMove(Board board, Mark mark)
    {
        int? winningCell = null;

        foreach (var cell in board.EmptyCells())
        {
            var probe = board.Clone();
            probe.TryPlace(cell, mark);

            var result = probe.Evaluate();
            var isWinning = mark == Mark.X
                ? result == SessionResult.XWins
                : result == SessionResult.OWins;

            if (isWinning)
            {
                winningCell = cell;
                break;
            }
        }

        return winningCell;
    }

    private int FindRandomMove(Board board)
    {
        var empty = board.EmptyCells().ToArray();

        if (empty.Length == 0)
        {
            throw new InvalidOperationException("Нет свободных клеток: ход невозможен.");
        }

        return empty[_random.Next(empty.Length)];
    }
}
