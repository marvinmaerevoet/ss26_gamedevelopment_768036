# Behavior Tree Demo

Die Demo ist das gemeinsame Testspiel fuer austauschbare Police-Decision-Adapter. Die aktuelle Szene liegt unter `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity` und enthaelt den eigenen CustomApproach sowie den GitAmend-Ansatz gleichzeitig. Pro Sheriff fuehrt der neutrale Host stets genau einen davon aus.

## Projektstand

- Unity: `6000.4.5f1`
- Render Pipeline: Built-in Render Pipeline
- Hauptszene: `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity`
- Build Settings: `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity` ist als einzige aktive Build-Szene eingetragen.
- Runtime-Tests: Die Refactor-Tasks wurden ausschliesslich im Edit Mode kompiliert und geprueft. Der Play-Mode-Smoke-Test bleibt manuell offen.

## Architektur

- `Scripts/Police/`: gemeinsame Wahrnehmung, Bewegung, Investigation, Arrest-Lifecycle, Blackboard, neutraler `PoliceDecisionController` und `PoliceDecisionHost`.
- `Scripts/Gameplay/`: Player, Carry, Delivery, Arrest-Praesentation und gemeinsamer Reset.
- `Scripts/Animation/`: gemeinsame Animator-Ansteuerung.
- `Scripts/UI/`: BT-unabhaengige Mission- und Arrest-Notifications.
- `Scripts/AI/CustomApproach/`: eigene BT-Runtime, Nodes, Runner und Diagnose-UI.
- `Scripts/AI/GitAmend/`: originalgetreu uebernommene GitAmend-Runtime, Police-Strategies, Adapter und eigenes Diagnose-UI.

Gemeinsamer Code importiert keine Typen aus `BehaviorTreeDemo.AI.CustomApproach` oder `BehaviorTreeDemo.AI.GitAmend`. Der Reset spricht je Sheriff den neutralen Host ueber `PoliceDecisionController.ResetDecisionState()` an; dieser delegiert an den ausgewaehlten Adapter. Weitere Adapter muessen denselben fachlichen Vertrag aus [FeatureParityContract.md](Docs/FeatureParityContract.md) erfuellen.

## Aktueller Demo-Ablauf

- Vier Sheriffs patrouillieren unabhaengig und waehlen zufaellige gueltige Patrol Points. Bei mindestens zwei Punkten wird eine direkte Wiederholung vermieden.
- Ein sichtbarer Player ist nur verdaechtig, wenn er die konfigurierte `MissionCrate` traegt. Rennen und Restricted Areas sind keine Suspicion-Gruende.
- Sichtbar und verdaechtig fuehrt zu Chase. Ein Drop beendet den noch nicht committed Smuggling-Chase.
- Bei Sichtverlust bleibt die letzte relevante Position eines verdaechtigen Players erhalten. Der Sheriff geht dorthin und schaut sich ungefaehr zwei Sekunden um.
- Arrest hat vor Chase und Investigate Vorrang. Der gemeinsame Police-Layer koordiniert Approach, `1.4 m ± 0.15 m` Standdistanz, Commit, Hold, Release und Cancel.
- Delivery und Arrest sind gegenseitig abgesichert. Eine abgeschlossene Delivery sperrt erneuten Pickup.
- `R` setzt Player, MissionCrate, Delivery, UI, Fade, Arrest-Sequenz, Police-State und jeden Decision-Adapter zurueck.

## Steuerung

- Bewegung: `WASD` oder Pfeiltasten
- Sprint: `Left Shift`, solange keine Kiste getragen wird
- Kiste aufnehmen/ablegen: `E`
- Custom Approach frisch starten: `F1`
- GitAmend Approach frisch starten: `F2`
- Sheriff im jeweils aktiven Debug UI auswaehlen: `1` bis `4`
- Demo-Reset: `R`

`F1` und `F2` speichern die neutrale Auswahl nur fuer den laufenden App-/Play-Start und laden die Hauptszene vollstaendig neu. Auch die Taste des bereits aktiven Ansatzes laedt neu. `R` behaelt die Auswahl bei und setzt den aktuell aktiven Ansatz ueber dessen Host zurueck.

## Animation und UI

`Player.controller`, `Sheriff.controller` und `Lydia.controller` sind handgepflegte Demo-Assets. `BasicAnimationDriver` verwendet beim Sheriff `NavMeshAgent.velocity.magnitude` und gemeinsame Police-Zustaende; beim Player verwendet er ausschliesslich den vom Movement Controller gemeldeten Player-State. Arrest-, Investigate- und Release-Praesentation bleiben BT-unabhaengig.

Die handgepflegten Animator Controller werden direkt als Projekt-Assets gepflegt und benoetigen keinen Bootstrap-Generator. Die Hauptszene ist vollstaendig verdrahtet und benoetigt kein separates Custom-Approach-Setup-MonoBehaviour.

## Blackboard und Adapter-Debug

`PoliceBlackboard` enthaelt nur gemeinsamen Police-State: Player-Referenz, Perception-Snapshot, relevante letzte Position, Arrest-Reichweite, Emergency-/Backup-State, Patrol-Ziel und neutralen `PoliceBehaviorMode`.

`CurrentNodeName`, `LastTreeStatus`, Tick-Pfad und andere Custom-Diagnosen liegen ausschliesslich am `PoliceBehaviorTreeRunner` und werden von `CustomApproachBehaviorTreeDebugUI` gelesen. Das GitAmend-UI zeigt die echte `Node.children`-Struktur, den letzten Root-Status und Process-Zeitpunkt sowie gemeinsamen Police-State. Es bildet bewusst keinen Custom-Tickpfad oder per-Node-Debugstatus nach.

## Bekannte Integrationshinweise

- Die drei Controller-losen Vendor-Animatoren auf zwei Rope-Objekten und dem Trainstation-Platform-Objekt bleiben unangetastet; es gibt keinen nachgewiesenen Einfluss auf eigene Runtime-Objekte.
- `Assets/ThirdParty/Synty/Tools/SyntyPropBoneTool/Editor/Utils/PropBoneToolEditorUtil.cs` enthaelt den projektlokalen Default-Pfad `Assets/ThirdParty/Synty/Tools/SyntyPropBoneTool/Configs/`. Ein Vendor-Update kann diese Pfadanpassung ueberschreiben.
- Die bekannte Game-Creator-`SerializeReference`-Warnung ist ein Vendor-Hinweis und wird nicht als eigener Projektfehler behandelt.
- Weitere Scene- und Environment-Befunde stehen in [SceneIntegrationNotes.md](Docs/SceneIntegrationNotes.md).

## Manuelle Abnahme

Vor der Abnahme sollten diese Play-Mode-Pfade fuer beide Adapter geprueft werden:

1. Patrol mit vier Sheriffs, inklusive Reset waehrend Bewegung.
2. Player ohne Kiste beim Gehen und Rennen: keine Verfolgung.
3. Pickup, Chase, Drop und anschliessende Investigation.
4. Arrest-Approach, Animationen, Fade, Jail, Crate-Reset und erneutes Spielen nach `R`.
5. Delivery, Success-Notification und erneutes Spielen nach `R`.
6. Wechsel mit `F1`/`F2`, vollstaendiger Scene-Neustart, korrekt sichtbares Debug UI und genau ein aktiver Adapter pro Sheriff.
7. Kamera-Kollision an Gebaeuden sowie Sichtblockierung durch Weltgeometrie.

Der vollstaendige kleine Testkatalog und die Austauschbarkeitsanalyse stehen in [FeatureParityContract.md](Docs/FeatureParityContract.md) und [ReplaceabilityAudit.md](Docs/ReplaceabilityAudit.md).
