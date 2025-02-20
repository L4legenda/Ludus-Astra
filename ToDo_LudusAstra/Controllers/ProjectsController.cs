using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_LudusAstra.Data;

namespace ToDo_LudusAstra.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }
        // 1. Создание нового проекта и добавление владельца
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var project = new Project
        {
            Name = model.Name,
            Description = model.Description,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Добавляем владельца в список участников проекта
        var projectUser = new ProjectUser
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = ProjectRole.Owner
        };

        _context.ProjectUsers.Add(projectUser);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Проект создан", projectId = project.Id });
    }

    // 2. Получение всех проектов пользователя
    [HttpGet]
    public async Task<IActionResult> GetMyProjects()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var projects = await _context.ProjectUsers
            .Where(pu => pu.UserId == userId)
            .Include(pu => pu.Project)
            .Select(pu => new
            {
                id = pu.Project.Id,
                name = pu.Project.Name,
                description = pu.Project.Description,
                ownerId = pu.Project.OwnerId,
                createdAt = pu.Project.CreatedAt
            })
            .ToListAsync();

        return Ok(projects);
    }

    // 3. Получение информации о конкретном проекте
    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProject(int projectId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var project = await _context.Projects
            .Where(p => p.Id == projectId)
            .Include(p => p.ProjectUsers)
            .ThenInclude(pu => pu.User)
            .FirstOrDefaultAsync();

        if (project == null)
            return NotFound(new { message = "Проект не найден" });

        // Проверяем, связан ли пользователь с проектом
        bool isUserInProject = project.ProjectUsers.Any(pu => pu.UserId == userId);
        if (!isUserInProject)
            return Forbid(); // Запрещаем доступ

        return Ok(new
        {
            id = project.Id,
            name = project.Name,
            description = project.Description,
            ownerId = project.OwnerId,
            createdAt = project.CreatedAt,
            users = project.ProjectUsers.Select(pu => new
            {
                id = pu.User.Id,
                name = pu.User.FullName,
                email = pu.User.Email,
                role = pu.Role.ToString()
            })
        });
    }

    // 4. Добавление пользователей в проект (только владельцем)
    [HttpPost("{projectId}/add-user")]
    public async Task<IActionResult> AddUserToProject(int projectId, [FromBody] AddUserToProjectRequest model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var project = await _context.Projects
            .Include(p => p.ProjectUsers)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound(new { message = "Проект не найден" });

        if (project.OwnerId != userId)
            return Forbid(); // Только владелец может добавлять пользователей

        var userToAdd = await _context.Users.FindAsync(model.UserId);
        if (userToAdd == null)
            return NotFound(new { message = "Пользователь не найден" });

        if (project.ProjectUsers.Any(pu => pu.UserId == model.UserId))
            return BadRequest(new { message = "Пользователь уже в проекте" });

        var projectUser = new ProjectUser
        {
            ProjectId = projectId,
            UserId = model.UserId,
            Role = model.Role ?? ProjectRole.Member // По умолчанию обычный участник
        };

        _context.ProjectUsers.Add(projectUser);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Пользователь добавлен в проект" });
    }

    // 5. Удаление пользователя из проекта (только владельцем)
    [HttpDelete("{projectId}/remove-user/{userId}")]
    public async Task<IActionResult> RemoveUserFromProject(int projectId, int userId)
    {
        var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var project = await _context.Projects
            .Include(p => p.ProjectUsers)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound(new { message = "Проект не найден" });

        if (project.OwnerId != ownerId)
            return Forbid(); // Только владелец может удалять пользователей

        var projectUser = project.ProjectUsers.FirstOrDefault(pu => pu.UserId == userId);
        if (projectUser == null)
            return NotFound(new { message = "Пользователь не найден в проекте" });

        _context.ProjectUsers.Remove(projectUser);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Пользователь удалён из проекта" });
    }

    // 6. Удаление проекта (только владелец)
    [HttpDelete("{projectId}")]
    public async Task<IActionResult> DeleteProject(int projectId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var project = await _context.Projects.FindAsync(projectId);

        if (project == null)
            return NotFound(new { message = "Проект не найден" });

        if (project.OwnerId != userId)
            return Forbid();

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Проект удалён" });
    }
    }
}

// Модель запроса для создания проекта
public class CreateProjectRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}

// Модель запроса для добавления пользователя в проект
public class AddUserToProjectRequest
{
    public int UserId { get; set; }
    public ProjectRole? Role { get; set; } // Роль по умолчанию "Member"
}