using Newtonsoft.Json.Linq;

using Spectre.Console;
using Spectre.Console.Rendering;

using XperienceCommunity.Manager.Helpers;
using XperienceCommunity.Manager.Services.Interfaces;

namespace XperienceCommunity.Manager.Services;

public class ReportRenderer(ISqlExecutor sqlExecutor) : IReportRenderer
{
    private const int BAR_CHART_WIDTH = 100;
    private const int MAX_COLUMN_CHARS = 200;
    private static TableBorder BorderStyle => TableBorder.Minimal;


    public async Task RenderAdminUserReport(string connectionString)
    {
        try
        {
            var enabledAdminUsers = await sqlExecutor.ExecuteQuery(connectionString, "EnabledUsersWithAdminAccess.sql");
            var table = GetTable(enabledAdminUsers);
            RenderSection("Enabled users with admin access", table);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }


    public async Task RenderClassConsistencyReport(string connectionString)
    {
        try
        {
            var classResults = new IEnumerable<JObject>[]
            {
                await sqlExecutor.ExecuteQuery(connectionString, "TablesWithoutClasses.sql"),
                await sqlExecutor.ExecuteQuery(connectionString, "ClassesWithoutTables.sql")
            };
            string header = "Class consistency";
            if (classResults.All(t => !t.Any()))
            {
                RenderSection(header, new Markup($"[{Constants.SUCCESS_COLOR}]All classes and tables accounted for![/]"));
            }
            else
            {
                var tablesToRender = classResults.Where(t => t.Any()).Select(GetTable);
                RenderSection(header, [.. tablesToRender]);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }


    public async Task RenderEventLogReport(string connectionString)
    {
        try
        {
            var eventLogErrors = await sqlExecutor.ExecuteQuery(connectionString, "CommonEventLogErrors.sql");
            var table = GetTable(eventLogErrors);
            RenderSection("Common Event log errors", table);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }


    public async Task RenderTableSizeReport(string connectionString)
    {
        try
        {
            var largestTables = await sqlExecutor.ExecuteQuery(connectionString, "LargestTables.sql");
            var table = GetTable(largestTables);
            RenderSection("Largest tables", table);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }


    public async Task RenderWorkspaceReport(string connectionString)
    {
        try
        {
            var workspaceStatistics = await sqlExecutor.ExecuteQuery(connectionString, "WorkspaceStatistics.sql");
            var workspaceBarItems = workspaceStatistics.Select(GetWorkspaceStatisticsBarItem);
            RenderSection("Workspace stats (content item count)", new BarChart() { Width = BAR_CHART_WIDTH }
                .AddItems(workspaceBarItems));
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }


    public async Task RenderChannelStatisticsReport(string connectionString)
    {
        try
        {
            var channelStatistics = await sqlExecutor.ExecuteQuery(connectionString, "ChannelStatistics.sql");
            var channelBarItems = channelStatistics.Select(GetChannelStatisticsBarItem);
            RenderSection("Channel stats (pages, headless items, emails)", new BarChart() { Width = BAR_CHART_WIDTH }
                .AddItems(channelBarItems));
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }


    public Task RenderAssetsReport(string workingDirectory, string? customDirectoryName)
    {
        try
        {
            var assetStatistics = AssetHelper.GetAssetStatistics(workingDirectory, customDirectoryName);
            var assetTable = MakeAssetTable(assetStatistics);
            if (assetTable is not null)
            {
                RenderSection("Assets", assetTable);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }

        return Task.CompletedTask;
    }


    public async Task RenderUnusedContentItemsReport(string connectionString)
    {
        try
        {
            string header = "Unused content items (top 50)";
            var unusedContentItems = await sqlExecutor.ExecuteQuery(connectionString, "UnusedContentItems.sql");
            if (!unusedContentItems.Any())
            {
                RenderSection(header, new Markup($"[{Constants.SUCCESS_COLOR}]No unused content items![/]"));
            }
            else
            {
                var table = GetTable(unusedContentItems);
                RenderSection(header, table);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }


    public async Task RenderScheduledTaskReport(string connectionString)
    {
        try
        {
            var taskResults = new IEnumerable<JObject>[]
            {
                await sqlExecutor.ExecuteQuery(connectionString, "RunningTasks.sql"),
                await sqlExecutor.ExecuteQuery(connectionString, "EnabledTasksNeverExecuted.sql"),
                await sqlExecutor.ExecuteQuery(connectionString, "NonRecurringScheduledTasksNotExecuted.sql"),
            };
            string header = "Scheduled tasks";
            if (taskResults.All(t => !t.Any()))
            {
                RenderSection(header, new Markup($"[{Constants.SUCCESS_COLOR}]No scheduled task issues detected![/]"));
            }
            else
            {
                var tablesToRender = taskResults.Where(t => t.Any()).Select(GetTable);
                RenderSection(header, [.. tablesToRender]);
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }


    private Table GetTable(IEnumerable<JObject> objects)
    {
        var table = new Table() { Border = BorderStyle };
        if (!objects.Any())
        {
            return table;
        }

        var firstRow = objects.FirstOrDefault();
        if (firstRow is null)
        {
            return table;
        }

        table.AddColumns(firstRow.Properties().Select(p => $"[{Constants.PROMPT_COLOR}]{p.Name}[/]").ToArray());
        foreach (var row in objects)
        {
            var rowValues = row.Values().Select(GetFormattedValue);
            table
                .AddEmptyRow() // Add an empty row to simulate padding
                .AddRow(rowValues.ToArray());
        }

        return table;
    }


    private static string GetFormattedValue(JToken token)
    {
        string stringValue = token.Value<string>() ?? string.Empty;
        stringValue = stringValue.Length > MAX_COLUMN_CHARS
            ? stringValue[..MAX_COLUMN_CHARS]
            : stringValue;

        return Markup.Escape(stringValue);
    }


    private static BarChartItem GetWorkspaceStatisticsBarItem(JObject row) => new(
        row.Value<string>("WorkspaceDisplayName") ?? string.Empty,
        row.Value<double>("Content items"));


    private static BarChartItem GetChannelStatisticsBarItem(JObject row)
    {
        string channelType = row.Value<string>("ChannelType") ?? "Website";
        var color = channelType switch
        {
            "Website" => Color.Red,
            "Email" => Color.Green,
            "Headless" => Color.Yellow,
            _ => Color.Red
        };

        string label = $"{row.Value<string>("ChannelName") ?? string.Empty} ({channelType})";

        return new(
            label,
            row.Value<double>("Statistic"),
            color);
    }


    private static Table? MakeAssetTable(IEnumerable<AssetStatistic> statistics)
    {
        if (!statistics.Any())
        {
            return null;
        }

        string[] headers = ["Folder", "Exists", "Count", "Size (MB)"];
        var table = new Table() { Border = BorderStyle }
            .AddColumns(headers.Select(h => $"[{Constants.PROMPT_COLOR}]{h}[/]").ToArray());
        foreach (var stat in statistics)
        {
            table
                .AddEmptyRow() // Add an empty row to simulate padding
                .AddRow(
                    stat.DirectoryName ?? string.Empty,
                    stat.Exists ? "Yes" : "No",
                    stat.Exists ? stat.FileCount.ToString() : "-",
                    stat.Exists ? stat.DirectorySizeMB.ToString("##.##") : "-");
        }

        return table;
    }


    private static void HandleException(Exception ex)
    {
        AnsiConsole.WriteException(ex);
        AnsiConsole.WriteLine();
    }


    private static void RenderSection(string header, params IRenderable[] renderables)
    {
        AnsiConsole.Write(new Rule(header)
        {
            Justification = Justify.Left,
            Style = Style.Parse(Constants.EMPHASIS_COLOR)
        });
        foreach (var renderable in renderables)
        {
            var padder = new Padder(renderable, new Padding(5, 1, 0, 1));
            AnsiConsole.Write(padder);
        }
    }
}
