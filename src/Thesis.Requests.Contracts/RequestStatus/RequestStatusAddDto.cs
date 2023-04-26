using System.ComponentModel.DataAnnotations;
using Thesis.Requests.Model;

namespace Thesis.Requests.Contracts.RequestStatus;

/// <summary>
/// Модель добавления статуса заявки
/// </summary>
public class RequestStatusAddDto
{
    /// <summary>
    /// Состояние статуса
    /// </summary>
    [Required(ErrorMessage = "Необходимо указать статус заявки")]
    public RequestStates State { get; set; }

    /// <summary>
    /// Комментарий к статусу
    /// </summary>
    [Required(ErrorMessage = "Необходимо указать текст комментария")]
    public string Comment { get; set; } = string.Empty;
}
