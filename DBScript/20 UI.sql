CREATE TABLE [dbo].[CategoryProductBoxTemplate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](400) NULL,
	[ViewPath] [nvarchar](400) NULL,
	[DisplayOrder] [int] NULL,
 CONSTRAINT [PK_CategoryProductBoxTemplate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO CategoryProductBoxTemplate VALUES('Product box','_ProductBox',1)
INSERT INTO CategoryProductBoxTemplate VALUES('ProductBoxFixedPlans','_ProductBoxFixedPlans',2)
INSERT INTO CategoryProductBoxTemplate VALUES('ProductBoxInternetPlan','_ProductBoxInternetPlan',3)
INSERT INTO CategoryProductBoxTemplate VALUES('ProductBoxMobilePlan','_ProductBoxMobilePlan',4)
INSERT INTO CategoryProductBoxTemplate VALUES('ProductBoxSmartLife','_ProductBoxSmartLife',5)
GO

ALTER TABLE Category ADD CategoryProductBoxTemplateId INT NOT NULL DEFAULT 1
GO

CREATE PROCEDURE SP_ProductTagCountByCategoryId
(
	@CategoryIds NVARCHAR(MAX) = ''
)
AS
BEGIN
	SET NOCOUNT ON
	
	SELECT pt.Id as [ProductTagId], COUNT(p.Id) as [ProductCount]
	FROM ProductTag pt with (NOLOCK)
	LEFT JOIN Product_ProductTag_Mapping pptm with (NOLOCK) ON pt.[Id] = pptm.[ProductTag_Id]
	LEFT JOIN Product p with (NOLOCK) ON pptm.[Product_Id] = p.[Id]
	INNER JOIN Product_Category_Mapping pcm on pcm.ProductId = p.Id
	WHERE
		p.[Deleted] = 0
		AND p.Published = 1
		AND pcm.CategoryId in (SELECT CAST(data as int) FROM [nop_splitstring_to_table](@CategoryIds, ','))
	GROUP BY pt.Id
	ORDER BY pt.Id

END
GO

CREATE TABLE [dbo].[NewsLetterSubscriptionType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](400) NULL,
	[DisplayOrder] [int],
 CONSTRAINT [PK_NewsLetterSubscriptionType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE NewsLetterSubscription ADD NewsLetterSubscriptionTypeId INT NOT NULL DEFAULT 0
GO

ALTER TABLE SpecificationAttribute ADD PictureId INT DEFAULT 0 NOT NULL
GO

CREATE TABLE [dbo].[SimCard](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CardNumber] [nvarchar](15) NOT NULL,
	[DisplayOrder] INT NOT NULL,
	[CreatedOnUtc] [datetime] NOT NULL,
	[UpdatedOnUtc] [date] NOT NULL,
 CONSTRAINT [PK_SimCard] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE ShoppingCartItem
ADD SimCardId INT DEFAULT 0 NOT NULL,
DevicePackage nvarchar(100)
GO

ALTER TABLE OrderItem
ADD SimCardId INT DEFAULT 0 NOT NULL,
DevicePackage nvarchar(100)
GO

INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product postpaid', 'ProductTemplate.Simple.Postpaid', 30, 10)
Go

INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product prepaid', 'ProductTemplate.Simple.Prepaid', 40, 10)
Go

INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product yoline', 'ProductTemplate.Simple.YolineOther', 50, 10)
Go

ALTER TABLE SpecificationAttribute
ADD IsShowInsideBox bit NOT NULL DEFAULT 'FALSE'
Go

INSERT INTO CategoryTemplate(Name, ViewPath, DisplayOrder) VALUES('Mobile Plans', 'CategoryTemplate.ProductsInGridOrLines.MobilePlans', 2)
Go

INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product smart life', 'ProductTemplate.Simple.SmartLife', 60, 10)
Go

INSERT INTO CategoryTemplate(Name, ViewPath, DisplayOrder) VALUES('Smart Life', 'CategoryTemplate.ProductsInGridOrLines.SmartLife', 3)
Go

INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product fixed line', 'ProductTemplate.Simple.FixedLine', 70, 10)
Go

ALTER TABLE Product_ProductAttribute_Mapping
ADD IsShowButton bit NOT NULL DEFAULT 'FALSE'
Go

INSERT INTO CategoryTemplate(Name, ViewPath, DisplayOrder) VALUES('Fixed Line', 'CategoryTemplate.ProductsInGridOrLines.FixedLine', 4)
Go

INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product Internet', 'ProductTemplate.Simple.Internet', 80, 10)
Go

INSERT INTO CategoryTemplate(Name, ViewPath, DisplayOrder) VALUES('Internet', 'CategoryTemplate.ProductsInGridOrLines.Internet', 5)
Go

INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product internet fiber', 'ProductTemplate.Simple.InternetFiber', 90, 10)
Go

INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product yoline postpaid', 'ProductTemplate.Simple.Yoline', 100, 10)
Go

INSERT INTO CategoryTemplate(Name, ViewPath, DisplayOrder) VALUES('ADSL', 'CategoryTemplate.ProductsInGridOrLines.ADSL', 6)
Go

INSERT INTO CategoryTemplate(Name, ViewPath, DisplayOrder) VALUES('Fiber', 'CategoryTemplate.ProductsInGridOrLines.Fiber', 7)
Go

INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product internet ADSL', 'ProductTemplate.Simple.InternetADSL', 110, 10)
Go

ALTER TABLE Category ADD ShowWithSubCategories bit NOT NULL DEFAULT 'FALSE'
GO

ALTER TABLE Category ADD MinimumPriceOfProduct decimal(18, 4) NOT NULL DEFAULT 0
Go

-- Monthly price attribute will create from admin
--INSERT INTO Setting(Name, Value, StoreId) VALUES('productattribute.monthlyprice', {Id from server}, 0)
-- Go

-- Monthly price attribute will create from admin
--INSERT INTO Setting(Name, Value, StoreId) VALUES('productattribute.advancedpaymentamount', {Id from server}, 0)
-- Go
INSERT INTO ProductTemplate(Name, ViewPath, DisplayOrder, IgnoredProductTypes) VALUES('Simple product internet fiber upgrade', 'ProductTemplate.Simple.InternetFiberUpgrade', 120, 10)
Go

ALTER TABLE Product ADD IsService bit NOT NULL DEFAULT 0
Go

CREATE TABLE [dbo].[Packages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](15) NOT NULL,
	[DisplayOrder] INT NOT NULL,
	[CategoryIds] [nvarchar](50) NOT NULL,
	[Published] [bit],
	[Deleted] [bit],
 CONSTRAINT [PK_Packages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PackageProduct](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PackageId] INT NOT NULL,
	[ProductId] INT NOT NULL,
	[DiscountIds] [nvarchar](50),
	[DisplayOrder] INT NOT NULL,
 CONSTRAINT [PK_PackageProduct] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE ShoppingCartItem ADD PackageId int NOT NULL DEFAULT 0
Go

ALTER TABLE OrderItem ADD PackageId int NOT NULL DEFAULT 0
Go

-- Not Executed on 77 Server
-- Pickup point id from admin
--INSERT INTO Setting(Name, Value, StoreId) VALUES('AppointmentBookingSettings.PickUpInStoreId', {Id from server}, 0)
-- Go
-- End

ALTER TABLE Product ADD IsHidePlanSelection bit NOT NULL DEFAULT 0

ALTER TABLE Product ADD  OfferDetailsCTA NVARCHAR(200),KnowingTerms NVARCHAR(max),Conditions NVARCHAR(MAX)
Go

ALTER TABLE Product ADD ImportantNotes NVARCHAR(MAX)
Go

INSERT INTO ProductAttribute VALUES('Sim Type','')
INSERT INTO PredefinedProductAttributeValue values((SELECT TOP 1 Id FROM ProductAttribute WHERE [Name] = 'Sim Type'),'Esim',0,0,0,0,1,1)
INSERT INTO PredefinedProductAttributeValue values((SELECT TOP 1 Id FROM ProductAttribute WHERE [Name] = 'Sim Type'),'Sim',0,0,0,0,0,2)
INSERT INTO Setting(Name, Value, StoreId) VALUES('productattribute.simtype', (SELECT TOP 1 Id FROM ProductAttribute WHERE [Name] = 'Sim Type'), 0)
INSERT INTO Setting(Name, Value, StoreId) VALUES('productattribute.simcardnumber', 43, 0)
INSERT INTO Setting(Name, Value, StoreId) VALUES('ProductAttribute.DevicePackage', 146, 0)


ALTER TABLE Accordingly 
ADD TabletHtml nvarchar(max) 
Go

ALTER TABLE ShoppingCartItem
Add SubsidyDiscount decimal Not Null Default '0'