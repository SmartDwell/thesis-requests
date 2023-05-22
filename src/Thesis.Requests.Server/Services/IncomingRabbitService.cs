using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Seljmov.AspNet.Commons.Helpers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Thesis.Requests.Contracts.RequestStatus;
using Thesis.Requests.Model;
using Thesis.Requests.Server.Options;

namespace Thesis.Requests.Server.Services;

/// <summary>
/// Сервис, отвечающий за получение новых комментариев и новых статусов из rabbitmq
/// </summary>
public class IncomingRabbitService
{
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly JwtReader _jwtReader;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Конструктор класса
    /// </summary>
    /// <param name="rabbitMqOptions">Настройки rabbitmq</param>
    /// <param name="jwtReader">Работа с jwt</param>
    /// <param name="serviceScopeFactory">Сервисы приложения</param>
    public IncomingRabbitService(IOptions<RabbitMqOptions> rabbitMqOptions, IServiceScopeFactory serviceScopeFactory, JwtReader jwtReader)
    {
        _rabbitMqOptions = rabbitMqOptions.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _jwtReader = jwtReader;
    }

    /// <summary>
    /// Создание слушателя очереди
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Fetch()
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
        channel.QueueDeclare("outgoing_dispatch", true, false, false);
        channel.BasicQos(0, 1, true);

        var consumer = new EventingBasicConsumer(channel);

        JsonSerializerOptions jso = new() { PropertyNameCaseInsensitive = true };
        consumer.Received += (_, ea) =>
        {
            var messageAsString = Encoding.UTF8.GetString(ea.Body.Span);
            var message = JsonSerializer.Deserialize<IncomingRabbitPayload>(messageAsString, jso);

            if (message == null)
            {
                channel.BasicNack(ea.DeliveryTag, false, true);
                throw new Exception("Invalid message in queue");
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var currentUser = GetAuthUserInfo(message.Token);

            switch (message.Type)
            {
                case IncomingRabbitPayload.IncomingPayloadType.NewComment:
                    CreateNewComment(context, JsonSerializer.Deserialize<IncomingRabbitComment>(message.Payload, jso), currentUser);
                    break;
                case IncomingRabbitPayload.IncomingPayloadType.NewStatus:
                    CreateNewStatus(context, JsonSerializer.Deserialize<IncomingRabbitStatus>(message.Payload, jso), currentUser);
                    break;
            }

            channel.BasicAck(ea.DeliveryTag, false);
        };
    }

    private static void CreateNewComment(DatabaseContext context, IncomingRabbitComment comment, AuthUserInfo user)
    {
        var newComment = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = comment.RequestId,
            Text = comment.Text,
            Images = comment.Images,
            CreatorId = user.GuidId,
            CreatorName = user.FullName,
        };

        context.RequestComments.Add(newComment);
        context.SaveChanges();
    }

    private static void CreateNewStatus(DatabaseContext context, IncomingRabbitStatus status, AuthUserInfo user)
    {
        var newStatus = new RequestStatus
        {
            Id = Guid.NewGuid(),
            RequestId = status.RequestId,
            State = status.State,
            Comment = status.Comment,
            CreatorId = user.GuidId,
            CreatorName = user.FullName,
        };

        context.RequestStatuses.Add(newStatus);
        context.SaveChanges();
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

    private class IncomingRabbitPayload
    {
        public IncomingPayloadType Type { get; set; }

        public string Token { get; set; } = string.Empty;

        public JsonNode? Payload { get; set; }

        public enum IncomingPayloadType
        {
            NewStatus,
            NewComment
        }
    }

    private class IncomingRabbitComment
    {
        public Guid RequestId { get; set; }

        public string Text { get; set; } = string.Empty;

        public List<Guid> Images { get; set; } = new();
    }

    internal class IncomingRabbitStatus
    {
        public Guid RequestId { get; set; }

        public RequestStates State { get; set; }

        public string Comment { get; set; } = string.Empty;
    }

    private AuthUserInfo? GetAuthUserInfo(string authHeader)
    {
        var token = authHeader?.Replace("Bearer ", "") ?? throw new ArgumentNullException($"Bearer token not found");

        _ = _jwtReader.ReadAccessToken(token, out var claims, out var validTo);
        if (claims is null) return null;

        var userInfo = new AuthUserInfo(
            Id: claims.Claims.FirstOrDefault(a => a.Type == ClaimsIdentity.DefaultIssuer)?.Value ?? throw new ArgumentNullException($"User's id from bearer token not found"),
            FullName: claims.Claims.FirstOrDefault(a => a.Type == ClaimsIdentity.DefaultNameClaimType)?.Value ?? throw new ArgumentNullException($"User's fullname from bearer token not found"),
            Role: claims.Claims.FirstOrDefault(a => a.Type == ClaimsIdentity.DefaultRoleClaimType)?.Value ?? throw new ArgumentNullException($"User's role from bearer token not found"),
            Email: claims.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Email)?.Value ?? throw new ArgumentNullException($"User's email from bearer token not found"),
            Phone: claims.Claims.FirstOrDefault(a => a.Type == ClaimTypes.MobilePhone)?.Value ?? throw new ArgumentNullException($"User's phone from bearer token not found")
        );

        return userInfo;
    }

    private record AuthUserInfo(string Id, string FullName, string Role, string Email, string Phone)
    {
        public Guid GuidId => Guid.TryParse(Id, out var guidId) ? guidId : throw new ArgumentNullException();
    }
}
