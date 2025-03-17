using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ToDo_LudusAstra.AIAgent;
using ToDo_LudusAstra.Data;

namespace ToDo_LudusAstra.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubTaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubTaskController(AppDbContext context)
        {
            _context = context;
        }
        
        // 1. Создание задачи в проекте
        [HttpPost("generate-sub-task")]
        public async Task<IActionResult> GegerateSubTask([FromBody] GenerateSubTaskRequest model)
        {
            const string template_prompt = "\n\n\n above is the task description, break this task into subtasks and give the answer as [\"option\", \"option\"]";
            var chatGpt = new ChatGpt();
            var response = await chatGpt.ChatStreamAsync(model.Description + template_prompt);
            
            // Убираем лишние символы (```json и ```)
            string jsonString = response.Replace("```json", "").Replace("```", "").Trim();

            // Десериализуем JSON-строку в массив строк
            string[] tasks = JsonConvert.DeserializeObject<string[]>(jsonString);
            return Ok(tasks);
        }
        
    }
}

// Модель запроса для создания под задачи
public class GenerateSubTaskRequest
{
    public string Description { get; set; }
}
