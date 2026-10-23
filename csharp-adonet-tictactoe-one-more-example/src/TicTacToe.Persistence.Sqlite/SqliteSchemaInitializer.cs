using TicTacToe.Persistence.Abstractions;


namespace TicTacToe.Persistence.Sqlite;


public sealed class SqliteSchemaInitializer
{
    private readonly IDbConnectionFactory _connections;

    public SqliteSchemaInitializer(IDbConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connections.Create();

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS GameSessions (
                Id          TEXT    NOT NULL PRIMARY KEY,
                StartedAt   TEXT    NOT NULL,
                EndedAt     TEXT    NULL,
                Result      INTEGER NOT NULL,
                Mode        INTEGER NOT NULL,
                BoardState  TEXT    NOT NULL CHECK (length(BoardState) = 9),
                CurrentMark TEXT    NOT NULL CHECK (CurrentMark IN ('X','O')),
                MovesCount  INTEGER NOT NULL DEFAULT 0
            );

            CREATE UNIQUE INDEX IF NOT EXISTS UX_GameSessions_Active
                ON GameSessions(Result) WHERE Result = 0;

            CREATE INDEX IF NOT EXISTS IX_GameSessions_StartedAt
                ON GameSessions(StartedAt DESC);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
