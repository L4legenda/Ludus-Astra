using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo_LudusAstra.Data;
using TaskStatus = ToDo_LudusAstra.Data.TaskStatus;


namespace ToDo_LudusAstra.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }
        
        // Поиск пользователей с фильтрами
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? name)
        {
            var today = DateTime.UtcNow;
            var nextWeek = today.AddDays(7);

            // Запрос всех пользователей
            var query = _context.Users
                .Include(u => u.TaskAssignments)
                .ThenInclude(ta => ta.Task)
                .AsQueryable();

            // Фильтрация по имени (если передано в запросе)
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(u => EF.Functions.ILike(u.FullName, $"%{name}%"));
            }

            var allUsers = await query.ToListAsync();
            
            // Метод для вычисления опыта пользователя
            int CalculateExp(User u) =>
                u.TaskAssignments
                    .Where(ta => ta.Task.Status == TaskStatus.Completed)
                    .Sum(ta => ta.Task.exp);

            // Категория 1: Свободные пользователи (нет активных задач)
            var freeUsers = allUsers
                .Where(u => !u.TaskAssignments.Any(ta => ta.Task.Status != TaskStatus.Completed && ta.Task.Status != TaskStatus.Cancelled))
                .Select(u => new
                {
                    id = u.Id,
                    name = u.FullName,
                    email = u.Email,
                    profilePictureUrl = u.ProfilePictureUrl,
                    exp = CalculateExp(u),
                })
                .ToList();

            // Категория 2: Скоро освободятся (есть задачи с дедлайном ≤ 7 дней)
            var soonFreeUsers = allUsers
                .Where(u => u.TaskAssignments.Any(ta =>
                    (ta.Task.Status == TaskStatus.InProgress || ta.Task.Status == TaskStatus.OnReview) &&
                    ta.Task.Deadline >= today && ta.Task.Deadline <= nextWeek))
                .Select(u => new
                {
                    id = u.Id,
                    name = u.FullName,
                    email = u.Email,
                    profilePictureUrl = u.ProfilePictureUrl,
                    exp = CalculateExp(u),
                    tasks = u.TaskAssignments
                        .Where(ta => ta.Task.Deadline >= today && ta.Task.Deadline <= nextWeek)
                        .Select(ta => new
                        {
                            id = ta.Task.Id,
                            title = ta.Task.Title,
                            deadline = ta.Task.Deadline,
                            status = ta.Task.Status
                        })
                        .ToList()
                })
                .ToList();

            // Категория 3: Остальные (с активными задачами)
            var busyUsers = allUsers
                .Where(u => !freeUsers.Any(fu => fu.id == u.Id) && !soonFreeUsers.Any(sfu => sfu.id == u.Id))
                .Select(u => new
                {
                    id = u.Id,
                    name = u.FullName,
                    email = u.Email,
                    profilePictureUrl = u.ProfilePictureUrl,
                    exp = CalculateExp(u),
                    tasks = u.TaskAssignments.Select(ta => new
                    {
                        id = ta.Task.Id,
                        title = ta.Task.Title,
                        deadline = ta.Task.Deadline,
                        status = ta.Task.Status
                    })
                    .ToList()
                })
                .ToList();

            return Ok(new
            {
                freeUsers,
                soonFreeUsers,
                busyUsers
            });
        }
    }
}
