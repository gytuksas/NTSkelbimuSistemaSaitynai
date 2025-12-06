
# NT Skelbimų sistema

## Paleidimo instrukcijos (Windows ir Linux)
Reikalavimai:
- Git;
- Docker;
- Duomenų bazės valdymo įrankis (pvz. DBeaver) ir mokėjimas juo naudotis.

Toliau esančios komandos turėtų būti vykdomos atsidarius PowerShell (Windows) arba terminalą (Linux) ir nuėjus į norimą aplanką.

1. Parsisiųskite ir išskleiskite šią repozitoriją.\
```git clone https://github.com/gytuksas/NTSkelbimuSistemaSaitynai.git```
1. Nueikite į parsisiųstą repozitoriją.\
```cd NTSkelbimuSistemaSaitynai```
1. Susikurkite aplanką slaptažodžiams ir kitai sensityviai informacijai.\
```mkdir secrets```
1. Nueikite į naujai sukurtą aplanką.\
```cd secrets```
1. Toliau reikia sukurti tokius failus (pakeiskite viską laužtiniuose skliaustuose, laužtinių skliaustų nepalikite):\
    - Postgres duombazės slaptažodis:\
    ```echo "[SLAPTAŽODIS]" > pgpass.txt```
    - Konfigūracijos informacija (JSON failas):\
    ```nano config.json``` (arba bet koks kitas redagavimo įrankis)
    Failo viduje turi būti pateikta tokia informacija būtent tokia struktūra:
    ```
        {
            "Jwt":
            {
                "Issuer": "DOMENAS, PVZ example.com",
                "Key": "SUPERSECRETKEY32BYTES"
            }
        }
    ```
1. Grįžkite į pradinę direktoriją.\
```cd ..```
1. Paleiskite programą.\
```docker-compose up -d --build```\
arba\
```docker compose up -d --build```
1. Duomenų bazė automatiškai susikurią reikalingą struktūrą ir įkelia netikrus duomenis. Jeigu netikrų duomenų nereikia, iš failo `PGSQLDockerfile` ištrinkite eilutę `COPY fake_db_data.sql /docker-entrypoint-initdb.d/` ir iš naujo įvykdykite praėjusius du veiksmus. Jeigu netikri duomenys Jums tinka, galite praleisti šį žingsnį. Papildoma informacija:
    - Duomenų bazės prisijungimo adresas: `localhost:5432`, Vartotojas: `postgres`, slaptažodis toks, kokį nurodėte kuriant `pgpass.txt` failą.
    - Duomenų bazė yra nepasiekiama ją talpinančiam kompiuteriui, tačiau, jeigu reikia daryti kažkokius pakeitimus ranka, faile `docker-compose.override.yml` reikia įjungti duomenų bazės ryšį su išore. Tai galima padarytį atkomentuojant šią eilutę:
    ```  
    db:
      networks:
    #    - bridged  <-------- IŠTRYNUS # DUOMENŲ BAZĖ TAPS PASIEKIAMA
        - dbnet
    ```
    Baigus darbus, rekomenduojama eilutę užkomentuoti vėl ir perkrauti programą.
1. Programa dabar turėtų būti pasiekiama.\
    - API taškai: ```http://localhost:8080/api```
    - Swagger sąsaja: ```http://localhost:8080/swagger```
    - Vartotojo sąsaja (tinklapis) ```http://localhost:4173/```
1. Norint išjungti programą, paleiskite šią komandą.\
```docker-compose down```\
arba\
```docker compose down```

### Sistemos aprašymas

Projekto tikslas – NT turto brokeriams lengvai pateikti ir valdyti nekilnojamo turto skelbimus bei pagerinti patirtį potencialiems pirkėjams leidžiant lengvai peržiūrėti visą su skelbimu susijusią informaciją, tokią, kaip atvirų durų dienas skelbimui, pardavėjo kontaktus ir leisti užsisakyti privačią turto apžiūrą patvirtinus pirkėjo informaciją, taip išvengiant potencialaus pardavėjo duomenų nutekinimo įvairiems automatiniams interneto naršymo robotams ir bereikalingo trukdymo.
Veikimo principas – kuriamą platformą sudaro dvi dalys: vartotojo sąsaja internetinio tinklapio pavidalu, kuria naudosis visi sistemos naudotojai bei aplikacijų programavimo sąsaja (angl. trump. API).

Neregistruotas vartotojas (pirkėjas) galės naudotis šios platformos bazinėmis funkcijomis, tokiomis kaip skelbimų paieška bei skelbimų informacijos peržiūra be jokių apribojimų, tačiau norėdamas peržiūrėti pardavėjo kontaktus arba užsisakyti privačią apžiūrą turės užsiregistruoti ir patvirtinti savo tapatybę. Užsiregistravus bei patvirtinus savo tapatybę pirkėjas galės lengvai peržiūrėti pardavėjų kontaktus, užsisakyti bei valdyti savo privačias apžiūras. 
NT Brokeris, norėdamas naudotis platforma, pirmiausiai turės užsiregistruoti ir būti patvirtintas administratoriaus. Tuomet jis galės įkelti savo parduodamus butus suvedant visą informaciją apie pastatą ir apie patį butą. Šią informaciją vėliau būtų galima redaguoti arba pašalinti. Butams brokeris galės įkelti nuotraukas, kurias gali pažymėti privačiomis, kad jos neatsirastų skelbimuose ir būtų prieinamos tiktais brokeriui. Suvedus visą informaciją, brokeris galės lengvai sukurti skelbimą butui, o visa pastato ir buto informacija bus automatiškai užpildyta, liktų tiktais įvesti kainą ir norimą papildomą informaciją. Taip pat brokeris turi galimybe nusistatyti savo prieinamumo laikus, kad pirkėjai galėtų užsisakyti privačias apžiūras neskambinant brokeriui tiesiogiai. Šią funkciją taip pat galima išjungti. Galiausiai, brokeris galėtų užsakytas privačias apžiūras butams prieš joms įvykstant patvirtinti arba atšaukti.
Administratorius tvirtina NT brokerių registracijas, pirkėjų tapatybes bei moderuoja tinklapį, trinant blogus skelbimus ir blokuojant piktnaudžiaujančius vartotojus.

### Funkciniai reikalavimai

Neregistruotas vartotojas galės:
- Ieškoti skelbimų;
- Peržiūrėti skelbimus ir jų informaciją (be kontaktų);
- Matyti atvirų durų dienas skelbimui;
- Prisijungti prie sistemos;
- Užsiregistruoti prie sistemos kaip registruotas naudotojas.
- Užsiregistruoti prie sistemos kaip NT brokeris.

Registruotas vartotojas galės:
- Atsijungti nuo sistemos;
- Patvirtinti savo tapatybę;
- Peržiūrėti pardavėjo kontaktus;
- Užsisakyti privačią apžiūrą.
- Peržiūrėti užsakytas privačias apžiūras.

NT Brokeris galės:
- Atsijungti nuo sistemos;
- Atnaujinti savo kontaktinę informaciją;
- Peržiūrėti savo pastatus;
- Įdėti/redaguoti/trinti pastatus;
- Peržiūrėti savo butus;
- Įdėti/redaguoti/trinti butus pastatui;
- Įkelti/trinti nuotraukas butams;
- Nustatyti, kurios nuotraukos yra matomos skelbime;
- Peržiūrėti savo skelbimus;
- Sukurti/redaguoti/trinti skelbimus butams;
- Nusistatyti, kada jis yra laisvas apžiūroms;
- Peržiūrėti užsakytas privačias peržiūras.
- Patvirtinti/atmesti privačių peržiūrų užklausas.

Administratorius galės:
- Peržiūrėti sistemos naudotojus;
- Blokuoti sistemos naudotojus;
- Patvirtinti NT brokerių registracijas;
- Naikinti skelbimus;

### Sistemos architektūra

Sistemos sudedamosios dalys:
- Kliento pusė (front-end) – React.JS;
- Serverio pusė (back-end) – C#;
- Duomenų bazė – PostgreSQL.

Žemiau esančiame paveikslėlyje pavaizduota kuriamos sistemos diegimo diagrama. Sistemai talpinti būtų naudojamas Linux serveris arba Windows Linux posistemė, kurioje veiktų Docker konteineriai. Kiekviena sistemos dalis diegiama tame pačiame serveryje, bet skirtinguose konteineriuose, kurie tarpusavyje komunikuoja vidiniu tinklu. Internetinis tinklapis yra pasiekiamas per HTTP/HTTPS protokolą. Tinklapio veikimui reikalinga NT API sąsaja, kuri gražina bei manipuliuoja svetainės duomenis. Sistemos naudotojas su šiuo API taip pat komunikuoja per HTTP/HTTPS protokolą. NT API komunikuoja su PostgreSQL duomenų bazę per vidinį tinklą PostgreSQL protokolu per TCP/IP.
<img width="642" height="361" alt="Picture" src="https://github.com/user-attachments/assets/36d22261-3c2a-4512-8ead-210f1f1698f1" />

### Naudotojo sąsajos projektas

Žemiau pateikta keletas sistemos langų „wireframe“ ir juos atitinkančios realizacijos.

![NT Saitynai_page-0001](https://github.com/user-attachments/assets/b447cab7-1e65-49e3-995e-2e0c6a64288b)
<img width="2560" height="1281" alt="Screenshot 2025-12-06 at 19-56-53 NT Saitynai" src="https://github.com/user-attachments/assets/2b551d0c-5471-4b79-9eef-9ea07cded408" />


![NT Saitynai-1_page-0001](https://github.com/user-attachments/assets/9a2e59ef-d0b4-4967-8979-417a6dce4b18)
<img width="2560" height="2257" alt="Screenshot 2025-12-06 at 19-57-02 NT Saitynai" src="https://github.com/user-attachments/assets/847b97a4-b0ae-4833-ab12-5e3c4a858290" />


![NT Saitynai-4_page-0001](https://github.com/user-attachments/assets/bc9810af-b283-42ea-9b3f-e19e4e8d90b5)
<img width="2560" height="1281" alt="Screenshot 2025-12-06 at 19-57-20 NT Saitynai" src="https://github.com/user-attachments/assets/6d8ef374-db5e-4f78-980e-82bd5074d6ff" />

### OpenAPI specifikacija
OpenAPI specifikacijos failas gali būti rastas šioje GitHub repozitorijoje [čia](https://github.com/gytuksas/NTSkelbimuSistemaSaitynai/blob/main/openapi.json).

### Projekto išvados

- Sėkmingai išmokta kurti pilną saitynų projektą su serverio ir naršyklės logika;
- Sėkmingai išmokta kurti taisyklingus API metodus su pilna dokumentacija;
- Sėkmingai išmokta JWT autentikacijos principų;
- Sėkmingai išmokta įdiegti projektą į internetą naudojantis “Cloudflare Tunnels”.

