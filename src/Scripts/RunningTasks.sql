SELECT
	ScheduledTaskConfigurationDisplayName AS 'Currently running task',
	ScheduledTaskConfigurationIsCancellationRequested AS 'Cancellation requested'
FROM
	CMS_ScheduledTaskConfiguration
WHERE
	ScheduledTaskConfigurationDeleteAfterLastRun = 0
	AND ScheduledTaskConfigurationIsRunning = 1