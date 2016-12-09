USE [dbname]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE SCHEMA GitHub

CREATE TABLE [GitHub].[ViewsByTwoWeeks](
	[date] [date] NOT NULL PRIMARY KEY,
	[count] [int] NOT NULL,
	[uniques] [int] NOT NULL,
	[created_time] [datetime] NOT NULL DEFAULT GETDATE(),
	[updated_time] [datetime] NOT NULL,
)

CREATE TABLE [GitHub].[ViewsByDate](
	[date] [date] NOT NULL PRIMARY KEY,
	[count] [int] NOT NULL,
	[uniques] [int] NOT NULL,
	[created_time] [datetime] NOT NULL DEFAULT GETDATE(),
	[updated_time] [datetime] NOT NULL,
)


CREATE TABLE [GitHub].[DownloadsByDate](
	[download_id] [nvarchar](20) NOT NULL PRIMARY KEY,
	[name] [nvarchar](max) NOT NULL,
	[version] [nvarchar](20) NOT NULL,
	[download_count] [int] NOT NULL,
	[created_time] [datetime] NOT NULL DEFAULT GETDATE(),
	[updated_time] [datetime] NOT NULL,
)

CREATE TABLE [GitHub].[PathsByTwoWeeks](
	[path_id] [nvarchar](20) NOT NULL PRIMARY KEY,
	[path] [nvarchar](max) NOT NULL,
	[count] [int] NOT NULL,
	[uniques] [int] NOT NULL,
	[created_time] [datetime] NOT NULL DEFAULT GETDATE(),
	[updated_time] [datetime] NOT NULL,
)

CREATE TABLE [GitHub].[ReferrersByTwoWeeks](
	[referrer_id] [nvarchar](20) NOT NULL PRIMARY KEY,
	[referrer] [nvarchar](max) NOT NULL,
	[count] [int] NOT NULL,
	[uniques] [int] NOT NULL,
	[created_time] [datetime] NOT NULL DEFAULT GETDATE(),
	[updated_time] [datetime] NOT NULL,
)

GO
