using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seljmov.AspNet.Commons.Helpers;
using Thesis.Requests.Contracts.Request;
using Thesis.Requests.Contracts.RequestComment;
using Thesis.Requests.Contracts.RequestStatus;
using Thesis.Requests.Contracts.Search;
using Thesis.Requests.Model;

namespace Thesis.Requests.Server.Controllers;

/// <summary>
/// Контроллер для работы с заявками жильцов
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RequestsController : ControllerBase
{
    private const double MinimalSearchScore = 0.6;
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
    public RequestsController(DatabaseContext context, JwtReader jwtReader, ILogger<RequestsController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _jwtReader = jwtReader ?? throw new ArgumentNullException(nameof(jwtReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                IncidentPointId = request.IncidentPointId,
                IncidentPointFullName = request.IncidentPointFullName,
                IsEdited = request.IsEdited,
                CurrentState = request.CurrentState,
            }).ToListAsync();
        
        return Ok(state is null 
            ? requests 
            : requests.Where(request => request.CurrentState == state));
    }
    
    /// <summary>
    /// Получить заявку по идентификатору
    /// </summary>
    /// <param name="requestId">Идентификатор заявки</param>
    /// <response code="200">Заявка</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="404">Заявка не найдена</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{requestId:guid}/details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestDetails([FromRoute] Guid requestId)
    {
        var request = await _context.Requests
            .Include(request => request.Statuses)
            .FirstOrDefaultAsync(request => request.Id == requestId);
            
        if (request is null)
            return NotFound("Заявка не найдена");

        var requestDto = new RequestDto
        {
            Id = request.Id,
            Number = request.Number,
            Title = request.Title,
            Description = request.Description,
            Images = request.Images,
            Created = request.Created,
            IncidentPointId = request.IncidentPointId,
            IncidentPointFullName = request.IncidentPointFullName,
            IsEdited = request.IsEdited,
            CurrentState = request.CurrentState,
        };
        
        return Ok(requestDto);
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
                IncidentPointId = request.IncidentPointId,
                IncidentPointFullName = request.IncidentPointFullName,
                IsEdited = request.IsEdited,
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
            Created = DateTime.UtcNow,
            CreatorId = creatorInfo.GuidId,
            CreatorName = creatorInfo.FullName,
            CreatorContact = $"{creatorInfo.Email}, {creatorInfo.Phone}",
            IncidentPointId = requestAddDto.IncidentPointId,
            IncidentPointFullName = requestAddDto.IncidentPointFullName,
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
    
    /// <summary>
    /// Редактировать заявку
    /// </summary>
    /// <param name="requestId">Идентификатор заявки</param>
    /// <param name="requestEditDto">Данные по заявке</param>
    /// <response code="200">Заявка успешно обновлена</response>
    /// <response code="400">Переданны некорректные данные</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="404">Заявка не найдена</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpPatch("{requestId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RequestDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchRequest([FromRoute] Guid requestId, [FromBody] RequestEditDto requestEditDto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var request = await _context.Requests
            .Include(item => item.Statuses)
            .FirstOrDefaultAsync(item => item.Id == requestId);
        
        if (request is null)
            return NotFound();

        request.Title = requestEditDto.Title;
        request.Description = requestEditDto.Description;
        request.IsEdited = true;

        _context.Requests.Update(request);
        await _context.SaveChangesAsync();

        return Ok(request);
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
    /// <response code="204">Комментарий к заявке успешно добавлен</response>
    /// <response code="400">Переданны некорректные данные</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="404">Заявка не найдена</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpPost("{requestId:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
            Created = DateTime.UtcNow
        };

        await _context.RequestComments.AddAsync(comment);
        await _context.SaveChangesAsync();
        return NoContent();
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
                Comment = status.Comment ?? string.Empty,
                Created = status.Created,
            }).ToListAsync();
        
        return Ok(statuses);
    }
    
    /// <summary>
    /// Добавить статус к заявке
    /// </summary>
    /// <param name="requestId">Идентификатор заявки</param>
    /// <param name="requestStatusAddDto">Данные по статусу</param>
    /// <response code="204">Статус к заявке успешно добавлен</response>
    /// <response code="400">Переданны некорректные данные</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="404">Заявка не найдена</response>
    /// <response code="409">Ошибка в статусе заявки</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpPost("{requestId:guid}/statuses")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRequestStatus([FromRoute] Guid requestId, [FromBody] RequestStatusAddDto requestStatusAddDto)
    {
        var creatorInfo = GetAuthUserInfo();
        if (creatorInfo is null)
            return Unauthorized();
        
        if (!ModelState.IsValid)
            return BadRequest(requestStatusAddDto);

        var request = await _context.Requests
            .Include(request => request.Statuses)
            .FirstOrDefaultAsync(request => request.Id == requestId);
        if (request is null)
            return NotFound();

        var requestStatuses = request.Statuses
            .OrderBy(status => status.Created)
            .Select(status => status.State).ToList();
        var currentState = requestStatuses[^1];
        if (currentState == RequestStates.CancelledByResident)
            return Conflict("Невозможно изменить статус заявки, потому что она была отменена жителем!");
        if (currentState == RequestStates.RejectedByDispatcher)
            return Conflict("Невозможно изменить статус заявки, потому что она была отклонена диспетчером!");
        if (requestStatuses.Contains(requestStatusAddDto.State))
            return Conflict("Ошибка! Передан повторный статус!");
        
        var status = new RequestStatus
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            State = requestStatusAddDto.State,
            Comment = requestStatusAddDto.Comment,
            CreatorId = creatorInfo.GuidId,
            CreatorName = creatorInfo.FullName,
            Created = DateTime.UtcNow
        };

        await _context.RequestStatuses.AddAsync(status);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    #endregion

    #region Extensions

    /// <summary>
    /// Получить список заявок, которые похожи на переданную строку поиска
    /// </summary>
    /// <param name="search">Строка поиска</param>
    /// <response code="200">Список заявок, которые похожи на переданную строку поиска</response>
    /// <response code="401">Токен доступа истек</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("similars")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SearchResultDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSimilars([FromQuery] string search)
    {
        var similars = await _context.Requests
            .Select(req => new SearchResultDto
            {
                Id = req.Id,
                Name = $"{req.Title} {req.Description}",
            }).ToListAsync();

        var filtered = similars
            .OrderByDescending(dto => dto.Score(search))
            //.Where(dto => dto.Score(search) > MinimalSearchScore)
            .Take(3);

        return Ok(filtered);
    }

    #endregion

    #region Assets

    /// <summary>
    /// Получить список заявок активов
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("assets")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dictionary<Guid, List<RequestDto>>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestsByAssetId()
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
                IncidentPointId = request.IncidentPointId,
                IncidentPointFullName = request.IncidentPointFullName,
                IsEdited = request.IsEdited,
                CurrentState = request.CurrentState,
            })
            .GroupBy(dto => dto.IncidentPointId)
            .ToDictionaryAsync(dtos => dtos.Key, dtos => dtos.ToList());
        
        return Ok(requests);
    }
    
    /// <summary>
    /// Получить список заявок актива по его идентификатору
    /// </summary>
    /// <param name="assetId">Идентификатор актива</param>
    [AllowAnonymous]
    [HttpGet("assets/{assetId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RequestDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssetRequests([FromRoute] Guid assetId)
    {
        var requests = await _context.Requests
            .Include(request => request.Statuses)
            .Where(request => request.IncidentPointId == assetId)
            .Select(request => new RequestDto
            {
                Id = request.Id,
                Number = request.Number,
                Title = request.Title,
                Description = request.Description,
                Images = request.Images,
                Created = request.Created,
                IncidentPointId = request.IncidentPointId,
                IncidentPointFullName = request.IncidentPointFullName,
                IsEdited = request.IsEdited,
                CurrentState = request.CurrentState,
            }).ToListAsync();
        
        return Ok(requests);
    }
    
    /// <summary>
    /// Получить список заявок актива по его имени
    /// </summary>
    /// <param name="name">Имя актива</param>
    [AllowAnonymous]
    [HttpGet("assets/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RequestDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssetRequestsByName([FromRoute] string name)
    {
        var requests = await _context.Requests
            .Include(request => request.Statuses)
            .Where(request => request.IncidentPointFullName.ToLower().Contains(name.ToLower()))
            .Select(request => new RequestDto
            {
                Id = request.Id,
                Number = request.Number,
                Title = request.Title,
                Description = request.Description,
                Images = request.Images,
                Created = request.Created,
                IncidentPointId = request.IncidentPointId,
                IncidentPointFullName = request.IncidentPointFullName,
                IsEdited = request.IsEdited,
                CurrentState = request.CurrentState,
            }).ToListAsync();
        
        return Ok(requests);
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
