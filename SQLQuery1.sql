IF OBJECT_ID('EventTypes', 'U') IS NULL
BEGIN
    CREATE TABLE EventTypes (
        EventTypeID INT IDENTITY(1,1) PRIMARY KEY,
        EventTypeName NVARCHAR(100) NOT NULL
    );

    INSERT INTO EventTypes (EventTypeName)
    VALUES ('Conference'), ('Wedding'), ('Concert'), ('Exhibition');
END;

IF COL_LENGTH('Events', 'EventTypeID') IS NULL
BEGIN
    ALTER TABLE Events ADD EventTypeID INT NULL;
END;

IF COL_LENGTH('Venues', 'IsAvailable') IS NULL
BEGIN
    ALTER TABLE Venues ADD IsAvailable BIT NOT NULL DEFAULT 1;
END;
