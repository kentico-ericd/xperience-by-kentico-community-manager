using XperienceCommunity.Manager.Commands.Base;
using XperienceCommunity.Manager.Configuration;
using XperienceCommunity.Manager.Options;
using XperienceCommunity.Manager.Services.Interfaces;
using XperienceCommunity.Manager.Wizards.Base;

namespace XperienceCommunity.Manager.Commands;

public class ReportCommand(IReportRenderer reportRenderer, IAppSettingsManager appSettingsManager, IWizard<ReportOptions> wizard) : AbstractCommand
{
    public override IEnumerable<string> Keywords => ["r", "report"];


    public override IEnumerable<string> Parameters => [];


    public override string Description => "Displays a report with important project information";


    public override bool RequiresProfile => true;


    /// <summary>
    /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected into the constuctor.
    /// </summary>
#pragma warning disable CS8625
    internal ReportCommand() : this(null, null, null) { }
#pragma warning restore CS8625


    public override Task Execute(ToolProfile? profile, string? action) => RunReport(profile);


    private async Task RunReport(ToolProfile? profile)
    {
        if (StopProcessing)
        {
            return;
        }

        string? connectionString = await appSettingsManager.GetConnectionString(profile, "CMSConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            LogError("Couldn't load connection string.");

            return;
        }

        string? workingDirectory = profile?.WorkingDirectory;
        if (string.IsNullOrEmpty(workingDirectory))
        {
            LogError("Couldn't load working directory.");

            return;
        }

        var options = await wizard.Run(workingDirectory);

        await RunReportInternal(connectionString, workingDirectory, options);
    }


    private async Task RunReportInternal(string connectionString, string workingDirectory, ReportOptions options)
    {
        await reportRenderer.RenderClassConsistencyReport(connectionString);
        await reportRenderer.RenderAssetsReport(workingDirectory, options.CustomAssetDirectoryName);
        await reportRenderer.RenderChannelStatisticsReport(connectionString);
        await reportRenderer.RenderWorkspaceReport(connectionString);
        await reportRenderer.RenderUnusedContentItemsReport(connectionString);
        await reportRenderer.RenderAdminUserReport(connectionString);
        await reportRenderer.RenderTableSizeReport(connectionString);
        await reportRenderer.RenderEventLogReport(connectionString);
        await reportRenderer.RenderScheduledTaskReport(connectionString);
        await reportRenderer.RenderContentSyncReport(connectionString, workingDirectory, options.CustomAssetDirectoryName);

        if (options.DoExport)
        {
            await reportRenderer.Export(options.ExportAsText, workingDirectory);
        }
    }
}
