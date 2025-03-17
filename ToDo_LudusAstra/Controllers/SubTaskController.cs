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
            if (string.IsNullOrEmpty(model.Description))
            {
                return BadRequest("Description is required");
            }

            const string template_prompt = "\n\n\n above is the task description, break this task into subtasks and give the answer as [\"option\", \"option\"]";
            var chatGpt = new ChatGpt();

            try
            {
                var response = await chatGpt.ChatStreamAsync(model.Description + template_prompt);

                if (string.IsNullOrEmpty(response))
                {
                    return StatusCode(500, "Empty response from ChatGPT");
                }

                string jsonString = response.Replace("```json", "").Replace("```", "").Trim();

                if (string.IsNullOrEmpty(jsonString) || !jsonString.StartsWith("[") || !jsonString.EndsWith("]"))
                {
                    return BadRequest("Invalid JSON format");
                }

                string[] tasks = JsonConvert.DeserializeObject<string[]>(jsonString);

                if (tasks == null || tasks.Length == 0)
                {
                    return NotFound("No tasks generated");
                }

                return Ok(tasks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
    }
}

// Модель запроса для создания под задачи
public class GenerateSubTaskRequest
{
    public string Description { get; set; }
}
