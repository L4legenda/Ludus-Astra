namespace ToDo_LudusAstra.Data;

public class TaskAttachment
{
    public int Id { get; set; }
    public string FileUrl { get; set; }  // URL файла
    public string FileType { get; set; }  // Тип файла (pdf, image, video и т. д.)
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int TaskId { get; set; }
    public Task Task { get; set; }

    public int UploadedById { get; set; }
    public User UploadedBy { get; set; }
}