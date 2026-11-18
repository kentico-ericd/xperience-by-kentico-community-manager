SELECT TOP 50
	ContentItemID AS 'ID',
	ContentItemName AS 'Name'
FROM
	CMS_ContentItem c
WHERE
	c.ContentItemIsReusable = 1 AND
	c.ContentItemID NOT IN (
		SELECT
			r.ContentItemReferenceTargetItemID
		FROM
			CMS_ContentItemReference r
	)
