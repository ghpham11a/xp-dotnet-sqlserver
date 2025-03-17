
# 1. Setup SQL Server

Inside xp-sqlserver, install this chart

```sh
helm install mssql-server . --set ACCEPT_EULA.value=Y --set MSSQL_PID.value=Developer --set Values.sa_password=mypassword1
```

Access MS SQL pod via mssql tools container with sidecar

```sh
kubectl run sqlcmd-pod -it --rm --image=mcr.microsoft.com/mssql-tools -- bash
```

From the side car, authenticate with sqlcmd in the sql server pod. Password was set in xp-sqlserver/values.yaml

```sh
sqlcmd -S 10.110.104.29,1433 -U SA -P "Toughpass1!"
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

# 3. Create Redis Cache

```
dotnet add package StackExchange.Redis
```

Install Redis Helm chart

```
helm install xp-redis oci://registry-1.docker.io/bitnamicharts/redis
```

This chart creates a Redis secret that will be attached to the deployment

Optional: connect to Redis

```
# Store the password in Powershell
$REDIS_PASSWORD = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String((kubectl get secret --namespace default xp-redis -o jsonpath="{.data.redis-password}")))

# Store the password in Bash
export REDIS_PASSWORD=$(kubectl get secret --namespace default xp-redis -o jsonpath="{.data.redis-password}" | base64 -d)

kubectl run --namespace default redis-client --restart='Never' --env REDIS_PASSWORD=$REDIS_PASSWORD  --image docker.io/bitnami/redis:7.4.2-debian-12-r4 --command -- sleep infinity

kubectl exec --tty -i redis-client --namespace default -- bash

redis-cli -h xp-redis-master -p 6379

AUTH [REDIS_PASSWORD]
```

# 4. Create Kafka

```
dotnet add package Confluent.Kafka
```

Install Kafka Helm chart

```
helm install xp-kafka oci://registry-1.docker.io/bitnamicharts/kafka
```

To check that pods were spun up

```
kubectl get pods --selector app.kubernetes.io/instance=xp-kafka
```

To create the topics, we will start another pod that goes into the Kafka pods and creates the topics and shuts down. To do this, we need to find the Kafka password and update it's value in the dev-kafka-topics-admin.yaml.

```
# This gets an encoded value
kubectl get secret xp-kafka-user-passwords -o jsonpath='{.data.client-passwords}'

# we need the decoded version to put into the yaml
# Powershell
[System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String((kubectl get secret xp-kafka-user-passwords -o jsonpath='{.data.client-passwords}')))

# bash
kubectl get secret xp-kafka-user-passwords -o jsonpath='{.data.client-passwords}' | base64 -d
```

Get the password and update the password field in dev-kafka-topics-admin.yaml

```
... required username=\"user1\" password=\"bFHKRJA2Y5\";" >> /tmp/kafka-client.properties ...
```

Then just run this command which applies the yaml updates

```
kubectl apply -f dev-kafka-topics-admin.yaml
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