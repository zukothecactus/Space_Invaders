# ?? Reorganizacija projekta - izvršena ?

## ?? Šta je ura?eno:

### ? **Fajlovi premeštteni u root direktorijum:**
- **README.md** - Preba?en iz `SpaceInvaders/Server/` u root sa kompletnom dokumentacijom
- **vezbe/** - Folder sa materijalima preba?en iz `SpaceInvaders/Server/vezbe/` u root
- **.gitignore** - Bio je ve? u root-u i optimizovan je

### ?? **Finalna struktura direktorijuma:**
```
C:\Users\obren\OneDrive\Desktop\fax\mreze\projekat\Space_Invaders\
??? .git/                       # Git repositorijum  
??? .gitignore                  # Git ignore fajl ?
??? .vs/                        # Visual Studio fajlovi
??? README.md                   # Glavna dokumentacija ?  
??? specifikacija.pdf           # Projektna specifikacija
??? vezbe/                      # Materijali za vežbe ?
?   ??? README.md
?   ??? prosledjivanje porta.txt
?   ??? specifikacija.txt
?   ??? vezbe1.txt kroz vezbe11.txt
?   ??? ...
??? SpaceInvaders/              # Glavna aplikacija
    ??? Server/                 # Server projekat
    ??? Player1/                # Prvi klijent
    ??? Player2/                # Drugi klijent
```

### ?? **Rezultat:**
- **README.md** sada sadrži kompletnu dokumentaciju sa instrukcijama za pokretanje
- **vezbe/** folder sa svim materijalima je u root-u gde treba
- **.gitignore** odgovaraju?e filtrira nepotrebne fajlove
- **Struktura je profesionalna** i pogodna za Git repositorijum

### ?? **Instrukcije za pokretanje (ažurirane u README.md):**
```bash
# Pokretanje servera
cd SpaceInvaders/Server
dotnet run

# Pokretanje igra?a
cd SpaceInvaders/Player1
dotnet run
```

Sve je sada organizovano kako treba za univerzitetski projekat! ??