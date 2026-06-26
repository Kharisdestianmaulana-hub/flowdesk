using FlowDesk.Core.Models;

namespace FlowDesk.Desktop.Messages;

public class TaskCommentReceivedMessage
{
    public TaskComment Comment { get; }
    
    public TaskCommentReceivedMessage(TaskComment comment)
    {
        Comment = comment;
    }
}
