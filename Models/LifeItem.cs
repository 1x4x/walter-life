namespace WalterLife.Models;
public enum LifeKind { Task, Reminder, Project, Note }
public enum LifePriority { Low, Medium, High }
public sealed class LifeItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public LifeKind Kind { get; set; } = LifeKind.Task;
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "Personal";
    public LifePriority Priority { get; set; } = LifePriority.Medium;
    public DateTime? DueDate { get; set; }
    public bool Completed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}