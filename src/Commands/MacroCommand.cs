using Spectre.Console;

using XperienceCommunity.Manager.Commands.Base;
using XperienceCommunity.Manager.Configuration;
using XperienceCommunity.Manager.Options;
using XperienceCommunity.Manager.Services;
using XperienceCommunity.Manager.Services.Interfaces;
using XperienceCommunity.Manager.Wizards.Base;

namespace XperienceCommunity.Manager.Commands;

/// <summary>
/// A command which allows re-signing of macros.
/// </summary>
public class MacroCommand(IWizard<MacroOptions> wizard, IShellRunner shellRunner, IScriptBuilder scriptBuilder) : AbstractCommand
{
    public override IEnumerable<string> Keywords => ["m", "macros"];


    public override IEnumerable<string> Parameters => [];


    public override string Description => "Re-signs macro signatures";


    public override bool RequiresProfile => true;


    /// <summary>
    /// Do not use. Workaround for circular dependency in <see cref="HelpCommand"/> when commands are injected into the constuctor.
    /// </summary>
#pragma warning disable CS8625
    internal MacroCommand() : this(null, null, null) { }
#pragma warning restore CS8625


    public override async Task Execute(ToolProfile? profile, string? action)
    {
        if (StopProcessing)
        {
            return;
        }

        var options = await wizard.Run();
        AnsiConsole.WriteLine();

        await AnsiConsole.Progress()
            .Columns(
                new SpinnerColumn(),
                new ElapsedTimeColumn(),
                new TaskDescriptionColumn()
            )
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"[{Constants.EMPHASIS_COLOR}]Re-signing macros...[/]");
                await ResignMacros(task, profile, options);
            });
    }


    public override async Task PostExecute(ToolProfile? profile, string? action)
    {
        if (Errors.Count == 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[{Constants.SUCCESS_COLOR}]Macros re-signed![/]\n");
        }

        await base.PostExecute(profile, action);
    }


    private async Task ResignMacros(ProgressTask task, ToolProfile? profile, MacroOptions options)
    {
        if (StopProcessing)
        {
            return;
        }

        string originalDescription = task.Description;
        string macroScript = scriptBuilder.SetScript(ScriptType.ResignMacros)
            .AppendSignAll(options.SignAll, options.UserName)
            .AppendSalts(options.OldSalt, options.NewSalt)
            .InsertOrAppendProject(profile?.ProjectName)
            .Build();
        await shellRunner.Execute(new(macroScript)
        {
            ErrorHandler = ErrorDataReceived,
            WorkingDirectory = profile?.WorkingDirectory,
            OutputHandler = (o, e) =>
            {
                if (e.Data?.Contains("Processing", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    // Message is something like "Processing 'Class' objects"
                    task.Description = e.Data;
                }
            }
        }).WaitForExitAsync();

        task.Description = originalDescription;
    }
}
