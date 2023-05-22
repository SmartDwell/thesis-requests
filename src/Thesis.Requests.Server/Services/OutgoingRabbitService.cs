using RabbitMQ.Client;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Options;
using Thesis.Requests.Server.Options;
using Thesis.Requests.Model;

namespace Thesis.Requests.Server.Services;

/// <summary>
/// Сервис публикации сообщений в очереди
/// </summary>
public class OutgoingRabbitService
{
    private readonly RabbitMqOptions _rabbitMqOptions;

    /// <summary>
    /// Конструктор класса
    /// </summary>
    /// <param name="rabbitMqOptions">Настройки RabbitMq</param>
    public OutgoingRabbitService(IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    /// <summary>
    /// Опубликовать новую заявку в очередь
    /// </summary>
    /// <param name="request">Заявка</param>
    public void PublishNewRequestToBroker(Request request)
    {
        var channel = InitChannel();

        var payload = new OutgoingRabbitPayload
        {
            Type = OutgoingRabbitPayload.OutgoingPayloadType.New,
            Payload = JsonSerializer.SerializeToNode(new
            {
                request.Id,
                request.Number,
                request.Title,
                request.Description,
                request.Images,
                request.Created,
                request.IncidentPointList,
                request.IncidentPointListAsString,
                request.CurrentState,
            })
        };

        Publish(channel, payload);
    }

    /// <summary>
    /// Опубликовать новый комментарий в очередь
    /// </summary>
    /// <param name="comment">Комментарий</param>
    public void PublishNewCommentToBroker(RequestComment comment)
    {
        var channel = InitChannel();

        var payload = new OutgoingRabbitPayload
        {
            Type = OutgoingRabbitPayload.OutgoingPayloadType.Comment,
            Payload = JsonSerializer.SerializeToNode(new
            {
                comment.Text,
                comment.Images,
                comment.Created,
                comment.RequestId,
                comment.CreatorName,
            })
        };

        Publish(channel, payload);
    }

    /// <summary>
    /// Опубликовать новый статус в очередь
    /// </summary>
    /// <param name="status">Статус</param>
    public void PublishNewStatusToBroker(RequestStatus status)
    {
        var channel = InitChannel();

        var payload = new OutgoingRabbitPayload
        {
            Type = OutgoingRabbitPayload.OutgoingPayloadType.Cancel,
            Payload = JsonSerializer.SerializeToNode(new
            {
                status.RequestId,
                status.State,
                status.Comment,
                status.Created
            })
        };

        Publish(channel, payload);
    }

    private static void Publish(IModel channel, OutgoingRabbitPayload payload) =>
    channel.BasicPublish(
        "incoming_mobile_exchange",
        string.Empty,
        null,
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));

    private IModel InitChannel()
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = _rabbitMqOptions.HostName,
            Port = _rabbitMqOptions.Port,
            UserName = _rabbitMqOptions.UserName,
            Password = _rabbitMqOptions.Password,
            VirtualHost = _rabbitMqOptions.VirtualHost,
        };

        if (!TryCreateConnection(connectionFactory, out var connection))
        {
            throw new Exception("Failed to create RabbitMQ connection");
        }

        var channel = connection.CreateModel();

        channel.ExchangeDeclare("incoming_mobile_exchange", ExchangeType.Direct, true, false);
        channel.QueueBind("incoming_mobile", "incoming_mobile_exchange", string.Empty);

        return channel;
    }

    private static bool TryCreateConnection(ConnectionFactory factory, out IConnection connection)
    {
        connection = null!;
        try
        {
            connection = factory.CreateConnection();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private class OutgoingRabbitPayload
    {
        public OutgoingPayloadType Type { get; set; }

        public JsonNode? Payload { get; set; }

        public enum OutgoingPayloadType
        {
            New,
            Cancel,
            Comment
        }
    }
}
