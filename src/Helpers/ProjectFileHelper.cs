using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace XperienceCommunity.Manager.Helpers;

public static class ProjectFileHelper
{
    private const string CENTRAL_PACKAGES_FILENAME = "Directory.Packages.props";


    /// <summary>
    /// Gets the names of the NuGet packages referenced directly by the project file, compared case-insensitively.
    /// </summary>
    /// <param name="projectFilePath">The full path of the .csproj file.</param>
    /// <exception cref="FileNotFoundException">Thrown if the project file doesn't exist.</exception>
    public static IReadOnlySet<string> GetPackageReferences(string projectFilePath)
    {
        if (!File.Exists(projectFilePath))
        {
            throw new FileNotFoundException($"The project file {projectFilePath} was not found.", projectFilePath);
        }

        var project = XDocument.Load(projectFilePath);
        var packageNames = GetElements(project, "PackageReference")
            .Select(element => element.Attribute("Include")?.Value)
            .OfType<string>();

        return new HashSet<string>(packageNames, StringComparer.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Finds the Directory.Packages.props file used for central package management, searching from the project
    /// directory upwards like MSBuild does. Returns <c>null</c> if there isn't one.
    /// </summary>
    /// <param name="projectDirectory">The directory containing the project file.</param>
    public static string? FindCentralPackagesFile(string projectDirectory)
    {
        var directory = new DirectoryInfo(projectDirectory);
        while (directory is not null)
        {
            string filePath = Path.Combine(directory.FullName, CENTRAL_PACKAGES_FILENAME);
            if (File.Exists(filePath))
            {
                return filePath;
            }

            directory = directory.Parent;
        }

        return null;
    }


    /// <summary>
    /// Sets the version of every PackageVersion item for the package in a Directory.Packages.props file, matching
    /// the package name case-insensitively. The rest of the file is left as it was.
    /// </summary>
    /// <remarks>
    /// <c>dotnet add package</c> matches PackageVersion items case-sensitively against the package ID on NuGet,
    /// so it adds a duplicate item if the file uses different casing.
    /// </remarks>
    /// <param name="centralPackagesFilePath">The full path of the Directory.Packages.props file.</param>
    /// <param name="packageName">The name of the package to update.</param>
    /// <param name="version">The new version.</param>
    /// <returns><c>true</c> if the file contained the package and was updated.</returns>
    public static bool SetCentralPackageVersion(string centralPackagesFilePath, string packageName, string version)
    {
        string originalText = File.ReadAllText(centralPackagesFilePath);
        var document = XDocument.Parse(originalText, LoadOptions.PreserveWhitespace);
        var packageVersions = GetElements(document, "PackageVersion")
            .Where(element => string.Equals(element.Attribute("Include")?.Value, packageName, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (packageVersions.Count == 0)
        {
            return false;
        }

        foreach (var packageVersion in packageVersions)
        {
            packageVersion.SetAttributeValue("Version", version);
        }

        // Keep the file's declaration, encoding and line endings so only the version changes in the diff
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = document.Declaration is null,
            Encoding = new UTF8Encoding(HasUtf8ByteOrderMark(centralPackagesFilePath)),
            NewLineHandling = NewLineHandling.Replace,
            NewLineChars = originalText.Contains("\r\n") ? "\r\n" : "\n"
        };
        using (var writer = XmlWriter.Create(centralPackagesFilePath, settings))
        {
            document.Save(writer);
        }

        return true;
    }


    // Match on the local name, as older project files declare an XML namespace
    private static IEnumerable<XElement> GetElements(XDocument document, string localName) =>
        document.Descendants().Where(element => element.Name.LocalName == localName);


    private static bool HasUtf8ByteOrderMark(string filePath)
    {
        byte[] preamble = Encoding.UTF8.GetPreamble();
        using var stream = File.OpenRead(filePath);
        byte[] start = new byte[preamble.Length];
        int read = stream.Read(start, 0, start.Length);

        return read == preamble.Length && start.SequenceEqual(preamble);
    }
}
