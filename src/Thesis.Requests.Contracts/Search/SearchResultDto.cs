namespace Thesis.Requests.Contracts.Search;

/// <summary>
/// Результат поиска
/// </summary>
public class SearchResultDto
{ 
    /// <summary>
    /// Идентификатор результата
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название результата
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Значение от 0 до 1, показывающее точность совпадения, 1 - полное, 0 - нет совпадения
    /// </summary>
    /// <param name="searchValue">Исследуемая строка</param>
    public double Score(string searchValue) => JaroWinklerDistance.Proximity(Name, searchValue);
}