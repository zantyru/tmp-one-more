using TicTacToe.Gameplay;

namespace TicTacToe.Persistence.Abstractions;

/// <summary>
/// Снимок игровой сессии для хранения. Состоит только из примитивов и доменных перечислений,
/// поэтому адаптер СУБД может менять схему, не затрагивая сервисный слой.
/// </summary>
/// <param name="BoardState">Девять символов: 'X', 'O' или пробел.</param>
public sealed record GameSessionSnapshot(
    Guid Id,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    SessionResult Result,
    GameMode Mode,
    string BoardState,
    Mark CurrentMark,
    int MovesCount);
