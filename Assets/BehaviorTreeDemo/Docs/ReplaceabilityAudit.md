# Final Replaceability Audit

## Ergebnis

**Behavior Tree Replaceability: 9 / 10**

Der gemeinsame Code besitzt keine Source-Abhaengigkeit auf `BehaviorTreeDemo.AI.CustomApproach` oder `BehaviorTreeDemo.AI.GitAmend`. Police-Perception, Suspicion, Patrol-Auswahl, Bewegung, Investigation, Arrest-Lifecycle, Gameplay, Animation, UI und Reset bleiben beim Austausch des Decision-Adapters erhalten. Der Reset kennt nur die abstrakte `PoliceDecisionController`-API und die Szene verwendet pro Sheriff einen neutralen `PoliceDecisionHost`.

Die verbleibenden Punkte liegen vor allem in der fehlenden Assembly-Grenze: Ohne `.asmdef` erzwingt der Compiler die Richtung Shared → Adapter nicht dauerhaft. Die Hauptszene enthaelt konkrete Adapter-Komponenten, waehrend Shared Gameplay und Reset nur den neutralen Host referenzieren.

Die Hauptszene ist vollstaendig verdrahtet. Ein separates Custom-Approach-Setup-MonoBehaviour oder Bootstrap-Script ist fuer Runtime und Authoring nicht erforderlich.

## Wenn `Scripts/AI/CustomApproach/` entfernt wird

### Compilation

- Gemeinsame Dateien unter `Scripts/Police`, `Scripts/Gameplay`, `Scripts/Animation` und `Scripts/UI` kompilieren ohne Custom-Typen weiter.
- Es gibt keine Shared-`using`-Direktive und kein gemeinsames Feld vom Typ `PoliceBehaviorTreeRunner`, `BTNode` oder `BTStatus`.
- Die Custom-Dokumentation und Scene enthalten danach fehlende Adapter-Referenzen, bis ein Ersatz verdrahtet ist.

### Erforderliche Scene-Neuverdrahtung

- Die vier Custom-Slots an den `PoliceDecisionHost`-Komponenten entfernen oder durch neue Adapter-Slots ersetzen.
- `CustomApproachBehaviorTreeDebugUI` entfernen; Host- und `DemoReset`-Referenzen bleiben bestehen.
- `PoliceAIContext`, `PoliceBlackboard`, NavMeshAgent, Animator, Vision Light, Player-, Mission- und UI-Referenzen bleiben bestehen.

### Noch Custom-spezifische Entscheidungen

- Aufbau und Prioritaet des konkreten Trees.
- Tick-Scheduling, Running-Child-/Reset-Semantik und Tree-Debugdaten.
- Die duennen Condition-/Action-Adapter und die Uebersetzung neutraler Movement-/Arrest-Statuswerte in `BTStatus`.
- Das Setzen des neutralen `PoliceBehaviorMode` durch die aktiven Custom-Branches.
- Emergency-Branch-Komposition und der Zeitpunkt von `BackupCalled`.

Diese Punkte sind Decision-Logik oder Adapter-Praesentation. Die fachlichen Operationen selbst liegen im gemeinsamen Layer und sind im [FeatureParityContract.md](FeatureParityContract.md) festgelegt.

## Reset und Presentation

- `DemoReset` ruft fuer jeden Sheriff den `PoliceDecisionHost` auf. Der Host delegiert `ResetDecisionState()` an den aktiven lokalen Adapter.
- Arrest-UI, Fade/Jail, Release-UI, Delivery, Carry und Mission-Success kennen keinen Custom-BT-Typ.
- `BasicAnimationDriver` liest `PoliceBlackboard`, `PoliceAIContext` und `DemoPlayerState`, nicht den Runner.
- Das Custom-Debug-UI liest Node-Name und Tree-Status direkt vom Runner. Das getrennte GitAmend-Debug-UI liest nur dessen echte Tree-Struktur und die bereits am Adapter vorhandenen Diagnosewerte.

## Build- und Repository-Befunde

- Unity-Version: `6000.4.5f1` (`cc83ebd631f8`).
- `ProjectSettings/EditorBuildSettings.asset` enthaelt `Assets/BehaviorTreeDemo/Scenes/BehaviorTreeDemo.unity` als einzige aktive Build-Szene.
- Getrackte Recovery-, Temp-, Logs-, Obj- und Library-Pfade gehoeren nicht zur Demo-Baseline. Der jeweils lokale Working-Tree-Zustand ist kein Bestandteil dieser Austauschbarkeitsbewertung.
- `Marvin/` bleibt ausserhalb des Demo-Refactor-Scopes.
- Der projektlokale Synty-PropBone-Config-Pfad in `PropBoneToolEditorUtil.cs` muss nach Vendor-Updates kontrolliert werden.
- Die bekannten drei Vendor-Animatoren ohne Controller und die Game-Creator-`SerializeReference`-Warnung bleiben dokumentierte Vendor-Befunde.
- Die Scene-YAML enthaelt weiterhin historische `m_EditorClassIdentifier`-Texte aus den alten Namespaces. Unity loest die Komponenten ueber unveraenderte MonoScript-GUIDs korrekt auf; der Edit-Mode-Audit fand keine Missing Scripts. Diese Texte wurden nicht blind in YAML bearbeitet.

## Verbleibende Validierung

Es gibt keinen Compile- oder Referenzblocker. CustomApproach und GitAmend sind gemeinsam verdrahtet; vor einem belastbaren Laufzeitvergleich sind die manuellen Tests aus dem Feature-Parity-Vertrag erforderlich. Eine spaetere `.asmdef`-Trennung waere der sinnvollste naechste Architektur-Schritt, falls die Shared→Adapter-Grenze compile-time erzwungen werden soll.
