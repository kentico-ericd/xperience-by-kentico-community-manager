using NUnit.Framework;

using XperienceCommunity.Manager.Commands.Base;

namespace XperienceCommunity.Manager.Tests.Tests.Commands;

/// <summary>
/// Tests for <see cref="AbstractCommand"/>.
/// </summary>
public class AbstractCommandTests
{
    [Test]
    public void IsInformationalMessage_LaunchSettingsMessage_ReturnsTrue() =>
        Assert.That(AbstractCommand.IsInformationalMessage(
            @"Using launch settings from C:\project\Properties\launchSettings.json..."), Is.True);


    [TestCase("Unhandled exception. System.InvalidOperationException: Using launch settings from failed")]
    [TestCase("error CS1002: ; expected")]
    public void IsInformationalMessage_Error_ReturnsFalse(string message) =>
        Assert.That(AbstractCommand.IsInformationalMessage(message), Is.False);
}
