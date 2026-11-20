DECLARE @StuckThresholdMinutes INT = 30;

SELECT
    SynchronizationGUID AS 'Incomplete restore',
    SynchronizationStatus AS 'Status',
    DATEDIFF(MINUTE, SynchronizationLastModified, GETDATE()) AS 'Time stuck (min)',
    CASE
        WHEN SynchronizationLastModified < DATEADD(HOUR, -24, GETDATE())
            THEN 'Not cleaned up'
        ELSE 'Transitional status'
    END AS 'Issue'
FROM
    CMS_Synchronization
WHERE
    SynchronizationDirection = 'Incoming'
    AND SynchronizationStatus NOT IN ('Created', 'Ready', 'Completed', 'Failed')
    AND SynchronizationLastModified < DATEADD(MINUTE, -@StuckThresholdMinutes, GETDATE())
ORDER BY SynchronizationLastModified ASC;