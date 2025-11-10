
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
1. Sukompiliuokite programą. Šis veiksmas gali užtrukti iki 10min ar ilgiau priklausomai nuo Jūsų interneto ir kompiuterio spartos.\
```docker-compose build```\
arba, jeigu praėjusi komanda nesuveikė, kartais būna tokia sintaksė\
```docker compose build```
1. Paleiskite programą.\
```docker-compose up -d```\
arba\
```docker compose up -d```
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
