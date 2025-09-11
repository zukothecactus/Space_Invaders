# Space Invaders - Simulacija u realnom vremenu

Simulacija igre Space Invaders u kojoj u?estvuju jedan ili dva igra?a povezana sa centralnim serverom. Igra se odvija u terminalskom okruženju i koristi TCP/UDP protokole za komunikaciju.

## Projekat struktura

```
Space_Invaders/
??? README.md                    # Projektna dokumentacija
??? .gitignore                   # Git ignore fajl
??? specifikacija.pdf            # Projektna specifikacija
??? vezbe/                       # Materijali za vežbe
??? SpaceInvaders/               # Glavna aplikacija
    ??? SpaceInvaders.sln        # Visual Studio solution
    ??? Server/                  # Server aplikacija
    ?   ??? Server.csproj
    ?   ??? ServerScreen.cs      # Entry point za server
    ?   ??? Server.cs            # Server klase
    ??? Player1/                 # Prvi klijent
    ?   ??? Player1.csproj
    ?   ??? Player1Screen.cs     # Entry point za prvi klijent
    ?   ??? Player1.cs           # Klijentska klasa
    ??? Player2/                 # Drugi klijent
        ??? Player2.csproj
        ??? Player2Screen.cs     # Entry point za drugi klijent
        ??? Player2.cs           # Klijentska klasa
```

## Klase i njihove funkcije

### Server klase (Server.cs):
- **`GameServer`** - Glavna server klasa za upravljanje igrom
- **`Position`** - Predstavlja poziciju na mapi (X, Y koordinate)
- **`Player`** - Predstavlja igra?a sa svim podacima (pozicija, skor, životi, mrežni endpoint-ovi)
- **`Obstacle`** - Predstavlja prepreku koja se spušta
- **`Bullet`** - Predstavlja metak koji puca igra?

### Klijent klase (Player1.cs / Player2.cs):
- **`SpaceInvadersClient`** - Glavna klijentska klasa za komunikaciju sa serverom

### Entry point klase:
- **`ServerScreen`** - Main klasa za pokretanje servera
- **`Player1Screen`** - Main klasa za pokretanje prvog klijenta  
- **`Player2Screen`** - Main klasa za pokretanje drugog klijenta

## Tehni?ki opis

### Komunikacija
- **TCP protokol** - Za po?etno povezivanje, odabir moda igre i razmenu osnovnih podataka
- **UDP protokol** - Za komunikaciju u realnom vremenu (kretanje, pucanje, ažuriranje pozicija)

### Mapa igre
- Dimenzije: 15x30 (visina x širina) - optimizovano za brže gaming
- Igra?i se nalaze na dnu mape
- Prepreke se generišu na vrhu i spuštaju se prema dole
- Metci se kre?u od igra?a prema preprekama

### Portovi
- TCP port: 51000
- UDP port: 51001

## Kako pokrenuti igru

### 1. Pokretanje servera
```bash
cd SpaceInvaders/Server
dotnet run
```

### 2. Pokretanje prvog igra?a
```bash
cd SpaceInvaders/Player1  
dotnet run
```

### 3. Pokretanje drugog igra?a (opciono)
```bash
cd SpaceInvaders/Player2
dotnet run
```

## Kontrole

- **A** ili **?** - kretanje levo
- **D** ili **?** - kretanje desno  
- **SPACE** - pucanje
- **Q** - izlaz iz igre

## Tok igre

1. **Po?etak igre**: Prvi igra? bira mod (1 ili 2 igra?a) i broj poena potreban za pobedu
2. **Povezivanje**: Igra?i se povezuju sa serverom putem TCP-a
3. **Priprema**: Igra?i se pripremaju za igru (pritisnuti bilo koji taster kada se pojavi poruka)
4. **Igra**: 
   - Server generiše prepreke koje se spuštaju ka igra?ima
   - Igra?i šalju kretanja i akcije serveru putem UDP-a
   - Server ažurira stanje igre i šalje nazad igra?ima
   - Igra?i dobijaju poene kada pogode prepreku
   - Igra?i gube život kada ih pogodi prepreka
5. **Završetak**: Igra se završava kada neki igra? dostigne ciljan broj poena ili svi igra?i izgube sve živote

## Konfiguracija mreže

### Za lokalno testiranje
- Svi projekti automatski nude opciju lokalne konekcije (127.0.0.1)
- Ne treba dodatna konfiguracija

### Za udaljenu komunikaciju preko interneta
Prema materijalima o prosle?ivanju porta iz `vezbe/prosledjivanje porta.txt`:

#### **Korak 1: Konfiguracija servera**

1. **Otkrijte lokalnu IP adresu servera**:
   ```cmd
   ipconfig
   ```
   Zabeležite IPv4 adresu (npr. 192.168.1.100)

2. **Pristupite ruteru**:
   - Otvorite pregleda? i idite na 192.168.1.1 ili 192.168.0.1
   - Prijavite se sa admin credentials (na nalepnici rutera)

3. **Konfigurisite Port Forwarding**:
   - Idite na "Port Forwarding" ili "Virtual Servers" sekciju
   - Dodajte pravila:
     ```
     Ime: SpaceInvaders-TCP
     Internal Port: 51000
     External Port: 51000  
     IP adresa: [lokalna IP servera]
     Protokol: TCP
     
     Ime: SpaceInvaders-UDP
     Internal Port: 51001
     External Port: 51001
     IP adresa: [lokalna IP servera] 
     Protokol: UDP
     ```

4. **Otkrijte eksternu IP adresu**:
   - Idite na https://whatismyipaddress.com/
   - Zabeležite Public IP adresu

5. **Pokrenite server**:
   ```bash
   cd SpaceInvaders/Server
   dotnet run
   ```
   Server ?e prikazati lokalnu IP adresu i instrukcije

#### **Korak 2: Povezivanje klijenata**

1. **Pokrenite klijentsku aplikaciju**:
   ```bash
   cd SpaceInvaders/Player1
   dotnet run
   ```

2. **Izaberite tip konekcije**:
   - `1` - Za lokalno testiranje (127.0.0.1)
   - `2` - Za udaljenu konekciju preko interneta

3. **Za udaljenu konekciju**:
   - Unesite eksternu IP adresu servera
   - Aplikacija ?e automatski pokušati konekciju

#### **Korak 3: Verifikacija**

1. **Testiranje ping-a**:
   ```cmd
   ping [eksterna_IP_adresa_servera]
   ```

2. **Testiranje porta** (opciono):
   ```cmd
   telnet [eksterna_IP_adresa_servera] 51000
   ```

### Rešavanje problema sa mrežom

#### "Neuspešno povezivanje sa serverom"
- ? Proverite da li je server pokrenut
- ? Proverite da li je port forwarding konfigurisan
- ? Proverite firewall postavke
- ? Proverite da li su portovi 51000 i 51001 dostupni

#### "Timeout konekcije"
- ? Proverite internet konekciju
- ? Proverite da li je eksterna IP adresa ispravna
- ? Proverite da li ruter blokira konekcije

#### "UDP greška" 
- ? Proverite da li je UDP port (51001) prosle?en na ruteru
- ? Proverite firewall postavke za UDP saobra?aj
- ? **Rešeno**: Timeout greške su sada ignorirane jer su normalne

#### **Automatska konfiguracija**
Projekat sada automatski:
- ?? Detektuje lokalnu IP adresu servera
- ?? Pruža opcije za lokalno i udaljeno povezivanje  
- ?? Prikazuje instrukcije za port forwarding
- ?? Daje detaljne poruke o greškama sa preporu?enim rešenjima
- ?? Testira konekciju sa timeout-om

## Ispravke i poboljšanja

### Verzija 2.4 - Port Forwarding i mrežna konfiguracija
- **Napredna mrežna konfiguracija**: Automatska detekcija lokalne IP adrese servera
- **Port Forwarding podrška**: Implementiran prema "prosledjivanje porta.txt" materijalima
- **Dinami?ka IP konfiguracija**: Klijenti mogu odabrati lokalnu ili udaljenu konekciju
- **Bolje error handling**: Detaljne poruke o greškama sa preporu?enim rešenjima
- **Timeout sistem**: 10-sekundni timeout za TCP konekcije
- **Instrukcije za setup**: Automatski prikaz instrukcija za port forwarding na serveru
- **Network diagnostics**: Ping i tracert instrukcije u dokumentaciji

### Verzija 2.3 - Refaktorisanje imena fajlova
- **Beskrivna imena fajlova**: Zamenjen generi?ki "Class1.cs" sa opisnim imenima
- **Server klase**: `Class1.cs` ? `Server.cs`
- **Player klase**: `Class1.cs` ? `Player1.cs` i `Player2.cs`
- **Entry point klase**: `Program.cs` ? `ServerScreen.cs`, `Player1Screen.cs`, `Player2Screen.cs`
- **Bolja organizacija**: Jasnija struktura projekta sa opisnim imenima

### Verzija 2.2 - Poboljšana collision detection
- **Napredna collision detection**: Dodato "near miss" prepoznavanje za brže objekte
- **Stabilniji sistem**: Lista-baziran pristup za sigurno uklanjanje objekata
- **Bez index grešaka**: Rešeni problemi sa menjanjem lista tokom iteracije
- **Pouzdanije poga?anje**: Metci više ne?e prolaziti kroz preprege

### Verzija 2.1 - Optimizacija brzine i kontrole
- **Brže osvežavanje igre**: Update loop 100ms umesto 150ms (6.7 FPS)
- **Sporiji pad prepreka**: Prepreke se pomeraju svakih 200ms umesto 100ms
- **Re?e generisanje prepreka**: Nova prepreka svakih 4 sekunde umesto 3
- **Bolja kontrola gameplay-a**: Balans izme?u brzine i kontrole

### Verzija 2.0 - Optimizacija i stabilnost
- **Optimizovana mapa**: Promenjena sa 20x40 na 15x30 za brži gameplay
- **Poboljšana collision detection**: Pouzdaniji sistem detekcije kolizija sa sigurnosnim proverama
- **Sporiji refresh rate**: Optimizovan za bolju kontrolu (3.3 FPS)
- **?ista arhitektura koda**: Uklonjen problemati?ni "Class1" declaration
- **Stabilniji gameplay**: Prepreke se generišu svakih 3 sekunde umesto 2

### Verzija 1.1 - Ispravke UDP komunikacije
- **Ispravka UDP socket greške**: Dodati timeout-ovi za UDP prijem da se izbegnu beskonine blokiraju?e operacije
- **Sinhronizacija igra?a**: Server sada ?eka da se svi igra?i pripremi ("READY" signal) pre po?etka igre
- **Bolje rukovanje greškama**: Ignorišu se timeout greške koje su normalne za UDP komunikaciju
- **Automatska alokacija portova**: UDP klijenti koriste port 0 za automatsku alokaciju dostupnog porta

### Tehni?ka poboljšanja:
1. **UDP Timeout**: Klijenti imaju 2-sekundni timeout za UDP prijem
2. **Ready sistem**: Igra?i šalju "READY" signal kada pritisnu taster za po?etak
3. **Bolje parsiranje**: Dodano `StringSplitOptions.RemoveEmptyEntries` za ?iš?e parsiranje poruka
4. **Robusnost**: Bolje rukovanje disconnect scenario
5. **Game timing**: Update loop 100ms, prikaz 150ms, prepreke svakih 4s
6. **Sporiji pad prepreka**: Prepreke se pomeraju svakih 200ms za bolju kontrolu
7. **Advanced collision**: "Near miss" detection i lista-baziran cleanup sistem
8. **Refaktorisana imena**: Opisni nazivi fajlova umesto generi?kih

## Saveti za bezbednost

- Koristite složenu lozinku za ruter
- Portovi se automatski zatvaraju kada se server zaustavi
- Za produkciju, koristite dodatne bezbednosne mere

## Primer pokretanja

1. **Pokretanje servera:**
   ```
   === SPACE INVADERS SERVER ===
   Server pokrenut na TCP portu 51000 i UDP portu 51001
   ?ekam igra?e...
   ```

2. **Prvi igra? se povezuje i bira mod:**
   ```
   === SPACE INVADERS - IGRA? 1 ===
   Uspešno povezan sa serverom!
   Odaberite mod igre:
   1 - Jedan igra?
   2 - Dva igra?a
   Unesite broj (1 ili 2): 1
   ```

3. **Igra? se priprema:**
   ```
   ????????????????????????????????????????????????
   ?               IGRA PO?INJE                  ?
   ????????????????????????????????????????????????
   
   Kontrole:
   A / ? - kretanje levo
   D / ? - kretanje desno
   SPACE - pucanje
   Q - izlaz iz igre
   
   Pritisnite bilo koji taster za po?etak...
   ```

4. **Igra po?inje sa prikazom mape na serveru:**
   ```
   ???????????? SPACE INVADERS ????????????
   ? Marko      ? Skor:  0 ? Životi: 3 ? Pos:( 7,13) ?
   ? Cilj: 10 poena                           ?
   ?????????????????????????????????????????

   ????????????????????????????????
   ?                              ?
   ?               #              ?
   ?                              ?
   ?                              ?
   ?                              ?
   ?                              ?
   ?                              ?
   ?                              ?
   ?                              ?
   ?                              ?
   ?                              ?
   ?                              ?
   ?                              ?
   ?       1                      ?
   ?                              ?
   ?                              ?
   ????????????????????????????????
   
   Prepreke:  1 ? Metci:  0 ? Status: AKTIVNA ? FPS: 6.7
   ```

## Greške i rešavanje problema

### "Neuspešno povezivanje sa serverom"
- Proverite da li je server pokrenut
- Proverite da li su portovi dostupni
- Za udaljenu konekciju, proverite port forwarding

### "UDP greška" 
- Proverite firewall postavke
- Proverite da li su UDP portovi blokirani
- **Rešeno**: Timeout greške su sada ignorirane jer su normalne

### "Timeout" greške
- Proverite kvalitet mreže konekcije
- Proverite da li je server preoptere?en
- **Rešeno**: Dodani su timeout-ovi da se izbegnu beskon?ne blokiraju?e operacije

### "You must call the Bind method before performing this operation"
- **Rešeno**: UDP klijenti sada koriste port 0 za automatsku alokaciju
- **Rešeno**: Dodani su timeout-ovi za UDP operacije

### "Collision detection ne radi"
- **Rešeno**: Implementiran napredni collision sistem sa "near miss" detection
- **Rešeno**: Lista-baziran pristup za bezbedno uklanjanje objekata
- **Rešeno**: Eliminisane index out of range greške

## Testiranje

Za lokalno testiranje:
1. Pokrenite server u jednom terminalu
2. Pokrenite Player1 u drugom terminalu
3. Opciono pokrenite Player2 u tre?em terminalu
4. Pratite instrukcije na ekranu

Igra je sada stabilna i optimizovana za lokalno i udaljeno testiranje!

## Arhitektura koda

### Trenutna struktura:
- **Server klase**: `GameServer` (glavna), `Position`, `Player`, `Obstacle`, `Bullet`
- **Klijent klase**: `SpaceInvadersClient`
- **Entry point klase**: `ServerScreen`, `Player1Screen`, `Player2Screen`

### Optimizacije:
- **Manja mapa** (15x30) za brži gameplay
- **Brži refresh** za bolju responzivnost (6.7 FPS)
- **Napredna collision detection** sa sigurnosnim proverama
- **Stabilniji timer sistem** sa counter-based pristupom
- **Opisna imena fajlova** za bolju ?itljivost koda

### Performance karakteristike:
- **Update loop**: 100ms (10 Hz)
- **Prikaz**: 150ms (6.7 FPS)
- **Pad prepreka**: 200ms (5 Hz)
- **Generisanje prepreka**: 4000ms (0.25 Hz)

Ovaj pristup ?ini kod jasnim i optimizovanim za performanse, sa opisnim imenima fajlova i stabilnim gameplay-om.

## Tehnologije
- **.NET 8** - Framework za razvoj aplikacije
- **C#** - Programski jezik
- **TCP/UDP** - Mrežni protokoli za komunikaciju
- **PowerShell** - Za administraciju i deployment
- **Git** - Kontrola verzija koda