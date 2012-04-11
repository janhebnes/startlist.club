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

Establishing a local database environment based on the Entity Framework Migration
------------
1. SQLExpress must be installed and your user account must have sa priviledges
2. Create a FlightLog.Models.FlightContext using the .bak from .\Migrations
3. Update to latest Code Migration using the Package Manager Console and "PM> Update-Database", this will create the Table model and create seed data for development. Seed data can be edited in the Migration/Configuration.cs

Tips
-------------
Currently production database has been started on the 4.1 generated scheme, this means the change migration scripts do not function out of the box because of new naming convensions on index and the like, this needs to be adressed in future updates. 

The aspnet membership tables can be readded with run C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regsql.exe 

The roles "Administrator" and "Editor" must exist and have been created manually in the Role provider and attach to a user account for enabling administrative access, user accounts are created using the application.

Ps. We are still experimenting with the best setup for database management using the EF migration model, input is welcome.

Links
-------------
Read more on the entity framework 4.3 migration management on
http://blogs.msdn.com/b/adonet/archive/2012/02/09/ef-4-3-code-based-migrations-walkthrough.aspx

If you want to use the ef drop and create on model change, the aspnet membership tables can be added to master.
http://www.paragm.com/ef-v4-1-code-first-and-asp-net-membership-service/
