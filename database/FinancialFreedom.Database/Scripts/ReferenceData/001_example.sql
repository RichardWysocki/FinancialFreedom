/*
  Reference / seed data scripts for the FinancialFreedom initiative.
  Wire into Post-Deployment\Script.PostDeployment.sql with :r when ready to run on publish.
*/

-- Example (disabled):
-- IF NOT EXISTS (SELECT 1 FROM [dbo].[FinancialGoals] WHERE [Id] = '00000000-0000-0000-0000-000000000001')
-- INSERT INTO [dbo].[FinancialGoals] ([Id], [Name], [TargetAmount], [CreatedAt])
-- VALUES ('00000000-0000-0000-0000-000000000001', N'Emergency fund', 10000.00, SYSUTCDATETIME());
