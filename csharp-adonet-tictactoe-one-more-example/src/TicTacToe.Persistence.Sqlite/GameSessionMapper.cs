using System.Data.Common;
using System.Globalization;
using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;

namespace TicTacToe.Persistence.Sqlite;

/// <summary>Единственное место, где известны имена и индексы колонок.</summary>
internal static class GameSessionMapper
{
    public const string SelectColumns =
        "Id, StartedAt, EndedAt, Result, Mode, BoardState, CurrentMark, MovesCount";

    public static GameSessionSnapshot Map(DbDataReader reader)
        => new(
            Id: Guid.Parse(reader.GetString(0)),
            StartedAt: ParseDate(reader.GetString(1)),
            EndedAt: reader.IsDBNull(2) ? null : ParseDate(reader.GetString(2)),
            Result: (SessionResult)reader.GetInt32(3),
            Mode: (GameMode)reader.GetInt32(4),
            BoardState: reader.GetString(5),
            CurrentMark: MarkExtensions.FromChar(reader.GetString(6)[0]),
            MovesCount: reader.GetInt32(7));

    private static DateTimeOffset ParseDate(string value)
        => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
