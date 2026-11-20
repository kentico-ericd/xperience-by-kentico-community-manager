using System.Reflection;

using Newtonsoft.Json.Linq;

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


    [TearDown]
    public void TearDown() => sqlExecutor.ClearReceivedCalls();


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


    [Test]
    public async Task RenderContentSyncReport_WithSyncFolder_RunsAllQueries()
    {
        await reportRenderer.RenderContentSyncReport(CONNSTRING, GetWorkingDirectory(), "customAssetDir");

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "SynchronizationFailedRestores.sql");
        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "SynchronizationsTransitionalStatus.sql");
        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "SynchronizationAllIncomingTasks.sql");
    }


    [Test]
    public async Task RenderContentSyncReport_NoSyncFolder_SkipsFolderChecks()
    {
        await reportRenderer.RenderContentSyncReport(CONNSTRING, GetWorkingDirectory(), null);

        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "SynchronizationFailedRestores.sql");
        await sqlExecutor.Received().ExecuteQuery(CONNSTRING, "SynchronizationsTransitionalStatus.sql");
        await sqlExecutor.DidNotReceive().ExecuteQuery(CONNSTRING, "SynchronizationAllIncomingTasks.sql");
    }


    [TestCase(true)]
    [TestCase(false)]
    public async Task Export_ExportsData(bool exportAsText)
    {
        string workingDirectory = "ExportDir";
        int testUserId = 123;
        string testUserName = "TestUser";

        var mockSqlExecutor = Substitute.For<ISqlExecutor>();
        mockSqlExecutor.ExecuteQuery(Arg.Any<string>(), "EnabledUsersWithAdminAccess.sql").Returns(
        [
            new JObject(new JProperty("UserID", JToken.FromObject(testUserId)), new JProperty("UserName", JToken.FromObject(testUserName)))
        ]);
        var renderer = new ReportRenderer(mockSqlExecutor);

        await renderer.RenderAdminUserReport("FAKE_CONNSTRING");
        await renderer.Export(exportAsText, workingDirectory);
        string[] files = Directory.GetFiles(Path.Combine(workingDirectory, "XperienceReports"), $"*{(exportAsText ? ".txt" : ".html")}");
        string fileContent = await File.ReadAllTextAsync(files[0]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(files, Has.Length.EqualTo(1));
            Assert.That(fileContent, Does.Contain(testUserId.ToString()));
            Assert.That(fileContent, Does.Contain(testUserName));
        }

        Directory.Delete(workingDirectory, true);
    }


    private static string GetWorkingDirectory()
    {
        string? location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(location))
        {
            throw new InvalidOperationException("Execution path not found.");
        }

        return Path.Combine(location, "Data");
    }
}
