using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Application;
using TicTacToe.Persistence.Sqlite;
using TicTacToe.Wpf.ViewModels;

namespace TicTacToe.Wpf;

// Явная квалификация базового типа обязательна: using TicTacToe.Application; вносит
// в область видимости одноимённое пространство имён, и короткое «Application»
// разрешается в него, а не в System.Windows.Application (CS0118).
public partial class App : System.Windows.Application
{
    private ServiceProvider? _provider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var databasePath = Path.Combine(AppContext.BaseDirectory, "tictactoe.db");

            var services = new ServiceCollection();
            services.AddTicTacToeApplication();
            services.AddSqlitePersistence(databasePath);
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

            _provider = services.BuildServiceProvider();

            await _provider.GetRequiredService<SqliteSchemaInitializer>().InitializeAsync();

            var window = _provider.GetRequiredService<MainWindow>();
            var viewModel = _provider.GetRequiredService<MainViewModel>();

            window.DataContext = viewModel;
            window.Show();

            await viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Ошибка запуска", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _provider?.Dispose();
        base.OnExit(e);
    }
}
