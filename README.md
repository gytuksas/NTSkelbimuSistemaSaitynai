
# NT Skelbimų sistema

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
