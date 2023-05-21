namespace Thesis.Requests.Model;

/// <summary>
/// Комментарий к заявке
/// </summary>
public class RequestComment
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
    /// Текст комментария
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
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
    /// Дата создания
    /// </summary>
    public DateTime Created { get; set; }
}