
# 1. Setup SQL Server

Inside xp-dotnet, install this chart

```sh
helm install mssql-server . --set ACCEPT_EULA.value=Y --set MSSQL_PID.value=Developer --set Values.sa_password=mypassword1
```

Access MS SQL pod via mssql tools container with sidecar

```sh
kubectl run sqlcmd-pod -it --rm --image=mcr.microsoft.com/mssql-tools -- bash
```

From the side car, authenticate with sqlcmd in the sql server pod. Password was set in xp-sqlserver/values.yaml

```sh
sqlcmd -S 10.98.130.97,1433 -U SA -P "Toughpass1!"
```

Create table and add values

```sql
CREATE DATABASE ApplicationDb;
GO

USE ApplicationDb;
GO

CREATE TABLE Accounts (
    Id INT IDENTITY PRIMARY KEY,
    Email NVARCHAR(50) NOT NULL,
    DateOfBirth DATE,
    AccountNumber NVARCHAR(20) UNIQUE,
    Balance DECIMAL(18, 2) DEFAULT 0.00,
    CreatedAt DATETIME DEFAULT GETDATE(),
);
GO

INSERT INTO Accounts (Email, DateOfBirth, AccountNumber, Balance)
VALUES 
('john.doe@example.com', '1985-06-15', 'ACC123456', 1000.00),
('jane.smith@example.com', '1990-09-25', 'ACC654321', 2500.50),
('alice.jones@example.com', '1978-12-05', 'ACC789012', 150.75);
GO
```

To quit

```sh
quit
```

Add connection string to dev-accounts-secrets with base64 encoding. Note the password and database name that was created

```sh
[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('Server=mssql-server-mssql-latest.default.svc.cluster.local,1433;Database=ApplicationDb;User ID=sa;Password=Toughpass1!;TrustServerCertificate=True;'))
```

# 2. Start server

Setup service to allow outside communication

```sh
kubectl apply -f dev-accounts-service.yaml
```

Setup configuration which will be used my application pods

```sh
kubectl apply -f dev-accounts-configmap.yaml
```

Setup secrets which will be used my application pods

```sh
kubectl apply -f dev-accounts-secrets.yaml
```

Setup deployment which is responsible for creating objects and manage the pods

```sh
kubectl apply -f dev-accounts-deployment.yaml
```