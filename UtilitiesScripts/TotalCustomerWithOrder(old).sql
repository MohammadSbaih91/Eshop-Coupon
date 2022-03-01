DECLARE @FromDate Date = '2020-09-28'
		
DECLARE @ToDate DATE
SET @ToDate = DATEADD(DAY,1, @FromDate)

SELECT 
	FORMAT(b.LastActivityDateUtcMin, 'dd/MM/yyyy HH:mm') as FromDate,
	FORMAT(b.LastActivityDateUtcMax, 'HH:mm') as ToDate,
	b.TotalCustomer,
	b.TotalOrder
FROM
(
	SELECT 
		MIN(ca.LastActivityDateUtc) as LastActivityDateUtcMin,
		MAX(ca.LastActivityDateUtc) as LastActivityDateUtcMax,
		COUNT(a.CustoemrId) AS TotalCustomer,
		SUM(a.TotalOrder) AS TotalOrder
	FROM (
		SELECT DISTINCT
			c.Id AS CustoemrId,
			COUNT(o.Id) AS TotalOrder
		FROM Customer c
		LEFT JOIN [Order] o on o.CustomerId = c.Id AND o.CreatedOnUtc > @FromDate AND o.CreatedOnUtc < @ToDate
		WHERE c.LastActivityDateUtc  > @FromDate and c.LastActivityDateUtc < @ToDate
		GROUP BY c.Id
	) a
	INNER JOIN Customer ca on ca.Id = a.CustoemrId
	GROUP BY (DATEPART(HOUR, LastActivityDateUtc)/2)
) b ORDER By LastActivityDateUtcMin
