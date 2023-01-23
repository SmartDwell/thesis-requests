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
    /// Список идентификаторов активов до точки инцидента
    /// </summary>
    public List<Guid> IncidentPointList { get; set; } = new();
    
    /// <summary>
    /// Наименование активов до точки инцидента
    /// </summary>
    public string IncidentPointListAsString { get; set; } = string.Empty;
    
    /// <summary>
    /// Актуальный статус заявки
    /// </summary>
    public RequestStates CurrentState { get; set; }
}