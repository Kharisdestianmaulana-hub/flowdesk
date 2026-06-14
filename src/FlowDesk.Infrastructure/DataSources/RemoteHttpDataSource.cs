using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FlowDesk.Core.Interfaces;
using FlowDesk.Core.Models;

namespace FlowDesk.Infrastructure.DataSources;

public class RemoteHttpDataSource : IDataSource
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public RemoteHttpDataSource(string hostUrl, string sessionToken)
    {
        _baseUrl = hostUrl.TrimEnd('/');
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {sessionToken}");
    }

    // Projects
    public async Task<List<Project>> GetProjectsAsync() => 
        await _httpClient.GetFromJsonAsync<List<Project>>("/api/projects") ?? new List<Project>();

    public async Task<Project?> GetProjectAsync(Guid id) => 
        await _httpClient.GetFromJsonAsync<Project>($"/api/projects/{id}");

    public async Task<Project> CreateProjectAsync(Project project)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/projects", project);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Project>() ?? project;
    }

    public async Task UpdateProjectAsync(Project project)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/projects/{project.Id}", project);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/projects/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Tasks
    public async Task<List<TaskItem>> GetTasksAsync() => 
        await _httpClient.GetFromJsonAsync<List<TaskItem>>("/api/tasks") ?? new List<TaskItem>();

    public async Task<TaskItem?> GetTaskAsync(Guid id) => 
        await _httpClient.GetFromJsonAsync<TaskItem>($"/api/tasks/{id}");

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/tasks", task);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TaskItem>() ?? task;
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/tasks/{task.Id}", task);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/tasks/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Tags
    public async Task<List<Tag>> GetTagsAsync() => 
        await _httpClient.GetFromJsonAsync<List<Tag>>("/api/tags") ?? new List<Tag>();

    public async Task<Tag?> GetTagAsync(Guid id) => 
        await _httpClient.GetFromJsonAsync<Tag>($"/api/tags/{id}");

    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/tags", tag);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Tag>() ?? tag;
    }

    public async Task UpdateTagAsync(Tag tag)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/tags/{tag.Id}", tag);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTagAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/tags/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateTaskTagsAsync(Guid taskId, string tagsString)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/tasks/{taskId}/tags", new { TagsString = tagsString });
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateProjectTagsAsync(Guid projectId, string tagsString)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/projects/{projectId}/tags", new { TagsString = tagsString });
        response.EnsureSuccessStatusCode();
    }

    // Requests
    public async Task<List<RequestItem>> GetRequestsAsync() => 
        await _httpClient.GetFromJsonAsync<List<RequestItem>>("/api/requests") ?? new List<RequestItem>();

    public async Task<RequestItem> CreateRequestAsync(RequestItem request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/requests", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RequestItem>() ?? request;
    }

    public async Task UpdateRequestAsync(RequestItem request)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/requests/{request.Id}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteRequestAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/requests/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Docs & Files
    public async Task<List<Document>> GetDocumentsAsync() => 
        await _httpClient.GetFromJsonAsync<List<Document>>("/api/documents") ?? new List<Document>();

    public async Task<Document> CreateDocumentAsync(Document doc)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/documents", doc);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Document>() ?? doc;
    }

    public async Task UpdateDocumentAsync(Document doc)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/documents/{doc.Id}", doc);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteDocumentAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/documents/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<FileItem>> GetFilesAsync() => 
        await _httpClient.GetFromJsonAsync<List<FileItem>>("/api/files") ?? new List<FileItem>();

    public async Task<FileItem> CreateFileAsync(FileItem file)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/files", file);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FileItem>() ?? file;
    }

    public async Task<FileItem?> UploadFileAsync(string localPath, Guid? projectId)
    {
        if (!System.IO.File.Exists(localPath)) return null;

        var fileInfo = new System.IO.FileInfo(localPath);
        using var content = new MultipartFormDataContent();
        
        using var fileStream = System.IO.File.OpenRead(localPath);
        using var streamContent = new StreamContent(fileStream);
        
        content.Add(streamContent, "file", fileInfo.Name);
        if (projectId.HasValue)
        {
            content.Add(new StringContent(projectId.Value.ToString()), "projectId");
        }

        var response = await _httpClient.PostAsync("/api/files/upload", content);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<FileItem>();
    }

    public async Task<bool> DownloadFileAsync(Guid id, string destinationPath)
    {
        var response = await _httpClient.GetAsync($"/api/files/{id}/download");
        if (!response.IsSuccessStatusCode) return false;

        using var fs = new System.IO.FileStream(destinationPath, System.IO.FileMode.Create);
        await response.Content.CopyToAsync(fs);
        return true;
    }

    public async Task UpdateFileAsync(FileItem file)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/files/{file.Id}", file);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteFileAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"/api/files/{id}");
        response.EnsureSuccessStatusCode();
    }

    // Activity Log
    public async Task<List<ActivityLog>> GetActivityLogsAsync() => 
        await _httpClient.GetFromJsonAsync<List<ActivityLog>>("/api/activity") ?? new List<ActivityLog>();

    public async Task LogActivityAsync(ActivityLog log)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/activity", log);
        response.EnsureSuccessStatusCode();
    }

    // Members
    public async Task<List<LocalUser>> GetMembersAsync() => 
        await _httpClient.GetFromJsonAsync<List<LocalUser>>("/api/members") ?? new List<LocalUser>();
}
