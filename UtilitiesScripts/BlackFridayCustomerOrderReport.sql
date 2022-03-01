DECLARE @from DATE='2020-11-25', @to DATE='2020-11-26'

----------------------------ORDERS + CUSTOMERS----------------------------
SELECT CreatedOnUtc [DATE], HOUR, SUM(ORDERS) AS ORDERS, SUM(CUSTOMERS) AS CUSTOMERS
FROM (
         SELECT CONVERT(char, CreatedOnUtc, 103)      CreatedOnUtc,
                DATEPART(hour, [o].[CreatedOnUtc]) AS [Hour],
                COUNT(*)                           AS [ORDERS],
                0                                  AS CUSTOMERS
         FROM [Order] AS [o]
         WHERE (CONVERT(date, [o].[CreatedOnUtc]) >= @from)
           AND (CONVERT(date, [o].[CreatedOnUtc]) <= @to)
         GROUP BY CONVERT(char, CreatedOnUtc, 103), DATEPART(hour, [o].[CreatedOnUtc])
         UNION
         SELECT CONVERT(char, CreatedOnUtc, 103)      CreatedOnUtc,
                DATEPART(hour, [c].[CreatedOnUtc]) AS [Hour],
                0,
                COUNT(*)                           AS [CUSTOMERS]
         FROM [Customer] AS [c]
         WHERE (CONVERT(date, [c].[CreatedOnUtc]) >= @from)
           AND (CONVERT(date, [c].[CreatedOnUtc]) <= @to)
         GROUP BY CONVERT(char, CreatedOnUtc, 103), DATEPART(hour, [c].[CreatedOnUtc])) A
GROUP BY CONVERT(char, CreatedOnUtc, 103), HOUR
ORDER BY CreatedOnUtc, HOUR

----------------------------ONLY ORDERS----------------------------
--DECLARE @from DATE='2020-11-25', @to DATE='2020-11-26'
SELECT CONVERT(char, CreatedOnUtc, 103)      CreatedOnUtc,
       DATEPART(hour, [o].[CreatedOnUtc]) AS [Hour],
       COUNT(*)                           AS [Count]
FROM [Order] AS [o]
WHERE (CONVERT(date, [o].[CreatedOnUtc]) >= @from)
  AND (CONVERT(date, [o].[CreatedOnUtc]) <= @to)
  AND DELETED = 0
GROUP BY CONVERT(char, CreatedOnUtc, 103), DATEPART(hour, [o].[CreatedOnUtc])
ORDER BY HOUR

----------------------------ONLY CUSTOMERS----------------------------
--DECLARE @from DATE='2020-11-25', @to DATE='2020-11-26'
SELECT CONVERT(char, CreatedOnUtc, 103)      CreatedOnUtc,
       DATEPART(hour, [c].[CreatedOnUtc]) AS [Hour],
       COUNT(*)                           AS [Count]
FROM [Customer] AS [c]
WHERE (CONVERT(date, [c].[CreatedOnUtc]) >= @from)
  AND (CONVERT(date, [c].[CreatedOnUtc]) <= @to)
GROUP BY CONVERT(char, CreatedOnUtc, 103), DATEPART(hour, [c].[CreatedOnUtc])
ORDER BY HOUR


