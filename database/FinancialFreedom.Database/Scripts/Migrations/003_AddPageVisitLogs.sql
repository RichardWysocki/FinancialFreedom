BEGIN TRANSACTION;
CREATE TABLE [PageVisitLogs] (
    [Id] uniqueidentifier NOT NULL,
    [VisitedAt] datetimeoffset NOT NULL,
    [UserId] nvarchar(450) NULL,
    [UserName] nvarchar(256) NULL,
    [PagePath] nvarchar(2048) NOT NULL,
    [QueryString] nvarchar(2048) NULL,
    CONSTRAINT [PK_PageVisitLogs] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_PageVisitLogs_VisitedAt] ON [PageVisitLogs] ([VisitedAt]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260501163333_AddPageVisitLogs', N'9.0.0');

COMMIT;
GO

