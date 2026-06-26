using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlowDesk.Core.Models;

namespace FlowDesk.Core.Interfaces;

public interface IDataSource
{
    // Projects
    Task<List<Project>> GetProjectsAsync();
    Task<Project?> GetProjectAsync(Guid id);
    Task<Project> CreateProjectAsync(Project project);
    Task UpdateProjectAsync(Project project);
    Task DeleteProjectAsync(Guid id);

    // Tasks
    Task<List<TaskItem>> GetTasksAsync();
    Task<TaskItem?> GetTaskAsync(Guid id);
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task UpdateTaskAsync(TaskItem task);
    Task DeleteTaskAsync(Guid id);

    // Comments
    Task<List<TaskComment>> GetTaskCommentsAsync(Guid taskId);
    Task<TaskComment> CreateCommentAsync(TaskComment comment);
    Task DeleteCommentAsync(Guid id);

    // Tags
    Task<List<Tag>> GetTagsAsync();
    Task<Tag?> GetTagAsync(Guid id);
    Task<Tag> CreateTagAsync(Tag tag);
    Task UpdateTagAsync(Tag tag);
    Task DeleteTagAsync(Guid id);
    Task UpdateTaskTagsAsync(Guid taskId, string tagsString);
    Task UpdateProjectTagsAsync(Guid projectId, string tagsString);

    // Requests (Approvals/Issues)
    Task<List<RequestItem>> GetRequestsAsync();
    Task<RequestItem> CreateRequestAsync(RequestItem request);
    Task UpdateRequestAsync(RequestItem request);
    Task DeleteRequestAsync(Guid id);

    // Docs & Files (Metadata)
    Task<List<Document>> GetDocumentsAsync();
    Task<Document> CreateDocumentAsync(Document doc);
    Task UpdateDocumentAsync(Document doc);
    Task DeleteDocumentAsync(Guid id);

    Task<List<FileItem>> GetFilesAsync();
    Task<FileItem> CreateFileAsync(FileItem file);
    Task<FileItem?> UploadFileAsync(string localPath, Guid? projectId);
    Task<bool> DownloadFileAsync(Guid id, string destinationPath);
    Task UpdateFileAsync(FileItem file);
    Task DeleteFileAsync(Guid id);
    
    // Activity Log
    Task<List<ActivityLog>> GetActivityLogsAsync();
    Task LogActivityAsync(ActivityLog log);

    // Members
    Task<List<LocalUser>> GetMembersAsync();
}
