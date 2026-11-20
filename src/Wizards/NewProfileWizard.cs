using Spectre.Console;

using XperienceCommunity.Manager.Options;
using XperienceCommunity.Manager.Steps;
using XperienceCommunity.Manager.Wizards.Base;

namespace XperienceCommunity.Manager.Wizards;

/// <summary>
/// A wizard which generates a <see cref="NewProfileOptions"/> for adding new profiles.
/// </summary>
public class NewProfileWizard : AbstractWizard<NewProfileOptions>
{
    public override Task InitSteps(params string[] args)
    {
        Steps.Add(new Step<string>(new()
        {
            Prompt = new TextPrompt<string>($"Give your profile a [{Constants.PROMPT_COLOR}]name[/]:"),
            ValueReceiver = (v) => Options.ProfileName = v,
        }));

        Steps.Add(new Step<string>(new()
        {
            Prompt = new TextPrompt<string>($"Name the [{Constants.PROMPT_COLOR}]project[/] (.csproj file):"),
            ValueReceiver = (v) => Options.ProjectName = v,
        }));

        Steps.Add(new Step<string>(new()
        {
            Prompt = new TextPrompt<string>($"Enter the [{Constants.PROMPT_COLOR}]full path[/] of the folder containing your Xperience " +
                "project:"),
            ValueReceiver = (v) => Options.WorkingDirectory = v,
        }));

        return Task.CompletedTask;
    }
}
