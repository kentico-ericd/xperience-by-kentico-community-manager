using XperienceCommunity.Manager.Commands;

namespace XperienceCommunity.Manager.Options;

/// <summary>
/// The options used to execute the <see cref="ReportCommand"/>. 
/// </summary>
public class ReportOptions : IWizardOptions
{
    /// <summary>
    /// The customized name of the assets directory.
    /// </summary>
    public string? CustomAssetDirectoryName { get; set; }


    /// <summary>
    /// If true, exports the results of the report after execution.
    /// </summary>
    public bool DoExport { get; set; }


    /// <summary>
    /// If true, exported report data is exported as plain text. Otherwise, it is exported as HTML.
    /// </summary>
    public bool ExportAsText { get; set; }
}
