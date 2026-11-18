namespace XperienceCommunity.Manager.Services.Interfaces;

/// <summary>
/// Contains methods for rendering report tables and graphs.
/// </summary>
public interface IReportRenderer : IService
{
    /// <summary>
    /// Displays the enabled administration users table.
    /// </summary>
    public Task RenderAdminUserReport(string connectionString);


    /// <summary>
    /// Displays tables related to the CMS_Class database table.
    /// </summary>
    public Task RenderClassConsistencyReport(string connectionString);


    /// <summary>
    /// Displays a table containing common Event log errors.
    /// </summary>
    public Task RenderEventLogReport(string connectionString);


    /// <summary>
    /// Displays a table showing the largest database tables.
    /// </summary>
    public Task RenderTableSizeReport(string connectionString);


    /// <summary>
    /// Displays a table showing the number and count of assets.
    /// </summary>
    public Task RenderAssetsReport(string workingDirectory, string? customDirectoryName);


    /// <summary>
    /// Displays a bar graph showing channels and their item counts.
    /// </summary>
    public Task RenderChannelStatisticsReport(string connectionString);


    /// <summary>
    /// Displays a bar graph showing workspaces and their item counts.
    /// </summary>
    public Task RenderWorkspaceReport(string connectionString);


    /// <summary>
    /// Displays a table of content items with no references.
    /// </summary>
    public Task RenderUnusedContentItemsReport(string connectionString);
}
