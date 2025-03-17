using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using ToDo_LudusAstra.Data;
using ToDo_LudusAstra.Services;
using TaskStatus = ToDo_LudusAstra.Data.TaskStatus;

namespace ToDo_LudusAstra.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, IConfiguration configuration, JwtService jwtService)
        {
            _context = context;
            _configuration = configuration;
            _jwtService = jwtService;
        }
        
        // 1. Регистрация пользователя
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest(new { message = "Email уже зарегистрирован" });
            }

            string avatarUrl = null;

            if (model.Avatar != null)
            {
                // Проверяем размер файла (не больше 5MB)
                if (model.Avatar.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "Размер файла не должен превышать 5MB" });

                // Проверяем расширение файла (разрешены только .jpg, .png)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(model.Avatar.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Разрешены только изображения формата .jpg, .png" });

                // Создаём путь для сохранения файла
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Генерируем уникальное имя файла
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Сохраняем файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(stream);
                }

                // URL для доступа к изображению
                avatarUrl = $"/uploads/avatars/{uniqueFileName}";
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                ProfilePictureUrl = avatarUrl
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Регистрация успешна", userId = user.Id, avatarUrl });
        }
        // 2. Авторизация и получение JWT-токена
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Неверные учетные данные" });
            }

            // Генерация JWT токена
            var token = _jwtService.GenerateToken(user);

            return Ok(new { Token = token });
        }
        
        // 3. Получение информации о пользователе
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound(new { message = "Пользователь не найден" });
            
            var completedTasks = await _context.Tasks
                .Where(t => t.TaskAssignments.Any(ta => ta.UserId == userId) && t.Status == TaskStatus.Completed)
                .ToListAsync();

            // Суммируем опыт из завершённых задач
            int totalExp = completedTasks.Sum(t => t.exp);
            
            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                exp = totalExp,
                profilePictureUrl = user.ProfilePictureUrl
            });
        }
        
        // 4. Проверка авторизации
        [Authorize]
        [HttpGet("check")]
        public async Task<IActionResult> check()
        {

            return Ok(new {});
        }

    }
}

// Модель запроса регистрации
public class RegisterRequest
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public IFormFile? Avatar { get; set; } // Фото профиля
}

// Модель запроса авторизации
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}