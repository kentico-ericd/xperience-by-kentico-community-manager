using NUnit.Framework;

using XperienceCommunity.Manager.Options;
using XperienceCommunity.Manager.Services;

namespace XperienceCommunity.Manager.Tests.Tests.Services;

/// <summary>
/// Tests for <see cref="ScriptBuilder"/>.
/// </summary>
public class ScriptBuilderTests
{
    private readonly ScriptBuilder scriptBuilder = new();
    private readonly InstallProjectOptions validProjectOptions = new()
    {
        ProjectName = "TEST",
        Template = "kentico-xperience-sample-mvc"
    };
    private readonly InstallDatabaseOptions validDatabaseOptions = new()
    {
        AdminPassword = "PW",
        DatabaseName = "DB",
        ServerName = "SERVER"
    };
    private readonly UpdateOptions validUpdateOptions = new() { PackageName = "kentico.xperience.webapp" };


    [Test]
    public void ProjectInstallScript_WithValidOptions_ReturnsValidScript()
    {
        string script = scriptBuilder.SetScript(ScriptType.ProjectInstall).WithPlaceholders(validProjectOptions).Build();
        string expected = $"dotnet new '{validProjectOptions.Template}' -n '{validProjectOptions.ProjectName}'";

        Assert.That(script, Is.EqualTo(expected));
    }


    [Test]
    public void ProjectInstallScript_AppendAllowScripts_AddsParameter()
    {
        string script = scriptBuilder.SetScript(ScriptType.ProjectInstall)
            .WithPlaceholders(validProjectOptions)
            .AppendAllowScripts(true)
            .AppendCloud(true)
            .Build();
        string expected = $"dotnet new '{validProjectOptions.Template}' -n '{validProjectOptions.ProjectName}' --allow-scripts yes --cloud";

        Assert.That(script, Is.EqualTo(expected));
    }


    [Test]
    public void ProjectInstallScript_AppendAllowScriptsFalse_DoesNotAddParameter()
    {
        string script = scriptBuilder.SetScript(ScriptType.ProjectInstall)
            .WithPlaceholders(validProjectOptions)
            .AppendAllowScripts(false)
            .Build();
        string expected = $"dotnet new '{validProjectOptions.Template}' -n '{validProjectOptions.ProjectName}'";

        Assert.That(script, Is.EqualTo(expected));
    }


    [Test]
    public void TemplateInstall_AppendVersion_AddsParameter()
    {
        var version = new Version(1, 0, 0);
        string script = scriptBuilder.SetScript(ScriptType.TemplateInstall)
            .WithPlaceholders(validProjectOptions)
            .AppendVersion(version)
            .Build();
        string expected = $"dotnet new install kentico.xperience.templates::{version}";

        Assert.That(script, Is.EqualTo(expected));
    }


    [Test]
    public void PackageUpdate_AppendVersion_AddsParameter()
    {
        var version = new Version(1, 0, 0);
        string script = scriptBuilder.SetScript(ScriptType.PackageUpdate)
            .WithPlaceholders(validUpdateOptions)
            .AppendVersion(version)
            .Build();
        string expected = $"dotnet add package '{validUpdateOptions.PackageName}' --version {version}";

        Assert.That(script, Is.EqualTo(expected));
    }


    [Test]
    public void DatabaseInstallScript_WithValidOptions_ReturnsValidScript()
    {
        string script = scriptBuilder.SetScript(ScriptType.DatabaseInstall).WithPlaceholders(validDatabaseOptions).Build();
        string expected = $"dotnet kentico-xperience-dbmanager -- -s '{validDatabaseOptions.ServerName}' -d '" +
            $"{validDatabaseOptions.DatabaseName}' -a '{validDatabaseOptions.AdminPassword}' --use-existing-database " +
            $"{validDatabaseOptions.UseExistingDatabase}";

        Assert.That(script, Is.EqualTo(expected));
    }


    [Test]
    public void DatabaseInstallScript_UseGlobalTool_RemovesOnlyLeadingDotnet()
    {
        var options = new InstallDatabaseOptions
        {
            AdminPassword = "dotnetPW",
            DatabaseName = "dotnet_db",
            ServerName = "dotnet-server"
        };
        string script = scriptBuilder.SetScript(ScriptType.DatabaseInstall)
            .WithPlaceholders(options)
            .UseGlobalTool(true)
            .Build();
        string expected = $"kentico-xperience-dbmanager -- -s 'dotnet-server' -d 'dotnet_db' -a 'dotnetPW' " +
            $"--use-existing-database {options.UseExistingDatabase}";

        Assert.That(script, Is.EqualTo(expected));
    }


    [Test]
    public void DatabaseInstallScript_UseGlobalToolFalse_KeepsDotnet()
    {
        string script = scriptBuilder.SetScript(ScriptType.DatabaseInstall)
            .WithPlaceholders(validDatabaseOptions)
            .UseGlobalTool(false)
            .Build();

        Assert.That(script, Does.StartWith("dotnet kentico-xperience-dbmanager"));
    }
}
