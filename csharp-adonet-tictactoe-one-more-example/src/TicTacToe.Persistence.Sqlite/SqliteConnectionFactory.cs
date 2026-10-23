using System.Data.Common;
using Microsoft.Data.Sqlite;
using TicTacToe.Persistence.Abstractions;

namespace TicTacToe.Persistence.Sqlite;

/// <summary>Создаёт соединения с файлом SQLite. Одно соединение — одна операция.</summary>
public sealed class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Путь к файлу БД не задан.", nameof(databasePath));
        }

        var fullPath = Path.GetFullPath(databasePath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            // Соединение открывается на операцию; пул не нужен и мешает удалять файл БД в тестах.
            Pooling = false
        }.ToString();
    }

    public DbConnection Create()
        => new SqliteConnection(_connectionString);
}
