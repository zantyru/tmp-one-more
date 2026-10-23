using System.Data.Common;
using System.Globalization;
using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;


namespace TicTacToe.Persistence.Sqlite;


public sealed class SqliteGameSessionRepository : IGameSessionRepository
{
    private readonly IDbConnectionFactory _connections;

    public SqliteGameSessionRepository(IDbConnectionFactory connections)
        => _connections = connections ?? throw new ArgumentNullException(nameof(connections));

    public async Task<GameSessionSnapshot?> FindUnfinishedAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connections.Create();

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = $"""
            SELECT {GameSessionMapper.SelectColumns}
            FROM GameSessions
            WHERE Result = @inProgress
            ORDER BY StartedAt DESC
            LIMIT 1;
            """;

        AddParameter(command, "@inProgress", (int)SessionResult.InProgress);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await
            reader.ReadAsync(cancellationToken)
            ? GameSessionMapper.Map(reader)
            : null;
    }

    public async Task<GameSessionSnapshot?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connections.Create();

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = $"""
            SELECT {GameSessionMapper.SelectColumns}
            FROM GameSessions
            WHERE Id = @id;
            """;

        AddParameter(command, "@id", id.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await
            reader.ReadAsync(cancellationToken)
            ? GameSessionMapper.Map(reader)
            : null;
    }

    public async Task InsertAsync(GameSessionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        await using var connection = _connections.Create();

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO GameSessions
                (Id, StartedAt, EndedAt, Result, Mode, BoardState, CurrentMark, MovesCount)
            VALUES
                (@id, @startedAt, @endedAt, @result, @mode, @board, @mark, @moves);
            """;

        AddParameter(command, "@id", snapshot.Id.ToString("D"));
        AddParameter(command, "@startedAt", FormatDate(snapshot.StartedAt));
        AddParameter(command, "@endedAt", snapshot.EndedAt is null ? null : FormatDate(snapshot.EndedAt.Value));
        AddParameter(command, "@result", (int)snapshot.Result);
        AddParameter(command, "@mode", (int)snapshot.Mode);
        AddParameter(command, "@board", snapshot.BoardState);
        AddParameter(command, "@mark", snapshot.CurrentMark.ToChar().ToString());
        AddParameter(command, "@moves", snapshot.MovesCount);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateAsync(GameSessionSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        await using var connection = _connections.Create();

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE GameSessions
            SET EndedAt = @endedAt,
                Result = @result,
                BoardState = @board,
                CurrentMark = @mark,
                MovesCount = @moves
            WHERE Id = @id;
            """;

        AddParameter(command, "@id", snapshot.Id.ToString("D"));
        AddParameter(command, "@endedAt", snapshot.EndedAt is null ? null : FormatDate(snapshot.EndedAt.Value));
        AddParameter(command, "@result", (int)snapshot.Result);
        AddParameter(command, "@board", snapshot.BoardState);
        AddParameter(command, "@mark", snapshot.CurrentMark.ToChar().ToString());
        AddParameter(command, "@moves", snapshot.MovesCount);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        if (affected != 1)
        {
            throw new InvalidOperationException($"Сессия {snapshot.Id} не найдена: обновлено строк — {affected}.");
        }
    }

    public async Task<IReadOnlyList<GameSessionSnapshot>> GetRecentAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take), "Количество записей должно быть положительным.");
        }

        await using var connection = _connections.Create();

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = $"""
            SELECT {GameSessionMapper.SelectColumns}
            FROM GameSessions
            ORDER BY StartedAt DESC
            LIMIT @take;
            """;

        AddParameter(command, "@take", take);

        var result = new List<GameSessionSnapshot>(take);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(GameSessionMapper.Map(reader));
        }

        return result;
    }

    private static string FormatDate(DateTimeOffset value)
        => value.ToString("O", CultureInfo.InvariantCulture);

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}
