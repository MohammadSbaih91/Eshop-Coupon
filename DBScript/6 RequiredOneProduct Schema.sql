ALTER TABLE [Product] ADD	[RequireAnyOneFromOtherProducts] [bit] NOT NULL default 0
go
ALTER TABLE [Product] ADD	[RequiredAnyOneFromOtherProductIds] [nvarchar](1000) NULL default null