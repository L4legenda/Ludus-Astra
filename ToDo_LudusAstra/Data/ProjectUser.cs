namespace ToDo_LudusAstra.Data;

public class ProjectUser
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public ProjectRole Role { get; set; }  // Роль в проекте (участник, руководитель)
}

// Перечисление ролей
public enum ProjectRole
{
    Owner,  // Создатель проекта
    Member  // Участник
}