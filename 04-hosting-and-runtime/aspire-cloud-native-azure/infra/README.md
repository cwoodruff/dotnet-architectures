# Deploying to Azure

There is no Bicep in this folder yet, on purpose. The intended path is the Azure Developer CLI (`azd`), which reads the Aspire AppHost and generates the infrastructure from it.

```bash
azd init     # run from the example root; azd detects src/Chinook.CloudNative.AppHost and proposes services
azd up       # provisions Azure resources and deploys the ApiService and Web as Azure Container Apps
```

`azd init` writes `azure.yaml` and, if you ask it to (`azd infra gen`), the Bicep files that would live in this folder. I would generate them only when I need to customize something the AppHost cannot express, and then commit them here.

## What happens to the database

In development the AppHost declares `builder.AddConnectionString("Chinook")`, an external connection string that points at the shared `chinook-mssql` container started by `database/start-db.sh`. Azure has no such container, so before `azd up` that resource has to become something Azure can provision. The intended change is to switch the AppHost to `builder.AddAzureSqlServer("sql").AddDatabase("Chinook")` (from `Aspire.Hosting.Azure.Sql`) when publishing. `azd` then provisions an Azure SQL server and a `Chinook` database and hands the connection string to the ApiService under the same `ConnectionStrings:Chinook` key, so the service code does not change.

Loading the Chinook schema and data into that Azure SQL database is a separate, one-time step. Run `database/chinook.docker.sql` against it with `sqlcmd` or Azure Data Studio. It is not part of `azd up`.
