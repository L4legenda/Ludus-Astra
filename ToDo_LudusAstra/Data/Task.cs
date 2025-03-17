using System.ComponentModel;

namespace ToDo_LudusAstra.Data;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; }  // Заголовок задачи
    public string Description { get; set; }  // Описание задачи
    public DateTime Deadline { get; set; }  // Дата дедлайна
    public TaskStatus Status { get; set; } = TaskStatus.New;  // Статус задачи
    public TaskPriority Priority { get; set; } = TaskPriority.Low;  // Приоритет задачи
    public string SubTasks { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int exp { get; set; } = 0;
    
    // Связь с проектом
    public int ProjectId { get; set; }
    public Project Project { get; set; }

    // Автор задачи (кто поставил)
    public int CreatorId { get; set; }
    public User Creator { get; set; }

    // Ответственные за выполнение
    public ICollection<TaskAssignment> TaskAssignments { get; set; }
    public ICollection<TaskComment> Comments { get; set; }
    public ICollection<TaskAttachment> Attachments { get; set; }
    public ICollection<TaskChecklistItem> Checklist { get; set; }
}



// Перечисление статусов задачи
public enum TaskStatus
{
    [Description("Новая")]
    New,
    
    [Description("В процессе")]
    InProgress,
    
    [Description("На проверке")]
    OnReview,
    
    [Description("Завершена")]
    Completed,
    
    [Description("Отменена")]
    Cancelled
}

// Перечисление приоритетов задачи
public enum TaskPriority
{
    [Description("Высокий")]
    High,
    
    [Description("Средний")]
    Medium,
    
    [Description("Низкий")]
    Low
}