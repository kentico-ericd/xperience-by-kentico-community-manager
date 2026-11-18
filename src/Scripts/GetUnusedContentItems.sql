SELECT
	ContentItemID AS 'ID',
	ContentItemName AS 'Name'
FROM
	CMS_ContentItem c
WHERE
	c.ContentItemID NOT IN (
		SELECT
			r.ContentItemReferenceTargetItemID
		FROM
			CMS_ContentItemReference r
	)
