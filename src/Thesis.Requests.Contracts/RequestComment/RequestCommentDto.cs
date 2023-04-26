namespace Thesis.Requests.Contracts.RequestComment;

/// <summary>
/// Модель комментария к заявке
/// </summary>
public class RequestCommentDto
{
    /// <summary>
    /// Текст комментария
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Идентификаторы изображений, прикрепленных к комментарию
    /// </summary>
    public List<Guid> Images { get; set; } = new();

    /// <summary>
    /// Имя и фамилия создателя комментария
    /// </summary>
    public string CreatorName { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime Created { get; set; }
}
