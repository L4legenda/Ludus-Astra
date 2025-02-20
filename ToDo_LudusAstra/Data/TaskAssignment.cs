namespace ToDo_LudusAstra.Data;

public class TaskAssignment
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public Task Task { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }
}