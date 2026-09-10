# Behavior Tree Demo

Die Demo ist das gemeinsame Testspiel fuer mehrere austauschbare Police-Decision-Adapter. Die aktuelle Szene liegt unter `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity` und verwendet derzeit den eigenen Adapter unter `Scripts/AI/CustomApproach/`.

## Projektstand

- Unity: `6000.4.5f1`
- Render Pipeline: Built-in Render Pipeline
- Hauptszene: `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity`
- Build Settings: Die Szenenliste ist derzeit leer. Vor einer Build-Abgabe muss die Hauptszene bewusst hinzugefuegt werden.
- Runtime-Tests: Die Refactor-Tasks wurden ausschliesslich im Edit Mode kompiliert und geprueft. Der Play-Mode-Smoke-Test bleibt manuell offen.

## Architektur

- `Scripts/Police/`: gemeinsame Wahrnehmung, Bewegung, Investigation, Arrest-Lifecycle, Blackboard und neutraler `PoliceDecisionController`.
- `Scripts/Gameplay/`: Player, Carry, Delivery, Arrest-Praesentation und gemeinsamer Reset.
- `Scripts/Animation/`: gemeinsame Animator-Ansteuerung.
- `Scripts/UI/`: BT-unabhaengige Mission- und Arrest-Notifications.
- `Scripts/AI/CustomApproach/`: eigene BT-Runtime, Nodes, Runner, Diagnose-UI und selektiver Setup-Helfer.

Gemeinsamer Code importiert keine Typen aus `BehaviorTreeDemo.AI.CustomApproach`. Der Reset spricht Adapter nur ueber `PoliceDecisionController.ResetDecisionState()` an. Weitere Adapter muessen denselben fachlichen Vertrag aus [FeatureParityContract.md](Docs/FeatureParityContract.md) erfuellen.

## Aktueller Demo-Ablauf

- Vier Sheriffs patrouillieren unabhaengig und waehlen zufaellige gueltige Patrol Points. Bei mindestens zwei Punkten wird eine direkte Wiederholung vermieden.
- Ein sichtbarer Player ist nur verdaechtig, wenn er die konfigurierte `MissionCrate` traegt. Rennen und `IsInRestrictedArea` sind kein Suspicion-Grund.
- Sichtbar und verdaechtig fuehrt zu Chase. Ein Drop beendet den noch nicht committed Smuggling-Chase.
- Bei Sichtverlust bleibt die letzte relevante Position eines verdaechtigen Players erhalten. Der Sheriff geht dorthin und schaut sich ungefaehr zwei Sekunden um.
- Arrest hat vor Chase und Investigate Vorrang. Der gemeinsame Police-Layer koordiniert Approach, `1.4 m ± 0.15 m` Standdistanz, Commit, Hold, Release und Cancel.
- Delivery und Arrest sind gegenseitig abgesichert. Eine abgeschlossene Delivery sperrt erneuten Pickup.
- `R` setzt Player, MissionCrate, Delivery, UI, Fade, Arrest-Sequenz, Police-State und jeden Decision-Adapter zurueck.

## Steuerung

- Bewegung: `WASD` oder Pfeiltasten
- Sprint: `Left Shift`, solange keine Kiste getragen wird
- Kiste aufnehmen/ablegen: `E`
- Demo-Reset: `R`

## Animation und UI

`Player.controller`, `Sheriff.controller` und `Lydia.controller` sind handgepflegte Demo-Assets. `BasicAnimationDriver` verwendet beim Sheriff `NavMeshAgent.velocity.magnitude` und gemeinsame Police-Zustaende; beim Player verwendet er Player-State und Eingabe. Arrest-, Investigate- und Release-Praesentation bleiben BT-unabhaengig.

Der Menuepunkt `Tools/Behavior Tree Demo/Bootstrap Animator Controllers (Empty Demo Only)` verweigert das Ueberschreiben vorhandener Controller. `CustomApproachSceneSetup` ist ebenfalls nur ein selektiver Authoring-Helfer fuer leere oder kleine Test-Setups und kein Rebuilder der Hauptszene.

## Blackboard und Adapter-Debug

`PoliceBlackboard` enthaelt nur gemeinsamen Police-State: Player-Referenz, Perception-Snapshot, relevante letzte Position, Arrest-Reichweite, Emergency-/Backup-State, Patrol-Ziel und neutralen `PoliceBehaviorMode`.

`CurrentNodeName`, `LastTreeStatus`, Tick-Pfad und andere BT-Diagnosen liegen ausschliesslich am `PoliceBehaviorTreeRunner` und werden von `CustomApproachBehaviorTreeDebugUI` gelesen.

## Bekannte Integrationshinweise

- Die drei Controller-losen Vendor-Animatoren auf zwei Rope-Objekten und dem Trainstation-Platform-Objekt bleiben unangetastet; es gibt keinen nachgewiesenen Einfluss auf eigene Runtime-Objekte.
- `Assets/ThirdParty/Synty/Tools/SyntyPropBoneTool/Editor/Utils/PropBoneToolEditorUtil.cs` enthaelt den projektlokalen Default-Pfad `Assets/ThirdParty/Synty/Tools/SyntyPropBoneTool/Configs/`. Ein Vendor-Update kann diese Pfadanpassung ueberschreiben.
- Die bekannte Game-Creator-`SerializeReference`-Warnung ist ein Vendor-Hinweis und wird nicht als eigener Projektfehler behandelt.
- Weitere Scene- und Environment-Befunde stehen in [SceneIntegrationNotes.md](Docs/SceneIntegrationNotes.md).

## Manuelle Abnahme

Vor dem zweiten Adapter sollten mindestens diese Play-Mode-Pfade geprueft werden:

1. Patrol mit vier Sheriffs, inklusive Reset waehrend Bewegung.
2. Player ohne Kiste beim Gehen und Rennen: keine Verfolgung.
3. Pickup, Chase, Drop und anschliessende Investigation.
4. Arrest-Approach, Animationen, Fade, Jail, Crate-Reset und erneutes Spielen nach `R`.
5. Delivery, Success-Notification und erneutes Spielen nach `R`.
6. Kamera-Kollision an Gebaeuden sowie Sichtblockierung durch Weltgeometrie.

Der vollstaendige kleine Testkatalog und die Austauschbarkeitsanalyse stehen in [FeatureParityContract.md](Docs/FeatureParityContract.md) und [ReplaceabilityAudit.md](Docs/ReplaceabilityAudit.md).
