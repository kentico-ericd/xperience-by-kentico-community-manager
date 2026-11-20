using NUnit.Framework;

using XperienceCommunity.Manager.Helpers;

namespace XperienceCommunity.Manager.Tests.Tests.Helpers;

/// <summary>
/// Tests for <see cref="ProjectFileHelper"/>.
/// </summary>
public class ProjectFileHelperTests
{
    private const string PROJECT_FILE = "./Data/project/myproj.csproj";
    private readonly List<string> tempDirectories = [];


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


    [Test]
    public void FindCentralPackagesFile_FileInParentDirectory_ReturnsFile()
    {
        string root = CreateTempDirectory();
        string projectDirectory = Directory.CreateDirectory(Path.Combine(root, "src", "app")).FullName;
        string propsFile = Path.Combine(root, "Directory.Packages.props");
        File.WriteAllText(propsFile, "<Project />");

        Assert.That(ProjectFileHelper.FindCentralPackagesFile(projectDirectory), Is.EqualTo(propsFile));
    }


    [Test]
    public void FindCentralPackagesFile_NoFile_ReturnsNull() =>
        Assert.That(ProjectFileHelper.FindCentralPackagesFile(CreateTempDirectory()), Is.Null);


    [Test]
    public void SetCentralPackageVersion_DifferentCasing_UpdatesOnlyTheVersion()
    {
        string propsFile = Path.Combine(CreateTempDirectory(), "Directory.Packages.props");
        string original =
            "<Project>\r\n" +
            "  <ItemGroup>\r\n" +
            "    <!-- Xperience -->\r\n" +
            "    <PackageVersion Include=\"kentico.xperience.azurestorage\" Version=\"31.10.2\" />\r\n" +
            "    <PackageVersion Include=\"Newtonsoft.Json\" Version=\"13.0.3\" />\r\n" +
            "  </ItemGroup>\r\n" +
            "</Project>\r\n";
        File.WriteAllText(propsFile, original, new System.Text.UTF8Encoding(true));

        bool updated = ProjectFileHelper.SetCentralPackageVersion(propsFile, "Kentico.Xperience.AzureStorage", "31.10.4");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated, Is.True);
            Assert.That(File.ReadAllText(propsFile), Is.EqualTo(original.Replace("31.10.2", "31.10.4")));
            // The byte order mark is kept
            Assert.That(File.ReadAllBytes(propsFile).Take(3), Is.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }));
        }
    }


    [Test]
    public void SetCentralPackageVersion_PackageNotListed_ReturnsFalseAndLeavesFile()
    {
        string propsFile = Path.Combine(CreateTempDirectory(), "Directory.Packages.props");
        string original = "<Project>\n  <ItemGroup>\n    <PackageVersion Include=\"Newtonsoft.Json\" Version=\"13.0.3\" />\n  </ItemGroup>\n</Project>\n";
        File.WriteAllText(propsFile, original);

        bool updated = ProjectFileHelper.SetCentralPackageVersion(propsFile, "kentico.xperience.admin", "31.10.4");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated, Is.False);
            Assert.That(File.ReadAllText(propsFile), Is.EqualTo(original));
        }
    }


    [TearDown]
    public void DeleteTempDirectories()
    {
        tempDirectories.ForEach(directory => Directory.Delete(directory, true));
        tempDirectories.Clear();
    }


    private string CreateTempDirectory()
    {
        string directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())).FullName;
        tempDirectories.Add(directory);

        return directory;
    }
}
