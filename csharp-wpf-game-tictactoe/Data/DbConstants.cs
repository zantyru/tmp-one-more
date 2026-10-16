namespace Data
{
    /// <summary>
    /// Константы слоя данных.
    /// </summary>
    public static class DbConstants
    {
        public const string DatabaseFileName = "tictactoe.db";
        public const string ConnectionStringFormat = "Data Source={0};";

        // Таблицы
        public const string SessionsTable = "GameSessions";
        public const string MovesTable = "GameMoves";

        // SQL-запросы (создание таблиц)
        public const string CreateSessionsTableSql = $"""
        CREATE TABLE IF NOT EXISTS {SessionsTable} (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            StartedAt   TEXT    NOT NULL,
            FinishedAt  TEXT,
            GameMode    TEXT    NOT NULL,
            Winner      TEXT,
            IsDraw      INTEGER NOT NULL DEFAULT 0
        );
        """;

        public const string CreateMovesTableSql = $"""
        CREATE TABLE IF NOT EXISTS {MovesTable} (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            SessionId   INTEGER NOT NULL,
            MoveNumber  INTEGER NOT NULL,
            Row         INTEGER NOT NULL,
            Col         INTEGER NOT NULL,
            Player      TEXT    NOT NULL,
            MadeAt      TEXT    NOT NULL,
            FOREIGN KEY (SessionId) REFERENCES {SessionsTable}(Id)
        );
        """;
    }
}
