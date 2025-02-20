using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_LudusAstra.Data;
using Task = ToDo_LudusAstra.Data.Task;
using TaskStatus = ToDo_LudusAstra.Data.TaskStatus;

namespace ToDo_LudusAstra.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }
        
        // 1. Создание задачи в проекте
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == model.ProjectId);

            if (project == null)
                return NotFound(new { message = "Проект не найден" });

            // Проверяем, является ли пользователь владельцем или участником проекта
            bool isUserInProject = project.ProjectUsers.Any(pu => pu.UserId == userId);
            if (!isUserInProject)
                return Forbid();

            var task = new Task
            {
                Title = model.Title,
                Description = model.Description,
                Deadline = model.Deadline,
                Status = TaskStatus.New,
                CreatorId = userId,
                ProjectId = model.ProjectId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Назначаем ответственных пользователей
            foreach (var assigneeId in model.AssigneeIds)
            {
                _context.TaskAssignments.Add(new TaskAssignment
                {
                    TaskId = task.Id,
                    UserId = assigneeId
                });
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "Задача создана", taskId = task.Id });
        }
        
        // 2. Получение всех задач пользователя (сортировка по проекту, дедлайну)
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTasks([FromQuery] int? projectId, [FromQuery] DateTime? deadline)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var tasksQuery = _context.Tasks
                .Where(t => t.TaskAssignments.Any(ta => ta.UserId == userId) || t.CreatorId == userId)
                .Include(t => t.Project)
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .AsQueryable();

            if (projectId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.ProjectId == projectId.Value);

            if (deadline.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Deadline.Date == deadline.Value.Date);

            var tasks = await tasksQuery.Select(t => new
            {
                id = t.Id,
                title = t.Title,
                description = t.Description,
                deadline = t.Deadline,
                status = t.Status.ToString(),
                project = new { id = t.Project.Id, name = t.Project.Name },
                assignees = t.TaskAssignments.Select(ta => new
                {
                    id = ta.User.Id,
                    name = ta.User.FullName
                })
            }).ToListAsync();

            return Ok(tasks);
        }
        
        // 3. Обновление задачи (изменение статуса, дедлайна, исполнителей)
        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateTaskRequest model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var task = await _context.Tasks
                .Include(t => t.TaskAssignments)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                return NotFound(new { message = "Задача не найдена" });

            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == task.ProjectId);

            bool isUserInProject = project.ProjectUsers.Any(pu => pu.UserId == userId);
            if (!isUserInProject)
                return Forbid();

            bool isTaskCreator = task.CreatorId == userId;
            bool isTaskAssignee = task.TaskAssignments.Any(ta => ta.UserId == userId);

            if (model.Title != null) task.Title = model.Title;
            if (model.Description != null) task.Description = model.Description;
            if (model.Deadline.HasValue) task.Deadline = model.Deadline.Value;

            // Изменение статуса задачи (может менять только создатель или исполнитель)
            if (model.Status.HasValue)
            {
                if (!isTaskCreator && !isTaskAssignee)
                    return StatusCode(403, new { message = "Вы не можете менять статус этой задачи" });

                if (!Enum.IsDefined(typeof(TaskStatus), model.Status.Value))
                    return BadRequest(new { message = "Некорректный статус задачи" });

                task.Status = model.Status.Value;
            }

            // Обновление списка ответственных пользователей (может менять только создатель)
            if (model.AssigneeIds != null)
            {
                if (!isTaskCreator)
                    return StatusCode(403, new { message = "Только создатель задачи может менять ответственных пользователей" });

                _context.TaskAssignments.RemoveRange(task.TaskAssignments);
                foreach (var assigneeId in model.AssigneeIds)
                {
                    _context.TaskAssignments.Add(new TaskAssignment
                    {
                        TaskId = task.Id,
                        UserId = assigneeId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Задача обновлена" });
        }

        
        // 4. Удаление задачи (только создателем)
        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var task = await _context.Tasks.FindAsync(taskId);

            if (task == null)
                return NotFound(new { message = "Задача не найдена" });

            if (task.CreatorId != userId)
                return Forbid();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Задача удалена" });
        }
        
        // Получение всех задач в конкретном проекте (доступно только участникам проекта)
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(int projectId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Проверяем, есть ли такой проект и является ли пользователь его участником
            var project = await _context.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return NotFound(new { message = "Проект не найден" });

            bool isUserInProject = project.ProjectUsers.Any(pu => pu.UserId == userId);
            if (!isUserInProject)
                return StatusCode(403, new { message = "Вы не являетесь участником проекта" });

            // Получаем все задачи проекта
            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .OrderBy(t => t.Deadline) // Сортируем по дедлайну
                .Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    description = t.Description,
                    deadline = t.Deadline,
                    status = t.Status.ToString(),
                    assignees = t.TaskAssignments.Select(ta => new
                    {
                        id = ta.User.Id,
                        name = ta.User.FullName
                    })
                })
                .ToListAsync();

            return Ok(tasks);
        }

        
        // Получение всех возможных статусов задачи
        [HttpGet("statuses")]
        public IActionResult GetTaskStatuses()
        {
            var statuses = Enum.GetValues(typeof(TaskStatus))
                .Cast<TaskStatus>()
                .Select(status => new { id = (int)status, name = status.ToString() })
                .ToList();

            return Ok(statuses);
        }
        
        
    }
    
    // Модель запроса для создания задачи
    public class CreateTaskRequest
    {
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public List<int> AssigneeIds { get; set; } = new List<int>(); // Ответственные пользователи
    }

// Модель запроса для обновления задачи
    public class UpdateTaskRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }
        public TaskStatus? Status { get; set; }
        public List<int> AssigneeIds { get; set; } = new List<int>();
    }
}

