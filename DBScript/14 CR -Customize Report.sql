IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.SP_CustomReport'))
BEGIN
	DROP PROCEDURE [dbo].[SP_CustomReport]
END
GO

CREATE PROCEDURE [dbo].[SP_CustomReport]
(
	@CreatedFromUtc		DATETIME = NULL,
	@CreatedToUtc		DATETIME = NULL,
	@OrderStatus		int = 0,
	@ProductId			int = 0,
	@PageIndex			int = 0,
	@PageSize			int = 2147483644
)
AS
BEGIN
	--paging
	DECLARE @PageLowerBound int
	DECLARE @PageUpperBound int
	DECLARE @RowsToReturn int
	SET @RowsToReturn = @PageSize * (@PageIndex + 1)	
	SET @PageLowerBound = @PageSize * @PageIndex
	SET @PageUpperBound = @PageLowerBound + @PageSize + 1

	DECLARE @OrderIdsTable TABLE(
		IndexId INT IDENTITY(1,1) PRIMARY KEY,
		OrderId INT
	)
	DECLARE @CustomReportTable TABLE(
		OrderId INT,
		CustomOrderNumber NVARCHAR(MAX),
		OrderStatusId INT,
		CustomerName NVARCHAR(MAX),
		PhoneNumber NVARCHAR(100),
		CustomerAddress NVARCHAR(MAX),
		CustomerEmail NVARCHAR(100),
		CreatedOn DATETIME,
		OrderTotal DECIMAL(18,4),
		ProductName NVARCHAR(MAX),
		VendorNames NVARCHAR(MAX),
		CustomerId NVARCHAR(100)
	)

	DECLARE @crTable TABLE(
		IndexId INT IDENTITY(1,1) PRIMARY KEY,
		OrderId INT,
		CustomOrderNumber NVARCHAR(MAX),
		OrderStatusId INT,
		CustomerName NVARCHAR(MAX),
		PhoneNumber NVARCHAR(100),
		CustomerAddress NVARCHAR(MAX),
		CustomerEmail NVARCHAR(100),
		CreatedOn DATETIME,
		OrderTotal DECIMAL(18,4),
		ProductName NVARCHAR(MAX),
		VendorNames NVARCHAR(MAX),
		CustomerId NVARCHAR(100)
	)


	INSERT INTO @CustomReportTable
		SELECT 
			o.Id,
			o.CustomOrderNumber,
			o.OrderStatusId,
			a.FirstName + ' ' + a.LastName AS CustomerName,
			a.PhoneNumber,
			a.Address1 + coalesce(',' + nullif(a.Address2,'') , '')  + coalesce(',' + nullif(a.City,'') , '') + coalesce(' - ' + nullif(a.ZipPostalCode,'') , '') CustomerAddress,
			a.Email,
			o.CreatedOnUtc,
			o.OrderTotal,
			p.[Name]  AS ProductName,
			v.[Name] AS VendorName,
			a.IdentityCardOrPassport as CustomerId
		FROM [Order] o
			INNER JOIN OrderItem oi on oi.OrderId = o.Id 
			LEFT JOIN [Address] a on a.Id = o.BillingAddressId
			LEFT JOIN Product p on p.Id = oi.ProductId
			LEFT JOIN Vendor v on v.Id = p.VendorId
		WHERE o.Deleted = 0 
			AND (@CreatedFromUtc IS NULL OR o.CreatedOnUtc >= @CreatedFromUtc)
			AND (@CreatedToUtc IS NULL OR o.CreatedOnUtc <= @CreatedToUtc)
			AND (@OrderStatus = 0 OR o.OrderStatusId = @OrderStatus)
			AND (@ProductId = 0 OR oi.ProductId = @ProductId)
		ORDER By o.Id

	INSERT INTO @crTable
	SELECT 
		t2.OrderId, 
		t2.CustomOrderNumber,
		t2.OrderStatusId,
		t2.CustomerName,
		t2.PhoneNumber,
		t2.CustomerAddress,
		t2.CustomerEmail,
		t2.CreatedOn,
		t2.OrderTotal,
		STUFF((SELECT DISTINCT ',' + CAST(ProductName AS varchar) FROM @CustomReportTable t1  where t1.OrderId =t2.OrderId FOR XML PATH('')), 1 ,1, '') AS Productname,
		STUFF((SELECT DISTINCT ',' + CAST(VendorNames AS varchar) FROM @CustomReportTable t3  where t3.OrderId =t2.OrderId FOR XML PATH('')), 1 ,1, '') AS Vendorname,
		t2.CustomerId
	FROM @CustomReportTable t2
	GROUP BY t2.OrderId, 
		t2.CustomOrderNumber,
		t2.OrderStatusId,
		t2.CustomerName,
		t2.PhoneNumber,
		t2.CustomerAddress,
		t2.CustomerEmail,
		t2.CreatedOn,
		t2.OrderTotal,
		t2.CustomerId

	SELECT TOP (@RowsToReturn)
		OrderId As Id,
		CustomOrderNumber,
		OrderStatusId,
		CustomerName,
		PhoneNumber,
		CustomerAddress,
		CustomerEmail,
		CreatedOn,
		OrderTotal,
		ProductName,
		VendorNames,
		CustomerId
	FROM @crTable t2
	WHERE
		[t2].IndexId > @PageLowerBound AND 
		[t2].IndexId < @PageUpperBound
	ORDER By t2.IndexId
END
