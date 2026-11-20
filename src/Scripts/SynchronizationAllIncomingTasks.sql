SELECT
	SynchronizationGUID,
	SynchronizationUserName AS 'Created by',
	SynchronizationCreatedWhen AS 'Created on',
	SynchronizationStatus AS 'Status'
FROM
	CMS_Synchronization
WHERE
	SynchronizationDirection = 'Incoming'