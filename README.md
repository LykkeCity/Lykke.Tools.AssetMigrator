# Lykke.Tools.AssetMigrator
Generic tool for assets migrations

**NB!** You should either cleanup migrations table before each migration session, or specify unique migration id.

## Usage example

dotnet migrate-asset.dll Transfer --balances-conn-string "azure-connection-string" --from 94853f34-3524-486d-923e-3275907aecb4 --to 2149a271-f94b-464a-8d23-673905f5feed --me-endpoint host:port --operations-url http://host/ --accuracy 5 --multiplier 3 --migration-id aa48914b-499a-480a-9334-900932b3dc07
