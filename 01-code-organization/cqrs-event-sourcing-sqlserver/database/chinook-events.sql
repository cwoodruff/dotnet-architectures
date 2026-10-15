-- Event store for the cqrs-event-sourcing-sqlserver example.
-- Lives in its own database, ChinookEvents, on the same SQL Server container as Chinook.
-- Chinook itself is never modified. The API creates the database and applies this script
-- on startup when the Events table is missing, so running it by hand is optional.
IF OBJECT_ID(N'dbo.Events', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Events
    (
        StreamId   uniqueidentifier NOT NULL,
        Version    int              NOT NULL,
        EventType  nvarchar(200)    NOT NULL,
        Payload    nvarchar(max)    NOT NULL,
        OccurredAt datetime2        NOT NULL,
        -- One row per (stream, version). Two writers appending the same next version collide
        -- here, which is the store's optimistic concurrency check.
        CONSTRAINT UQ_Events_StreamId_Version UNIQUE CLUSTERED (StreamId, Version)
    );
END
