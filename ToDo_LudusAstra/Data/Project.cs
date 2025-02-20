namespace ToDo_LudusAstra.Data;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }  // Название проекта
    public string Description { get; set; }  // Описание проекта
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Руководитель проекта (создатель)
    public int OwnerId { get; set; }
    public User Owner { get; set; }

    // Связь: проект может включать нескольких пользователей
    public ICollection<ProjectUser> ProjectUsers { get; set; }
    public ICollection<Task> Tasks { get; set; }
}