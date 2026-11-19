using NSubstitute;

using NUnit.Framework;

using XperienceCommunity.Manager.Commands;
using XperienceCommunity.Manager.Configuration;
using XperienceCommunity.Manager.Options;
using XperienceCommunity.Manager.Services;
using XperienceCommunity.Manager.Services.Interfaces;
using XperienceCommunity.Manager.Wizards.Base;

namespace XperienceCommunity.Manager.Tests.Tests.Commands;

/// <summary>
/// Tests for <see cref="UpdateCommand"/>.
/// </summary>
public class UpdateCommandTests : TestBase
{
    private const string PROJECT = "myproj";
    private const string WORKING_DIRECTORY = "./Data/project";
    private readonly Version version = new(1, 0, 0);
    private readonly IShellRunner shellRunner = Substitute.For<IShellRunner>();
    private readonly IWizard<UpdateOptions> updateWizard = Substitute.For<IWizard<UpdateOptions>>();


    [SetUp]
    public void UpdateCommandTestsSetUp()
    {
        updateWizard.Run().Returns(new UpdateOptions
        {
            Version = version
        });

        shellRunner.Execute(Arg.Any<ShellOptions>()).Returns((x) => GetDummyProcess());
    }


    [Test]
    public async Task Execute_UpdatesOnlyReferencedPackages()
    {
        ToolProfile profile = new()
        {
            ProjectName = PROJECT,
            WorkingDirectory = WORKING_DIRECTORY
        };
        var command = new UpdateCommand(shellRunner, new ScriptBuilder(), updateWizard);
        await command.PreExecute(profile, string.Empty);
        await command.Execute(profile, string.Empty);

        // Referenced in Data/project/myproj.csproj
        string[] referencedPackages =
        [
            "kentico.xperience.admin",
            "kentico.xperience.imageprocessing",
            "kentico.xperience.webapp"
        ];
        string[] unreferencedPackages =
        [
            "kentico.xperience.azurestorage",
            "kentico.xperience.cloud",
            "kentico.xperience.mjml",
            "kentico.xperience.amazonstorage",
            "kentico.xperience.sendgrid",
            "kentico.xperience.managementapi"
        ];

        Assert.That(command.Errors, Is.Empty);
        foreach (string p in referencedPackages)
        {
            shellRunner.Received().Execute(Arg.Is<ShellOptions>(x => x.Script.Equals(
                $"dotnet add package '{p}' --version {version} --project '{PROJECT}.csproj'")));
        }

        foreach (string p in unreferencedPackages)
        {
            shellRunner.DidNotReceive().Execute(Arg.Is<ShellOptions>(x => x.Script.Contains($"'{p}'")));
        }
    }


    [Test]
    public async Task Execute_ProjectFileMissing_LogsErrorAndRunsNothing()
    {
        ToolProfile profile = new()
        {
            ProjectName = "missing",
            WorkingDirectory = WORKING_DIRECTORY
        };
        var command = new UpdateCommand(shellRunner, new ScriptBuilder(), updateWizard);
        await command.PreExecute(profile, string.Empty);
        await command.Execute(profile, string.Empty);

        Assert.That(command.Errors, Has.Count.EqualTo(1));
        await updateWizard.DidNotReceive().Run();
        shellRunner.DidNotReceive().Execute(Arg.Any<ShellOptions>());
    }
}
