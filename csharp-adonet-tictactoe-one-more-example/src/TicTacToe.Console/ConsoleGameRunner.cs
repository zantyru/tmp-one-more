using static System.Console;
using TicTacToe.Application;
using TicTacToe.Gameplay;


namespace TicTacToe.Console;


public sealed class ConsoleGameRunner
{
    private static readonly TimeSpan ComputerDelay = TimeSpan.FromMilliseconds(400);

    private readonly IGameSessionService _service;

    public ConsoleGameRunner(IGameSessionService service)
        => _service = service ?? throw new ArgumentNullException(nameof(service));

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        PrintBanner();

        var session = await _service.TryResumeAsync(cancellationToken);

        if (session is not null)
        {
            WriteLine($"Восстановлена незавершённая сессия {session.Id:D}.");
            WriteLine($"Режим: {DescribeMode(session.Mode)}. Ходов: {session.MovesCount}.");
        }
        else
        {
            session = await _service.StartNewAsync(PromptMode(), cancellationToken);
        }

        var shouldExit = false;

        while (!shouldExit)
        {
            session = _service.Current ?? session;
            Draw(session);

            if (session.IsComputerTurn)
            {
                await Task.Delay(ComputerDelay, cancellationToken);

                var computerOutcome = await _service.MakeComputerMoveAsync(cancellationToken);

                if (!computerOutcome.Accepted)
                {
                    WriteLine($"! {computerOutcome.Error}");
                    shouldExit = true;
                }
            }
            else
            {
                WriteLine(
                    session.IsFinished
                    ? "Команды: new — новая игра, history — история, exit — выход."
                    : "Ваш ход: 1–9. Команды: abort — прервать, history — история, exit — выход."
                );

                Write("> ");
                var input = ReadLine()?.Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                shouldExit = await ProcessCommandAsync(input, cancellationToken);
            }
        }
    }

    private async Task<bool> ProcessCommandAsync(string input, CancellationToken cancellationToken)
    {
        var shouldExit = false;

        switch (input)
        {
            case "exit":
            case "quit":
                WriteLine("Незавершённая сессия сохранена и будет восстановлена при следующем запуске.");
                shouldExit = true;
                break;

            case "new":
                await _service.StartNewAsync(PromptMode(), cancellationToken);
                break;

            case "abort":
                var abortOutcome = await _service.AbortAsync(cancellationToken);
                WriteLine(abortOutcome.Accepted ? "Сессия прервана." : $"! {abortOutcome.Error}");
                break;

            case "history":
                await PrintHistoryAsync(cancellationToken);
                break;

            default:
                await ProcessCellAsync(input, cancellationToken);
                break;
        }

        return shouldExit;
    }

    private async Task ProcessCellAsync(string input, CancellationToken cancellationToken)
    {
        var isValidNumber = int.TryParse(input, out var number) && number is >= 1 and <= 9;

        if (!isValidNumber)
        {
            WriteLine("Не понял команду. Введите число 1–9 или одну из команд.");

            return;
        }

        var moveOutcome = await _service.MakeHumanMoveAsync(number - 1, cancellationToken);

        if (!moveOutcome.Accepted)
        {
            WriteLine($"! {moveOutcome.Error}");
        }
    }

    private static void PrintBanner()
    {
        WriteLine();
        WriteLine("  Крестики-нолики 3x3 — учебный пример C# / ADO.NET / SQLite");
        WriteLine("  Логика игры ничего не знает о БД и UI.");
        WriteLine();
    }

    private static void Draw(GameSession session)
    {
        WriteLine();

        for (var row = 0; row < Board.Size; row++)
        {
            var cells =
                Enumerable
                .Range(row * Board.Size, Board.Size)
                .Select(index => session.Board[index] == Mark.None
                    ? (index + 1).ToString()
                    : session.Board[index].ToChar().ToString());

            WriteLine("   " + string.Join(" | ", cells));

            if (row < Board.Size - 1)
            {
                WriteLine("  ---+---+---");
            }
        }

        WriteLine();
        WriteLine($"  Режим:  {DescribeMode(session.Mode)}");
        WriteLine($"  Статус: {DescribeResult(session.Result)}");

        if (!session.IsFinished)
        {
            var who = session.IsComputerTurn ? "компьютер" : "вы";
            WriteLine($"  Ход:    {session.CurrentMark.ToChar()} — {who}");
        }

        WriteLine($"  Ходов:  {session.MovesCount}");
        WriteLine();
    }

    private static GameMode PromptMode()
    {
        GameMode? selected = null;

        while (selected is null)
        {
            WriteLine("Выберите режим новой игры:");
            WriteLine("  1 — человек против человека");
            WriteLine("  2 — человек против компьютера");
            WriteLine("  3 — компьютер против компьютера");
            Write("> ");

            var input = ReadLine()?.Trim();

            switch (input)
            {
                case "1":
                    selected = GameMode.HumanVsHuman;
                    break;
                
                case "2":
                    selected = GameMode.HumanVsComputer;
                    break;
                
                case "3":
                    selected = GameMode.ComputerVsComputer;
                    break;
                
                case null:
                    selected = GameMode.HumanVsHuman;
                    break;
                
                default:
                    WriteLine("Введите 1, 2 или 3.");
                    break;
            }
        }

        return selected.Value;
    }

    private static string DescribeMode(GameMode mode)
        => mode switch
        {
            GameMode.HumanVsHuman => "человек против человека",
            GameMode.HumanVsComputer => "человек против компьютера",
            GameMode.ComputerVsComputer => "компьютер против компьютера",
            _ => mode.ToString()
        };

    private static string DescribeResult(SessionResult result)
        => result switch
        {
            SessionResult.InProgress => "в процессе",
            SessionResult.XWins => "победили крестики",
            SessionResult.OWins => "победили нолики",
            SessionResult.Draw => "ничья",
            SessionResult.Aborted => "прервана",
            _ => result.ToString()
        };

    private async Task PrintHistoryAsync(CancellationToken cancellationToken)
    {
        var history = await _service.GetHistoryAsync(10, cancellationToken);

        WriteLine();

        if (history.Count == 0)
        {
            WriteLine("История пуста.");
        }
        else
        {
            WriteLine("Последние сессии:");

            foreach (var item in history)
            {
                var ended = item.EndedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "—";
                
                WriteLine(
                    $"  {item.StartedAt:yyyy-MM-dd HH:mm:ss} → {ended} | " +
                    $"{DescribeResult(item.Result),-20} | {DescribeMode(item.Mode),-30} | ходов: {item.MovesCount}"
                );
            }
        }

        WriteLine();
    }
}
