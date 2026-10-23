namespace TicTacToe.Gameplay;

/// <summary>Метка клетки: пусто, крестик или нолик.</summary>
public enum Mark
{
    None = 0,
    X = 1,
    O = 2
}

public static class MarkExtensions
{
    /// <summary>Противоположная метка. Для <see cref="Mark.None"/> возвращает <see cref="Mark.None"/>.</summary>
    public static Mark Opponent(this Mark mark)
        => mark switch
        {
            Mark.X => Mark.O,
            Mark.O => Mark.X,
            _ => Mark.None
        };

    public static char ToChar(this Mark mark)
        => mark switch
        {
            Mark.X => 'X',
            Mark.O => 'O',
            _ => ' '
        };

    public static Mark FromChar(char value)
        => value switch
        {
            'X' or 'x' => Mark.X,
            'O' or 'o' => Mark.O,
            _ => Mark.None
        };
}
