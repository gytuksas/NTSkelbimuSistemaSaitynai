-- USERS
INSERT INTO "user"(name, surname, email, phone, password, registrationTime, profilePicture) VALUES
('Jonas','Petrauskas','jonas.petrauskas@example.lt','+37060011223','hashedpass1','2023-01-10','jonas.jpg'),
('Aistė','Kazlauskaitė','aiste.kazlauskaite@example.lt','+37060011224','hashedpass2','2023-02-15','aiste.jpg'),
('Mantas','Jankauskas','mantas.jankauskas@example.lt','+37060011225','hashedpass3','2023-03-20',NULL),
('Rūta','Žukauskaitė','ruta.zukauskaite@example.lt','+37060011226','hashedpass4','2023-05-12','ruta.png'),
('Dainius','Stankevičius','dainius.stankevicius@example.lt','+37060011227','hashedpass5','2023-06-23',NULL),
('Gabija','Kavaliauskaitė','gabija.kavaliauskaite@example.lt','+37060011228','hashedpass6','2023-07-01','gabija.jpg'),
('Tomas','Vasiliauskas','tomas.vasiliauskas@example.lt','+37060011229','hashedpass7','2023-08-10',NULL),
('Ieva','Urbonaitė','ieva.urbonaite@example.lt','+37060011230','hashedpass8','2023-09-05','ieva.png'),
('Paulius','Adomaitis','paulius.adomaitis@example.lt','+37060011231','hashedpass9','2023-09-20',NULL),
('Eglė','Savickaitė','egle.savickaite@example.lt','+37060011232','hashedpass10','2023-09-28','egle.png'),
('Aurimas','Valeika','aurimas.valeika@example.lt','+37060011233','hashedpass11','2023-10-01',NULL),
('Simona','Šimkutė','simona.simkute@example.lt','+37060011234','hashedpass12','2023-10-03','simona.jpg');

-- ADMIN (first 2 users)
INSERT INTO administrator(id_User) VALUES (1),(2);

-- BROKERS (users 3-7)
INSERT INTO broker(confirmed, blocked, id_User) VALUES
(true,false,3),
(true,false,4),
(true,false,5),
(true,false,6),
(true,false,7);

-- BUYERS (users 8-12)
INSERT INTO buyer(confirmed, blocked, id_User) VALUES
(true,false,8),
(true,false,9),
(true,false,10),
(true,false,11),
(true,false,12);

-- AVAILABILITY
INSERT INTO availability("from","to",fk_Brokerid_User) VALUES
('2023-10-05','2023-10-06',3),
('2023-10-07','2023-10-09',4),
('2023-10-10','2023-10-11',5),
('2023-10-12','2023-10-14',6),
('2023-10-15','2023-10-17',7),
('2023-10-18','2023-10-19',3),
('2023-10-20','2023-10-21',4),
('2023-10-22','2023-10-23',5),
('2023-10-24','2023-10-25',6),
('2023-10-26','2023-10-28',7);

-- BUILDINGS (20 total, distributed between brokers)
INSERT INTO building(city,address,area,year,lastRenovationYear,floors,energy,fk_Brokerid_User) VALUES
('Vilnius','Gedimino pr. 10',120.5,1990,2018,5,2,3),
('Kaunas','Laisvės al. 50',200.0,1980,2015,4,3,4),
('Vilnius','Pilies g. 7',85.7,2005,NULL,3,1,5),
('Kaunas','Savanorių pr. 120',300.0,1975,2020,6,4,6),
('Vilnius','Naugarduko g. 15',150.3,1999,NULL,4,2,7),
('Panevėžys','Respublikos g. 3',220.0,1985,2019,5,5,3),
('Vilnius','Šnipiškių g. 45',175.0,1992,NULL,4,1,4),
('Klaipėda','Liepų g. 30',265.0,1978,2010,6,2,5),
('Vilnius','Švitrigailos g. 8',140.0,2004,NULL,3,3,6),
('Kaunas','Žemaičių g. 20',195.0,1996,2016,4,2,7),
('Šiauliai','Tilžės g. 11',230.0,1990,2021,5,2,3),
('Vilnius','Antakalnio g. 55',160.0,2008,NULL,3,1,4),
('Kaunas','Kęstučio g. 17',210.0,1983,2013,4,4,5),
('Klaipėda','Taikos pr. 72',250.0,1977,NULL,8,3,6),
('Vilnius','Žirmūnų g. 89',90.0,2002,NULL,2,1,7),
('Panevėžys','Kniaudiškių g. 25',100.0,1995,2018,3,2,3),
('Kaunas','Raudondvario pl. 140',310.0,1981,NULL,5,4,4),
('Vilnius','Ozo g. 15',400.0,1999,2019,7,1,5),
('Klaipėda','Baltijos pr. 99',350.0,2005,NULL,6,2,6),
('Vilnius','Kalvarijų g. 150',270.0,1993,2012,5,3,7);

-- CONFIRMATIONS
INSERT INTO confirmation(id,expires,fk_Buyerid_User) VALUES
('conf1','2023-12-01',8),
('conf2','2023-12-05',9),
('conf3','2023-12-10',10),
('conf4','2023-12-15',11),
('conf5','2023-12-20',12);

-- APARTMENTS (25 entries, some whole buildings)
INSERT INTO apartment(apartmentNumber,area,floor,rooms,notes,heating,finish,fk_Buildingid_Building,isWholeBuilding) VALUES
(12,60.0,2,3,'Erdvus butas su balkonu',1,1,1,false),
(5,45.0,1,2,'Renovuotas vonios kambarys',2,2,1,false),
(8,95.0,3,4,'Su parkavimo vieta',1,1,2,false),
(NULL,200.0,NULL,8,'Visas pastatas komercijai',3,1,2,true),
(7,70.0,2,3,'Gražus vaizdas pro langą',4,2,3,false),
(3,85.0,1,3,'Arti miesto centro',5,3,4,false),
(10,55.0,4,2,'Su balkonu ir sandėliuku',2,1,5,false),
(NULL,150.0,NULL,6,'Nuomojamas visas pastatas',1,2,6,true),
(22,65.0,5,3,'Tvarkingas, šviesus butas',3,1,7,false),
(15,120.0,2,5,'Didelė svetainė',2,1,8,false),
(2,40.0,1,1,'Mažas pigus butas',4,3,9,false),
(NULL,220.0,NULL,10,'Visas namas su kiemu',2,1,10,true),
(18,75.0,3,3,'Buto langai į parką',1,2,11,false),
(25,80.0,4,3,'Nauji baldai',2,1,12,false),
(6,68.0,2,3,'Su sandėliuku',3,1,13,false),
(9,95.0,5,4,'Arti jūros',1,2,14,false),
(NULL,90.0,NULL,2,'Visas kotedžas',4,1,15,true),
(11,55.0,3,2,'Šviesus kambarys',2,2,16,false),
(14,105.0,4,4,'Didelė virtuvė',5,3,17,false),
(NULL,400.0,NULL,12,'Biurų pastatas',1,1,18,true),
(3,85.0,1,3,'Patogus išplanavimas',2,1,19,false),
(7,130.0,6,5,'Penthaus su terasa',1,2,20,false),
(1,42.0,1,2,'Tvarkingas ir pigus',5,2,11,false),
(4,58.0,2,2,'Su erdviu balkonu',3,1,12,false),
(NULL,160.0,NULL,7,'Visas pastatas su ofisais',2,1,13,true);

-- VIEWINGS (use 10 availabilities only)
INSERT INTO viewing("from","to",status,fk_Availabilityid_Availability) VALUES
('2023-10-06','2023-10-06',2,1),
('2023-10-08','2023-10-08',1,2),
('2023-10-11','2023-10-11',2,3),
('2023-10-14','2023-10-14',3,4),
('2023-10-17','2023-10-17',2,5),
('2023-10-19','2023-10-19',1,6),
('2023-10-21','2023-10-21',5,7),
('2023-10-23','2023-10-23',2,8),
('2023-10-25','2023-10-25',4,9),
('2023-10-27','2023-10-27',1,10);

-- PICTURES
INSERT INTO picture(id,public,fk_Apartmentid_Apartment) VALUES
('pic1',true,1),
('pic2',true,2),
('pic3',false,3),
('pic4',true,4),
('pic5',true,5),
('pic6',false,6),
('pic7',true,7),
('pic8',true,8),
('pic9',false,9),
('pic10',true,10),
('pic11',true,11),
('pic12',false,12),
('pic13',true,13),
('pic14',false,14),
('pic15',true,15),
('pic16',true,16),
('pic17',false,17),
('pic18',true,18),
('pic19',true,19),
('pic20',false,20),
('pic21',true,21),
('pic22',false,22),
('pic23',true,23),
('pic24',true,24),
('pic25',false,25);

-- LISTINGS (first 10 tied to viewings)
INSERT INTO listing(description,askingPrice,rent,fk_Viewingid_Viewing,fk_Pictureid) VALUES
('Parduodamas tvarkingas 3 kambarių butas miesto centre',150000,false,1,'pic1'),
('Nuomojamas erdvus 2 kambarių butas',600,true,2,'pic2'),
('Parduodamas butas su parkavimo vieta garaže',200000,false,3,'pic3'),
('Nuomojamas visas pastatas komercijai',3000,true,4,'pic4'),
('Butas su gražiu vaizdu į parką',120000,false,5,'pic5'),
('Parduodamas 3 kambarių butas arti senamiesčio',135000,false,6,'pic6'),
('Nuomojamas 2 kambarių butas ilgalaikei nuomai',500,true,7,'pic7'),
('Butas su balkonu ir sandėliuku',145000,false,8,'pic8'),
('Nuomojamas šviesus 3 kambarių butas',650,true,9,'pic9'),
('Parduodamas erdvus 5 kambarių butas šeimai',250000,false,10,'pic10');
