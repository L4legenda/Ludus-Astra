namespace ToDo_LudusAstra.Data;

public class Notification
{
    public int Id { get; set; }
    public string Message { get; set; }  // Текст уведомления
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Получатель уведомления
    public int UserId { get; set; }
    public User User { get; set; }
}