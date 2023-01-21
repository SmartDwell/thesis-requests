namespace Thesis.Requests.Model;

/// <summary>
/// Статус заявки
/// </summary>
public class RequestStatus
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Идентификатор заявки
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Заявка
    /// </summary>
    public virtual Request Request { get; set; } = null!;
    
    /// <summary>
    /// Состояние статуса
    /// </summary>
    public RequestStates State { get; set; }

    /// <summary>
    /// Комментарий к статусу
    /// </summary>
    public string Comment { get; set; } = string.Empty;
    
    /// <summary>
    /// Идентификатор создателя
    /// </summary>
    public Guid CreatorId { get; set; }
    
    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime Created { get; set; }
}