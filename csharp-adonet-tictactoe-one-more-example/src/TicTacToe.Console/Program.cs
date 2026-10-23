using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Application;
using TicTacToe.Persistence.Sqlite;

namespace TicTacToe.Console;

internal static class Program
{
    private static async Task<int> Main()
    {
        try
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch
        {
        }

        var databasePath = Path.Combine(AppContext.BaseDirectory, "tictactoe.db");

        var services = new ServiceCollection();
        services.AddTicTacToeApplication();
        services.AddSqlitePersistence(databasePath);

        await using var provider = services.BuildServiceProvider();

        await provider.GetRequiredService<SqliteSchemaInitializer>().InitializeAsync();

        var runner = new ConsoleGameRunner(provider.GetRequiredService<IGameSessionService>());

        await runner.RunAsync();

        return 0;
    }
}
