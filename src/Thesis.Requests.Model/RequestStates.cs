namespace Thesis.Requests.Model;

/// <summary>
/// Состояния заявки
/// </summary>
public enum RequestStates
{
    /// <summary>
    /// Новая
    /// </summary>
    New = 0,
    
    /// <summary>
    /// В работе
    /// </summary>
    InProgress = 1,
    
    /// <summary>
    /// Завершена
    /// </summary>
    Completed = 2,
    
    /// <summary>
    /// На доработке
    /// </summary>
    UnderCompletion = 3,
    
    /// <summary>
    /// Отменена жителем
    /// </summary>
    CancelledByResident = 4,
    
    /// <summary>
    /// Отклонена диспетчером
    /// </summary>
    RejectedByDispatcher = 5,
}