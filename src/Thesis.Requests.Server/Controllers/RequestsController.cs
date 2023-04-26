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
        var creatorId = GetAuthUserId();
        var creatorFullname = GetAuthUserFullname();
        if (creatorId is null || string.IsNullOrEmpty(creatorFullname))
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
            CreatorId = (Guid) creatorId,
            CreatorName = creatorFullname,
        };

        await _context.RequestComments.AddAsync(comment);
        await _context.SaveChangesAsync();
        return Ok();
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
        var creatorId = GetAuthUserId();
        var creatorFullname = GetAuthUserFullname();
        if (creatorId is null || string.IsNullOrEmpty(creatorFullname))
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
            CreatorId = (Guid) creatorId,
            CreatorName = creatorFullname,
        };

        await _context.RequestStatuses.AddAsync(status);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
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
        var creatorId = GetAuthUserId();
        var creatorFullname = GetAuthUserFullname();
        if (creatorId is null || string.IsNullOrEmpty(creatorFullname))
            return Unauthorized();
        
        if (!ModelState.IsValid)
            return BadRequest();
        
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = requestAddDto.Title,
            Description = requestAddDto.Description,
            Images = requestAddDto.Images,
            CreatorId = (Guid) creatorId,
            CreatorName = creatorFullname,
            IncidentPointList = requestAddDto.IncidentPointList,
            IncidentPointListAsString = requestAddDto.IncidentPointListAsString,
        };

        var requestNew = new RequestStatus
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            State = RequestStates.New,
            Comment = "Заявка добавлена",
            CreatorId = (Guid) creatorId,
            CreatorName = creatorFullname,
        };
        
        await _context.Requests.AddAsync(request);
        await _context.RequestStatuses.AddAsync(requestNew);
        await _context.SaveChangesAsync();

        return Ok(request.Id);
    }

    #region Claims
    
    private bool GetJwtClaims(out ClaimsPrincipal? claims, out DateTime validTo)
    {
        string? authHeader = Request.Headers["Authorization"];
        var token = authHeader?.Replace("Bearer ", "") ?? string.Empty;

        return _jwtReader.ReadAccessToken(token, out claims, out validTo);
    }

    private Guid? GetAuthUserId()
    {
        _ = GetJwtClaims(out var claims, out var validTo);
        var creatorClaimId = claims?.Claims.FirstOrDefault(a => a.Type == ClaimsIdentity.DefaultIssuer);
        if (creatorClaimId is null) return null;

        var parsed = Guid.TryParse(creatorClaimId.Value, out var creatorId);
        return parsed ? creatorId : null;
    }
    
    private string? GetAuthUserFullname()
    {
        _ = GetJwtClaims(out var claims, out var validTo);
        var creatorFullname = claims?.Claims.FirstOrDefault(a => a.Type == ClaimsIdentity.DefaultNameClaimType);
        return creatorFullname?.Value;
    }

    #endregion
}
