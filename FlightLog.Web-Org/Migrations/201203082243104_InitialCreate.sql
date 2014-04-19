USE [FlightLog.Models.FlightContext]
GO
/****** Object:  Table [dbo].[Locations]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Locations](
	[LocationId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
 CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED 
(
	[LocationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Clubs]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clubs](
	[ClubId] [int] IDENTITY(1,1) NOT NULL,
	[ShortName] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[DefaultStartLocationId] [int] NOT NULL,
	[DefaultStartLocation_LocationId] [int] NULL,
 CONSTRAINT [PK_Clubs] PRIMARY KEY CLUSTERED 
(
	[ClubId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PilotStatusTypes]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PilotStatusTypes](
	[PilotStatusId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[ClubId] [int] NULL,
 CONSTRAINT [PK_PilotStatusTypes] PRIMARY KEY CLUSTERED 
(
	[PilotStatusId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StartTypes]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StartTypes](
	[StartTypeId] [int] IDENTITY(1,1) NOT NULL,
	[ShortName] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[ClubId] [int] NULL,
 CONSTRAINT [PK_StartTypes] PRIMARY KEY CLUSTERED 
(
	[StartTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Planes]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Planes](
	[PlaneId] [int] IDENTITY(1,1) NOT NULL,
	[Registration] [nvarchar](max) NOT NULL,
	[CompetitionId] [nvarchar](max) NOT NULL,
	[Seats] [float] NOT NULL,
	[Engines] [float] NOT NULL,
	[EntryDate] [datetime] NOT NULL,
	[ExitDate] [datetime] NULL,
	[StartTypeId] [int] NULL,
 CONSTRAINT [PK_Planes] PRIMARY KEY CLUSTERED 
(
	[PlaneId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pilots]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pilots](
	[PilotId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[UnionId] [nvarchar](max) NULL,
	[MemberId] [nvarchar](max) NULL,
	[ClubId] [int] NOT NULL,
	[PilotStatus_PilotStatusId] [int] NULL,
 CONSTRAINT [PK_Pilots] PRIMARY KEY CLUSTERED 
(
	[PilotId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FlightVersionHistory]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FlightVersionHistory](
	[FlightId] [uniqueidentifier] NOT NULL,
	[Created] [datetime] NOT NULL,
	[State] [nvarchar](max) NULL,
	[Date] [datetime] NOT NULL,
	[Departure] [datetime] NULL,
	[Landing] [datetime] NULL,
	[PlaneId] [int] NOT NULL,
	[PilotId] [int] NOT NULL,
	[PilotBackseatId] [int] NULL,
	[StartTypeId] [int] NOT NULL,
	[BetalerId] [int] NOT NULL,
	[StartedFromId] [int] NOT NULL,
	[LandedOnId] [int] NULL,
	[TachoDeparture] [float] NULL,
	[TachoLanding] [float] NULL,
	[Description] [nvarchar](max) NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedBy] [nvarchar](max) NULL,
 CONSTRAINT [PK_FlightVersionHistory] PRIMARY KEY CLUSTERED 
(
	[FlightId] ASC,
	[Created] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Flights]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Flights](
	[FlightId] [uniqueidentifier] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Departure] [datetime] NULL,
	[Landing] [datetime] NULL,
	[PlaneId] [int] NOT NULL,
	[PilotId] [int] NOT NULL,
	[PilotBackseatId] [int] NULL,
	[StartTypeId] [int] NOT NULL,
	[StartedFromId] [int] NOT NULL,
	[LandedOnId] [int] NULL,
	[TachoDeparture] [float] NULL,
	[TachoLanding] [float] NULL,
	[TaskDistance] [float] NULL,
	[Description] [nvarchar](max) NULL,
	[BetalerId] [int] NOT NULL,
	[StartCost] [float] NOT NULL,
	[FlightCost] [float] NOT NULL,
	[TachoCost] [float] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
	[LastUpdatedBy] [nvarchar](max) NULL,
	[RecordKey] [int] NOT NULL,
 CONSTRAINT [PK_Flights] PRIMARY KEY CLUSTERED 
(
	[FlightId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PilotLogs]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PilotLogs](
	[PilotLogid] [uniqueidentifier] NOT NULL,
	[Lesson] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Pilot_PilotId] [int] NULL,
	[Flight_FlightId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PilotLogs] PRIMARY KEY CLUSTERED 
(
	[PilotLogid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notes]    Script Date: 03/28/2012 00:30:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notes](
	[NoteId] [int] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Flight_FlightId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Notes] PRIMARY KEY CLUSTERED 
(
	[NoteId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  ForeignKey [FK_Clubs_Locations_DefaultStartLocation_LocationId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Clubs]  WITH CHECK ADD  CONSTRAINT [FK_Clubs_Locations_DefaultStartLocation_LocationId] FOREIGN KEY([DefaultStartLocation_LocationId])
REFERENCES [dbo].[Locations] ([LocationId])
GO
ALTER TABLE [dbo].[Clubs] CHECK CONSTRAINT [FK_Clubs_Locations_DefaultStartLocation_LocationId]
GO
/****** Object:  ForeignKey [FK_Flights_Locations_LandedOnId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Flights]  WITH CHECK ADD  CONSTRAINT [FK_Flights_Locations_LandedOnId] FOREIGN KEY([LandedOnId])
REFERENCES [dbo].[Locations] ([LocationId])
GO
ALTER TABLE [dbo].[Flights] CHECK CONSTRAINT [FK_Flights_Locations_LandedOnId]
GO
/****** Object:  ForeignKey [FK_Flights_Locations_StartedFromId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Flights]  WITH CHECK ADD  CONSTRAINT [FK_Flights_Locations_StartedFromId] FOREIGN KEY([StartedFromId])
REFERENCES [dbo].[Locations] ([LocationId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Flights] CHECK CONSTRAINT [FK_Flights_Locations_StartedFromId]
GO
/****** Object:  ForeignKey [FK_Flights_Pilots_BetalerId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Flights]  WITH CHECK ADD  CONSTRAINT [FK_Flights_Pilots_BetalerId] FOREIGN KEY([BetalerId])
REFERENCES [dbo].[Pilots] ([PilotId])
GO
ALTER TABLE [dbo].[Flights] CHECK CONSTRAINT [FK_Flights_Pilots_BetalerId]
GO
/****** Object:  ForeignKey [FK_Flights_Pilots_PilotBackseatId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Flights]  WITH CHECK ADD  CONSTRAINT [FK_Flights_Pilots_PilotBackseatId] FOREIGN KEY([PilotBackseatId])
REFERENCES [dbo].[Pilots] ([PilotId])
GO
ALTER TABLE [dbo].[Flights] CHECK CONSTRAINT [FK_Flights_Pilots_PilotBackseatId]
GO
/****** Object:  ForeignKey [FK_Flights_Pilots_PilotId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Flights]  WITH CHECK ADD  CONSTRAINT [FK_Flights_Pilots_PilotId] FOREIGN KEY([PilotId])
REFERENCES [dbo].[Pilots] ([PilotId])
GO
ALTER TABLE [dbo].[Flights] CHECK CONSTRAINT [FK_Flights_Pilots_PilotId]
GO
/****** Object:  ForeignKey [FK_Flights_Planes_PlaneId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Flights]  WITH CHECK ADD  CONSTRAINT [FK_Flights_Planes_PlaneId] FOREIGN KEY([PlaneId])
REFERENCES [dbo].[Planes] ([PlaneId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Flights] CHECK CONSTRAINT [FK_Flights_Planes_PlaneId]
GO
/****** Object:  ForeignKey [FK_Flights_StartTypes_StartTypeId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Flights]  WITH CHECK ADD  CONSTRAINT [FK_Flights_StartTypes_StartTypeId] FOREIGN KEY([StartTypeId])
REFERENCES [dbo].[StartTypes] ([StartTypeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Flights] CHECK CONSTRAINT [FK_Flights_StartTypes_StartTypeId]
GO
/****** Object:  ForeignKey [FK_FlightVersionHistory_Locations_LandedOnId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[FlightVersionHistory]  WITH CHECK ADD  CONSTRAINT [FK_FlightVersionHistory_Locations_LandedOnId] FOREIGN KEY([LandedOnId])
REFERENCES [dbo].[Locations] ([LocationId])
GO
ALTER TABLE [dbo].[FlightVersionHistory] CHECK CONSTRAINT [FK_FlightVersionHistory_Locations_LandedOnId]
GO
/****** Object:  ForeignKey [FK_FlightVersionHistory_Locations_StartedFromId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[FlightVersionHistory]  WITH CHECK ADD  CONSTRAINT [FK_FlightVersionHistory_Locations_StartedFromId] FOREIGN KEY([StartedFromId])
REFERENCES [dbo].[Locations] ([LocationId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FlightVersionHistory] CHECK CONSTRAINT [FK_FlightVersionHistory_Locations_StartedFromId]
GO
/****** Object:  ForeignKey [FK_FlightVersionHistory_Pilots_BetalerId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[FlightVersionHistory]  WITH CHECK ADD  CONSTRAINT [FK_FlightVersionHistory_Pilots_BetalerId] FOREIGN KEY([BetalerId])
REFERENCES [dbo].[Pilots] ([PilotId])
GO
ALTER TABLE [dbo].[FlightVersionHistory] CHECK CONSTRAINT [FK_FlightVersionHistory_Pilots_BetalerId]
GO
/****** Object:  ForeignKey [FK_FlightVersionHistory_Pilots_PilotBackseatId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[FlightVersionHistory]  WITH CHECK ADD  CONSTRAINT [FK_FlightVersionHistory_Pilots_PilotBackseatId] FOREIGN KEY([PilotBackseatId])
REFERENCES [dbo].[Pilots] ([PilotId])
GO
ALTER TABLE [dbo].[FlightVersionHistory] CHECK CONSTRAINT [FK_FlightVersionHistory_Pilots_PilotBackseatId]
GO
/****** Object:  ForeignKey [FK_FlightVersionHistory_Pilots_PilotId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[FlightVersionHistory]  WITH CHECK ADD  CONSTRAINT [FK_FlightVersionHistory_Pilots_PilotId] FOREIGN KEY([PilotId])
REFERENCES [dbo].[Pilots] ([PilotId])
GO
ALTER TABLE [dbo].[FlightVersionHistory] CHECK CONSTRAINT [FK_FlightVersionHistory_Pilots_PilotId]
GO
/****** Object:  ForeignKey [FK_FlightVersionHistory_Planes_PlaneId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[FlightVersionHistory]  WITH CHECK ADD  CONSTRAINT [FK_FlightVersionHistory_Planes_PlaneId] FOREIGN KEY([PlaneId])
REFERENCES [dbo].[Planes] ([PlaneId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FlightVersionHistory] CHECK CONSTRAINT [FK_FlightVersionHistory_Planes_PlaneId]
GO
/****** Object:  ForeignKey [FK_FlightVersionHistory_StartTypes_StartTypeId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[FlightVersionHistory]  WITH CHECK ADD  CONSTRAINT [FK_FlightVersionHistory_StartTypes_StartTypeId] FOREIGN KEY([StartTypeId])
REFERENCES [dbo].[StartTypes] ([StartTypeId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FlightVersionHistory] CHECK CONSTRAINT [FK_FlightVersionHistory_StartTypes_StartTypeId]
GO
/****** Object:  ForeignKey [FK_Notes_Flights_Flight_FlightId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Notes]  WITH CHECK ADD  CONSTRAINT [FK_Notes_Flights_Flight_FlightId] FOREIGN KEY([Flight_FlightId])
REFERENCES [dbo].[Flights] ([FlightId])
GO
ALTER TABLE [dbo].[Notes] CHECK CONSTRAINT [FK_Notes_Flights_Flight_FlightId]
GO
/****** Object:  ForeignKey [FK_PilotLogs_Flights_Flight_FlightId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[PilotLogs]  WITH CHECK ADD  CONSTRAINT [FK_PilotLogs_Flights_Flight_FlightId] FOREIGN KEY([Flight_FlightId])
REFERENCES [dbo].[Flights] ([FlightId])
GO
ALTER TABLE [dbo].[PilotLogs] CHECK CONSTRAINT [FK_PilotLogs_Flights_Flight_FlightId]
GO
/****** Object:  ForeignKey [FK_PilotLogs_Pilots_Pilot_PilotId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[PilotLogs]  WITH CHECK ADD  CONSTRAINT [FK_PilotLogs_Pilots_Pilot_PilotId] FOREIGN KEY([Pilot_PilotId])
REFERENCES [dbo].[Pilots] ([PilotId])
GO
ALTER TABLE [dbo].[PilotLogs] CHECK CONSTRAINT [FK_PilotLogs_Pilots_Pilot_PilotId]
GO
/****** Object:  ForeignKey [FK_Pilots_Clubs_ClubId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Pilots]  WITH CHECK ADD  CONSTRAINT [FK_Pilots_Clubs_ClubId] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Pilots] CHECK CONSTRAINT [FK_Pilots_Clubs_ClubId]
GO
/****** Object:  ForeignKey [FK_Pilots_PilotStatusTypes_PilotStatus_PilotStatusId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Pilots]  WITH CHECK ADD  CONSTRAINT [FK_Pilots_PilotStatusTypes_PilotStatus_PilotStatusId] FOREIGN KEY([PilotStatus_PilotStatusId])
REFERENCES [dbo].[PilotStatusTypes] ([PilotStatusId])
GO
ALTER TABLE [dbo].[Pilots] CHECK CONSTRAINT [FK_Pilots_PilotStatusTypes_PilotStatus_PilotStatusId]
GO
/****** Object:  ForeignKey [FK_PilotStatusTypes_Clubs_ClubId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[PilotStatusTypes]  WITH CHECK ADD  CONSTRAINT [FK_PilotStatusTypes_Clubs_ClubId] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO
ALTER TABLE [dbo].[PilotStatusTypes] CHECK CONSTRAINT [FK_PilotStatusTypes_Clubs_ClubId]
GO
/****** Object:  ForeignKey [FK_Planes_StartTypes_StartTypeId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[Planes]  WITH CHECK ADD  CONSTRAINT [FK_Planes_StartTypes_StartTypeId] FOREIGN KEY([StartTypeId])
REFERENCES [dbo].[StartTypes] ([StartTypeId])
GO
ALTER TABLE [dbo].[Planes] CHECK CONSTRAINT [FK_Planes_StartTypes_StartTypeId]
GO
/****** Object:  ForeignKey [FK_StartTypes_Clubs_ClubId]    Script Date: 03/28/2012 00:30:55 ******/
ALTER TABLE [dbo].[StartTypes]  WITH CHECK ADD  CONSTRAINT [FK_StartTypes_Clubs_ClubId] FOREIGN KEY([ClubId])
REFERENCES [dbo].[Clubs] ([ClubId])
GO
ALTER TABLE [dbo].[StartTypes] CHECK CONSTRAINT [FK_StartTypes_Clubs_ClubId]
GO
