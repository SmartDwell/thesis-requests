using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
public class RequestsController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly ILogger<RequestsController> _logger;

    /// <summary>
    /// Контроллер класса <see cref="RequestsController"/>
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="logger">Логгер</param>
    /// <exception cref="ArgumentNullException">Аргумент не инициализирован</exception>
    public RequestsController(DatabaseContext context, ILogger<RequestsController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Получить заявки
    /// </summary>
    /// <param name="state">Состояние заявки</param>
    /// <response code="200">Список всех заявок</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestDto>))]
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
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestDto>))]
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
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{requestId:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestCommentDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestComments([FromRoute] Guid requestId)
    {
        var comments = await _context.RequestComments
            .Where(comment  => comment.RequestId == requestId)
            .Select(comment => new RequestCommentDto
            {
                Text = comment.Text,
                Images = comment.Images,
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
    /// <response code="404">Заявка не найдена</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpPost("{requestId:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRequestComment([FromRoute] Guid requestId, [FromBody] RequestCommentAddDto commentAddDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(commentAddDto);

        var request = await _context.Requests.FirstOrDefaultAsync(request => request.Id == requestId);
        if (request is null)
            return NotFound();

        var fakeCreatorId = Guid.NewGuid();
        var comment = new RequestComment
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            Text = commentAddDto.Text,
            Images = commentAddDto.Images,
            CreatorId = fakeCreatorId,
        };

        await _context.RequestComments.AddAsync(comment);
        await _context.SaveChangesAsync();
        return Ok();
    }
    
    /// <summary>
    /// Получить статусы (историю) заявки
    /// </summary>
    /// <param name="requestId">Идентификатор заявки</param>
    /// <response code="200">Список статусов заявки</response>
    /// <response code="500">Ошибка сервера</response>
    [HttpGet("{requestId:guid}/statuses")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RequestStatusDto>))]
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
    /// <response code="500">Ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddRequest([FromBody] RequestAddDto requestAddDto)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        
        var fakeCreatorId = Guid.NewGuid();
        var request = new Request
        {
            Id = Guid.NewGuid(),
            Title = requestAddDto.Title,
            Description = requestAddDto.Description,
            Images = requestAddDto.Images,
            CreatorId = fakeCreatorId,
            IncidentPointList = requestAddDto.IncidentPointList,
        };

        var requestNew = new RequestStatus
        {
            Id = Guid.NewGuid(),
            RequestId = request.Id,
            State = RequestStates.New,
            Comment = "Заявка добавлена",
            CreatorId = fakeCreatorId,
        };
        
        await _context.Requests.AddAsync(request);
        await _context.RequestStatuses.AddAsync(requestNew);
        await _context.SaveChangesAsync();

        return Ok(request.Id);
    }
}
