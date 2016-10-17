
/****** Object:  Table [dbo].[Logging]    Script Date: 09/26/2011 08:54:44 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Logging]') AND type in (N'U'))
 DROP TABLE [dbo].[Logging]
GO

/****** Object:  Table [dbo].[Logging]    Script Date: 09/26/2011 08:54:44 ******/
CREATE TABLE [dbo].Logging(
	LoggingId [int] IDENTITY(1000,1) NOT NULL,
	[Created] [datetimeoffset] NOT NULL default getutcdate(),
	[Message] [varchar](4096) NOT NULL,
	[Level] [varchar](16) NOT NULL,
	[Category] [varchar](32) NULL,
	-- fields below are added to match elmah fields, in case we want to combine the two tables later
	[Source] nvarchar(60) not null , -- which application the log message came from
	Host nvarchar(50) not null , -- machine the log came from
	[Type] nvarchar(100) not null , -- if an error, what is the type
	ThreadId nvarchar(8) null ,

 CONSTRAINT [PK_Logging] PRIMARY KEY CLUSTERED 
(
	[LoggingId] ASC
))


CREATE NONCLUSTERED INDEX IDX_Logging_Created ON [dbo].Logging
(
	Created ASC
)

CREATE NONCLUSTERED INDEX IDX_Logging_Source ON [dbo].Logging
(
	[Source] ASC
)

CREATE NONCLUSTERED INDEX IDX_Logging_Level ON [dbo].Logging
(
	[Level] ASC
)

CREATE NONCLUSTERED INDEX IDX_Logging_Type ON [dbo].Logging
(
	[Type] ASC
)

GO
/****
    Purpose: 
             INSERT procedure for the Logging table   
    Notes:   
             Standard INSERT procedure. 
****/


If EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'LoggingInsert')
    BEGIN
        PRINT 'Dropping Procedure: LoggingInsert'
        DROP PROCEDURE LoggingInsert
    END
GO

PRINT 'Creating Procedure LoggingInsert'
GO
CREATE PROCEDURE LoggingInsert
	@Message [varchar](4096) ,
	@Level [varchar](16) ,
	@Category [varchar](32) = NULL,
	@Source nvarchar(60) ,
	@Host nvarchar(50) ,
	@Type nvarchar(100),
	@ThreadId nvarchar(4) = null ,
	@LoggingId [int] OUTPUT 
	
AS
 BEGIN

    DECLARE @Err int
	
    INSERT
        dbo.Logging
        (
		Created ,
		[Message] ,
		[Level] ,
		[Category] ,
		[Source] ,
		Host ,
		[Type] ,
		ThreadId 
        )
    VALUES
        (
		getutcdate() ,
		@Message ,
		@Level ,
		@Category ,
		@Source ,
		@Host ,
		@Type  ,
		@ThreadId 
        )

    SELECT @Err = @@ERROR

    IF(@Err = 0)
    BEGIN
	    SET @LoggingId = SCOPE_IDENTITY()
    END
	

    RETURN @Err

 END

GO

 
-- run on master db (set the password from KeePass)
CREATE LOGIN LoggingUser WITH PASSWORD=N'in KeePass'
GO
-- user needs to be added to master db too!
create User LoggingUser from login LoggingUser
Go

-- run against DB
create User LoggingUser from login LoggingUser

EXEC sp_addrolemember 'db_datareader', 'LoggingUser'
EXEC sp_addrolemember 'db_datawriter', 'LoggingUser'

-- need this so user can exec the proc
grant execute on LoggingInsert to LoggingUser
grant execute on LoggingClientInsert to LoggingUser



/************************************************
LoggingClient - Only logs errors from non server components, e.g. RWAC

************************************************/


/****** Object:  Table [dbo].[LoggingClient]    Script Date: 09/26/2011 08:54:44 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoggingClient]') AND type in (N'U'))
 DROP TABLE [dbo].[LoggingClient]
GO

/****** Object:  Table [dbo].[LoggingClient]    Script Date: 09/26/2011 08:54:44 ******/
CREATE TABLE [dbo].LoggingClient(
	LoggingClientId [int] IDENTITY(1000,1) NOT NULL,
	[Created] [datetimeoffset] NOT NULL default getutcdate(),
	[Message] [varchar](4096) NOT NULL,
	[Level] [varchar](16) NOT NULL,
	[Category] [varchar](32) NULL,
	-- fields below are added to match elmah fields, in case we want to combine the two tables later
	[Source] nvarchar(60) not null , -- which application the log message came from
	Host nvarchar(50) not null , -- machine the log came from
	[Type] nvarchar(100) not null , -- if an error, what is the type
	ThreadId nvarchar(8) null ,

	-- extra for LoggingClient
	[AppKey] [int] NULL,
	[Version] [nvarchar](32) NULL,
	[Component] [nvarchar](64) NULL,
	

 CONSTRAINT [PK_LoggingClient] PRIMARY KEY CLUSTERED 
(
	[LoggingClientId] ASC
))


CREATE NONCLUSTERED INDEX IDX_LoggingClient_Created ON [dbo].LoggingClient
(
	Created ASC
)

CREATE NONCLUSTERED INDEX IDX_LoggingClient_Source ON [dbo].LoggingClient
(
	[Source] ASC
)

CREATE NONCLUSTERED INDEX IDX_LoggingClient_Level ON [dbo].LoggingClient
(
	[Level] ASC
)

CREATE NONCLUSTERED INDEX IDX_LoggingClient_Type ON [dbo].LoggingClient
(
	[Type] ASC
)

CREATE NONCLUSTERED INDEX IDX_LoggingClient_AppKey ON [dbo].LoggingClient
(
	AppKey ASC
)
GO

/****
    Purpose: 
             INSERT procedure for the LoggingClient table   
    Notes:   
             Standard INSERT procedure. 
****/


If EXISTS (SELECT * FROM sysobjects WHERE type = 'P' AND name = 'LoggingClientInsert')
    BEGIN
        PRINT 'Dropping Procedure: LoggingClientInsert'
        DROP PROCEDURE LoggingClientInsert
    END
GO

PRINT 'Creating Procedure LoggingClientInsert'
GO
CREATE PROCEDURE LoggingClientInsert
	@Message [varchar](4096) ,
	@Level [varchar](16) ,
	@Category [varchar](32) = NULL,
	@Source nvarchar(60) ,
	@Host nvarchar(50) ,
	@Type nvarchar(100),
	@ThreadId nvarchar(4) = null , 
	
	@AppKey int NULL,
	@Version nvarchar(32) NULL,
	@Component nvarchar(64) NULL,

	@LoggingClientId [int] OUTPUT 
	
AS
 BEGIN

    DECLARE @Err int
	
    INSERT
        dbo.LoggingClient
        (
		Created ,
		[Message] ,
		[Level] ,
		[Category] ,
		[Source] ,
		Host ,
		[Type] ,
		ThreadId ,
		
		[AppKey] ,
		[Version] ,
		[Component] 
        )
    VALUES
        (
		getutcdate() ,
		@Message ,
		@Level ,
		@Category ,
		@Source ,
		@Host ,
		@Type ,
		@ThreadId ,
		
		@AppKey ,
		@Version ,
		@Component 

        )

    SELECT @Err = @@ERROR

    IF(@Err = 0)
    BEGIN
	    SET @LoggingClientId = SCOPE_IDENTITY()
    END
	

    RETURN @Err

 END

GO

grant execute on LoggingClientInsert to LoggingUser
