namespace ToDo_LudusAstra.Data;

public class TaskChecklistItem
{
    public int Id { get; set; }
    public string Content { get; set; }  // Текст подзадачи
    public bool IsCompleted { get; set; } = false;

    public int TaskId { get; set; }
    public Task Task { get; set; }
}