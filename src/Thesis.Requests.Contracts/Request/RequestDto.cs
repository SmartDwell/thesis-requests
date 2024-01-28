using Thesis.Requests.Model;

namespace Thesis.Requests.Contracts.Request;

/// <summary>
/// Модель с информацией по заявке
/// </summary>
public class RequestDto
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Номер заявки
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Заголовок
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Описание
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Идентификаторы изображений, прикрепленных к комментарию
    /// </summary>
    public List<Guid> Images { get; set; } = new();

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор точки инцидента
    /// </summary>
    public Guid IncidentPointId { get; set; } = new();

    /// <summary>
    /// Полное имя точки инцидента
    /// </summary>
    public string IncidentPointFullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Признак того, что заявка была отредактирована
    /// </summary>
    public bool IsEdited { get; set; }
    
    /// <summary>
    /// Актуальный статус заявки
    /// </summary>
    public RequestStates CurrentState { get; set; }
}