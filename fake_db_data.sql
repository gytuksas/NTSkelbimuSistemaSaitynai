-- =====================================================
-- Users (10 entries)
-- =====================================================
INSERT INTO "user"(name, surname, email, phone, password, registrationTime, profilePicture)
VALUES
('Jonas','Kazlauskas','jonas.kazlauskas@example.lt','+37060000001','hashed_pw','2023-01-10 09:15:00','/img/u1.png'),
('Ieva','Petrauskienė','ieva.petrauskiene@example.lt','+37060000002','hashed_pw','2023-02-12 16:40:00','/img/u2.png'),
('Marius','Jankauskas','marius.jankauskas@example.lt','+37060000003','hashed_pw','2023-03-05 10:25:00',NULL),
('Rūta','Vaitkutė','ruta.vaitkute@example.lt','+37060000004','hashed_pw','2023-03-11 11:50:00','/img/u3.png'),
('Tomas','Žukauskas','tomas.zukauskas@example.lt','+37060000005','hashed_pw','2023-03-18 08:05:00',NULL),
('Aistė','Stankevičiūtė','aiste.stankeviciute@example.lt','+37060000006','hashed_pw','2023-04-22 12:35:00','/img/u4.png'),
('Mindaugas','Jakubauskas','mindaugas.jakubauskas@example.lt','+37060000007','hashed_pw','2023-05-02 15:00:00','/img/u5.png'),
('Simona','Butkutė','simona.butkute@example.lt','+37060000008','hashed_pw','2023-05-20 09:40:00',NULL),
('Karolis','Grigonis','karolis.grigonis@example.lt','+37060000009','hashed_pw','2023-06-11 17:25:00','/img/u6.png'),
('Eglė','Navickaitė','egle.navickaite@example.lt','+37060000010','hashed_pw','2023-07-14 13:15:00',NULL);

-- =====================================================
-- Administrator
-- =====================================================
INSERT INTO administrator(id_User) VALUES (1);

-- =====================================================
-- Brokers (confirmed, not blocked)
-- =====================================================
INSERT INTO broker(confirmed, blocked, id_User) VALUES
(true,false,2),
(true,false,3),
(true,false,4),
(true,false,5),
(true,false,6);

-- =====================================================
-- Buyers (confirmed, not blocked)
-- =====================================================
INSERT INTO buyer(confirmed, blocked, id_User) VALUES
(true,false,7),
(true,false,8),
(true,false,9),
(true,false,10);

-- =====================================================
-- Availabilities (10 entries)
-- =====================================================
INSERT INTO availability("from","to",fk_Brokerid_User)
VALUES
('2023-08-01 10:00','2023-08-01 14:00',2),
('2023-08-02 09:00','2023-08-02 12:30',3),
('2023-08-03 11:00','2023-08-03 16:00',2),
('2023-08-04 14:00','2023-08-04 18:00',4),
('2023-08-05 08:00','2023-08-05 11:30',6),
('2023-08-06 09:30','2023-08-06 12:00',5),
('2023-08-07 10:00','2023-08-07 15:00',3),
('2023-08-08 13:00','2023-08-08 17:00',2),
('2023-08-09 10:00','2023-08-09 12:00',6),
('2023-08-10 15:00','2023-08-10 18:00',4);

-- =====================================================
-- Buildings (10 entries)
-- =====================================================
INSERT INTO building(city,address,area,year,lastRenovationYear,floors,energy,fk_Brokerid_User)
VALUES
('Vilnius','Gedimino pr. 10',250.5,1972,2015,5,4,2),
('Kaunas','Laisvės al. 55',180.0,1988,NULL,4,5,3),
('Klaipėda','Taikos pr. 145',320.2,1995,2020,7,3,2),
('Šiauliai','Tilžės g. 89',150.7,2001,NULL,3,6,6),
('Panevėžys','Respublikos g. 12',210.5,1978,2010,6,4,4),
('Vilnius','Šnipiškių g. 3',95.0,2010,NULL,2,2,3),
('Kaunas','Savanorių pr. 200',305.3,1992,NULL,9,7,5),
('Klaipėda','Baltijos pr. 8',400.0,1980,2018,10,3,6),
('Šiauliai','Aušros al. 25',175.4,2005,NULL,5,2,2),
('Panevėžys','Knypavos g. 4',230.1,1999,NULL,4,5,3);

-- =====================================================
-- Confirmations (4 buyers)
-- =====================================================
INSERT INTO confirmation(id,expires,fk_Buyerid_User)
VALUES
('conf_1','2023-09-01 12:00',7),
('conf_2','2023-09-02 12:00',8),
('conf_3','2023-09-03 12:00',9),
('conf_4','2023-09-04 12:00',10);

-- =====================================================
-- Apartments (25 entries)
-- =====================================================
INSERT INTO apartment(apartmentNumber,area,floor,rooms,notes,heating,finish,fk_Buildingid_Building,isWholeBuilding)
VALUES
(1,55.5,1,2,'Jaukus butas su balkonu',1,1,1,false),
(2,42.0,3,1,'Tvarkingas, šalia centro',2,2,1,false),
(3,68.2,2,3,'Didelė svetainė',3,3,2,false),
(4,90.1,5,4,'Puikus vaizdas į miestą',1,1,3,false),
(5,37.7,1,1,'Mažas studijos tipo butas',4,2,4,false),
(6,120.0,NULL,5,'Visas pastatas',5,1,5,true),
(7,48.1,2,2,'Renovuotas',2,1,6,false),
(8,72.2,4,3,'Su terasa',1,2,7,false),
(9,95.3,6,4,'Šeimos būstas',3,1,8,false),
(10,35.8,1,1,'Ekonomiškas variantas',4,2,9,false),
(11,52.9,2,2,'Rami vieta',2,3,10,false),
(12,65.5,3,3,'Šviesus butas',1,1,1,false),
(13,78.6,4,3,'Netoli parko',5,2,2,false),
(14,44.7,2,1,'Tvarkingas',3,2,3,false),
(15,81.0,5,3,'Centro širdyje',1,3,4,false),
(16,36.6,1,1,'Mažytis butas',4,2,5,false),
(17,110.5,7,5,'Didelis šeimos butas',2,1,6,false),
(18,59.2,2,2,'Modernus dizainas',1,1,7,false),
(19,66.4,3,2,'Erdvus koridorius',5,2,8,false),
(20,45.8,1,1,'Netoli universiteto',4,3,9,false),
(21,73.0,4,3,'Renovuotas namas',2,1,10,false),
(22,50.5,2,2,'Su parkavimo vieta',2,2,1,false),
(23,82.1,5,3,'Puikus išplanavimas',1,3,2,false),
(24,64.2,3,2,'Pigus variantas',3,2,3,false),
(25,NULL,NULL,8,'Visas pastatas',5,1,4,true);

-- =====================================================
-- Viewings (25 entries, one per apartment for demo)
-- =====================================================
INSERT INTO viewing("from","to",status,fk_Availabilityid_Availability)
VALUES
('2023-09-11 10:00','2023-09-11 11:00',1,1),
('2023-09-12 14:00','2023-09-12 15:00',2,2),
('2023-09-13 09:00','2023-09-13 10:00',3,3),
('2023-09-14 11:00','2023-09-14 12:00',4,4),
('2023-09-15 16:00','2023-09-15 17:00',1,5),
('2023-09-16 10:00','2023-09-16 11:00',2,6),
('2023-09-17 13:00','2023-09-17 14:00',5,7),
('2023-09-18 09:30','2023-09-18 10:30',1,8),
('2023-09-19 10:00','2023-09-19 11:00',2,9),
('2023-09-20 15:00','2023-09-20 16:00',3,10),
('2023-09-21 10:00','2023-09-21 11:00',1,1),
('2023-09-22 14:00','2023-09-22 15:00',2,2),
('2023-09-23 09:00','2023-09-23 10:00',3,3),
('2023-09-24 11:00','2023-09-24 12:00',4,4),
('2023-09-25 16:00','2023-09-25 17:00',1,5),
('2023-09-26 10:00','2023-09-26 11:00',2,6),
('2023-09-27 13:00','2023-09-27 14:00',5,7),
('2023-09-28 09:30','2023-09-28 10:30',1,8),
('2023-09-29 10:00','2023-09-29 11:00',2,9),
('2023-09-30 15:00','2023-09-30 16:00',3,10),
('2023-10-01 09:00','2023-10-01 10:00',1,1),
('2023-10-02 09:00','2023-10-02 10:00',1,2),
('2023-10-03 09:00','2023-10-03 10:00',2,3),
('2023-10-04 09:00','2023-10-04 10:00',3,4),
('2023-10-05 09:00','2023-10-05 10:00',4,5);

-- =====================================================
-- Pictures (25 entries, one per apartment)
-- =====================================================
INSERT INTO picture(id,public,fk_Apartmentid_Apartment)
VALUES
('pic_1',true,1),
('pic_2',true,2),
('pic_3',true,3),
('pic_4',false,4),
('pic_5',true,5),
('pic_6',true,6),
('pic_7',false,7),
('pic_8',true,8),
('pic_9',true,9),
('pic_10',false,10),
('pic_11',true,11),
('pic_12',true,12),
('pic_13',true,13),
('pic_14',true,14),
('pic_15',true,15),
('pic_16',true,16),
('pic_17',true,17),
('pic_18',true,18),
('pic_19',true,19),
('pic_20',true,20),
('pic_21',true,21),
('pic_22',true,22),
('pic_23',true,23),
('pic_24',true,24),
('pic_25',true,25);

-- =====================================================
-- Listings (25 entries, covers all apartments; rent prices=monthly)
-- =====================================================
INSERT INTO listing(description,askingPrice,rent,fk_Viewingid_Viewing,fk_Pictureid)
VALUES
('Butas Vilniaus centre',120000,false,1,'pic_1'),
('Mažas butas studentams',450,true,2,'pic_2'),
('Šeimos būstas Kaune',150000,false,3,'pic_3'),
('Prabangus apartamentas Klaipėdoje',250000,false,4,'pic_4'),
('Ekonomiškas variantas Šiauliuose',380,true,5,'pic_5'),
('Visas pastatas nuomai Panevėžyje',1500,true,6,'pic_6'),
('Renovuotas butas Vilniuje',135000,false,7,'pic_7'),
('Erdvus apartamentas Kaune',170000,false,8,'pic_8'),
('Butas prie jūros',200000,false,9,'pic_9'),
('Pigus butas',320,true,10,'pic_10'),
('Šviesus butas',120000,false,11,'pic_11'),
('Netoli miesto parko',500,true,12,'pic_12'),
('Centro rajonas',160000,false,13,'pic_13'),
('Studentų butas',350,true,14,'pic_14'),
('Modernus loftas',190000,false,15,'pic_15'),
('Studija jaunimui',400,true,16,'pic_16'),
('Didelis šeimos butas',210000,false,17,'pic_17'),
('Butas su terasa',650,true,18,'pic_18'),
('Erdvus koridorius',175000,false,19,'pic_19'),
('Butas prie universiteto',480,true,20,'pic_20'),
('Renovuotas namas',185000,false,21,'pic_21'),
('Parkavimas įskaičiuotas',550,true,22,'pic_22'),
('Puikus išplanavimas',195000,false,23,'pic_23'),
('Ekonomiškas variantas',370,true,24,'pic_24'),
('Visas pastatas Šiauliuose',2000,true,25,'pic_25');
