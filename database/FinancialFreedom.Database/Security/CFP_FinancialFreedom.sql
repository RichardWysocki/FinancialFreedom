/*
  User without FOR LOGIN so SSDT does not require a server Login in the model (SQL71501 on Login).

  On a database that already has this user, use publish/compare or run ALTER/DROP outside this file;
  for new SQL auth installs, create the server login and run:
    ALTER USER [CFP_FinancialFreedom] WITH LOGIN = [CFP_FinancialFreedom];
*/

CREATE USER [CFP_FinancialFreedom] WITHOUT LOGIN;
