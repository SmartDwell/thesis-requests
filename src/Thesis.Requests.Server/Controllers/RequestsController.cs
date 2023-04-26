using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seljmov.AspNet.Commons.Helpers;
using Thesis.Requests.Contracts.Request;
using Thesis.Requests.Contracts.RequestComment;
using Thesis.Requests.Contracts.RequestStatus;
using Thesis.Requests.Model;

namespace Thesis.Requests.Server.Controllers;

/// <summary>
/// Контроллер для работы с заявками жильцов
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly JwtReader _jwtReader;
    private readonly ILogger<RequestsController> _logger;

    /// <summary>
    /// Контроллер класса <see cref="RequestsController"/>
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="logger">Логгер</param>
    /// <param name="jwtReader">Расшифровщик данных пользователя из JWT</param>
    /// <exception cref="ArgumentNullException">Аргумент не инициализирован</exception>
    public RequestsController(DatabaseContext context, ILogger<RequestsController> logger, JwtReader jwtReader)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jwtReader = jwtReader ?? throw new ArgumentNullException(nameof(jwtReader));
    }

    #region Request

    /// <summary>
    /// Получить заявки
    /// </summary>
    /// <param name="state">Состояние заявки</param>
    /// <response code="200">Список всех заявок</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequests([FromQuery] RequestStates? state)
    {
        var requests = await _context.Requests
            .Include(request => request.Statuses)
            .Select(request => new RequestDto
            {
                Id = request.Id,
                Number = request.Number,
                Title = request.Title,
                Description = request.Description,
                Images = request.Images,
                Created = request.Created,
                IncidentPointList = request.IncidentPointList,
                IncidentPointListAsString = request.IncidentPointListAsString,
                CurrentState = request.CurrentState,
            }).ToListAsync();
        
        return Ok(state is null 
            ? requests 
            : requests.Where(request => request.CurrentState == state).ToList());
    }
    
    /// <summary>
    /// Получить заявки пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <response code="200">Список заявок пользователя</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserRequests([FromRoute] Guid userId)
    {
        var requests = await _context.Requests
            .Include(request => request.Statuses)
            .Where(request => request.CreatorId == userId)
            .Select(request => new RequestDto
            {
                Id = request.Id,
                Number = request.Number,
                Title = request.Title,
                Description = request.Description,
                Images = request.Images,
                Created = request.Created,
                IncidentPointList = request.IncidentPointList,
                IncidentPointListAsString = request.IncidentPointListAsString,
                CurrentState = request.CurrentState,
            }).ToListAsync();

        return Ok(requests);
    }
    
    /// <summary>
    /// Добавить заявку
    /// </summary>
    /// <param name="requestAddDto">Данные по заявке</param>
    /// <response code="200">Заявка успешно добавлена</response>
    /// <response code="400">Переданны некорректные данные</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRequest([FromBody] RequestAddDto requestAddDto)
    {
        var creatorInfo = GetAuthUserInfo();
        if (creatorInfo is null)
            return Unauthorized();
        
        if (!ModelState.IsValid)
            return BadRequest();
        
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = requestAddDto.Title,
            Description = requestAddDto.Description,
            Images = requestAddDto.Images,
            CreatorId = creatorInfo.GuidId,
            CreatorName = creatorInfo.FullName,
            IncidentPointList = requestAddDto.IncidentPointList,
            IncidentPointListAsString = requestAddDto.IncidentPointListAsString,
        };

        var requestNew = new RequestStatus
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            State = RequestStates.New,
            Comment = "Заявка добавлена",
            CreatorId = creatorInfo.GuidId,
            CreatorName = creatorInfo.FullName,
        };
        
        await _context.Requests.AddAsync(request);
        await _context.RequestStatuses.AddAsync(requestNew);
        await _context.SaveChangesAsync();

        return Ok(request.Id);
    }

    #endregion

    #region RequestComment

    /// <summary>
    /// Получить комментарии к заявке
    /// </summary>
    /// <param name="requestId">Идентификатор заявки</param>
    /// <response code="200">Список комментариев к заявке</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{requestId:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestCommentDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestComments([FromRoute] Guid requestId)
    {
        var comments = await _context.RequestComments
            .Where(comment  => comment.RequestId == requestId)
            .Select(comment => new RequestCommentDto
            {
                Text = comment.Text,
                Images = comment.Images,
                CreatorName = comment.CreatorName,
                Created = comment.Created,
            }).ToListAsync();
        
        return Ok(comments);
    }

    /// <summary>
    /// Добавить комментарий к заявке
    /// </summary>
    /// <param name="requestId">Идентификатор заявки</param>
    /// <param name="commentAddDto">Данные по комментарию</param>
    /// <response code="200">Комментарий к заявке успешно добавлен</response>
    /// <response code="400">Переданны некорректные данные</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="404">Заявка не найдена</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpPost("{requestId:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRequestComment([FromRoute] Guid requestId, [FromBody] RequestCommentAddDto commentAddDto)
    {
        var creatorInfo = GetAuthUserInfo();
        if (creatorInfo is null)
            return Unauthorized();
        
        if (!ModelState.IsValid)
            return BadRequest(commentAddDto);

        var request = await _context.Requests.FirstOrDefaultAsync(request => request.Id == requestId);
        if (request is null)
            return NotFound();
        
        var comment = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            Text = commentAddDto.Text,
            Images = commentAddDto.Images,
            CreatorId = creatorInfo.GuidId,
            CreatorName = creatorInfo.FullName,
        };

        await _context.RequestComments.AddAsync(comment);
        await _context.SaveChangesAsync();
        return Ok();
    }

    #endregion

    #region RequestStatus

    /// <summary>
    /// Получить статусы (историю) заявки
    /// </summary>
    /// <param name="requestId">Идентификатор заявки</param>
    /// <response code="200">Список статусов заявки</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{requestId:guid}/statuses")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestStatusDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestStatuses([FromRoute] Guid requestId)
    {
        var statuses = await _context.RequestStatuses
            .Where(status => status.RequestId == requestId)
            .Select(status => new RequestStatusDto
            {
                State = status.State,
                Comment = status.Comment,
                Created = status.Created,
            }).ToListAsync();
        
        return Ok(statuses);
    }
    
    /// <summary>
    /// Добавить статус к заявке
    /// </summary>
    /// <param name="requestId">Идентификатор заявки</param>
    /// <param name="requestStatusAddDto">Данные по статусу</param>
    /// <response code="200">Статус к заявке успешно добавлен</response>
    /// <response code="400">Переданны некорректные данные</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="404">Заявка не найдена</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpPost("{requestId:guid}/statuses")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRequestStatus([FromRoute] Guid requestId, [FromBody] RequestStatusAddDto requestStatusAddDto)
    {
        var creatorInfo = GetAuthUserInfo();
        if (creatorInfo is null)
            return Unauthorized();
        
        if (!ModelState.IsValid)
            return BadRequest(requestStatusAddDto);

        var request = await _context.Requests.FirstOrDefaultAsync(request => request.Id == requestId);
        if (request is null)
            return NotFound();
        
        var status = new RequestStatus
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            State = requestStatusAddDto.State,
            Comment = requestStatusAddDto.Comment,
            CreatorId = creatorInfo.GuidId,
            CreatorName = creatorInfo.FullName,
        };

        await _context.RequestStatuses.AddAsync(status);
        await _context.SaveChangesAsync();
        return Ok();
    }

    #endregion

    #region Claims
    
    private AuthUserInfo? GetAuthUserInfo()
    {
        string? authHeader = Request.Headers["Authorization"];
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

    #endregion
}
