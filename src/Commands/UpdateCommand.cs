using Spectre.Console;

using XperienceCommunity.Manager.Commands.Base;
using XperienceCommunity.Manager.Configuration;
using XperienceCommunity.Manager.Helpers;
using XperienceCommunity.Manager.Options;
using XperienceCommunity.Manager.Services;
using XperienceCommunity.Manager.Services.Interfaces;
using XperienceCommunity.Manager.Wizards.Base;

namespace XperienceCommunity.Manager.Commands;

/// <summary>
/// A command which updates the NuGet packages and database of the Xperience by Kentico project in
/// the current directory.
/// </summary>
public class UpdateCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder, IWizard<UpdateOptions> wizard) : AbstractCommand
{
    private readonly IEnumerable<string> packageNames =
    [
        "kentico.xperience.admin",
        "kentico.xperience.azurestorage",
        "kentico.xperience.cloud",
        "kentico.xperience.imageprocessing",
        "kentico.xperience.webapp",
        "kentico.xperience.mjml",
        "kentico.xperience.amazonstorage",
        "kentico.xperience.sendgrid",
        "kentico.xperience.managementapi",
    ];


    public override IEnumerable<string> Keywords => ["u", "update"];


    public override IEnumerable<string> Parameters => [];


    public override string Description => "Updates a project's NuGet packages and database version";


    public override bool RequiresProfile => true;


    /// <summary>
    /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected into the constuctor.
    /// </summary>
#pragma warning disable CS8625
    internal UpdateCommand() : this(null, null, null) { }
#pragma warning restore CS8625


    public override async Task Execute(ToolProfile? profile, string? action)
    {
        if (StopProcessing)
        {
            return;
        }

        // Find the packages to update before prompting, so the user isn't asked for a version if there's nothing to update
        var packagesToUpdate = GetPackagesToUpdate(profile);
        if (StopProcessing)
        {
            return;
        }

        var options = await wizard.Run();

        await UpdatePackages(options, profile, packagesToUpdate);
        await BuildProject(profile);
        await UpdateDatabase(profile);
    }


    public override async Task PostExecute(ToolProfile? profile, string? action)
    {
        if (Errors.Count == 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Update complete![/]\n");
        }

        await base.PostExecute(profile, action);
    }


    private async Task BuildProject(ToolProfile? profile)
    {
        if (StopProcessing)
        {
            return;
        }

        AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Attempting to build the project...[/]");

        string buildScript = scriptBuilder.SetScript(ScriptType.BuildProject).Build();
        await shellRunner.Execute(new(buildScript)
        {
            ErrorHandler = ErrorDataReceived,
            WorkingDirectory = profile?.WorkingDirectory
        }).WaitForExitAsync();
    }


    /// <summary>
    /// Gets the Xperience packages which the project already references.
    /// </summary>
    private List<string> GetPackagesToUpdate(ToolProfile? profile)
    {
        if (string.IsNullOrEmpty(profile?.WorkingDirectory) || string.IsNullOrEmpty(profile.ProjectName))
        {
            LogError("Couldn't load the profile's working directory or project name.");

            return [];
        }

        string projectFilePath = Path.Combine(profile.WorkingDirectory, $"{profile.ProjectName}.csproj");
        if (!File.Exists(projectFilePath))
        {
            LogError($"The project file {projectFilePath} was not found.");

            return [];
        }

        var referencedPackages = ProjectFileHelper.GetPackageReferences(projectFilePath);
        var packagesToUpdate = packageNames.Where(referencedPackages.Contains).ToList();
        if (packagesToUpdate.Count == 0)
        {
            LogError($"The project file {projectFilePath} doesn't reference any Xperience by Kentico packages.");
        }

        return packagesToUpdate;
    }


    private async Task UpdatePackages(UpdateOptions options, ToolProfile? profile, IEnumerable<string> packagesToUpdate)
    {
        if (options.Version is null || string.IsNullOrEmpty(profile?.WorkingDirectory))
        {
            LogError("Couldn't load the version or the profile's working directory.");

            return;
        }

        string version = NuGetVersionHelper.ToPackageVersion(options.Version);
        string? centralPackagesFile = ProjectFileHelper.FindCentralPackagesFile(profile.WorkingDirectory);
        foreach (string package in packagesToUpdate)
        {
            if (StopProcessing)
            {
                return;
            }

            AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updating {package} to version {version}...[/]");

            // Edit central package versions directly, as "dotnet add package" adds a duplicate if the casing differs
            if (centralPackagesFile is not null && ProjectFileHelper.SetCentralPackageVersion(centralPackagesFile, package, version))
            {
                continue;
            }

            options.PackageName = package;
            string packageScript = scriptBuilder.SetScript(ScriptType.PackageUpdate)
                .WithPlaceholders(options)
                .AppendVersion(options.Version)
                .InsertOrAppendProject(profile?.ProjectName)
                .Build();
            await shellRunner.Execute(new(packageScript)
            {
                ErrorHandler = ErrorDataReceived,
                WorkingDirectory = profile?.WorkingDirectory
            }).WaitForExitAsync();
        }
    }


    private async Task UpdateDatabase(ToolProfile? profile)
    {
        if (StopProcessing)
        {
            return;
        }

        AnsiConsole.MarkupLineInterpolated($"[{Constants.EMPHASIS_COLOR}]Updating database...[/]");
        string dbScript = scriptBuilder.SetScript(ScriptType.DatabaseUpdate)
            .InsertOrAppendProject(profile?.ProjectName)
            .Build();
        await shellRunner.Execute(new(dbScript)
        {
            ErrorHandler = ErrorDataReceived,
            WorkingDirectory = profile?.WorkingDirectory
        }).WaitForExitAsync();
    }
}
