using Spectre.Console;

using XperienceCommunity.Manager.Commands.Base;
using XperienceCommunity.Manager.Configuration;
using XperienceCommunity.Manager.Services;
using XperienceCommunity.Manager.Services.Interfaces;

namespace XperienceCommunity.Manager.Commands;

/// <summary>
/// A command which builds an Xperience by Kentico project.
/// </summary>
public class BuildCommand(IShellRunner shellRunner, IScriptBuilder scriptBuilder) : AbstractCommand
{
    public override IEnumerable<string> Keywords => ["b", "build"];


    public override IEnumerable<string> Parameters => [];


    public override string Description => "Builds a project";


    public override bool RequiresProfile => true;


    /// <summary>
    /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected into the constuctor.
    /// </summary>
#pragma warning disable CS8625
    internal BuildCommand() : this(null, null) { }
#pragma warning restore CS8625


    public override async Task Execute(ToolProfile? profile, string? action) => await BuildProject(profile);


    public override async Task PostExecute(ToolProfile? profile, string? action)
    {
        if (Errors.Count == 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Build complete![/]\n");
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
}
