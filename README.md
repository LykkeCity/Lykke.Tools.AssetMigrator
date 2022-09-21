# Lykke.Tools.AssetMigrator

## Generic tool for assets migrations

**NB!** You should either cleanup migrations table before each copy/transfer session, or specify unique migration id.

**Usage:** dotnet migrate-asset.dll [command] [options] 

**Generic options:**
> --version  Show version information

**Commands:**

> burn      
> copy      
> transfer
> credit

**Burn options**:

> -h|--help - Show help information  
> --asset-accuracy - Asset accuracy  
> --asset-id - Asset id  
> --balances-conn-string - Lykke.Service.Balances connection string  
> --me-endpoint - ME endpoint (host:port)


**Copy and transfer options**

> -h|--help - Show help information  
> --balances-conn-string - Lykke.Service.Balances connection string  
> --me-endpoint - ME endpoint (host:port)
> --operations-url - Lykke.Service.OperationsRepository url    
> --from - Source asset id  
> --accuracy - Target asset accuracy  
> --to - Target asset id  
> --migration-id - Migration id (guid, optional)  
> --multiplier - Multiplier (greater or equal to one, optional)  

**Credit options**

> -h|--help - Show help information  
> --balances-conn-string - Lykke.Service.Balances connection string  
> --me-endpoint - ME endpoint (host:port) 
> --execution-id - Unique execution id (guid)
> --credit-plan-csv-path - Path to the CSV file with the credit plan in format: [wallet_id, asset_id, asset_accuracy, amount] and header

### Usage example

dotnet migrate-asset.dll transfer --balances-conn-string "azure-connection-string" -- --operations-url http://localhost --from 94853f34-3524-486d-923e-3275907aecb4 --to 2149a271-f94b-464a-8d23-673905f5feed --me-endpoint host:port --accuracy 5 --multiplier 3 --migration-id aa48914b-499a-480a-9334-900932b3dc07
