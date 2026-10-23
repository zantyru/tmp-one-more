using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Persistence.Abstractions;


namespace TicTacToe.Persistence.Sqlite;


public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlitePersistence(this IServiceCollection services, string databasePath)
    {
        services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(databasePath));
        services.AddSingleton<IGameSessionRepository, SqliteGameSessionRepository>();
        services.AddSingleton<SqliteSchemaInitializer>();

        return services;
    }
}
