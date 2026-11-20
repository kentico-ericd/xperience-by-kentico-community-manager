using Microsoft.Data.SqlClient;

using Spectre.Console;

using XperienceCommunity.Manager.Commands.Base;
using XperienceCommunity.Manager.Configuration;
using XperienceCommunity.Manager.Services;
using XperienceCommunity.Manager.Services.Interfaces;

namespace XperienceCommunity.Manager.Commands;

/// <summary>
/// A command which deletes an Xperience by Kentico project.
/// </summary>
public class DeleteCommand(
    IShellRunner shellRunner,
    ISqlExecutor sqlExecutor,
    IScriptBuilder scriptBuilder,
    IConfigManager configManager,
    IAppSettingsManager appSettingsManager) : AbstractCommand
{
    private bool deleteConfirmed;


    public override IEnumerable<string> Keywords => ["d", "delete"];


    public override IEnumerable<string> Parameters => [];


    public override string Description => "Deletes a project and its database";


    public override bool RequiresProfile => true;


    public override async Task Execute(ToolProfile? profile, string? action)
    {
        deleteConfirmed = await AnsiConsole.ConfirmAsync($"This will [{Constants.ERROR_COLOR}]delete[/] the current profile's physical folder " +
            $"and database!\nDo you want to continue?", false);
        if (!deleteConfirmed)
        {
            return;
        }

        AnsiConsole.WriteLine();
        await DropDatabase(profile);
        await UninstallFiles(profile);
        if (!StopProcessing)
        {
            await configManager.RemoveProfile(profile);
        }
    }


    public override async Task PostExecute(ToolProfile? profile, string? action)
    {
        if (!deleteConfirmed)
        {
            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Delete cancelled[/]\n");
        }
        else if (Errors.Count == 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Delete complete![/]\n");
        }

        await base.PostExecute(profile, action);
    }


    private async Task DropDatabase(ToolProfile? profile)
    {
        if (StopProcessing)
        {
            return;
        }

        AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Deleting database...[/]");

        string? connString = await appSettingsManager.GetConnectionString(profile, "CMSConnectionString");
        if (connString is null)
        {
            LogError("Couldn't load connection string.");

            return;
        }

        // Find "Initial Catalog" in connection string
        var builder = new SqlConnectionStringBuilder(connString);
        string databaseName = builder.InitialCatalog;
        // Remove "Initial Catalog" from connection string, or trying to delete will throw "in use" error
        builder.InitialCatalog = "";
        connString = builder.ToString();

        Dictionary<string, object> parameters = new() { { "@DBName", databaseName } };
        await sqlExecutor.ExecuteNonQuery(connString, "DropDatabase.sql", parameters);
    }


    private async Task UninstallFiles(ToolProfile? profile)
    {
        if (StopProcessing)
        {
            return;
        }

        if (string.IsNullOrEmpty(profile?.WorkingDirectory))
        {
            LogError("Couldn't load working directory.");

            return;
        }

        AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Deleting local files...[/]");

        string uninstallScript = scriptBuilder.SetScript(ScriptType.DeleteDirectory).WithPlaceholders(profile).Build();
        await shellRunner.Execute(new(uninstallScript)
        {
            ErrorHandler = ErrorDataReceived
        }).WaitForExitAsync();
    }
}
