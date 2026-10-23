using TicTacToe.Gameplay;
using TicTacToe.Persistence.Abstractions;


namespace TicTacToe.Application;


public sealed class GameSession
{
    private GameSession(
        Guid id,
        DateTimeOffset startedAt,
        DateTimeOffset? endedAt,
        GameMode mode,
        Board board,
        Mark currentMark,
        SessionResult result)
    {
        Id = id;
        StartedAt = startedAt;
        EndedAt = endedAt;
        Mode = mode;
        Board = board;
        CurrentMark = currentMark;
        Result = result;
    }

    public Guid Id { get; }

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset? EndedAt { get; private set; }

    public GameMode Mode { get; }

    public Board Board { get; }

    public Mark CurrentMark { get; private set; }

    public SessionResult Result { get; private set; }

    public bool IsFinished
        => Result != SessionResult.InProgress;

    public int MovesCount
        => Board.MovesCount;

    public bool IsComputerTurn
        => !IsFinished && Mode switch
        {
            GameMode.HumanVsHuman => false,
            GameMode.HumanVsComputer => CurrentMark == Mark.O,
            GameMode.ComputerVsComputer => true,
            _ => false
        };

    public static GameSession Start(GameMode mode, DateTimeOffset now)
        => new(
            id: Guid.NewGuid(),
            startedAt: now,
            endedAt: null,
            mode: mode,
            board: new Board(),
            currentMark: Mark.X,
            result: SessionResult.InProgress
        );

    public static GameSession Restore(GameSessionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var board = Board.FromStateString(snapshot.BoardState);

        if (snapshot.CurrentMark == Mark.None)
        {
            throw new InvalidOperationException("В снимке не указана текущая метка.");
        }

        if (snapshot.MovesCount < 0 || snapshot.MovesCount > Board.CellCount)
        {
            throw new InvalidOperationException("Некорректное число ходов в снимке.");
        }

        if (board.MovesCount != snapshot.MovesCount)
        {
            throw new InvalidOperationException("Число ходов не совпадает с состоянием поля.");
        }

        var evaluated = board.Evaluate();

        if (snapshot.Result == SessionResult.InProgress)
        {
            if (evaluated != SessionResult.InProgress)
            {
                throw new InvalidOperationException("Незавершённая сессия содержит законченную партию.");
            }

            if (snapshot.EndedAt is not null)
            {
                throw new InvalidOperationException("У незавершённой сессии не должно быть даты окончания.");
            }
        }
        else
        {
            if (snapshot.EndedAt is null)
            {
                throw new InvalidOperationException("У завершённой сессии должна быть дата окончания.");
            }

            if (snapshot.Result != SessionResult.Aborted && evaluated != snapshot.Result)
            {
                throw new InvalidOperationException("Результат сессии не соответствует состоянию поля.");
            }
        }

        return new GameSession(
            id: snapshot.Id,
            startedAt: snapshot.StartedAt,
            endedAt: snapshot.EndedAt,
            mode: snapshot.Mode,
            board: board,
            currentMark: snapshot.CurrentMark,
            result: snapshot.Result
        );
    }

    public MoveOutcome ApplyMove(int cellIndex, DateTimeOffset now)
    {
        if (IsFinished)
        {
            return MoveOutcome.Fail("Сессия уже завершена.");
        }

        if (cellIndex < 0 || cellIndex >= Board.CellCount)
        {
            return MoveOutcome.Fail("Некорректный индекс клетки.");
        }

        if (!Board.TryPlace(cellIndex, CurrentMark))
        {
            return MoveOutcome.Fail("Клетка уже занята.");
        }

        Result = Board.Evaluate();

        if (Result == SessionResult.InProgress)
        {
            CurrentMark = CurrentMark.Opponent();
        }
        else
        {
            EndedAt = now;
        }

        return MoveOutcome.Ok();
    }

    /// <summary>Досрочное прерывание сессии по явной команде пользователя.</summary>
    public MoveOutcome Abort(DateTimeOffset now)
    {
        if (IsFinished)
        {
            return MoveOutcome.Fail("Сессия уже завершена.");
        }

        Result = SessionResult.Aborted;
        EndedAt = now;

        return MoveOutcome.Ok();
    }

    public GameSessionSnapshot ToSnapshot()
        => new(
            Id: Id,
            StartedAt: StartedAt,
            EndedAt: EndedAt,
            Result: Result,
            Mode: Mode,
            BoardState: Board.ToStateString(),
            CurrentMark: CurrentMark,
            MovesCount: MovesCount
        );
}
