# Von Ubuntu auf die Xbox Series S

## 1. Windows-Build auf GitHub

Du brauchst kein eigenes Windows. GitHub Actions nutzt für diesen Workflow einen Windows-2022-Server mit UWP-Werkzeugen. Öffne im Repository **Actions → Xbox-Paket bauen → Run workflow**. Auch Änderungen am Projekt auf main/master starten den Build. Der erste Build kann mehrere Minuten dauern.

Erst ein grüner Lauf bedeutet, dass die App kompiliert und verpackt wurde. Bei rotem Lauf das Buildprotokoll prüfen; es gibt dann kein zugesagtes Installationspaket. Ein erfolgreicher Build ist noch kein Xbox-Hardwaretest.

Unten im erfolgreichen Lauf unter **Artifacts** `VaceHub-Xbox-x64` herunterladen (bei GitHub anmelden). Die ZIP unter Ubuntu vollständig entpacken. **Nicht** die Quellcode-ZIP von der Code-Schaltfläche verwenden.

## 2. Xbox vorbereiten

Auf der Series S die offizielle Xbox Dev Mode-App verwenden, den dort angegebenen Aktivierungsprozess abschließen und in den Entwicklermodus wechseln. Kontoanforderungen und Bedingungen bei Microsoft prüfen. In Dev Home Remotezugriff/Device Portal aktivieren, Zugangsdaten festlegen und die angezeigte Adresse ablesen. Ubuntu-PC und Xbox müssen sich im gleichen lokalen Netzwerk erreichen können.

## 3. Paket übertragen

1. Die in Dev Home angezeigte Device-Portal-Adresse im Ubuntu-Browser öffnen. Das lokale Portal kann ein selbstsigniertes Zertifikat nutzen; Konsole und Adresse selbst überprüfen, bevor du fortfährst.
2. Mit den selbst eingerichteten Zugangsdaten anmelden.
3. Unter **Apps / My games & apps → Add / Install** die erzeugte `VaceHubLauncher…appx` auswählen (Bezeichnungen können variieren). Nicht eine Abhängigkeit als Hauptpaket wählen.
4. Wenn nach Abhängigkeiten gefragt wird, die mitgelieferten passenden **x64**-Pakete aus `Dependencies/x64` hinzufügen. Falls keine x64-Abhängigkeiten vorhanden sind und die Installation fehlende Frameworks meldet: Fehlermeldung sichern und Build prüfen, keine zufälligen Downloads verwenden.
5. Installation abschließen und VaceHub in Dev Home starten. Eine `.cer` ist ein öffentliches Testzertifikat, keine App. Falls das Portal ein Zertifikat verlangt, die mitgelieferte CER verwenden; unter Ubuntu muss sie nicht systemweit installiert werden.

Kein Dev Mode? Dann lässt sich dieses eigene UWP-Paket nicht einfach per USB oder als Retail-App installieren. Integrierte Emulatoren fehlen weiterhin; diese Version verwaltet deine Sammlung.

## Signierung und spätere Updates

Ohne zusätzliche Einrichtung erzeugt jeder GitHub-Lauf einen eigenen kurzlebigen Entwicklungs-Schlüssel, signiert das Paket und veröffentlicht nur das öffentliche Zertifikat. Der private Schlüssel wird nicht hochgeladen. Das genügt für einen ersten Testbuild; spätere Builds haben ein anderes Zertifikat und können eine Deinstallation/Neuinstallation erfordern. **Vorher die Bibliothek sichern**, da eine Deinstallation App-Daten löschen kann.

Für stabile Updates kann der Repository-Inhaber später zwei GitHub-Actions-Secrets hinterlegen: `VACEHUB_PFX_BASE64` (Base64 eines eigenen Code-Signing-PFX mit Subject `CN=VaceHub`) und `VACEHUB_PFX_PASSWORD` (dessen Passwort). Keine Schlüssel oder Passwörter als normale Repository-Dateien hochladen und nicht im Chat teilen. Die Workflow-Konfiguration unterstützt diese Secrets bereits. Nicht erforderlich, um den ersten Build zu starten.

## Fehler melden

Bei Buildfehlern: Link zum roten Actions-Lauf senden. Bei Installationsfehlern: vollständigen Fehlercode und Meldung nennen. Auf echter Xbox wurde die App noch nicht getestet.

Offizielle Quellen:
- https://github.com/actions/runner-images/blob/main/images/windows/Windows2022-Readme.md
- https://docs.github.com/en/actions/how-tos/manage-workflow-runs/download-workflow-artifacts
- https://learn.microsoft.com/en-us/windows/uwp/apps-for-xbox/development-options
- https://learn.microsoft.com/en-us/windows/uwp/debug-test-perf/device-portal
