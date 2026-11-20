using Spectre.Console;

using System.Reflection;

using XperienceCommunity.Manager.Commands.Base;
using XperienceCommunity.Manager.Configuration;
using XperienceCommunity.Manager.Helpers;
using XperienceCommunity.Manager.Repositories;

namespace XperienceCommunity.Manager.Commands;

/// <summary>
/// A command which displays the current tool version, a list of commands, and their descriptions.
/// </summary>
/// <param name="commandRepository">Resolved lazily, as this command is itself part of the repository.</param>
public class HelpCommand(Lazy<ICommandRepository> commandRepository) : AbstractCommand
{

    public override IEnumerable<string> Keywords => ["?", "help"];


    public override IEnumerable<string> Parameters => [];


    public override string Description => "Displays the help menu (this screen)";


    public override async Task Execute(ToolProfile? profile, string? action)
    {
        AnsiConsole.Write(
            new FigletText("xman")
                .LeftJustified()
                .Color(Color.LightGoldenrod2_1));

        var v = Assembly.GetExecutingAssembly().GetName().Version;
        if (v is not null)
        {
            AnsiConsole.WriteLine($" v{v.Major}.{v.Minor}.{v.Build}");
        }

        AnsiConsole.MarkupInterpolated($" [{Constants.EMPHASIS_COLOR}]https://github.com/kentico-ericd/xperience-by-kentico-community-manager[/]\n");

        var table = new Table()
            .AddColumn("Command")
            .AddColumn("Parameters")
            .AddColumn("Description");
        foreach (var command in commandRepository.Value.GetAll())
        {
            table.AddRow(
                string.Join(", ", command.Keywords),
                string.Join(", ", command.Parameters),
                command.Description);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        if (v is not null)
        {
            try
            {
                var latestVersion = await NuGetVersionHelper.GetLatestVersion("XperienceCommunity.Manager", v);
                if (latestVersion is not null)
                {
                    AnsiConsole.MarkupInterpolated($" New version [{Constants.SUCCESS_COLOR}]{latestVersion}[/] available!\n\n");
                }
            }
            catch
            {
                // Don't print exceptions in Help menu when failing update check
            }
        }
    }
}
