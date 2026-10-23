using TicTacToe.Gameplay;


namespace TicTacToe.Persistence.Abstractions;


public sealed record GameSessionSnapshot(
    Guid Id,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    SessionResult Result,
    GameMode Mode,
    string BoardState,
    Mark CurrentMark,
    int MovesCount
);
