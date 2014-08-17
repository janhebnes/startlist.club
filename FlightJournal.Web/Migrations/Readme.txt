2014-08-17 jan.hebnes@gmail.com

Defined code migrations are required for updating the database model. 
Migrations have been enabled for both databases and any changes to the models must be documented by a new code migration for functioning. 

Using the Package-Manager Console

PM> Add-Migration -ConfigurationTypeName FlightJournal.Web.Migrations.FlightContext.Configuration "xxx"
PM> Add-Migration -ConfigurationTypeName FlightJournal.Web.Migrations.ApplicationDbContext.Configuration "xxx"

Please also document any changes to the model by adding SQL migration scripts.

PM> Update-Database -script -ConfigurationTypeName FlightJournal.Web.Migrations.FlightContext.Configuration 
PM> Update-Database -script -ConfigurationTypeName FlightJournal.Web.Migrations.ApplicationDbContext.Configuration 

For a specific source to target script

PM> Update-Database -SourceMigration "InitialDatabaseCreation" -TargetMigration "Renamed Plane Description to Note" -script -ConfigurationTypeName FlightJournal.Web.Migrations.FlightContext.Configuration

Related Links:

Handling multiplace ContextDb migrations 
http://stackoverflow.com/questions/21537558/multiple-db-contexts-in-the-same-db-and-application-in-ef-6-and-code-first-migra

Understanding relation between createdatabaseifnotexist and migrations 
http://social.msdn.microsoft.com/Forums/en-US/f194c802-f1b4-4b04-86d5-396945624e56/createdatabaseifnotexists-and-migratedatabasetolatestversion?forum=adodotnetentityframework




PM> get-help Update-Database 
NAME
    Update-Database
SYNOPSIS
    Applies any pending migrations to the database.
SYNTAX
    Update-Database [-SourceMigration <String>] [-TargetMigration <String>] [-Script] [-Force] [-ProjectName <String>] [-StartUpProjectName <String>] [-ConfigurationTypeName <String>] [-ConnectionStringName <String>] [-AppDomainBaseDirectory <String>] [<
    CommonParameters>]
    
    Update-Database [-SourceMigration <String>] [-TargetMigration <String>] [-Script] [-Force] [-ProjectName <String>] [-StartUpProjectName <String>] [-ConfigurationTypeName <String>] -ConnectionString <String> -ConnectionProviderName <String> [-AppDomai
    nBaseDirectory <String>] [<CommonParameters>]
DESCRIPTION
    Updates the database to the current model by applying pending migrations.


PM> get-help Add-Migration
NAME
    Add-Migration
SYNOPSIS
    Scaffolds a migration script for any pending model changes.
SYNTAX
    Add-Migration [-Name] <String> [-Force] [-ProjectName <String>] [-StartUpProjectName <String>] [-ConfigurationTypeName <String>] [-ConnectionStringName <String>] [-IgnoreChanges] [-AppDomainBaseDirectory <String>] [<CommonParameters>]
    
    Add-Migration [-Name] <String> [-Force] [-ProjectName <String>] [-StartUpProjectName <String>] [-ConfigurationTypeName <String>] -ConnectionString <String> -ConnectionProviderName <String> [-IgnoreChanges] [-AppDomainBaseDirectory <String>] [<CommonP
    arameters>]
DESCRIPTION
    Scaffolds a new migration script and adds it to the project.