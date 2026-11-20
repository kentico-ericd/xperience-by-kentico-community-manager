SELECT
	SynchronizationGUID AS 'Failed restore',
	SynchronizationUserName AS 'Created by',
	SynchronizationCreatedWhen AS 'Created on'
FROM
	CMS_Synchronization
WHERE
	SynchronizationDirection = 'Incoming'
	AND SynchronizationStatus = 'Failed'