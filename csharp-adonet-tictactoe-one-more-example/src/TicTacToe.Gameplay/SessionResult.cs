namespace TicTacToe.Gameplay;

/// <summary>
/// Чем закончилась игровая сессия. Одно перечисление вместо пары «статус + победитель»:
/// меньше комбинаций, которые могут противоречить друг другу.
/// </summary>
public enum SessionResult
{
    InProgress = 0,
    XWins = 1,
    OWins = 2,
    Draw = 3,
    Aborted = 4
}
