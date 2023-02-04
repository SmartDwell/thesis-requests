using System.ComponentModel.DataAnnotations;

namespace Thesis.Requests.Contracts.RequestComment;

/// <summary>
/// Модель добавления комментария к заявке
/// </summary>
public class RequestCommentAddDto
{
    /// <summary>
    /// Текст комментария
    /// </summary>
    [Required(ErrorMessage = "Необходимо указать текст комментария")]
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Идентификаторы изображений, прикрепленных к комментарию
    /// </summary>
    public List<Guid> Images { get; set; } = new();
}
