IF NOT EXISTS(SELECT 1 FROM sys.tables WHERE [name] = '{{MigrationTable}}') 
	CREATE TABLE dbo.[{{MigrationTable}}] (
		ScriptsVersion [int] NOT NULL
	)

INSERT INTO dbo.[{{MigrationTable}}] (ScriptsVersion) 
	SELECT 0 
	WHERE NOT EXISTS(SELECT 1 FROM dbo.[{{MigrationTable}}]) 