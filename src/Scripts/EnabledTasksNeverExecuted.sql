SELECT
	ScheduledTaskConfigurationDisplayName AS 'Enabled task never executed',
	ScheduledTaskConfigurationNextRunTime AS 'Scheduled run time'
FROM
	CMS_ScheduledTaskConfiguration
WHERE
	ScheduledTaskConfigurationDeleteAfterLastRun = 0
	AND ScheduledTaskConfigurationEnabled = 1
	AND (ScheduledTaskConfigurationExecutions IS NULL OR ScheduledTaskConfigurationExecutions = 0)