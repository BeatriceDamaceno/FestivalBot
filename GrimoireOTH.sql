CREATE TABLE Users (
	UserID int PRIMARY KEY,
	UserName varchar,
	KillCount int,
	DeathCount int,
	Faction varchar(255),
	Persona varchar(255),
	HP int NOT NULL default 100
)

-- Factions main table
CREATE TABLE IF NOT EXISTS Factions (
    ID          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT    NOT NULL UNIQUE,
    CreationDate TEXT   NOT NULL,    -- 'YYYY-MM-DD'
    Banner      TEXT
);

-- Link between factions and users
CREATE TABLE IF NOT EXISTS FactionMembers (
    FactionID   INTEGER NOT NULL,
    UserID      TEXT    NOT NULL,
    JoinedDate  TEXT    NOT NULL DEFAULT (date('now')),
    PRIMARY KEY (FactionID, UserID),
    FOREIGN KEY (FactionID) REFERENCES Factions(ID) ON DELETE CASCADE,
    FOREIGN KEY (UserID)     REFERENCES Users(UserID)  ON DELETE CASCADE
);

-- Areas definition (optional)
CREATE TABLE IF NOT EXISTS Areas (
    ID   INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE
);

-- Which areas are controlled by which faction
CREATE TABLE IF NOT EXISTS FactionAreas (
    FactionID INTEGER NOT NULL,
    AreaID    INTEGER NOT NULL,
    PRIMARY KEY (FactionID, AreaID),
    FOREIGN KEY (FactionID) REFERENCES Factions(ID) ON DELETE CASCADE,
    FOREIGN KEY (AreaID)    REFERENCES Areas(ID)    ON DELETE CASCADE
);

INSERT INTO Users  (UserID, UserName, KillCount, DeathCount, Faction, Persona, HP)
VALUES (59636255259898682, 'Bea', 0, 0, 'Grimoire of the heart', 'Hades', 999)

SELECT UserName from Usuarios u
SELECT name FROM sqlite_master WHERE type='table';
ALTER TABLE Users ADD COLUMN FactionID INTEGER REFERENCES Factions(ID);

SELECT UserID FROM Users u 

UPDATE Users 
SET FactionID = 1 
WHERE UserID = '59636255259898682';

INSERT INTO Factions (Name, CreationDate, Banner)
VALUES ('Grimoire of the hearts', date('now'), '')