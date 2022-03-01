SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Slider](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[WidgetZone] [nvarchar](max) NULL,
	[Published] [bit] NOT NULL,
	[Deleted] [bit] NOT NULL,
 CONSTRAINT [PK_Slider] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Accordingly](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SliderId] [int] NOT NULL,
	[Html] [nvarchar](max) NULL,
	[MobileHtml] [nvarchar](max) NULL,
	[DisplayOrder] [nvarchar](max) NULL,
 CONSTRAINT [PK_Accordingly] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Accordingly]  WITH CHECK ADD  CONSTRAINT [FK_Accordingly_Slider_SliderId] FOREIGN KEY([SliderId])
REFERENCES [dbo].[Slider] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Accordingly] CHECK CONSTRAINT [FK_Accordingly_Slider_SliderId]
GO


