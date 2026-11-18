using XperienceCommunity.Manager.Commands;

namespace XperienceCommunity.Manager.Options
{
    /// <summary>
    /// The options used to execute the <see cref="ReportCommand"/>. 
    /// </summary>
    public class ReportOptions : IWizardOptions
    {
        /// <summary>
        /// The customized name of the assets directory.
        /// </summary>
        public string? CustomAssetDirectoryName { get; set; }
    }
}
