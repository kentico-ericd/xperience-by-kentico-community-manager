using NUnit.Framework;

using XperienceCommunity.Manager.Helpers;

namespace XperienceCommunity.Manager.Tests.Tests.Helpers;

/// <summary>
/// Tests for <see cref="NuGetVersionHelper"/>.
/// </summary>
public class NuGetVersionHelperTests
{
    [TestCase("31.10.4.0", "31.10.4")]
    [TestCase("31.10.4", "31.10.4")]
    [TestCase("31.10", "31.10")]
    [TestCase("1.2.3.4", "1.2.3.4")]
    public void ToPackageVersion_FormatsVersion(string version, string expected) =>
        Assert.That(NuGetVersionHelper.ToPackageVersion(Version.Parse(version)), Is.EqualTo(expected));
}
