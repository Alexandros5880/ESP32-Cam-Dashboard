CREATE TABLE [dbo].[EmailSender] (
    [Id]    INT           IDENTITY (1, 1) NOT NULL,
    [Email] NCHAR (255)   NULL,
    [Pass]  NVARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[FilesDirs]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Name] NCHAR(250) NOT NULL, 
    [Path] NCHAR(250) NOT NULL
);

CREATE TABLE [dbo].[FilesFormats] (
    [Id]   INT NOT NULL IDENTITY(1,1),
    [avi]  BIT DEFAULT ((0)) NOT NULL,
    [mp4]  BIT DEFAULT ((0)) NOT NULL,
    [webm] BIT DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[myCameras] (
    [Id]                NVARCHAR (50) NOT NULL,
    [urls]              NCHAR (333)   NOT NULL,
    [name]              NCHAR (50)    NOT NULL,
    [username]          NCHAR (50)    NULL,
    [password]          NCHAR (50)    NULL,
    [net_stream_port]   NCHAR (333)   NULL,
    [net_stream_prefix] NCHAR (333)   NULL,
    [net_stream]        BIT           DEFAULT ((0)) NULL,
    [Face_Detection]    BIT           DEFAULT ((0)) NOT NULL,
    [Face_Recognition]  BIT           DEFAULT ((0)) NOT NULL,
    [Brightness]        INT           DEFAULT ((0)) NOT NULL,
    [Contrast]          INT           DEFAULT ((0)) NOT NULL,
    [Darkness]          INT           DEFAULT ((0)) NOT NULL,
    [Recording]         BIT           DEFAULT ((0)) NOT NULL,
    [On_Move_Pic]       BIT           DEFAULT ((0)) NOT NULL,
    [On_Move_Rec]       BIT           DEFAULT ((0)) NOT NULL,
    [On_Move_SMS]       BIT           DEFAULT ((0)) NOT NULL,
    [On_Move_EMAIL]     BIT           DEFAULT ((0)) NOT NULL,
    [Move_Sensitivity]  INT           DEFAULT ((2)) NOT NULL,
    [Up_req]            NCHAR (333)   NULL,
    [Down_req]          NCHAR (333)   NULL,
    [Left_req]          NCHAR (333)   NULL,
    [Right_req]         NCHAR (333)   NULL,
    [isEsp32] BIT NOT NULL DEFAULT ((0)), 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[SMS]
(
	[Id]    INT           IDENTITY (1, 1) NOT NULL,
    [AccountSID] NCHAR (255)   NOT NULL,
    [AccountTOKEN]  NCHAR(255) NOT NULL,
    [Phone] NCHAR(55) NOT NULL, 
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[Users] (
    [Id]        INT         IDENTITY (1, 1) NOT NULL,
    [FirstName] NCHAR (255) NOT NULL,
    [LastName]  NCHAR (255) NOT NULL,
    [Email]     NCHAR (255) NOT NULL UNIQUE,
    [Phone]     NCHAR (255) NOT NULL,
    [Password]  NCHAR (50)  NOT NULL,
    [Licences]  NCHAR (100) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[Logged] (
    [Id]   NCHAR (100) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
