namespace TicTacToe.Application;

/// <summary>Результат попытки хода или команды: принято ли действие и почему нет.</summary>
public readonly record struct MoveOutcome(bool Accepted, string? Error)
{
    public static MoveOutcome Ok()
        => new(true, null);

    public static MoveOutcome Fail(string error)
        => new(false, error);
}
