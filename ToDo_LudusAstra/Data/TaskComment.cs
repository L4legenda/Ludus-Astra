namespace ToDo_LudusAstra.Data;

public class TaskComment
{
    public int Id { get; set; }
    public string Content { get; set; }  // Текст комментария
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Связь с задачей
    public int TaskId { get; set; }
    public Task Task { get; set; }

    // Автор комментария
    public int UserId { get; set; }
    public User User { get; set; }
}