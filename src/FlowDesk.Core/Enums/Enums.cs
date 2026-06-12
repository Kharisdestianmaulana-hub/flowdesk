namespace FlowDesk.Core.Enums;

public enum WorkspaceMode
{
    Private,
    Local,
    Server
}

public enum ProjectStatus
{
    Idea,
    Planning,
    Active,
    OnHold,
    Completed,
    Archived
}

public enum ProjectType
{
    General,
    Company,
    Agency,
    Software,
    Game,
    Marketing,
    School,
    Event,
    Custom
}

public enum TaskStatus
{
    Backlog,
    ToDo,
    InProgress,
    Review,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Urgent
}
