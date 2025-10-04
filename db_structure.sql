-- script.ddl (PostgreSQL-compliant with BIGINT IDs, original order preserved)

CREATE TABLE "User"
(
    name varchar(255) NOT NULL,
    surname varchar(255) NOT NULL,
    email varchar(255) NOT NULL,
    phone varchar(255) NOT NULL,
    password varchar(255) NOT NULL,
    registrationTime timestamptz NOT NULL,
    profilePicture varchar(255) NULL,
    id_User bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    PRIMARY KEY(id_User)
);

CREATE TABLE EnergyClass
(
    id_EnergyClass bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    name varchar(4) NOT NULL,
    PRIMARY KEY(id_EnergyClass)
);
INSERT INTO EnergyClass(name) VALUES
('A+++'), ('A++'), ('A+'), ('A'), ('B'), ('C'), ('D'), ('E'), ('F'), ('G');

CREATE TABLE FinishTypes
(
    id_FinishTypes bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    name varchar(15) NOT NULL,
    PRIMARY KEY(id_FinishTypes)
);
INSERT INTO FinishTypes(name) VALUES
('complete'), ('partial'), ('unfinished'), ('not fully built'), ('foundations'), ('other');

CREATE TABLE HeatingTypes
(
    id_HeatingTypes bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    name varchar(10) NOT NULL,
    PRIMARY KEY(id_HeatingTypes)
);
INSERT INTO HeatingTypes(name) VALUES
('central'), ('gas'), ('solid fuel'), ('electric'), ('heat pump'), ('other');

CREATE TABLE ViewingStatus
(
    id_ViewingStatus bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    name varchar(9) NOT NULL,
    PRIMARY KEY(id_ViewingStatus)
);
INSERT INTO ViewingStatus(name) VALUES
('pending'), ('confirmed'), ('rejected'), ('cancelled'), ('public');

CREATE TABLE Administrator
(
    id_User bigint NOT NULL,
    PRIMARY KEY(id_User),
    FOREIGN KEY(id_User) REFERENCES "User"(id_User)
);

CREATE TABLE Broker
(
    confirmed boolean NOT NULL,
    blocked boolean NOT NULL,
    id_User bigint NOT NULL,
    PRIMARY KEY(id_User),
    FOREIGN KEY(id_User) REFERENCES "User"(id_User)
);

CREATE TABLE Buyer
(
    confirmed boolean NOT NULL,
    blocked boolean NOT NULL,
    id_User bigint NOT NULL,
    PRIMARY KEY(id_User),
    FOREIGN KEY(id_User) REFERENCES "User"(id_User)
);

CREATE TABLE "Session"
(
    id varchar(255) NOT NULL,
    created timestamptz NOT NULL,
    remember boolean NOT NULL,
    lastActivity timestamptz NOT NULL,
    fk_Userid_User bigint NOT NULL,
    PRIMARY KEY(id),
    FOREIGN KEY(fk_Userid_User) REFERENCES "User"(id_User)
);

CREATE TABLE Availability
(
    "from" timestamptz NOT NULL,
    "to" timestamptz NOT NULL,
    id_Availability bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    fk_Brokerid_User bigint NOT NULL,
    PRIMARY KEY(id_Availability),
    FOREIGN KEY(fk_Brokerid_User) REFERENCES Broker(id_User)
);

CREATE TABLE Building
(
    city varchar(255) NOT NULL,
    address varchar(255) NOT NULL,
    area double precision NOT NULL,
    year integer NOT NULL,
    lastRenovationYear integer NULL,
    floors integer NOT NULL,
    energy bigint NULL,
    id_Building bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    fk_Brokerid_User bigint NOT NULL,
    PRIMARY KEY(id_Building),
    FOREIGN KEY(energy) REFERENCES EnergyClass(id_EnergyClass),
    FOREIGN KEY(fk_Brokerid_User) REFERENCES Broker(id_User)
);

CREATE TABLE Confirmation
(
    id varchar(255) NOT NULL,
    expires timestamptz NOT NULL,
    fk_Buyerid_User bigint NOT NULL,
    PRIMARY KEY(id),
    UNIQUE(fk_Buyerid_User),
    FOREIGN KEY(fk_Buyerid_User) REFERENCES Buyer(id_User)
);

CREATE TABLE Apartment
(
    apartmentNumber integer NULL,
    area double precision NOT NULL,
    floor integer NULL,
    rooms integer NOT NULL,
    notes varchar(10000) NULL,
    heating bigint NULL,
    finish bigint NOT NULL,
    id_Apartment bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    fk_Buildingid_Building bigint NOT NULL,
    isWholeBuilding boolean NOT NULL,
    PRIMARY KEY(id_Apartment),
    FOREIGN KEY(heating) REFERENCES HeatingTypes(id_HeatingTypes),
    FOREIGN KEY(finish) REFERENCES FinishTypes(id_FinishTypes),
    FOREIGN KEY(fk_Buildingid_Building) REFERENCES Building(id_Building)
);

CREATE TABLE Picture
(
    id varchar(255) NOT NULL,
    public boolean NOT NULL,
    fk_Apartmentid_Apartment bigint NOT NULL,
    PRIMARY KEY(id),
    FOREIGN KEY(fk_Apartmentid_Apartment) REFERENCES Apartment(id_Apartment)
);

CREATE TABLE Listing
(
    description varchar(10000) NOT NULL,
    askingPrice double precision NOT NULL,
    rent boolean NOT NULL,
    id_Listing bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    fk_Pictureid varchar(255) NOT NULL,
    PRIMARY KEY(id_Listing),
    UNIQUE(fk_Pictureid),
    FOREIGN KEY(fk_Pictureid) REFERENCES Picture(id)
);

CREATE TABLE Viewing
(
    "from" timestamptz NOT NULL,
    "to" timestamptz NOT NULL,
    status bigint NOT NULL,
    id_Viewing bigint GENERATED ALWAYS AS IDENTITY NOT NULL,
    fk_Availabilityid_Availability bigint NOT NULL,
    fk_Listingid_Listing bigint NOT NULL,
    PRIMARY KEY(id_Viewing),
    UNIQUE(fk_Listingid_Listing),
    FOREIGN KEY(status) REFERENCES ViewingStatus(id_ViewingStatus),
    FOREIGN KEY(fk_Availabilityid_Availability) REFERENCES Availability(id_Availability),
    FOREIGN KEY(fk_Listingid_Listing) REFERENCES Listing(id_Listing)
);
