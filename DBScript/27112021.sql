CREATE TABLE [dbo].[SliderGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SliderId] [int] NOT NULL,
	[Title] [nvarchar](400) NULL,
	[Description] [nvarchar](Max) NULL,
	[DisplayOrder] [int] NULL,
	[Deleted] [bit],
 CONSTRAINT [PK_SliderGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE Accordingly
ADD SliderGroupId	 int NOT NULL DEFAULT 0,
PictureId	 int NULL,
MobilePictureId	 int NULL,
TabletPictureId	 int NULL,
Position	 int NULL,
Alignment	 int NULL,
ClickToAction NVARCHAR(MAX)
GO