SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EmployeeDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeName] [nvarchar](max) NOT NULL,
	[EmployeeId] [nvarchar](50) NULL,
	[EmployeeContactNumber] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[Months] [int] NULL,
	[Amount] [decimal](18, 2) NULL,
	[OrderNumber] INT NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_EmployeeDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

INSERT INTO MessageTemplate VALUES('OrderPlaced.EmployeeDetail',NULL,'Employee Detail',	'',1,NULL,0,0,1,0)
GO

INSERT INTO Setting VALUES('orderemployee.emailid','p.d.dobariya11@gmail.com',0)
GO
