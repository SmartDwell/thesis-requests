using System.ComponentModel.DataAnnotations;

namespace Thesis.Requests.Contracts.Request;

/// <summary>
/// Модель добавления заявки
/// </summary>
public class RequestAddDto
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

    /// <summary>
    /// Идентификаторы изображений, прикрепленных к комментарию
    /// </summary>
    public List<Guid> Images { get; set; } = new();
    
    /// <summary>
    /// Список идентификаторов активов до точки инцидента
    /// </summary>
    [Required(ErrorMessage = "Необходимо указать местоположение инцидента")]
    public List<Guid> IncidentPointList { get; set; } = new();
}
