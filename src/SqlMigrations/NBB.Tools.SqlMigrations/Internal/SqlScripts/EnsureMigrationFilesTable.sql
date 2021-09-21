IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE [name] = '{{MigrationFileTable}}') 
	CREATE TABLE dbo.[{{MigrationFileTable}}] (
		FileName nvarchar(255) NOT NULL
	)