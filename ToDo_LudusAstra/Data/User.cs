namespace ToDo_LudusAstra.Data;

public class User
{

    public int Id { get; set; }
    public string FullName { get; set; }  // Полное имя пользователя
    public string Email { get; set; }  // Уникальный email для входа
    public string PasswordHash { get; set; }  // Захешированный пароль
    public string? ProfilePictureUrl { get; set; }  // URL аватара
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Дата регистрации

    // Связь: один пользователь может участвовать во многих проектах
    public ICollection<ProjectUser> ProjectUsers { get; set; }
    public ICollection<TaskAssignment> TaskAssignments { get; set; }


}