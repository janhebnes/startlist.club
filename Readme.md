Flight Journal
=============

Flight journal for Flight Clubs.
Created for handling Soaring flights in Danish (possibly nordic) flight clubs.

Deployment
------------
http://startlist.club
http://log.flyveklubben.dk

Contributing
------------

1. Fork it.
2. Create a branch (`git checkout -b my_feature`)
3. Commit your changes (`git commit -am "Added feature x"`)
4. Push to the branch (`git push origin my_feature`)

Watch the Trello board for upcomming request.
https://trello.com/board/flight-journal/4f592a73bb5d895218126fed

Establishing local development - Requires using the Entity Framework for generating the databases
------------
1. Visual Studio and SQL or SQLExpress must be installed and your user account must have owner priviledges
2. Enable the #if DEBUG section in \FlightJournal.Web\App_Start\IdentityConfig.cs for allowing FlightJournal.Membership database to be auto created (the admin user is seeded inside this section)
3. Enable the #if DEBUG section in \FlightJournal.Web\Models\FlightContext.cs for allowing FlightJournal database to be auto created
4. Launch the application or Update to latest Code Migration using the Package Manager Console and "PM> Update-Database", this will create the Table model and create seed data for development (if step 2 and 3 are uncommented).

Ps. Further migration scripts are stored native to Entity Framework practices in the .\Migrations folder 

Tips
-------------
The roles "Admin" and "Editor" must exist and have been created manually in the Role provider and attach to a user account for enabling administrative access, user accounts are created using the application.


