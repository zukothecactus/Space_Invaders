# Space Invaders - Simulacija u realnom vremenu

Simulacija igre Space Invaders u kojoj učestvuju jedan ili dva igrača povezana sa centralnim serverom. Igra se odvija u terminalskom okruženju i koristi TCP/UDP protokole za komunikaciju.

## Projekat struktura

- **Server** - Centralni server koji upravlja igrom
  - `Class1.cs` - Server klase (`GameServer`, `Position`, `Player`, `Obstacle`, `Bullet`)
  - `Program.cs` - Entry point za server aplikaciju
- **Player1** - Klijentska aplikacija za prvog igrača  
  - `Class1.cs` - Klijentska klasa (`SpaceInvadersClient`)
  - `Program.cs` - Entry point za prvi klijent
- **Player2** - Klijentska aplikacija za drugog igrača
  - `Class1.cs` - Klijentska klasa (`SpaceInvadersClient`)
  - `Program.cs` - Entry point za drugi klijent

## Klase i njihove funkcije

### Server klase:
- **`GameServer`** - Glavna server klasa za upravljanje igrom
- **`Position`** - Predstavlja poziciju na mapi (X, Y koordinate)
- **`Player`** - Predstavlja igrača sa svim podacima (pozicija, skor, životi, mrežni endpoint-ovi)
- **`Obstacle`** - Predstavlja prepreku koja se spušta
- **`Bullet`** - Predstavlja metak koji puca igrač

### Klijent klase:
- **`SpaceInvadersClient`** - Glavna klijentska klasa za komunikaciju sa serverom

## Tehnički opis

### Komunikacija
- **TCP protokol** - Za početno povezivanje, odabir moda igre i razmenu osnovnih podataka
- **UDP protokol** - Za komunikaciju u realnom vremenu (kretanje, pucanje, ažuriranje pozicija)

### Mapa igre
- Dimenzije: 15x30 (visina x širina) - optimizovano za brže gaming
- Igrači se nalaze na dnu mape
- Prepreke se generišu na vrhu i spuštaju se prema dole
- Metci se kreću od igrača prema preprekama

### Portovi
- TCP port: 51000
- UDP port: 51001

## Kako pokrenuti igru

### 1. Pokretanje servera
```bash
cd Server
dotnet run
```

### 2. Pokretanje prvog igrača
```bash
cd Player1  
dotnet run
```

### 3. Pokretanje drugog igrača (opciono)
```bash
cd Player2
dotnet run
```

## Kontrole

- **A** ili **←** - kretanje levo
- **D** ili **→** - kretanje desno  
- **SPACE** - pucanje
- **Q** - izlaz iz igre

## Tok igre

1. **Početak igre**: Prvi igrač bira mod (1 ili 2 igrača) i broj poena potreban za pobedu
2. **Povezivanje**: Igrači se povezuju sa serverom putem TCP-a
3. **Priprema**: Igrači se pripremaju za igru (pritisnuti bilo koji taster kada se pojavi poruka)
4. **Igra**: 
   - Server generiše prepreke koje se spuštaju ka igračima
   - Igrači šalju kretanja i akcije serveru putem UDP-a
   - Server ažurira stanje igre i šalje nazad igračima
   - Igrači dobijaju poene kada pogode prepreku
   - Igrači gube život kada ih pogodi prepreka
5. **Završetak**: Igra se završava kada neki igrač dostigne ciljan broj poena ili svi igrači izgube sve živote

## Konfiguracija mreže

### Za lokalno testiranje
- Svi projekti koriste `127.0.0.1` (localhost)
- Ne treba dodatna konfiguracija

### Za udaljenu komunikaciju
Prema materijalima o prosleđivanju porta:

1. **Otkrijte lokalnu IP adresu**:
   ```cmd
   ipconfig
   ```

2. **Konfigurišite ruter**:
   - Pristupite ruteru preko pregledača (obično 192.168.1.1 ili 192.168.0.1)
   - Idite na "Port Forwarding" ili "Virtual Servers"
   - Dodajte pravila za:
     - TCP port 51000 → lokalna IP adresa servera
     - UDP port 51001 → lokalna IP adresa servera

3. **Otkrijte eksternu IP adresu** (za server):
   - Koristite sajt poput https://whatismyipaddress.com/

4. **Ažurirajte kod klijenta**:
   - U `Player1/Class1.cs` i `Player2/Class1.cs`
   - Promenite `SERVER_IP` sa "127.0.0.1" na eksternu IP adresu servera

5. **Testiranje konekcije**:
   ```cmd
   ping [eksterna_IP_adresa_servera]
   ```

## Ispravke i poboljšanja

### Verzija 2.2 - Poboljšana collision detection
- **Napredna collision detection**: Dodato "near miss" prepoznavanje za brže objekte
- **Stabilniji sistem**: Lista-baziran pristup za sigurno uklanjanje objekata
- **Bez index grešaka**: Rešeni problemi sa menjanjem lista tokom iteracije
- **Pouzdanije pogađanje**: Metci više neće prolaziti kroz prepreke

### Verzija 2.1 - Optimizacija brzine i kontrole
- **Brže osvežavanje igre**: Update loop 100ms umesto 150ms (6.7 FPS)
- **Sporiji pad prepreka**: Prepreke se pomeraju svakih 200ms umesto 100ms
- **Ređe generisanje prepreka**: Nova prepreka svakih 4 sekunde umesto 3
- **Bolja kontrola gameplay-a**: Balans između brzine i kontrole

### Verzija 2.0 - Optimizacija i stabilnost
- **Optimizovana mapa**: Promenjena sa 20x40 na 15x30 za brži gameplay
- **Poboljšana collision detection**: Pouzdaniji sistem detekcije kolizija sa sigurnosnim proverama
- **Sporiji refresh rate**: Optimizovan za bolju kontrolu (3.3 FPS)
- **Čista arhitektura koda**: Uklonjen problematični "Class1" declaration
- **Stabilniji gameplay**: Prepreke se generišu svakih 3 sekunde umesto 2

### Verzija 1.1 - Ispravke UDP komunikacije
- **Ispravka UDP socket greške**: Dodati timeout-ovi za UDP prijem da se izbegnu beskončne blokirajuće operacije
- **Sinhronizacija igrača**: Server sada čeka da se svi igrači pripremi ("READY" signal) pre početka igre
- **Bolje rukovanje greškama**: Ignorišu se timeout greške koje su normalne za UDP komunikaciju
- **Automatska alokacija portova**: UDP klijenti koriste port 0 za automatsku alokaciju dostupnog porta

### Tehnička poboljšanja:
1. **UDP Timeout**: Klijenti imaju 2-sekundni timeout za UDP prijem
2. **Ready sistem**: Igrači šalju "READY" signal kada pritisnu taster za početak
3. **Bolje parsiranje**: Dodano `StringSplitOptions.RemoveEmptyEntries` za čišće parsiranje poruka
4. **Robusnost**: Bolje rukovanje disconnect scenario
5. **Game timing**: Update loop 100ms, prikaz 150ms, prepreke svakih 4s
6. **Sporiji pad prepreka**: Prepreke se pomeraju svakih 200ms za bolju kontrolu
7. **Advanced collision**: "Near miss" detection i lista-baziran cleanup sistem

## Saveti za bezbednost

- Koristite složenu lozinku za ruter
- Portovi se automatski zatvaraju kada se server zaustavi
- Za produkciju, koristite dodatne bezbednosne mere

## Primer pokretanja

1. **Pokretanje servera:**
   ```
   === SPACE INVADERS SERVER ===
   Server pokrenut na TCP portu 51000 i UDP portu 51001
   Čekam igrače...
   ```

2. **Prvi igrač se povezuje i bira mod:**
   ```
   === SPACE INVADERS - IGRAČ 1 ===
   Uspešno povezan sa serverom!
   Odaberite mod igre:
   1 - Jedan igrač
   2 - Dva igrača
   Unesite broj (1 ili 2): 1
   ```

3. **Igrač se priprema:**
   ```
   ╔══════════════════════════════════════════════╗
   ║               IGRA POČINJE                  ║
   ╚══════════════════════════════════════════════╝
   
   Kontrole:
   A / ← - kretanje levo
   D / → - kretanje desno
   SPACE - pucanje
   Q - izlaz iz igre
   
   Pritisnite bilo koji taster za početak...
   ```

4. **Igra počinje sa prikazom mape na serveru:**
   ```
   ╔═══════════ SPACE INVADERS ═══════════╗
   ║ Marko      │ Skor:  0 │ Životi: 3 │ Pos:( 7,13) ║
   ║ Cilj: 10 poena                           ║
   ╚═══════════════════════════════════════╝

   ┌──────────────────────────────┐
   │                              │
   │               #              │
   │                              │
   │                              │
   │                              │
   │                              │
   │                              │
   │                              │
   │                              │
   │                              │
   │                              │
   │                              │
   │       1                      │
   │                              │
   │                              │
   └──────────────────────────────┘
   
   Prepreke:  1 │ Metci:  0 │ Status: AKTIVNA │ FPS: 6.7
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
- Proverite da li je server preopteređen
- **Rešeno**: Dodani su timeout-ovi da se izbegnu beskončne blokirajuće operacije

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
3. Opciono pokrenite Player2 u trećem terminalu
4. Pratite instrukcije na ekranu

Igra je sada stabilna i optimizovana za lokalno i udaljeno testiranje!

## Arhitektura koda

### Trenutna struktura:
- **Server klase**: `GameServer` (glavna), `Position`, `Player`, `Obstacle`, `Bullet`
- **Klijent klase**: `SpaceInvadersClient`
- **Program klase**: Entry point za svaki projekat

### Optimizacije:
- **Manja mapa** (15x30) za brži gameplay
- **Brži refresh** za bolju responzivnost (6.7 FPS)
- **Napredna collision detection** sa sigurnosnim proverama
- **Stabilniji timer sistem** sa counter-based pristupom

### Performance karakteristike:
- **Update loop**: 100ms (10 Hz)
- **Prikaz**: 150ms (6.7 FPS)
- **Pad prepreka**: 200ms (5 Hz)
- **Generisanje prepreka**: 4000ms (0.25 Hz)

Ovaj pristup čini kod jasnim i optimizovanim za performanse, sa očuvanim imenima fajlova i stabilnim gameplay-om.