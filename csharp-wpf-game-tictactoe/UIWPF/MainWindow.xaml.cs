using System.IO;
using System.Windows;
using Data;


namespace UIWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                DbConstants.DatabaseFileName
            );

            var repository = new SqliteGameRepository(dbPath);
            var viewModel = new MainViewModel(repository);

            DataContext = viewModel;

            Loaded += async (_, _) =>
            {
                await repository.InitializeAsync();
            };
        }
    }
}