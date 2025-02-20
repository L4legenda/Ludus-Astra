using Microsoft.EntityFrameworkCore;

namespace ToDo_LudusAstra.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {}
    
    // Определяем DbSet для каждой модели
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectUser> ProjectUsers { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }
    public DbSet<TaskAttachment> TaskAttachments { get; set; }
    public DbSet<TaskChecklistItem> TaskChecklistItems { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Связь "Пользователь - Проект" (многие ко многим)
        modelBuilder.Entity<ProjectUser>()
            .HasKey(pu => new { pu.ProjectId, pu.UserId });

        modelBuilder.Entity<ProjectUser>()
            .HasOne(pu => pu.Project)
            .WithMany(p => p.ProjectUsers)
            .HasForeignKey(pu => pu.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectUser>()
            .HasOne(pu => pu.User)
            .WithMany(u => u.ProjectUsers)
            .HasForeignKey(pu => pu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь "Задача - Ответственные" (многие ко многим)
        modelBuilder.Entity<TaskAssignment>()
            .HasKey(ta => new { ta.TaskId, ta.UserId });

        modelBuilder.Entity<TaskAssignment>()
            .HasOne(ta => ta.Task)
            .WithMany(t => t.TaskAssignments)
            .HasForeignKey(ta => ta.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskAssignment>()
            .HasOne(ta => ta.User)
            .WithMany(u => u.TaskAssignments)
            .HasForeignKey(ta => ta.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь "Проект - Пользователь (руководитель)"
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Связь "Задача - Проект"
        modelBuilder.Entity<Task>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь "Задача - Автор"
        modelBuilder.Entity<Task>()
            .HasOne(t => t.Creator)
            .WithMany()
            .HasForeignKey(t => t.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Связь "Задача - Комментарии"
        modelBuilder.Entity<TaskComment>()
            .HasOne(tc => tc.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskComment>()
            .HasOne(tc => tc.User)
            .WithMany()
            .HasForeignKey(tc => tc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь "Задача - Вложения"
        modelBuilder.Entity<TaskAttachment>()
            .HasOne(ta => ta.Task)
            .WithMany(t => t.Attachments)
            .HasForeignKey(ta => ta.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskAttachment>()
            .HasOne(ta => ta.UploadedBy)
            .WithMany()
            .HasForeignKey(ta => ta.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Связь "Задача - Чек-лист"
        modelBuilder.Entity<TaskChecklistItem>()
            .HasOne(tc => tc.Task)
            .WithMany(t => t.Checklist)
            .HasForeignKey(tc => tc.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь "Уведомления - Пользователь"
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}