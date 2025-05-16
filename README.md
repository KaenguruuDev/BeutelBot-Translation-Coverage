# Konfiguration

## Linux

1. Terminal öffnen
2. `git clone "https://github.com/KaenguruuDev/BeutelBot-Translation-Coverage" Pfad/Zu/Ordner` (Ordner nicht vorher erstellen)
3. `Pfad/Zu/Ordner` kopieren mit Zusatz `/bin/Debug/net9.0/TranslationCoverage`
4. `cd` in den Ordner für TranslationCoverage
5. `dotnet build`
6. `cd` in den Projektordner von RTOSharp
7. Von dort aus: `cd .git/hooks`
8. `nano pre-commit`
9. Folgenden Code eingeben:
   ```bash
   #!bin/bash

   echo "Checking Translation Coverage"
   Pfad/Zu/Ordner/bin/Debug/net9.0/TranslationCoverage
   exit_code=$?

   exit $exit_code
   ```
10. IDE neustarten, Test Commit ausführen.


## Windows

1. Terminal öffnen
2. `git clone "https://github.com/KaenguruuDev/BeutelBot-Translation-Coverage" Pfad/Zu/Ordner` (Ordner nicht vorher erstellen)
3. `Pfad/Zu/Ordner` kopieren mit Zusatz `/bin/Debug/net9.0/TranslationCoverage`
4. `cd` in den Ordner für TranslationCoverage
5. `dotnet build`
6. Terminal schließen, Explorer öffnen im Pfad für RTOSharp
7. Von dort aus: ".git/hooks" (Ggf. "versteckte Ordner anzeigen" aktivieren)
8. Neue Datei "pre-commit" (Keine Dateiendung) öffnen
9. Folgenden Code eingeben:
   ```bash
   #!bin/bash

   echo "Checking Translation Coverage"
   Pfad/Zu/Ordner/bin/Debug/net9.0/TranslationCoverage
   exit_code=$?

   exit $exit_code
   ```
10. Datei speichern, IDE neustarten, Test Commit ausführen.
