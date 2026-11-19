using NSubstitute;

using NUnit.Framework;

using XperienceCommunity.Manager.Services;
using XperienceCommunity.Manager.Services.Interfaces;

namespace XperienceCommunity.Manager.Tests.Tests.Services;

/// <summary>
/// Tests of <see cref="ReportRenderer"/>.
/// </summary>
public class ReportRendererTests
{
    private ReportRenderer reportRenderer;
    private readonly ISqlExecutor sqlExecutor = Substitute.For<ISqlExecutor>();
    private const string CONNSTRING = "FAKE_CONN_STRING";


    [OneTimeSetUp]
    public void OneTimeSetUp() =>
        reportRenderer = new ReportRenderer(sqlExecutor);


    [Test]
    public async Task RenderAdminUserReport_RunsQuery()
    {
        await reportRenderer.RenderAdminUserReport(CONNSTRING);

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "EnabledUsersWithAdminAccess.sql");
    }


    [Test]
    public async Task RenderClassConsistencyReport_RunsQuery()
    {
        await reportRenderer.RenderClassConsistencyReport(CONNSTRING);

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "TablesWithoutClasses.sql");
        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "ClassesWithoutTables.sql");
    }


    [Test]
    public async Task RenderEventLogReport_RunsQuery()
    {
        await reportRenderer.RenderEventLogReport(CONNSTRING);

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "CommonEventLogErrors.sql");
    }


    [Test]
    public async Task RenderTableSizeReport_RunsQuery()
    {
        await reportRenderer.RenderTableSizeReport(CONNSTRING);

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "LargestTables.sql");
    }


    [Test]
    public async Task RenderWorkspaceReport_RunsQuery()
    {
        await reportRenderer.RenderWorkspaceReport(CONNSTRING);

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "WorkspaceStatistics.sql");
    }


    [Test]
    public async Task RenderChannelStatisticsReport_RunsQuery()
    {
        await reportRenderer.RenderChannelStatisticsReport(CONNSTRING);

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "ChannelStatistics.sql");
    }


    [Test]
    public async Task RenderUnusedContentItemsReport_RunsQuery()
    {
        await reportRenderer.RenderUnusedContentItemsReport(CONNSTRING);

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "UnusedContentItems.sql");
    }


    [Test]
    public async Task RenderScheduledTaskReport_RunsQuery()
    {
        await reportRenderer.RenderScheduledTaskReport(CONNSTRING);

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "RunningTasks.sql");
        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "EnabledTasksNeverExecuted.sql");
        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "NonRecurringScheduledTasksNotExecuted.sql");
    }
}
