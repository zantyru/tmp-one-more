using TicTacToe.Gameplay;

namespace TicTacToe.Wpf.ViewModels;

/// <summary>Элемент выпадающего списка режимов: значение для сервиса и подпись для пользователя.</summary>
public sealed record ModeOption(GameMode Mode, string Display);
