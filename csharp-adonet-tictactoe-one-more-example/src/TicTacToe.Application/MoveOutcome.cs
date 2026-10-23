namespace TicTacToe.Application;


public readonly record struct MoveOutcome(bool Accepted, string? Error)
{
    public static MoveOutcome Ok()
        => new(true, null);

    public static MoveOutcome Fail(string error)
        => new(false, error);
}
