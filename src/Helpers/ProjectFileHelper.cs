using System.Xml.Linq;

namespace XperienceCommunity.Manager.Helpers;

public static class ProjectFileHelper
{
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
        var packageNames = project.Descendants()
            // Match on the local name, as older project files declare an XML namespace
            .Where(element => element.Name.LocalName == "PackageReference")
            .Select(element => element.Attribute("Include")?.Value)
            .OfType<string>();

        return new HashSet<string>(packageNames, StringComparer.OrdinalIgnoreCase);
    }
}
