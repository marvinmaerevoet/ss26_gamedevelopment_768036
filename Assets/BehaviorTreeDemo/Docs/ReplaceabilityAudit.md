# Final Replaceability Audit

## Ergebnis

**Behavior Tree Replaceability: 8.5 / 10**

Der gemeinsame Code besitzt keine Source-Abhaengigkeit auf `BehaviorTreeDemo.AI.CustomApproach`. Police-Perception, Suspicion, Patrol-Auswahl, Bewegung, Investigation, Arrest-Lifecycle, Gameplay, Animation, UI und Reset bleiben beim Austausch des Decision-Adapters erhalten. Der Reset kennt nur die abstrakte `PoliceDecisionController`-API.

Die verbleibenden Punkte liegen in der Scene-Verkabelung und in fehlender Assembly-Grenze: Ohne `.asmdef` erzwingt der Compiler die Richtung Shared → Adapter nicht dauerhaft, und die Hauptszene referenziert den konkreten Adapter weiterhin erwartungsgemaess.

## Wenn `Scripts/AI/CustomApproach/` entfernt wird

### Compilation

- Gemeinsame Dateien unter `Scripts/Police`, `Scripts/Gameplay`, `Scripts/Animation` und `Scripts/UI` kompilieren ohne Custom-Typen weiter.
- Es gibt keine Shared-`using`-Direktive und kein gemeinsames Feld vom Typ `PoliceBehaviorTreeRunner`, `BTNode` oder `BTStatus`.
- Die Custom-Dokumentation und Scene enthalten danach fehlende Adapter-Referenzen, bis ein Ersatz verdrahtet ist.

### Erforderliche Scene-Neuverdrahtung

- Vier `PoliceBehaviorTreeRunner`-Komponenten durch je einen neuen `PoliceDecisionController`-Adapter ersetzen.
- Die primäre und die drei zusaetzlichen Decision-Controller-Referenzen am `DemoReset` auf diese vier Komponenten setzen.
- `CustomApproachBehaviorTreeDebugUI` entfernen oder durch ein adaptereigenes Diagnose-UI ersetzen.
- `CustomApproachSceneSetup` entfernen oder durch einen passenden Authoring-Helfer ersetzen; es ist nicht fuer das Runtime-Gameplay erforderlich.
- `PoliceAIContext`, `PoliceBlackboard`, NavMeshAgent, Animator, Vision Light, Player-, Mission- und UI-Referenzen bleiben bestehen.

### Noch Custom-spezifische Entscheidungen

- Aufbau und Prioritaet des konkreten Trees.
- Tick-Scheduling, Running-Child-/Reset-Semantik und Tree-Debugdaten.
- Die duennen Condition-/Action-Adapter und die Uebersetzung neutraler Movement-/Arrest-Statuswerte in `BTStatus`.
- Das Setzen des neutralen `PoliceBehaviorMode` durch die aktiven Custom-Branches.
- Emergency-Branch-Komposition und der Zeitpunkt von `BackupCalled`.

Diese Punkte sind Decision-Logik oder Adapter-Praesentation. Die fachlichen Operationen selbst liegen im gemeinsamen Layer und sind im [FeatureParityContract.md](FeatureParityContract.md) festgelegt.

## Reset und Presentation

- `DemoReset` ruft fuer jeden Sheriff `PoliceDecisionController.ResetDecisionState()` auf; ein neuer Adapter kann dieselbe Scene-Referenz uebernehmen.
- Arrest-UI, Fade/Jail, Release-UI, Delivery, Carry und Mission-Success kennen keinen Custom-BT-Typ.
- `BasicAnimationDriver` liest `PoliceBlackboard`, `PoliceAIContext` und `DemoPlayerState`, nicht den Runner.
- Nur das Custom-Debug-UI liest Node-Name und Tree-Status direkt vom Runner.

## Build- und Repository-Befunde

- Unity-Version: `6000.4.5f1` (`cc83ebd631f8`).
- `ProjectSettings/EditorBuildSettings.asset` enthaelt derzeit keine Scene. Vor einer Build-Abgabe muss `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity` hinzugefuegt werden.
- Es wurden keine getrackten Recovery-, Temp-, Logs-, Obj- oder Library-Pfade und keine unbeabsichtigten ungetrackten Dateien gefunden.
- `Marvin/todo.md` ist eine vorbestehende lokale Aenderung und gehoert nicht zu den Refactor-Commits.
- Der projektlokale Synty-PropBone-Config-Pfad in `PropBoneToolEditorUtil.cs` muss nach Vendor-Updates kontrolliert werden.
- Die bekannten drei Vendor-Animatoren ohne Controller und die Game-Creator-`SerializeReference`-Warnung bleiben dokumentierte Vendor-Befunde.
- Die Scene-YAML enthaelt weiterhin historische `m_EditorClassIdentifier`-Texte aus den alten Namespaces. Unity loest die Komponenten ueber unveraenderte MonoScript-GUIDs korrekt auf; der Edit-Mode-Audit fand keine Missing Scripts. Diese Texte wurden nicht blind in YAML bearbeitet.

## Blocker vor dem zweiten Adapter

Es gibt keinen Compile- oder Referenzblocker. Vor einem belastbaren Laufzeitvergleich sind die manuellen Tests aus dem Feature-Parity-Vertrag erforderlich. Vor einer Build-Abgabe ist ausserdem die Hauptszene in die Build Settings aufzunehmen. Eine spaetere `.asmdef`-Trennung waere der sinnvollste naechste Architektur-Schritt, falls die Shared→Adapter-Grenze compile-time erzwungen werden soll.
