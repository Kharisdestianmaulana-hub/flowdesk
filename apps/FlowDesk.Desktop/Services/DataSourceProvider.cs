using FlowDesk.Core.Interfaces;
using FlowDesk.Infrastructure.DataSources;

namespace FlowDesk.Desktop.Services;

public static class DataSourceProvider
{
    public static IDataSource Current { get; set; } = new LocalSqliteDataSource();

    public static void Initialize(string mode, string? hostUrl = null, string? token = null)
    {
        if (mode == "Joined" && !string.IsNullOrEmpty(hostUrl) && !string.IsNullOrEmpty(token))
        {
            Current = new RemoteHttpDataSource(hostUrl, token);
        }
        else
        {
            Current = new LocalSqliteDataSource();
        }
    }
}
