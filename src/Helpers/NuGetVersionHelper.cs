using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace XperienceCommunity.Manager.Helpers;

public static class NuGetVersionHelper
{
    private const string NUGET_SOURCE = "https://api.nuget.org/v3/index.json";


    /// <summary>
    /// Formats a version the way NuGet packages are versioned, e.g. "31.10.4" rather than "31.10.4.0". The fourth
    /// number is only included if it's set.
    /// </summary>
    public static string ToPackageVersion(Version version)
    {
        if (version.Revision > 0)
        {
            return version.ToString();
        }

        return version.Build >= 0 ? version.ToString(3) : version.ToString(2);
    }


    /// <summary>
    /// Gets all versions of the provided NuGet package.
    /// </summary>
    public static async Task<IEnumerable<NuGetVersion>> GetPackageVersions(string package)
    {
        var repository = Repository.Factory.GetCoreV3(NUGET_SOURCE);
        var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
        return resource is null
            ? throw new InvalidOperationException($"Could not find package resource for {package}")
            : await resource.GetAllVersionsAsync(
            package,
            new SourceCacheContext(),
            NullLogger.Instance,
            CancellationToken.None);
    }


    /// <summary>
    /// Gets the latest version of the provided NuGet package, or <n>null</n> if <paramref name="currentVersion"/>
    /// is the latest version.
    /// </summary>
    public static async Task<Version?> GetLatestVersion(string package, Version currentVersion)
    {
        var repository = Repository.Factory.GetCoreV3(NUGET_SOURCE);
        var resource = await repository.GetResourceAsync<FindPackageByIdResource>()
            ?? throw new InvalidOperationException($"Could not find package resource for {package}");
        var versions = await resource.GetAllVersionsAsync(
            package,
            new SourceCacheContext(),
            NullLogger.Instance,
            CancellationToken.None);

        var latest = versions.OrderByDescending(v => v).FirstOrDefault()?.Version ?? currentVersion;

        return latest > currentVersion ? latest : null;
    }
}
