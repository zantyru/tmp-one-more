using System.Data.Common;


namespace TicTacToe.Persistence.Abstractions;


public interface IDbConnectionFactory
{
    public DbConnection Create();
}
