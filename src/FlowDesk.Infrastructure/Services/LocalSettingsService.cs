using System;
using System.IO;
using System.Text.Json;

namespace FlowDesk.Infrastructure.Services;

public class LocalSettings
{
    public string ThemePreference { get; set; } = "System";
    public string AccentColor { get; set; } = "Purple";
    public double AppZoomLevel { get; set; } = 1.0;
    public string LastUserName { get; set; } = string.Empty;
}

public class LocalSettingsService
{
    private readonly string _settingsFilePath;

    public LocalSettingsService()
    {
        var customPath = Environment.GetEnvironmentVariable("FLOWDESK_DATA_DIR");
        var flowDeskFolder = string.IsNullOrWhiteSpace(customPath) 
            ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlowDeskData")
            : customPath;
        
        if (!Directory.Exists(flowDeskFolder))
        {
            Directory.CreateDirectory(flowDeskFolder);
        }

        _settingsFilePath = Path.Join(flowDeskFolder, "settings.json");
    }

    public LocalSettings LoadSettings()
    {
        if (File.Exists(_settingsFilePath))
        {
            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                return JsonSerializer.Deserialize<LocalSettings>(json) ?? new LocalSettings();
            }
            catch
            {
                // Fallback to default if corrupted
                return new LocalSettings();
            }
        }

        return new LocalSettings();
    }

    public void SaveSettings(LocalSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            new ExceptionLogger().LogException(ex);
        }
    }
}
