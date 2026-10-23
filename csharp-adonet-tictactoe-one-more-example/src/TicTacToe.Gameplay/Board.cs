namespace TicTacToe.Gameplay;


public sealed class Board
{
    public const int Size = 3;
    public const int CellCount = Size * Size;

    private static readonly int[][] WinningLines =
    {
        new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 },
        new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 },
        new[] { 0, 4, 8 }, new[] { 2, 4, 6 }
    };

    private readonly Mark[] _cells;

    public Board()
        : this(new Mark[CellCount])
    {
    }

    public Board(IEnumerable<Mark> cells)
    {
        ArgumentNullException.ThrowIfNull(cells);

        _cells = cells.ToArray();

        if (_cells.Length != CellCount)
        {
            throw new ArgumentException($"Поле должно содержать ровно {CellCount} клеток.", nameof(cells));
        }
    }

    public Mark this[int index]
        => _cells[index];

    public int MovesCount
        => _cells.Count(cell => cell != Mark.None);

    public bool IsFull
        => MovesCount == CellCount;

    public bool IsEmpty(int index)
        => _cells[index] == Mark.None;

    public bool TryPlace(int index, Mark mark)
    {
        if (index < 0 || index >= CellCount)
        {
            return false;
        }

        if (mark == Mark.None)
        {
            return false;
        }

        if (!IsEmpty(index))
        {
            return false;
        }

        _cells[index] = mark;

        return true;
    }

    public IEnumerable<int> EmptyCells()
    {
        for (var index = 0; index < CellCount; index++)
        {
            if (_cells[index] == Mark.None)
            {
                yield return index;
            }
        }
    }

    public SessionResult Evaluate()
    {
        var winningMark = Mark.None;

        foreach (var line in WinningLines)
        {
            var first = _cells[line[0]];
            var lineIsWinning =
                first != Mark.None
                && _cells[line[1]] == first
                && _cells[line[2]] == first;

            if (lineIsWinning)
            {
                winningMark = first;
                break;
            }
        }

        var result = winningMark switch
        {
            Mark.X => SessionResult.XWins,
            Mark.O => SessionResult.OWins,
            _ => IsFull ? SessionResult.Draw : SessionResult.InProgress
        };

        return result;
    }

    public Board Clone()
        => new(_cells);

    public string ToStateString()
        => new(_cells.Select(mark => mark.ToChar()).ToArray());

    public static Board FromStateString(string state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.Length != CellCount)
        {
            throw new ArgumentException($"Строка состояния должна содержать {CellCount} символов.", nameof(state));
        }

        return new Board(state.Select(MarkExtensions.FromChar));
    }

    public override string ToString()
        => ToStateString();
}
