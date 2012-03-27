Flight Journal
=============

Flight journal for Flight Clubs.
Created for handling Soaring flights in Danish flight clubs.

Deployment
------------

http://log.flyveklubben.dk

Contributing
------------

1. Fork it.
2. Create a branch (`git checkout -b my_markup`)
3. Commit your changes (`git commit -am "Added Snarkdown"`)
4. Push to the branch (`git push origin my_markup`)

Watch the Trello board for upcomming request.
https://trello.com/board/flight-journal/4f592a73bb5d895218126fed

Establishing a local environment
------------
1.SQLExpress must be installed and your user account must have sa priviledges
2. Create a FlightLog.Models.FlightContext 
3. Create EntityFramework system tabel: MigrationHistory e.g. using EF tools or FlightLog.Web\Migrations\MigrationHistory.sql
4. Create InitialCreate tables with either Update-Database PM tools or with \FlightLog.Web\Migrations\InitialCreate.sql
5. Add aspnet tables with run C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regsql.exe 

Migrate to the latest database migration
create the roles "Administrator" and "Editor" manually in the Role provider for administrative access.

Read more on the entity framework 4.3 migration management on
http://blogs.msdn.com/b/adonet/archive/2012/02/09/ef-4-3-code-based-migrations-walkthrough.aspx