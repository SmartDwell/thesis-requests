using System.ComponentModel.DataAnnotations;

namespace Thesis.Requests.Contracts.Request;

/// <summary>
/// Модель редактирования заявки
/// </summary>
public class RequestEditDto
{
    /// <summary>
    /// Заголовок
    /// </summary>
    [Required(ErrorMessage = "Необходимо указать заголовок к заявке")]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Описание
    /// </summary>
    [Required(ErrorMessage = "Необходимо указать описание к заявке")]
    public string Description { get; set; } = string.Empty;
}
