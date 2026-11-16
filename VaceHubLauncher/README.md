# VaceHub Launcher

Native C# / XAML UWP-App für einen Xbox-Series-S-Gaming-Hub im Dev Mode.
Stand: 16. November 2026. Vollständiger Projektquelltext, **kein fertig kompiliertes oder auf Xbox getestetes Installationspaket**. Windows-SDK und Visual Studio fehlen auf dem Erstellungsrechner (Linux).

## Was funktioniert / was fehlt

- Eigene Logos: V.png als Markenbild, LaunscherBG.png als Dashboard-Hintergrund, App Logo.png als Paketlogos. Originaldateien bleiben unverändert im übergeordneten logos-Ordner.
- Dunkle, TV-orientierte Oberfläche mit grüner Fokusmarkierung und 48-Pixel-Seitenabstand.
- Spiele manuell erfassen: Titel, System und Notizen, Favoriten setzen, suchen, nach System filtern, Details anzeigen und Einträge nach Bestätigung löschen.
- Persistente lokale JSON-Bibliothek; Schreiben über temporäre Datei und Ersetzen. Beschädigte Bibliotheken sperren das Speichern, statt das Original zu überschreiben.
- PS1, PS2, PSP, N64, GB/GBC/GBA, NES, SNES, Mega Drive, Dreamcast und Original-Xbox als Sammlungskategorien.
- **Keine Emulatoren integriert, kein ROM-Import, kein Emulator-Download, kein ROM-/BIOS-Paket, kein Spielstart.** Die Oberfläche sagt das ausdrücklich. Ein Kategorieeintrag ist keine Zusage für die Emulierbarkeit eines Systems.
- Kein Zugriff auf installierte Xbox-Retailspiele, kein Ersatz für das Xbox-Systemdashboard, keine Xbox-360/Series-S-Emulation.
- Kein automatischer Start fremder EXE-Dateien. UWP ist isoliert. Ein späterer Start anderer Apps benötigt deren dokumentiertes, tatsächlich unterstütztes Aktivierungsprotokoll und Xbox-Tests. Ein pauschales „ROM an irgendeinen Emulator senden“ ist nicht implementiert.

## Öffnen und bauen (Windows-PC erforderlich)

1. Projektordner auf einen Windows-10/11-PC kopieren.
2. Visual Studio 2022 mit UWP-C#-Werkzeugen installieren. Je nach Installer ist UWP unter „Universal Windows Platform development“ oder als UWP-Komponente der Windows-Entwicklung aufgeführt. Windows 10 SDK **10.0.19041.0**, UWP .NET- und .NET-Native-Werkzeuge installieren. Keine WinUI-3-/Windows-App-SDK-App erstellen: Diese Vorlage ist klassisches UWP.
3. `VaceHubLauncher.csproj` in Visual Studio öffnen. NuGet-Wiederherstellung erlauben (`Microsoft.NETCore.UniversalWindowsPlatform` 6.2.14). Falls ein anderer SDK installiert ist: 19041 nachinstallieren oder das Ziel-SDK bewusst über Projekteigenschaften anpassen.
4. **Debug / x64** auswählen. Unter Windows Entwicklermodus einschalten. Zunächst mit „Lokaler Computer“ starten.
5. Optional in einer Developer PowerShell für VS: `msbuild VaceHubLauncher.csproj /restore /p:Configuration=Debug /p:Platform=x64`. Das baut, installiert aber nicht automatisch.
6. Für Release zusätzlich .NET Native prüfen. Im Projekt ist Paketsignierung anfangs deaktiviert, damit kein privater Schlüssel mitgeliefert wird. Für ein weiterzugebendes Paket unter Projekteigenschaften → Verpackung ein eigenes Testzertifikat erzeugen und Signierung aktivieren. Der Publisher muss zum Manifest `CN=VaceHub` passen; alternativ beide auf den eigenen Publisher ändern. Keine Zertifikate Dritter übernehmen.

## Xbox Series S / Dev Mode

1. Auf der Konsole die offizielle **Xbox Dev Mode**-App verwenden und deren aktuellen Aktivierungsprozess durchlaufen. Microsoft-Entwicklerkonto/Registrierung und eine unterstützte Systemversion können erforderlich sein; Verfügbarkeit und Kontobedingungen direkt bei Microsoft prüfen.
2. In den Entwicklermodus wechseln. PC und Xbox ins gleiche vertrauenswürdige lokale Netz bringen. In Dev Home Remotezugriff/Device Portal aktivieren und die dort angezeigte Adresse und Zugangsdaten verwenden.
3. In Visual Studio unter Debuggen den **Remotecomputer** als Ziel wählen, Xbox-IP eintragen und Xbox-/Universal-Authentifizierung gemäß dem angebotenen Dialog konfigurieren. Falls verlangt, die PIN aus Dev Home zur Kopplung nutzen. **x64**, nicht ARM oder x86.
4. Zunächst Debug bereitstellen und auf der Konsole öffnen. Alternativ über Visual Studios „App-Pakete erstellen“ ein Sideload-Paket erzeugen und über den Bereich Apps im Xbox Device Portal installieren; sämtliche vom Build erzeugten x64-Abhängigkeiten mitliefern. Bei Signaturproblemen das eigene Zertifikat und den Publisher prüfen; keine Sicherheitsprüfungen pauschal abschalten.
5. VaceHub in Dev Home starten. Es ist eine App mit Bibliothek, noch kein Emulator. Im normalen Retail-Modus lässt sich das Projekt nicht einfach als EXE von USB installieren. Der Wechsel zum Retail-Modus erfolgt über Dev Home; persönliche Dev-App-Daten beim Wechsel nicht versehentlich löschen.

Microsoft dokumentiert viele Xbox-UWP-Schritte unter „Xbox One“. Das ist eine Entwicklungsreferenz, kein auf dieser Series S durchgeführter Kompatibilitätsnachweis. Aktivierung, Remote-Bereitstellung und Controllerverhalten müssen auf der Zielkonsole bestätigt werden. Das Projekt deklariert Windows.Universal, x64 und mindestens Windows-Version 17763.

## Bedienung

- Steuerkreuz / linker Stick: native UWP-Fokusnavigation. A: fokussierte Aktion. B / Esc: zurück zur gesamten Bibliothek, außer ein Systemdialog ist geöffnet; dort die Abbrechen-/Zurück-Aktion verwenden.
- Tastatur: Tab / Umschalt+Tab, Pfeiltasten und Enter. Maus ebenfalls möglich.
- Kategorien liegen horizontal scrollbar. Bei Fokuswechsel sollte UWP die jeweilige Kategorie sichtbar machen; dies gehört zum Hardwaretest.
- Texteingabe über die vom Betriebssystem bereitgestellte Tastatur oder eine angeschlossene Tastatur.
- „Spiel erfassen“ erstellt ausschließlich Metadaten. Notizen können einen Speicherorthinweis enthalten, verleihen der App aber keinen Dateizugriff.
- Suche gilt zusammen mit Kategorie oder Favoriten. B setzt alle Filter zurück.

## Daten und Sicherung

Bibliothek: `ApplicationData.Current.LocalFolder/library.json`, unter Windows üblicherweise `%LOCALAPPDATA%/Packages/<Paketfamilie>/LocalState/library.json`. Auf Xbox, soweit vom Device Portal angeboten, über dessen App-Dateizugriff erreichbar. Vor Deinstallation sichern; App-Daten können dabei gelöscht werden. Die App braucht keine Netzwerkberechtigung.

Beispiel für das Dateiformat (nur Anleitung, wird nicht als Demo-Spiel geladen):

```json
[{"title":"Mein eigenes Spiel","system":"PS1","notes":"Eigener Dump; externen Emulator separat einrichten","favorite":true}]
```

Bei Ladefehler: App schließen, Original sichern, JSON reparieren (Systemnamen müssen exakt den Kategorien entsprechen) oder die Datei nach Sicherung umbenennen. App neu starten. Eine eventuell zurückgebliebene `library.pending.json` ist nicht die primäre Bibliothek. Externe Bearbeitung nur bei geschlossener App. Es werden keine ROMs verändert.

## Verifikation und Abnahme

Hier geprüft: XML-Dateien parsebar, referenzierte Bilddateien vorhanden, Bildabmessungen passend, Paketidentität und Projektstruktur geprüft. **Nicht durchgeführt:** Windows-Kompilierung, Signierung, Deployment, visuelle Laufzeitprüfung oder Xbox-Hardwaretest. Erst nach diesen Schritten als installierbare Xbox-Version betrachten.

Auf Windows und Xbox prüfen:
1. Start ohne Daten zeigt eine ehrliche leere Bibliothek.
2. Mit Controller alle Kategorien, horizontal verdeckte Schaltflächen, Suchfeld und Dialoge erreichen; Fokus muss sichtbar bleiben.
3. Spiel mit Umlauten erfassen, favorisieren, filtern, suchen; App schließen/öffnen und Daten kontrollieren.
4. Löschen abbrechen und bestätigen; nur Bibliothekseintrag darf verschwinden.
5. B aus Suche/Filter testen, Dialoge abbrechen, Bildschirmtastatur und lange Listen prüfen.
6. Bei geschlossener App beschädigte JSON-Datei einsetzen: Fehler muss sichtbar und Speichern gesperrt bleiben.
7. Auf 720p/1080p-Ausgabe TV-Ränder, Lesbarkeit und Fokus prüfen; gegebenenfalls Layout nachjustieren.

## Technische Grenzen und nächste Ausbaustufe

Für echte Emulation müsste pro System ein lizenzrechtlich passender, UWP-/x64-kompatibler Emulator integriert oder eine belegte Schnittstelle zu einer separat installierten App umgesetzt werden. Das betrifft native Bibliotheken, Dateiberechtigungen, Grafik/Audio, Eingabe, BIOS-Anforderungen und Leistung. Besonders PS2 und Original-Xbox werden hier ausdrücklich nicht als funktionierend zugesagt. Nur rechtmäßig verfügbare Spiele und BIOS-Dateien verwenden.

Eine Website oder Browser-Vorschau bildet diese Xbox-Bereitstellung nicht ab; das gelieferte Projekt ist deshalb eine native UWP-App.

## Offizielle Referenzen

- [Xbox: Entwicklungsoptionen und Einstieg](https://learn.microsoft.com/en-us/windows/uwp/apps-for-xbox/development-options)
- [Xbox-UWP-FAQ, unter anderem x64 statt x86](https://learn.microsoft.com/en-us/previous-versions/windows/uwp/xbox-apps/frequently-asked-questions)
- [UWP: Gamepad- und Fernbedienungsnavigation](https://learn.microsoft.com/en-us/windows/uwp/ui-input/gamepad-and-remote-interactions)
- [Auf Xbox eingeschränkte UWP-Funktionen](https://learn.microsoft.com/en-us/uwp/extension-sdks/uwp-limitations-on-xbox)
- [Windows Device Portal](https://learn.microsoft.com/en-us/windows/uwp/debug-test-perf/device-portal)
- [UWP mit C# in Visual Studio](https://learn.microsoft.com/en-us/visualstudio/get-started/csharp/tutorial-uwp)
