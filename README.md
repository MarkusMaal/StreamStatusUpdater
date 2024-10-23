# StreamStatusUpdater

Otseülekande tekstifailide dünaamilise muutmise süsteem.

## Ehitamine ja silumine

Käsureal: `dotnet run`


## Täisversiooni ehitamine

Käsureal: `dotnet build -c Release`<br>
Väljund paigutatakse kataloogi `bin/Release`


## Kasutamine

1. Määrake kõigepealt failiteed, kuhu muutuvad tekstifailid panna
2. Seejärel määrake sihtkellaaeg taimeri jaoks
3. Programm ooterežiimis: saate muuta telegraafi kui pärast trükkimist vajutate sisestusklahvi
4. Programmi katkestamiseks vajutage klahvikombinatsiooni Ctrl+C (k.a. macOS)


## Toetatud meediumiesitajad

Muusikapala pealkirja hankimine toimib hetkel ainult Windowsis. Linux ja macOS saab programmi siiski kasutada telegraafi ja taimeri jaoks, kuid muusikapala juures kuvatakse alati tekst "Meediumiesitajat pole avatud".

Saate kasutada järgmiseid meediumiesitajaid:
* VLC Media Player
* Media Player Classic (tuleb konfigureerida vastavalt, et akna pealkirjas kuvatakse failitee)
