/* MIGRATION BATCH FROM RELEASE 1.0 to RELEASE 2.0 OF THE FLIGHT JOURNAL DATABASE LAYER (ACTUAL LAUNCH DATE) */ 

DELETE FROM FlightJournal.dbo.Locations
SET IDENTITY_INSERT FlightJournal.dbo.Locations ON 
INSERT INTO FlightJournal.dbo.Locations(LocationId, Name) SELECT LocationId, Name FROM [FlightLog.Models.FlightContext].dbo.Locations 
SET IDENTITY_INSERT FlightJournal.dbo.Locations OFF
SELECT * FROM FlightJournal.dbo.Locations


DELETE FROM FlightJournal.dbo.Clubs
SET IDENTITY_INSERT FlightJournal.dbo.Clubs ON 
INSERT INTO FlightJournal.dbo.Clubs(ClubId,ShortName,Name,LocationId) SELECT ClubId,ShortName,Name,ISNULL(DefaultStartLocation_LocationId,1) FROM [FlightLog.Models.FlightContext].dbo.Clubs 
SET IDENTITY_INSERT FlightJournal.dbo.Clubs OFF
SELECT * FROM FlightJournal.dbo.Clubs


DELETE FROM FlightJournal.dbo.StartTypes
SET IDENTITY_INSERT FlightJournal.dbo.StartTypes ON 
INSERT INTO FlightJournal.dbo.StartTypes(StartTypeId, ShortName, Name, ClubId) SELECT StartTypeId, ShortName, Name, ClubId FROM [FlightLog.Models.FlightContext].dbo.StartTypes 
SET IDENTITY_INSERT FlightJournal.dbo.StartTypes OFF
SELECT * FROM FlightJournal.dbo.StartTypes


DELETE FROM FlightJournal.dbo.Planes
SET IDENTITY_INSERT FlightJournal.dbo.Planes ON 
INSERT INTO FlightJournal.dbo.Planes(PlaneId, Registration, CompetitionId, Seats, Engines, ExitDate, StartTypeId, Type) SELECT PlaneId, Registration, CompetitionId, Seats, Engines, ExitDate, StartTypeId, '' FROM [FlightLog.Models.FlightContext].dbo.Planes 
SET IDENTITY_INSERT FlightJournal.dbo.Planes OFF
SELECT * FROM FlightJournal.dbo.Planes


DELETE FROM FlightJournal.dbo.Pilots
SET IDENTITY_INSERT FlightJournal.dbo.Pilots ON 
INSERT INTO FlightJournal.dbo.Pilots(PilotId, Name, UnionId, MemberId, ClubId, PilotStatus_PilotStatusId, MobilNumber, Email) SELECT PilotId, Name, UnionId, MemberId, ClubId, NULL, NULL, NULL FROM [FlightLog.Models.FlightContext].dbo.Pilots 
SET IDENTITY_INSERT FlightJournal.dbo.Pilots OFF
SELECT * FROM FlightJournal.dbo.Pilots


DELETE FROM FlightJournal.dbo.Flights
INSERT INTO FlightJournal.dbo.Flights(FlightId, [Date], Departure, Landing, PlaneId, PilotId, PilotBackseatId, StartTypeId, StartedFromId, LandedOnId, TachoDeparture, TachoLanding, TaskDistance, [Description], BetalerId, StartCost, FlightCost, TachoCost, LastUpdated, LastUpdatedBy, RecordKey, LandingCount) SELECT FlightId, [Date], Departure, Landing, PlaneId, PilotId, PilotBackseatId, StartTypeId, StartedFromId, LandedOnId, TachoDeparture, TachoLanding, TaskDistance, [Description], BetalerId, StartCost, FlightCost, TachoCost, LastUpdated, LastUpdatedBy, RecordKey, 1 FROM [FlightLog.Models.FlightContext].dbo.Flights 
SELECT * FROM FlightJournal.dbo.Flights order by LastUpdated desc


DELETE FROM FlightJournal.dbo.FlightVersionHistory
INSERT INTO FlightJournal.dbo.FlightVersionHistory(FlightId, Created, [State], [Date], Departure, Landing, PlaneId, PilotId, PilotBackseatId, StartTypeId, StartedFromId, LandedOnId, TachoDeparture, TachoLanding, [Description], BetalerId, LastUpdated, LastUpdatedBy, LandingCount) SELECT FlightId, Created, [State], [Date], Departure, Landing, PlaneId, PilotId, PilotBackseatId, StartTypeId, StartedFromId, LandedOnId, TachoDeparture, TachoLanding, [Description], BetalerId, LastUpdated, LastUpdatedBy, 1 FROM [FlightLog.Models.FlightContext].dbo.FlightVersionHistory 
SELECT * FROM FlightJournal.dbo.FlightVersionHistory





