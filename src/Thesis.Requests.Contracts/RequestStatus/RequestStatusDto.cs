using Thesis.Requests.Model;

namespace Thesis.Requests.Contracts.RequestStatus;

/// <summary>
/// Модель статуса заявки
/// </summary>
public class RequestStatusDto
{
    /// <summary>
    /// Состояние статуса
    /// </summary>
    public RequestStates State { get; set; }

    /// <summary>
    /// Комментарий к статусу
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime Created { get; set; }
}
