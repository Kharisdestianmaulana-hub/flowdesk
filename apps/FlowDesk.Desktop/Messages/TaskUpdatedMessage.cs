namespace FlowDesk.Desktop.Messages;

public class TaskUpdatedMessage
{
    public FlowDesk.Core.Models.TaskItem Task { get; }

    public TaskUpdatedMessage(FlowDesk.Core.Models.TaskItem task)
    {
        Task = task;
    }
}
