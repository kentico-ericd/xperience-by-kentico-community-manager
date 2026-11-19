SELECT
	ScheduledTaskConfigurationDisplayName AS 'Task not executed',
	ScheduledTaskConfigurationNextRunTime AS 'Scheduled run time'
FROM
	CMS_ScheduledTaskConfiguration
WHERE
	ScheduledTaskConfigurationDeleteAfterLastRun = 1
	AND ScheduledTaskConfigurationNextRunTime < DATEADD(hour, -24, GETDATE())