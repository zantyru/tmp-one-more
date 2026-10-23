using System.Data.Common;

namespace TicTacToe.Persistence.Abstractions;

/// <summary>
/// Абстракция над созданием соединения ADO.NET. Возвращает базовый <see cref="DbConnection"/>,
/// поэтому репозиторий не зависит от конкретного провайдера (SQLite, SqlClient и т.д.).
/// </summary>
public interface IDbConnectionFactory
{
    public DbConnection Create();
}
