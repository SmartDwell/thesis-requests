namespace Thesis.Requests.Model;

/// <summary>
/// Заявка
/// </summary>
public class Request
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
    /// Идентификатор создателя
    /// </summary>
    public Guid CreatorId { get; set; }

    /// <summary>
    /// Имя создателя
    /// </summary>
    public string CreatorName { get; set; } = string.Empty;
    
    /// <summary>
    /// Контакты создателя
    /// </summary>
    public string CreatorContact { get; set; } = string.Empty;
    
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
    /// Признак отредактированности заявки
    /// </summary>
    public bool IsEdited { get; set; }
    
    /// <summary>
    /// Комментарии к заявке
    /// </summary>
    public virtual ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
    
    /// <summary>
    /// Статусы заявки
    /// </summary>
    public virtual ICollection<RequestStatus> Statuses { get; set; } = new List<RequestStatus>();

    /// <summary>
    /// Получить актуальный статус заявки
    /// </summary>
    public RequestStates CurrentState => Statuses.OrderBy(status => status.Created).Last().State;
}