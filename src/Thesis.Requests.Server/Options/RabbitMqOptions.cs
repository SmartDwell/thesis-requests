namespace Thesis.Requests.Server.Options;

/// <summary>
/// Настройки подключения к брокеру RabbitMQ
/// </summary>
public class RabbitMqOptions
{
    /// <summary>
    /// Адрес сервера RabbitMQ
    /// </summary>
    public string HostName { get; set; } = string.Empty;

    /// <summary>
    /// Порт сервера RabbitMQ
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Имя пользователя для доступа к RabbitMQ
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Пароль для доступа к RabbitMQ
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Виртуальный адрес на сервере RabbitMQ
    /// </summary>
    public string VirtualHost { get; set; } = string.Empty;
}