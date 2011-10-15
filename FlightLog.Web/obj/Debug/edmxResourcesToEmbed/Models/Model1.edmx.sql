
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 10/06/2011 03:18:19
-- Generated from EDMX file: C:\Users\Jan\Workshop\Visual Studio 2010\Projects\Flyveklubben\FlightLog\FlightLog.Web\obj\Debug\edmxResourcesToEmbed\Models\Model1.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [dbefd4ddb28807417dafda9f6b00b8cbbf];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Club_DefaultStartLocation]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Clubs] DROP CONSTRAINT [FK_Club_DefaultStartLocation];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Club_StartTypes]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[StartTypes] DROP CONSTRAINT [FK_Club_StartTypes];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Flight_Betaler]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Flights] DROP CONSTRAINT [FK_Flight_Betaler];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Flight_LandedOn]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Flights] DROP CONSTRAINT [FK_Flight_LandedOn];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Flight_Pilot]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Flights] DROP CONSTRAINT [FK_Flight_Pilot];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Flight_PilotBackseat]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Flights] DROP CONSTRAINT [FK_Flight_PilotBackseat];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Flight_PilotLogs]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[PilotLogs] DROP CONSTRAINT [FK_Flight_PilotLogs];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Flight_StartedFrom]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Flights] DROP CONSTRAINT [FK_Flight_StartedFrom];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Flight_StartType]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Flights] DROP CONSTRAINT [FK_Flight_StartType];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_FlightVersionHistory_Betaler]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[FlightVersionHistory] DROP CONSTRAINT [FK_FlightVersionHistory_Betaler];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_FlightVersionHistory_LandedOn]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[FlightVersionHistory] DROP CONSTRAINT [FK_FlightVersionHistory_LandedOn];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_FlightVersionHistory_Pilot]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[FlightVersionHistory] DROP CONSTRAINT [FK_FlightVersionHistory_Pilot];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_FlightVersionHistory_PilotBackseat]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[FlightVersionHistory] DROP CONSTRAINT [FK_FlightVersionHistory_PilotBackseat];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_FlightVersionHistory_Plane]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[FlightVersionHistory] DROP CONSTRAINT [FK_FlightVersionHistory_Plane];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_FlightVersionHistory_StartedFrom]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[FlightVersionHistory] DROP CONSTRAINT [FK_FlightVersionHistory_StartedFrom];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_FlightVersionHistory_StartType]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[FlightVersionHistory] DROP CONSTRAINT [FK_FlightVersionHistory_StartType];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Note_Flight]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Notes] DROP CONSTRAINT [FK_Note_Flight];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Pilot_Club]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Pilots] DROP CONSTRAINT [FK_Pilot_Club];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Pilot_PilotLogs]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[PilotLogs] DROP CONSTRAINT [FK_Pilot_PilotLogs];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Pilot_PilotStatus]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Pilots] DROP CONSTRAINT [FK_Pilot_PilotStatus];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_PilotStatusType_Club]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[PilotStatusTypes] DROP CONSTRAINT [FK_PilotStatusType_Club];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Plane_DefaultStartType]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Planes] DROP CONSTRAINT [FK_Plane_DefaultStartType];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FK_Plane_Flights]', 'F') IS NOT NULL
    ALTER TABLE [FlightLogModelsFlightContextModelStoreContainer].[Flights] DROP CONSTRAINT [FK_Plane_Flights];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[Clubs]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[Clubs];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[Flights]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[Flights];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[FlightVersionHistory]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[FlightVersionHistory];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[Locations]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[Locations];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[Notes]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[Notes];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[PilotLogs]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[PilotLogs];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[Pilots]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[Pilots];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[PilotStatusTypes]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[PilotStatusTypes];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[Planes]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[Planes];
GO
IF OBJECT_ID(N'[FlightLogModelsFlightContextModelStoreContainer].[StartTypes]', 'U') IS NOT NULL
    DROP TABLE [FlightLogModelsFlightContextModelStoreContainer].[StartTypes];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Clubs'
CREATE TABLE [dbo].[Clubs] (
    [ClubId] int IDENTITY(1,1) NOT NULL,
    [ShortName] nvarchar(4000)  NULL,
    [Name] nvarchar(4000)  NULL,
    [DefaultStartLocationId] int  NOT NULL,
    [DefaultStartLocation_LocationId] int  NULL
);
GO

-- Creating table 'Flights'
CREATE TABLE [dbo].[Flights] (
    [FlightId] uniqueidentifier  NOT NULL,
    [Date] datetime  NOT NULL,
    [Departure] datetime  NULL,
    [Landing] datetime  NULL,
    [PlaneId] int  NOT NULL,
    [PilotId] int  NOT NULL,
    [PilotBackseatId] int  NULL,
    [StartTypeId] int  NOT NULL,
    [StartedFromId] int  NOT NULL,
    [LandedOnId] int  NULL,
    [TachoDeparture] float  NULL,
    [TachoLanding] float  NULL,
    [TaskDistance] float  NULL,
    [Description] nvarchar(4000)  NULL,
    [BetalerId] int  NOT NULL,
    [StartCost] float  NOT NULL,
    [FlightCost] float  NOT NULL,
    [TachoCost] float  NOT NULL,
    [LastUpdated] datetime  NOT NULL,
    [LastUpdatedBy] nvarchar(4000)  NULL,
    [RecordKey] int  NOT NULL
);
GO

-- Creating table 'FlightVersionHistory'
CREATE TABLE [dbo].[FlightVersionHistory] (
    [FlightId] uniqueidentifier  NOT NULL,
    [Created] datetime  NOT NULL,
    [State] nvarchar(4000)  NULL,
    [Date] datetime  NOT NULL,
    [Departure] datetime  NULL,
    [Landing] datetime  NULL,
    [PlaneId] int  NOT NULL,
    [PilotId] int  NOT NULL,
    [PilotBackseatId] int  NULL,
    [StartTypeId] int  NOT NULL,
    [BetalerId] int  NOT NULL,
    [StartedFromId] int  NOT NULL,
    [LandedOnId] int  NULL,
    [TachoDeparture] float  NULL,
    [TachoLanding] float  NULL,
    [Description] nvarchar(4000)  NULL,
    [LastUpdated] datetime  NOT NULL,
    [LastUpdatedBy] nvarchar(4000)  NULL
);
GO

-- Creating table 'Locations'
CREATE TABLE [dbo].[Locations] (
    [LocationId] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(4000)  NULL
);
GO

-- Creating table 'Notes'
CREATE TABLE [dbo].[Notes] (
    [NoteId] int IDENTITY(1,1) NOT NULL,
    [CreatedOn] datetime  NOT NULL,
    [Description] nvarchar(4000)  NULL,
    [Flight_FlightId] uniqueidentifier  NULL
);
GO

-- Creating table 'PilotLogs'
CREATE TABLE [dbo].[PilotLogs] (
    [PilotLogid] uniqueidentifier  NOT NULL,
    [Lesson] nvarchar(4000)  NULL,
    [Description] nvarchar(4000)  NULL,
    [Pilot_PilotId] int  NULL,
    [Flight_FlightId] uniqueidentifier  NULL
);
GO

-- Creating table 'Pilots'
CREATE TABLE [dbo].[Pilots] (
    [PilotId] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(4000)  NOT NULL,
    [UnionId] nvarchar(4000)  NULL,
    [MemberId] nvarchar(4000)  NULL,
    [ClubId] int  NOT NULL,
    [PilotStatus_PilotStatusId] int  NULL
);
GO

-- Creating table 'PilotStatusTypes'
CREATE TABLE [dbo].[PilotStatusTypes] (
    [PilotStatusId] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(4000)  NULL,
    [Description] nvarchar(4000)  NULL,
    [ClubId] int  NULL
);
GO

-- Creating table 'Planes'
CREATE TABLE [dbo].[Planes] (
    [PlaneId] int IDENTITY(1,1) NOT NULL,
    [Registration] nvarchar(4000)  NOT NULL,
    [CompetitionId] nvarchar(4000)  NOT NULL,
    [Seats] float  NOT NULL,
    [Engines] float  NOT NULL,
    [EntryDate] datetime  NOT NULL,
    [ExitDate] datetime  NULL,
    [StartTypeId] int  NULL
);
GO

-- Creating table 'StartTypes'
CREATE TABLE [dbo].[StartTypes] (
    [StartTypeId] int IDENTITY(1,1) NOT NULL,
    [ShortName] nvarchar(4000)  NULL,
    [Name] nvarchar(4000)  NULL,
    [ClubId] int  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ClubId] in table 'Clubs'
ALTER TABLE [dbo].[Clubs]
ADD CONSTRAINT [PK_Clubs]
    PRIMARY KEY CLUSTERED ([ClubId] ASC);
GO

-- Creating primary key on [FlightId] in table 'Flights'
ALTER TABLE [dbo].[Flights]
ADD CONSTRAINT [PK_Flights]
    PRIMARY KEY CLUSTERED ([FlightId] ASC);
GO

-- Creating primary key on [FlightId], [Created] in table 'FlightVersionHistory'
ALTER TABLE [dbo].[FlightVersionHistory]
ADD CONSTRAINT [PK_FlightVersionHistory]
    PRIMARY KEY CLUSTERED ([FlightId], [Created] ASC);
GO

-- Creating primary key on [LocationId] in table 'Locations'
ALTER TABLE [dbo].[Locations]
ADD CONSTRAINT [PK_Locations]
    PRIMARY KEY CLUSTERED ([LocationId] ASC);
GO

-- Creating primary key on [NoteId] in table 'Notes'
ALTER TABLE [dbo].[Notes]
ADD CONSTRAINT [PK_Notes]
    PRIMARY KEY CLUSTERED ([NoteId] ASC);
GO

-- Creating primary key on [PilotLogid] in table 'PilotLogs'
ALTER TABLE [dbo].[PilotLogs]
ADD CONSTRAINT [PK_PilotLogs]
    PRIMARY KEY CLUSTERED ([PilotLogid] ASC);
GO

-- Creating primary key on [PilotId] in table 'Pilots'
ALTER TABLE [dbo].[Pilots]
ADD CONSTRAINT [PK_Pilots]
    PRIMARY KEY CLUSTERED ([PilotId] ASC);
GO

-- Creating primary key on [PilotStatusId] in table 'PilotStatusTypes'
ALTER TABLE [dbo].[PilotStatusTypes]
ADD CONSTRAINT [PK_PilotStatusTypes]
    PRIMARY KEY CLUSTERED ([PilotStatusId] ASC);
GO

-- Creating primary key on [PlaneId] in table 'Planes'
ALTER TABLE [dbo].[Planes]
ADD CONSTRAINT [PK_Planes]
    PRIMARY KEY CLUSTERED ([PlaneId] ASC);
GO

-- Creating primary key on [StartTypeId] in table 'StartTypes'
ALTER TABLE [dbo].[StartTypes]
ADD CONSTRAINT [PK_StartTypes]
    PRIMARY KEY CLUSTERED ([StartTypeId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [DefaultStartLocation_LocationId] in table 'Clubs'
ALTER TABLE [dbo].[Clubs]
ADD CONSTRAINT [FK_Club_DefaultStartLocation]
    FOREIGN KEY ([DefaultStartLocation_LocationId])
    REFERENCES [dbo].[Locations]
        ([LocationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Club_DefaultStartLocation'
CREATE INDEX [IX_FK_Club_DefaultStartLocation]
ON [dbo].[Clubs]
    ([DefaultStartLocation_LocationId]);
GO

-- Creating foreign key on [ClubId] in table 'StartTypes'
ALTER TABLE [dbo].[StartTypes]
ADD CONSTRAINT [FK_Club_StartTypes]
    FOREIGN KEY ([ClubId])
    REFERENCES [dbo].[Clubs]
        ([ClubId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Club_StartTypes'
CREATE INDEX [IX_FK_Club_StartTypes]
ON [dbo].[StartTypes]
    ([ClubId]);
GO

-- Creating foreign key on [ClubId] in table 'Pilots'
ALTER TABLE [dbo].[Pilots]
ADD CONSTRAINT [FK_Pilot_Club]
    FOREIGN KEY ([ClubId])
    REFERENCES [dbo].[Clubs]
        ([ClubId])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Pilot_Club'
CREATE INDEX [IX_FK_Pilot_Club]
ON [dbo].[Pilots]
    ([ClubId]);
GO

-- Creating foreign key on [ClubId] in table 'PilotStatusTypes'
ALTER TABLE [dbo].[PilotStatusTypes]
ADD CONSTRAINT [FK_PilotStatusType_Club]
    FOREIGN KEY ([ClubId])
    REFERENCES [dbo].[Clubs]
        ([ClubId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PilotStatusType_Club'
CREATE INDEX [IX_FK_PilotStatusType_Club]
ON [dbo].[PilotStatusTypes]
    ([ClubId]);
GO

-- Creating foreign key on [BetalerId] in table 'Flights'
ALTER TABLE [dbo].[Flights]
ADD CONSTRAINT [FK_Flight_Betaler]
    FOREIGN KEY ([BetalerId])
    REFERENCES [dbo].[Pilots]
        ([PilotId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Flight_Betaler'
CREATE INDEX [IX_FK_Flight_Betaler]
ON [dbo].[Flights]
    ([BetalerId]);
GO

-- Creating foreign key on [LandedOnId] in table 'Flights'
ALTER TABLE [dbo].[Flights]
ADD CONSTRAINT [FK_Flight_LandedOn]
    FOREIGN KEY ([LandedOnId])
    REFERENCES [dbo].[Locations]
        ([LocationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Flight_LandedOn'
CREATE INDEX [IX_FK_Flight_LandedOn]
ON [dbo].[Flights]
    ([LandedOnId]);
GO

-- Creating foreign key on [PilotId] in table 'Flights'
ALTER TABLE [dbo].[Flights]
ADD CONSTRAINT [FK_Flight_Pilot]
    FOREIGN KEY ([PilotId])
    REFERENCES [dbo].[Pilots]
        ([PilotId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Flight_Pilot'
CREATE INDEX [IX_FK_Flight_Pilot]
ON [dbo].[Flights]
    ([PilotId]);
GO

-- Creating foreign key on [PilotBackseatId] in table 'Flights'
ALTER TABLE [dbo].[Flights]
ADD CONSTRAINT [FK_Flight_PilotBackseat]
    FOREIGN KEY ([PilotBackseatId])
    REFERENCES [dbo].[Pilots]
        ([PilotId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Flight_PilotBackseat'
CREATE INDEX [IX_FK_Flight_PilotBackseat]
ON [dbo].[Flights]
    ([PilotBackseatId]);
GO

-- Creating foreign key on [Flight_FlightId] in table 'PilotLogs'
ALTER TABLE [dbo].[PilotLogs]
ADD CONSTRAINT [FK_Flight_PilotLogs]
    FOREIGN KEY ([Flight_FlightId])
    REFERENCES [dbo].[Flights]
        ([FlightId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Flight_PilotLogs'
CREATE INDEX [IX_FK_Flight_PilotLogs]
ON [dbo].[PilotLogs]
    ([Flight_FlightId]);
GO

-- Creating foreign key on [StartedFromId] in table 'Flights'
ALTER TABLE [dbo].[Flights]
ADD CONSTRAINT [FK_Flight_StartedFrom]
    FOREIGN KEY ([StartedFromId])
    REFERENCES [dbo].[Locations]
        ([LocationId])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Flight_StartedFrom'
CREATE INDEX [IX_FK_Flight_StartedFrom]
ON [dbo].[Flights]
    ([StartedFromId]);
GO

-- Creating foreign key on [StartTypeId] in table 'Flights'
ALTER TABLE [dbo].[Flights]
ADD CONSTRAINT [FK_Flight_StartType]
    FOREIGN KEY ([StartTypeId])
    REFERENCES [dbo].[StartTypes]
        ([StartTypeId])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Flight_StartType'
CREATE INDEX [IX_FK_Flight_StartType]
ON [dbo].[Flights]
    ([StartTypeId]);
GO

-- Creating foreign key on [Flight_FlightId] in table 'Notes'
ALTER TABLE [dbo].[Notes]
ADD CONSTRAINT [FK_Note_Flight]
    FOREIGN KEY ([Flight_FlightId])
    REFERENCES [dbo].[Flights]
        ([FlightId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Note_Flight'
CREATE INDEX [IX_FK_Note_Flight]
ON [dbo].[Notes]
    ([Flight_FlightId]);
GO

-- Creating foreign key on [PlaneId] in table 'Flights'
ALTER TABLE [dbo].[Flights]
ADD CONSTRAINT [FK_Plane_Flights]
    FOREIGN KEY ([PlaneId])
    REFERENCES [dbo].[Planes]
        ([PlaneId])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Plane_Flights'
CREATE INDEX [IX_FK_Plane_Flights]
ON [dbo].[Flights]
    ([PlaneId]);
GO

-- Creating foreign key on [BetalerId] in table 'FlightVersionHistory'
ALTER TABLE [dbo].[FlightVersionHistory]
ADD CONSTRAINT [FK_FlightVersionHistory_Betaler]
    FOREIGN KEY ([BetalerId])
    REFERENCES [dbo].[Pilots]
        ([PilotId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FlightVersionHistory_Betaler'
CREATE INDEX [IX_FK_FlightVersionHistory_Betaler]
ON [dbo].[FlightVersionHistory]
    ([BetalerId]);
GO

-- Creating foreign key on [LandedOnId] in table 'FlightVersionHistory'
ALTER TABLE [dbo].[FlightVersionHistory]
ADD CONSTRAINT [FK_FlightVersionHistory_LandedOn]
    FOREIGN KEY ([LandedOnId])
    REFERENCES [dbo].[Locations]
        ([LocationId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FlightVersionHistory_LandedOn'
CREATE INDEX [IX_FK_FlightVersionHistory_LandedOn]
ON [dbo].[FlightVersionHistory]
    ([LandedOnId]);
GO

-- Creating foreign key on [PilotId] in table 'FlightVersionHistory'
ALTER TABLE [dbo].[FlightVersionHistory]
ADD CONSTRAINT [FK_FlightVersionHistory_Pilot]
    FOREIGN KEY ([PilotId])
    REFERENCES [dbo].[Pilots]
        ([PilotId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FlightVersionHistory_Pilot'
CREATE INDEX [IX_FK_FlightVersionHistory_Pilot]
ON [dbo].[FlightVersionHistory]
    ([PilotId]);
GO

-- Creating foreign key on [PilotBackseatId] in table 'FlightVersionHistory'
ALTER TABLE [dbo].[FlightVersionHistory]
ADD CONSTRAINT [FK_FlightVersionHistory_PilotBackseat]
    FOREIGN KEY ([PilotBackseatId])
    REFERENCES [dbo].[Pilots]
        ([PilotId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FlightVersionHistory_PilotBackseat'
CREATE INDEX [IX_FK_FlightVersionHistory_PilotBackseat]
ON [dbo].[FlightVersionHistory]
    ([PilotBackseatId]);
GO

-- Creating foreign key on [PlaneId] in table 'FlightVersionHistory'
ALTER TABLE [dbo].[FlightVersionHistory]
ADD CONSTRAINT [FK_FlightVersionHistory_Plane]
    FOREIGN KEY ([PlaneId])
    REFERENCES [dbo].[Planes]
        ([PlaneId])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FlightVersionHistory_Plane'
CREATE INDEX [IX_FK_FlightVersionHistory_Plane]
ON [dbo].[FlightVersionHistory]
    ([PlaneId]);
GO

-- Creating foreign key on [StartedFromId] in table 'FlightVersionHistory'
ALTER TABLE [dbo].[FlightVersionHistory]
ADD CONSTRAINT [FK_FlightVersionHistory_StartedFrom]
    FOREIGN KEY ([StartedFromId])
    REFERENCES [dbo].[Locations]
        ([LocationId])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FlightVersionHistory_StartedFrom'
CREATE INDEX [IX_FK_FlightVersionHistory_StartedFrom]
ON [dbo].[FlightVersionHistory]
    ([StartedFromId]);
GO

-- Creating foreign key on [StartTypeId] in table 'FlightVersionHistory'
ALTER TABLE [dbo].[FlightVersionHistory]
ADD CONSTRAINT [FK_FlightVersionHistory_StartType]
    FOREIGN KEY ([StartTypeId])
    REFERENCES [dbo].[StartTypes]
        ([StartTypeId])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FlightVersionHistory_StartType'
CREATE INDEX [IX_FK_FlightVersionHistory_StartType]
ON [dbo].[FlightVersionHistory]
    ([StartTypeId]);
GO

-- Creating foreign key on [Pilot_PilotId] in table 'PilotLogs'
ALTER TABLE [dbo].[PilotLogs]
ADD CONSTRAINT [FK_Pilot_PilotLogs]
    FOREIGN KEY ([Pilot_PilotId])
    REFERENCES [dbo].[Pilots]
        ([PilotId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Pilot_PilotLogs'
CREATE INDEX [IX_FK_Pilot_PilotLogs]
ON [dbo].[PilotLogs]
    ([Pilot_PilotId]);
GO

-- Creating foreign key on [PilotStatus_PilotStatusId] in table 'Pilots'
ALTER TABLE [dbo].[Pilots]
ADD CONSTRAINT [FK_Pilot_PilotStatus]
    FOREIGN KEY ([PilotStatus_PilotStatusId])
    REFERENCES [dbo].[PilotStatusTypes]
        ([PilotStatusId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Pilot_PilotStatus'
CREATE INDEX [IX_FK_Pilot_PilotStatus]
ON [dbo].[Pilots]
    ([PilotStatus_PilotStatusId]);
GO

-- Creating foreign key on [StartTypeId] in table 'Planes'
ALTER TABLE [dbo].[Planes]
ADD CONSTRAINT [FK_Plane_DefaultStartType]
    FOREIGN KEY ([StartTypeId])
    REFERENCES [dbo].[StartTypes]
        ([StartTypeId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Plane_DefaultStartType'
CREATE INDEX [IX_FK_Plane_DefaultStartType]
ON [dbo].[Planes]
    ([StartTypeId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------