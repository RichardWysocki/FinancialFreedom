CREATE TABLE [dbo].[PageVisitLogs]
(
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [VisitedAt]    DATETIMEOFFSET (7) NOT NULL,
    [UserId]       NVARCHAR (450)   NULL,
    [UserName]     NVARCHAR (256)   NULL,
    [PagePath]     NVARCHAR (2048)  NOT NULL,
    [QueryString]  NVARCHAR (2048)  NULL,
    CONSTRAINT [PK_PageVisitLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

CREATE NONCLUSTERED INDEX [IX_PageVisitLogs_VisitedAt]
    ON [dbo].[PageVisitLogs]([VisitedAt] ASC);
