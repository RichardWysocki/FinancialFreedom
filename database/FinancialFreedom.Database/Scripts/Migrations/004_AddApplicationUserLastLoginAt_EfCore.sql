BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502020534_AddApplicationUserLastLoginAt'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [LastLoginAt] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502020534_AddApplicationUserLastLoginAt'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502020534_AddApplicationUserLastLoginAt', N'9.0.0');
END;

COMMIT;
GO

