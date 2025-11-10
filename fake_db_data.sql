-- USERS
INSERT INTO "User" (name, surname, email, phone, password, registrationTime, profilePicture) VALUES
('Jonas', 'Petrauskas', 'jonas.petrauskas@example.lt', '+37061234567', 'hashedpwd1', NOW(), NULL),
('Aistė', 'Kazlauskaitė', 'aiste.kazlauskaite@example.lt', '+37061234568', 'hashedpwd2', NOW(), NULL),
('Mantas', 'Žilinskas', 'mantas.zilinskas@example.lt', '+37061234569', 'hashedpwd3', NOW(), NULL),
('Eglė', 'Jankauskienė', 'egle.jankauskiene@example.lt', '+37061234570', 'hashedpwd4', NOW(), NULL),
('Tomas', 'Vasiliauskas', 'tomas.vasiliauskas@example.lt', '+37061234571', 'hashedpwd5', NOW(), NULL),
('Gintarė', 'Rimkutė', 'gintare.rimkute@example.lt', '+37061234572', 'hashedpwd6', NOW(), NULL),
('Rokas', 'Urbonas', 'rokas.urbonas@example.lt', '+37061234573', 'hashedpwd7', NOW(), NULL),
('Agnė', 'Balčiūnaitė', 'agne.balciunaite@example.lt', '+37061234574', 'hashedpwd8', NOW(), NULL),
('Lukas', 'Kvedaras', 'lukas.kvedaras@example.lt', '+37061234575', 'hashedpwd9', NOW(), NULL),
('Simona', 'Martinkutė', 'simona.martinkute@example.lt', '+37061234576', 'hashedpwd10', NOW(), NULL),
('Ignas', 'Kairys', 'ignas.kairys@example.lt', '+37061234577', 'hashedpwd11', NOW(), NULL),
('Rūta', 'Kasparaitė', 'ruta.kasparaite@example.lt', '+37061234578', 'hashedpwd12', NOW(), NULL),
('Admin', 'Account', 'admin@example.lt', '+37060000000', 'hashedadminpwd', NOW(), NULL),
('Bruno', 'Admin', 'admin@example.com', '+37060000100', 'password123', NOW(), NULL),
('Bruno', 'Broker', 'broker@example.com', '+37060000101', 'password123', NOW(), NULL),
('Bruno', 'Buyer', 'buyer@example.com', '+37060000102', 'password123', NOW(), NULL);

-- BROKERS
INSERT INTO Broker (id_User, confirmed, blocked) VALUES
(1, TRUE, FALSE), (2, TRUE, FALSE), (3, TRUE, FALSE), (4, TRUE, FALSE), (5, TRUE, FALSE), (6, TRUE, FALSE),
((SELECT id_User FROM "User" WHERE email='broker@example.com'), TRUE, FALSE);

-- BUYERS
INSERT INTO Buyer (id_User, confirmed, blocked) VALUES
(7, TRUE, FALSE), (8, TRUE, FALSE), (9, TRUE, FALSE), (10, TRUE, FALSE), (11, TRUE, FALSE), (12, TRUE, FALSE),
((SELECT id_User FROM "User" WHERE email='buyer@example.com'), TRUE, FALSE);

-- ADMINISTRATOR
INSERT INTO Administrator (id_User)
SELECT id_User FROM "User" WHERE email = 'admin@example.lt';

-- Bruno Admin as Administrator too
INSERT INTO Administrator (id_User)
SELECT id_User FROM "User" WHERE email = 'admin@example.com';

-- CONFIRMATIONS
INSERT INTO Confirmation (id, expires, fk_Buyerid_User) VALUES
('conf1', NOW() + INTERVAL '30 days', 7),
('conf2', NOW() + INTERVAL '30 days', 8),
('conf3', NOW() + INTERVAL '30 days', 9),
('conf4', NOW() + INTERVAL '30 days', 10),
('conf5', NOW() + INTERVAL '30 days', 11),
('conf6', NOW() + INTERVAL '30 days', 12);

-- AVAILABILITIES
INSERT INTO Availability ("from", "to", fk_Brokerid_User) VALUES
(NOW() + INTERVAL '1 day', NOW() + INTERVAL '1 day' + INTERVAL '2 hour', 1),
(NOW() + INTERVAL '2 day', NOW() + INTERVAL '2 day' + INTERVAL '2 hour', 2),
(NOW() + INTERVAL '3 day', NOW() + INTERVAL '3 day' + INTERVAL '3 hour', 3),
(NOW() + INTERVAL '4 day', NOW() + INTERVAL '4 day' + INTERVAL '4 hour', 4),
(NOW() + INTERVAL '5 day', NOW() + INTERVAL '5 day' + INTERVAL '2 hour', 5),
(NOW() + INTERVAL '6 day', NOW() + INTERVAL '6 day' + INTERVAL '3 hour', 6),
(NOW() + INTERVAL '7 day', NOW() + INTERVAL '7 day' + INTERVAL '1 hour', 1),
(NOW() + INTERVAL '8 day', NOW() + INTERVAL '8 day' + INTERVAL '2 hour', 2),
(NOW() + INTERVAL '9 day', NOW() + INTERVAL '9 day' + INTERVAL '1 hour', 3),
(NOW() + INTERVAL '10 day', NOW() + INTERVAL '10 day' + INTERVAL '2 hour', 4);

-- BUILDINGS
INSERT INTO Building (city, address, area, year, lastRenovationYear, floors, energy, fk_Brokerid_User) VALUES
('Vilnius', 'Gedimino pr. 10', 850.5, 2001, 2019, 5, 1, 1),
('Kaunas', 'Laisvės al. 45', 720.3, 2005, 2020, 6, 2, 2),
('Klaipėda', 'Tiltų g. 12', 560.0, 1998, 2018, 4, 3, 3),
('Šiauliai', 'Vilniaus g. 23', 630.9, 2000, 2015, 5, 4, 4),
('Panevėžys', 'Respublikos g. 2', 590.4, 2002, 2019, 3, 5, 5),
('Vilnius', 'Ukmergės g. 178', 1020.0, 2010, 2022, 7, 1, 6),
('Kaunas', 'Kovo 11-osios g. 33', 890.5, 2008, 2021, 6, 2, 1),
('Klaipėda', 'Baltijos pr. 55', 770.0, 1999, 2017, 5, 3, 2),
('Vilnius', 'Antakalnio g. 88', 940.0, 2012, 2023, 8, 1, 3),
('Kaunas', 'Šilainiai g. 100', 650.0, 2006, 2016, 4, 4, 4);

-- APARTMENTS
INSERT INTO Apartment (apartmentNumber, area, floor, rooms, notes, heating, finish, fk_Buildingid_Building, isWholeBuilding) VALUES
(1, 65.3, 2, 3, 'Šviesus butas su balkonu', 1, 1, 1, FALSE),
(2, 72.0, 3, 4, 'Puikus vaizdas į miestą', 3, 2, 1, FALSE),
(3, 85.2, 5, 5, 'Modernus interjeras', 2, 1, 2, FALSE),
(4, 44.5, 1, 2, 'Kompaktiškas butas centre', 4, 3, 2, FALSE),
(5, 120.0, 6, 5, 'Erdvus šeimos butas', 2, 1, 3, FALSE),
(6, 55.3, 2, 3, NULL, 5, 2, 3, FALSE),
(7, 33.2, 1, 1, 'Studentams', 1, 3, 4, FALSE),
(8, 69.5, 4, 4, 'Liftas name', 3, 1, 4, FALSE),
(9, 95.0, 7, 5, 'Penthouse su terasa', 2, 1, 5, FALSE),
(10, 200.0, 1, 7, 'Visa vila', 2, 1, 5, TRUE),
(11, 78.4, 3, 3, 'Naujai renovuotas', 4, 2, 6, FALSE),
(12, 58.6, 2, 2, 'Tylus rajonas', 3, 2, 6, FALSE),
(13, 101.4, 5, 4, 'A+ klasės butas', 5, 1, 7, FALSE),
(14, 39.2, 1, 2, 'Ekonomiškas būstas', 1, 3, 7, FALSE),
(15, 123.8, 8, 6, 'Su garažu', 2, 1, 8, FALSE),
(16, 45.0, 2, 2, NULL, 3, 2, 8, FALSE),
(17, 56.7, 3, 3, 'Puiki izoliacija', 2, 2, 9, FALSE),
(18, 68.3, 4, 3, 'Arti stoties', 3, 1, 9, FALSE),
(19, 47.1, 2, 2, 'Nedidelis bet jaukus', 4, 2, 10, FALSE),
(20, 111.6, 5, 5, 'Du balkonai', 1, 1, 10, FALSE),
(21, 70.2, 3, 3, 'Modernus būstas', 2, 1, 1, FALSE),
(22, 64.5, 2, 2, 'Tvarkingas 2 kambarių butas', 3, 2, 2, FALSE),
(23, 118.9, 6, 5, 'Kampinis butas', 2, 1, 3, FALSE),
(24, 52.7, 1, 2, 'Šalia parko', 5, 3, 4, FALSE),
(25, 99.3, 5, 4, 'Renovuotas', 1, 1, 5, FALSE);

-- PICTURES
INSERT INTO Picture (id, public, fk_Apartmentid_Apartment) VALUES
('pic1', TRUE, 1), ('pic2', TRUE, 2), ('pic3', TRUE, 3), ('pic4', TRUE, 4), ('pic5', TRUE, 5),
('pic6', TRUE, 6), ('pic7', TRUE, 7), ('pic8', TRUE, 8), ('pic9', TRUE, 9), ('pic10', TRUE, 10);

-- LISTINGS
INSERT INTO Listing (description, askingPrice, rent, fk_Pictureid) VALUES
('Parduodamas šviesus butas centre', 120000, FALSE, 'pic1'),
('Erdvus butas su balkonu', 95000, TRUE, 'pic2'),
('Modernus penthouse', 250000, FALSE, 'pic3'),
('Butas senamiestyje', 89000, TRUE, 'pic4'),
('Vila su sodu', 320000, FALSE, 'pic5'),
('Būstas studentams', 55000, TRUE, 'pic6'),
('Butas su parkavimo vieta', 115000, FALSE, 'pic7'),
('Renovuotas būstas', 98000, FALSE, 'pic8'),
('Šeimos butas su terasa', 160000, FALSE, 'pic9'),
('Erdvūs apartamentai', 140000, TRUE, 'pic10');

-- VIEWINGS
INSERT INTO Viewing ("from", "to", status, fk_Availabilityid_Availability, fk_Listingid_Listing) VALUES
(NOW() + INTERVAL '2 day', NOW() + INTERVAL '2 day' + INTERVAL '1 hour', 2, 1, 1),
(NOW() + INTERVAL '3 day', NOW() + INTERVAL '3 day' + INTERVAL '1 hour', 1, 2, 2),
(NOW() + INTERVAL '4 day', NOW() + INTERVAL '4 day' + INTERVAL '1.5 hour', 2, 3, 3),
(NOW() + INTERVAL '5 day', NOW() + INTERVAL '5 day' + INTERVAL '1 hour', 5, 4, 4),
(NOW() + INTERVAL '6 day', NOW() + INTERVAL '6 day' + INTERVAL '1 hour', 2, 5, 5),
(NOW() + INTERVAL '7 day', NOW() + INTERVAL '7 day' + INTERVAL '1 hour', 1, 6, 6),
(NOW() + INTERVAL '8 day', NOW() + INTERVAL '8 day' + INTERVAL '1 hour', 5, 7, 7),
(NOW() + INTERVAL '9 day', NOW() + INTERVAL '9 day' + INTERVAL '1.5 hour', 2, 8, 8),
(NOW() + INTERVAL '10 day', NOW() + INTERVAL '10 day' + INTERVAL '1 hour', 1, 9, 9),
(NOW() + INTERVAL '11 day', NOW() + INTERVAL '11 day' + INTERVAL '1.5 hour', 2, 10, 10);
