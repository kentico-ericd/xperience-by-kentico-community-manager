using NUnit.Framework;

using XperienceCommunity.Manager.Helpers;

namespace XperienceCommunity.Manager.Tests.Tests.Helpers;

/// <summary>
/// Tests for <see cref="ProjectFileHelper"/>.
/// </summary>
public class ProjectFileHelperTests
{
    private const string PROJECT_FILE = "./Data/project/myproj.csproj";


    [Test]
    public void GetPackageReferences_ReturnsAllPackageReferences()
    {
        var packages = ProjectFileHelper.GetPackageReferences(PROJECT_FILE);

        Assert.That(packages, Is.EquivalentTo(
        [
            "Kentico.Xperience.Admin",
            "kentico.xperience.webapp",
            "Kentico.Xperience.ImageProcessing",
            "Newtonsoft.Json"
        ]));
    }


    [Test]
    public void GetPackageReferences_IgnoresCase()
    {
        var packages = ProjectFileHelper.GetPackageReferences(PROJECT_FILE);

        using (Assert.EnterMultipleScope())
        {
            // Call the set's Contains directly, as NUnit's Does.Contain ignores the set's comparer
            Assert.That(packages.Contains("KENTICO.XPERIENCE.WEBAPP"), Is.True);
            Assert.That(packages.Contains("kentico.xperience.admin"), Is.True);
        }
    }


    [Test]
    public void GetPackageReferences_MissingFile_Throws() =>
        Assert.Throws<FileNotFoundException>(() => ProjectFileHelper.GetPackageReferences("./Data/project/missing.csproj"));
}
