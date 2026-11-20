using NUnit.Framework;

using System.Diagnostics;
using System.Text;

using XperienceCommunity.Manager.Services;

namespace XperienceCommunity.Manager.Tests.Tests.Services;

/// <summary>
/// Tests for <see cref="ShellRunner"/>.
/// </summary>
public class ShellRunnerTests
{
    private readonly ShellRunner shellRunner = new();


    [Test]
    public void Execute_ErrorHandler_CapturesError()
    {
        var builder = new StringBuilder();
        string invalidPackage = "PACKAGE_DOESNT_EXIST";
        var proc = shellRunner.Execute(new($"dotnet new install {invalidPackage}") { ErrorHandler = (o, e) => builder.Append(e.Data) });
        proc.WaitForExit();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(proc.HasExited);
            Assert.That(proc.StartInfo.RedirectStandardError);
            Assert.That(builder.ToString(), Contains.Substring($"{invalidPackage} could not be installed, the package does not exist"));
        }
    }


    [Test]
    public void Execute_OutputHandler_ExitsProcess()
    {
        string question = "How old";
        var proc = shellRunner.Execute(new($"Read-Host '{question}'")
        {
            KeepOpen = true,
            OutputHandler = (o, e) =>
            {
                var p = o as Process;
                if (e.Data?.Contains(question) ?? false)
                {
                    p?.StandardInput.WriteLine(42);
                    p?.StandardInput.Close();
                }
            }
        });
        proc.WaitForExit();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(proc.HasExited);
            Assert.That(proc.StartInfo.RedirectStandardOutput);
        }
    }
}
