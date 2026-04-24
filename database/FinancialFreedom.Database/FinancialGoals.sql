CREATE TABLE [dbo].[FinancialGoals]
(
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [Name]         NVARCHAR (200)   NOT NULL,
    [TargetAmount] DECIMAL (18, 2)  NOT NULL,
    [CreatedAt]    DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_FinancialGoals] PRIMARY KEY CLUSTERED ([Id] ASC)
);
