using Spectre.Console;

using XperienceCommunity.Manager.Helpers;
using XperienceCommunity.Manager.Options;
using XperienceCommunity.Manager.Steps;
using XperienceCommunity.Manager.Wizards.Base;

namespace XperienceCommunity.Manager.Wizards;

/// <summary>
/// A wizard which generates an <see cref="ReportOptions"/> for generating reports. The 
/// <see cref="AbstractWizard{TOptions}.Run(string[])"/> method should be passed the working directory of the project.
/// </summary>
public class ReportWizard : AbstractWizard<ReportOptions>
{
    public override Task InitSteps(params string[] args)
    {
        if (args.Length != 1)
        {
            throw new InvalidOperationException($"{nameof(ReportWizard)} requires exactly 1 argument.");
        }

        bool isUsingCustomDir = false;
        string workingDirectory = args[0];
        if (!AssetHelper.DefaultDirectoryExists(workingDirectory))
        {
            isUsingCustomDir = true;
        }

        Steps.Add(new Step<string>(new()
        {
            Prompt = new TextPrompt<string>($"Custom asset directory [{Constants.PROMPT_COLOR}]name[/]:"),
            ValueReceiver = (v) => Options.CustomAssetDirectoryName = v,
            SkipChecker = () => !isUsingCustomDir
        }));
        Steps.Add(new Step<bool>(new()
        {
            Prompt = new ConfirmationPrompt("Export the report results?")
            {
                DefaultValue = false
            },
            ValueReceiver = (v) => Options.DoExport = v
        }));
        Steps.Add(new Step<bool>(new()
        {
            Prompt = new ConfirmationPrompt("Export the report as plain text (Y) or HTML (N)?")
            {
                DefaultValue = false
            },
            ValueReceiver = (v) => Options.ExportAsText = v
        }));

        return Task.CompletedTask;
    }
}
