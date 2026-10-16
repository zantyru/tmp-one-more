using Microsoft.Data.Sqlite;


namespace Data
{
    /// <summary>
    /// Адаптер SQLite для абстракции IGameRepository.
    /// </summary>
    public class SqliteGameRepository : IGameRepository
    {
        private readonly string _connectionString;

        public SqliteGameRepository(string databasePath)
        {
            _connectionString = string.Format(DbConstants.ConnectionStringFormat, databasePath);
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(ct);

            // Включаем WAL для повышения производительности и параллелизма
            await using (var walCmd = connection.CreateCommand())
            {
                walCmd.CommandText = "PRAGMA journal_mode = 'wal';";
                await walCmd.ExecuteNonQueryAsync(ct);
            }

            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = DbConstants.CreateSessionsTableSql;
                await cmd.ExecuteNonQueryAsync(ct);
            }

            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = DbConstants.CreateMovesTableSql;
                await cmd.ExecuteNonQueryAsync(ct);
            }
        }

        public async Task<long> CreateSessionAsync(GameMode mode, CancellationToken ct = default)
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"""
            INSERT INTO {DbConstants.SessionsTable} (StartedAt, GameMode)
            VALUES (@startedAt, @gameMode);
            SELECT last_insert_rowid();
            """;
            cmd.Parameters.AddWithValue("@startedAt", DateTime.UtcNow.ToString("O"));
            cmd.Parameters.AddWithValue("@gameMode", mode.ToString());

            var result = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt64(result);
        }

        public async Task SaveMoveAsync(long sessionId, int moveNumber, int row, int col, string player, CancellationToken ct = default)
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"""
            INSERT INTO {DbConstants.MovesTable} (SessionId, MoveNumber, Row, Col, Player, MadeAt)
            VALUES (@sessionId, @moveNumber, @row, @col, @player, @madeAt);
            """;
            cmd.Parameters.AddWithValue("@sessionId", sessionId);
            cmd.Parameters.AddWithValue("@moveNumber", moveNumber);
            cmd.Parameters.AddWithValue("@row", row);
            cmd.Parameters.AddWithValue("@col", col);
            cmd.Parameters.AddWithValue("@player", player);
            cmd.Parameters.AddWithValue("@madeAt", DateTime.UtcNow.ToString("O"));

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task FinishSessionAsync(long sessionId, string? winner, bool isDraw, CancellationToken ct = default)
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"""
            UPDATE {DbConstants.SessionsTable}
            SET FinishedAt = @finishedAt, Winner = @winner, IsDraw = @isDraw
            WHERE Id = @sessionId;
            """;
            cmd.Parameters.AddWithValue("@finishedAt", DateTime.UtcNow.ToString("O"));
            cmd.Parameters.AddWithValue("@winner", (object?)winner ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@isDraw", isDraw ? 1 : 0);
            cmd.Parameters.AddWithValue("@sessionId", sessionId);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<IReadOnlyList<GameSession>> GetSessionsAsync(CancellationToken ct = default)
        {
            var sessions = new List<GameSession>();

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"""
            SELECT Id, StartedAt, FinishedAt, GameMode, Winner, IsDraw
            FROM {DbConstants.SessionsTable}
            ORDER BY StartedAt DESC;
            """;

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                sessions.Add(new GameSession
                {
                    Id = reader.GetInt64(0),
                    StartedAt = DateTime.Parse(reader.GetString(1)),
                    FinishedAt = reader.IsDBNull(2) ? null : DateTime.Parse(reader.GetString(2)),
                    GameMode = Enum.Parse<GameMode>(reader.GetString(3)),
                    Winner = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IsDraw = reader.GetInt32(5) == 1
                });
            }

            return sessions;
        }

        public async Task<IReadOnlyList<GameMove>> GetMovesAsync(long sessionId, CancellationToken ct = default)
        {
            var moves = new List<GameMove>();

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"""
            SELECT Id, SessionId, MoveNumber, Row, Col, Player, MadeAt
            FROM {DbConstants.MovesTable}
            WHERE SessionId = @sessionId
            ORDER BY MoveNumber;
            """;
            cmd.Parameters.AddWithValue("@sessionId", sessionId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                moves.Add(new GameMove
                {
                    Id = reader.GetInt64(0),
                    SessionId = reader.GetInt64(1),
                    MoveNumber = reader.GetInt32(2),
                    Row = reader.GetInt32(3),
                    Col = reader.GetInt32(4),
                    Player = reader.GetString(5),
                    MadeAt = DateTime.Parse(reader.GetString(6))
                });
            }

            return moves;
        }
    }
}
