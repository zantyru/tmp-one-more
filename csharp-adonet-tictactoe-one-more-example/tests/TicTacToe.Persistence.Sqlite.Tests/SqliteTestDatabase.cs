using TicTacToe.Persistence.Abstractions;


namespace TicTacToe.Persistence.Sqlite.Tests;


public sealed class SqliteTestDatabase : IDisposable
{
    public SqliteTestDatabase()
    {
        DatabasePath = Path.Combine(
            Path.GetTempPath(),
            $"tictactoe-test-{Guid.NewGuid():N}.db"
        );

        Factory = new SqliteConnectionFactory(DatabasePath);
        Repository = new SqliteGameSessionRepository(Factory);
    }

    public string DatabasePath { get; }

    public IDbConnectionFactory Factory { get; }

    public SqliteGameSessionRepository Repository { get; }

    public async Task InitializeAsync()
        => await new SqliteSchemaInitializer(Factory).InitializeAsync();

    public void Dispose()
    {
        TryDelete(DatabasePath);
        TryDelete(DatabasePath + "-wal");
        TryDelete(DatabasePath + "-shm");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }
}
