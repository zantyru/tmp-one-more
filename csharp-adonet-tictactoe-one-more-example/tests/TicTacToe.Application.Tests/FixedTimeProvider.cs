namespace TicTacToe.Application.Tests;


public sealed class FixedTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now)
        => _now = now;

    public override DateTimeOffset GetUtcNow()
        => _now;

    public void Advance(TimeSpan delta)
        => _now = _now.Add(delta);
}
