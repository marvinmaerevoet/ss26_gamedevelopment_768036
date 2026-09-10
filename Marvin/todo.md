TODO:

Demo Using AI Approach done (polish that)

Demo using git amend approach
Demo Using unity built in behavior Tree package
Demo Using Behavior Designer Pro
Demo Using Game Creator 2


Okay dann soll er jetzt mal machen dass es so aussieht als würde man die kiste durch die gegend tragen, bevor du einen prompt schreibst gib mir ideen wie du das sinnvoll zu generieren findest, ich stell mir das nicht so leicht vor codex zu sagen er soll jetzt eine animation bauen und die sieht am ende nicht kacke aus. 



Klar. Ich würde daraus eine **Master-Checkliste** machen, die wir anschließend Punkt für Punkt abarbeiten und jeweils entscheiden: **Fix / später / bewusst so lassen**.

## Master-Checkliste vor den weiteren Behavior-Tree-Ansätzen

### Phase 1 – unbedingt vor Ansatz 2

* [ ] **1. Selector-/Reset-Nebenwirkung beheben**
  Prüfen, dass ein alter/inaktiver BT-Zweig nicht Movement eines neu aktivierten Zweigs stoppen kann. Besonders `BTSelector` ↔ `A_ArrestPlayer.Reset()`. 

* [ ] **2. Arrest-Lifecycle vollständig BT-neutral machen**
  Approach, Commit, Hold, Release, Cancel und Reset aus `A_ArrestPlayer` herausziehen bzw. vollständig über gemeinsame Police-API abwickeln. Der BT-Node soll nur noch starten, Status lesen und auf `BTStatus` abbilden. 

* [ ] **3. Shared Police Reset vervollständigen**
  Gemeinsame Arrest-/Movement-Zustände dürfen nicht nur deshalb sauber werden, weil gerade der Custom-BT-Node seinen `Reset()` ausführt. 

* [ ] **4. Vision `obstacleMask` korrekt konfigurieren**
  Gebäude müssen Sicht blockieren; alle vier Sheriffs müssen dieselbe Perception-Basis haben. Aktuell ist die Maske leer. 

* [ ] **5. Gemeinsamen Perception-Vertrag definieren**
  Klären, wann `RefreshPerception()` ausgeführt wird und dafür sorgen, dass jeder BT-Ansatz denselben Snapshot verarbeitet. Keine mehrfachen Refreshes aus einzelnen Nodes. 

* [ ] **6. Chase-bei-Drop-Regel festlegen**
  Entscheiden: Wenn der Player während Chase die MissionCrate fallen lässt, endet Chase sofort oder verfolgt der Sheriff weiter? Danach für alle fünf Ansätze verbindlich machen. 

* [ ] **7. LastKnownPosition-Regel festlegen**
  Entscheiden, ob LKP „letzte Position jedes sichtbaren Players“ oder „letzte relevante Position eines Verdächtigen“ bedeutet. Für unsere Schmuggel-Demo würde ich letzteres bevorzugen. 

* [ ] **8. Arrest-vs.-Delivery-Regel festlegen**
  Verhindern, dass Delivery und Arrest gleichzeitig erfolgreich werden können. Klare Priorität definieren. 

* [ ] **9. Facing / LookAtPlayer BT-neutral machen**
  Keine direkte Sheriff-Rotation aus Custom-Nodes mehr. Rotation über gemeinsame Police-API. 

* [ ] **10. Investigate / LookAround BT-neutral machen**
  Lookaround darf nicht von der Tickrate des jeweiligen BT-Systems abhängen. Gemeinsame zeitbasierte Operation + eindeutige Investigate-Phase. 

* [ ] **11. Animator-Generator absichern**
  `PoliceAnimatorControllerSetup` darf bestehende funktionierende Controller nicht einfach löschen und mit einem veralteten Stand ersetzen. 

* [ ] **12. Feature-Parity-Vertrag dokumentieren**
  Eine verbindliche Spezifikation für alle fünf Ansätze schreiben: Patrol, Suspicion, Chase, Investigate, Arrest, Reset, Multi-Sheriff usw. 

* [ ] **13. Veraltete README-/BT-Dokumentation korrigieren**
  Running/Restricted Area als alte Suspicion-Gründe, alte Blackboard-Felder und alte Setup-Anweisungen entfernen bzw. aktualisieren. 

---

### Phase 2 – danach sinnvoll bereinigen

* [ ] **14. `PoliceBehaviorMode` Ownership bereinigen**
  Mode nur fachlich setzen; Debug-/Runner-Auswertung darf ihn nicht zurück ins Blackboard schreiben. 

* [ ] **15. Arrest-Approach Failure/Timeout definieren**
  Kein endloses `Running`, wenn keine gültige/erreichbare Arrest-Position gefunden wird. 

* [ ] **16. `TrySetDestination()` korrigieren**
  Tatsächlichen Rückgabewert von `NavMeshAgent.SetDestination()` verwenden. 

* [ ] **17. Movement-Ownership sauber definieren**
  Klären, welche gemeinsame Komponente Destination, Stop, Path Reset, Velocity und Rotation kontrolliert. BT-Nodes sollen NavMesh-Details möglichst nicht direkt anfassen. 

* [ ] **18. Emergency-Movement festlegen**
  Emergency darf nicht zufällig Walk/Run vom vorherigen Verhalten erben. 

* [ ] **19. Disable-/Cancel-Lifecycle ergänzen**
  Runner und ArrestSequence sollen beim Deaktivieren laufende Operationen definiert abbrechen. 

* [ ] **20. Carry während Arrest sperren**
  `E` darf während laufender Festnahme nicht noch Pickup/Drop auslösen. 

* [ ] **21. Player-Speed-Reporting stabilisieren**
  Update-Reihenfolgeabhängigkeit von `movementReportedThisFrame` beseitigen. 

* [ ] **22. Alle Sheriffs über denselben Reset-Pfad behandeln**
  Original-Sheriff und `_01–03` vereinheitlichen. 

* [ ] **23. Inspector-/Reference-Validation verbessern**
  Fallbacks dürfen bleiben, aber fehlende Required References sollen klar erkannt werden, statt automatisch halb repariert zu werden. 

* [ ] **24. Setup-Helfer neu einordnen**
  `CustomApproachSceneSetup` klar als Bootstrap/Authoring-Utility kennzeichnen, nicht als vollständigen Rebuilder der heutigen Demo. 

* [ ] **25. Perception nur einmal pro Entscheidungszyklus berechnen**
  Snapshot statt mehrfacher Aktualisierung aus Runner und Nodes. 

* [ ] **26. AnimationDriver kleine Caches prüfen**
  Wiederholte `GetComponent`- und Animator-Parameter-Abfragen reduzieren. Kein Performance-Großprojekt. 

* [ ] **27. Drei defekte Environment-MeshCollider prüfen/fixen**
  Nur eigene Scene-Overrides oder Ersatz-Collider; keine Synty-Vendor-Dateien verändern. 

* [ ] **28. Gemeinsames Sheriff-Prefab bewerten**
  Prüfen, ob ein eigenes neutrales Sheriff-Basis-Prefab Konfigurationsdrift zwischen vier Sheriffs verhindert. Nicht zwingend. 

* [ ] **29. Synty Vendor-Patch dokumentieren**
  Anpassung in `PropBoneToolEditorUtil.cs` dokumentieren, da sie bei Vendor-Updates verloren gehen kann. 

* [ ] **30. Build-Szene festlegen**
  `BehaviorTreeDemo.unity` vor Abgabe in Build Settings aufnehmen. 

* [ ] **31. Kleine Vertragstests überlegen**
  Besonders: Selector-Branch-Wechsel, Arrest-Cancel, Reset, Patrol-Auswahl, Arrest-vs.-Delivery und jeder Decision-Adapter. 

---

### Phase 3 – Naming, Struktur und Cleanup

* [ ] **32. Shared Namespaces `CustomApproachDemo.*` → `BehaviorTreeDemo.*`**

* [ ] **33. Custom-BT Namespace → `BehaviorTreeDemo.AI.CustomApproach.*`**

* [ ] **34. `CustomApproachDemoReset` → neutraler Name, z. B. `DemoReset`**

* [ ] **35. `VisionLightCone` → `VisionLightOrigin`**

* [ ] **36. `CrateReset` → z. B. `ArrestCrateSpawn`**

* [ ] **37. `CA_ReadableNight_*` Legacy-Präfix entfernen**

* [ ] **38. alte `Custom Approach Demo` Logs/Menu-Namen neutralisieren**

* [ ] **39. Naming Convention endgültig festlegen**
  Shared: `BehaviorTreeDemo.<Bereich>`; Adapter: `BehaviorTreeDemo.AI.<Ansatz>`. Bestehende `A_`/`C_` Nodes können bis zu einer bewussten Gesamtrunde bleiben. 

* [ ] **40. Leere/Legacy-Ordner entfernen**
  Beispielsweise leerer `AI/CustomApproach/Editor`, sofern wirklich leer. 

* [ ] **41. Runtime-State und Inspector-Konfiguration langfristig trennen**
  Blackboard-/Debug-Felder nicht unnötig öffentlich/serialisiert halten. 

* [ ] **42. Dead Code einzeln prüfen**
  `movingThreshold`, `CancelArrestApproach`, `PoliceBehaviorMode.Suspicion`, Diagnoseflags, alter Animator-Pfadfilter usw. Nicht blind löschen. 

* [ ] **43. Notification-Code auf Duplikation prüfen**
  Eventuell gemeinsamen Fade/View-Helper machen – aber nur wenn es wirklich einfacher wird. 

* [ ] **44. Scene-Hierarchy moderat ordnen**
  Etwa `Actors`, `MissionMarkers`, `Systems`, ggf. `PoliceMarkers`. Vorsichtig mit World Transforms. 

* [ ] **45. Kamera-Hindernismaske prüfen**

* [ ] **46. Vision-Spotlight-Shadows bewusst entscheiden**

* [ ] **47. vermutlich ungenutztes Global Volume prüfen** 

* [ ] **48. Zwei asmdefs später bewerten**
  `BehaviorTreeDemo.Core` + `BehaviorTreeDemo.CustomApproach`, damit Core compile-time nicht von Custom abhängen kann. 

* [ ] **49. Optional gemeinsames Police-Tuning-Asset**
  Nur falls wir für alle fünf Ansätze dieselben Parameter zentral und reproduzierbar halten wollen. 

* [ ] **50. Optional deterministischer Patrol-Random-Seed**
  Für wirklich reproduzierbare Vergleichsläufe. 

---

### Bewusst erstmal nicht anfassen

* [ ] **51. Environment-Missing-Avatars beobachten, aber nicht reparieren**, solange kein sichtbares Problem entsteht. 
* [ ] **52. Patrol-/SafePoint-Höhen lassen**, solange Navigation funktioniert. 
* [ ] **53. Kein EventSystem hinzufügen**, solange UI nicht interaktiv wird. 
* [ ] **54. Kein DI-Framework / Service Locator / Event Bus**
* [ ] **55. `PoliceAIContext` nicht künstlich in sechs Services zerlegen**
* [ ] **56. `PoliceDecisionController` als kleinen neutralen Vertrag behalten**
* [ ] **57. Direkte Inspector-Referenzen grundsätzlich behalten**
* [ ] **58. Zentralen Demo-Reset-Koordinator grundsätzlich behalten**
* [ ] **59. Polling für Perception, Movement und Animation grundsätzlich behalten**
* [ ] **60. GameCreator-Warnung/Vendor-Code unangetastet lassen** 

### Reihenfolge für die Prompts

Ich würde daraus **nicht 60 einzelne Prompts** machen. Sinnvolle Arbeitspakete wären ungefähr:

**Prompt 1:** Selector/Movement-Cancel
**Prompt 2:** kompletter neutraler Arrest-Lifecycle + Reset
**Prompt 3:** Perception + obstacleMask + LKP + Chase-Drop-Regel
**Prompt 4:** Investigate/Facing/Rotation neutralisieren
**Prompt 5:** Arrest-vs.-Delivery + Carry während Arrest
**Prompt 6:** Animator-Generator + Setup-Helfer absichern
**Prompt 7:** Movement-/Lifecycle-Kleinkram
**Prompt 8:** Scene-/Environment-Korrekturen
**Prompt 9:** Naming + Namespaces + Hierarchy/Dead Code
**Prompt 10:** Docs + Feature-Parity-Contract + finale Austauschbarkeitsprüfung

Damit hätten wir eine kontrollierte Refactor-Serie statt eines riesigen riskanten Umbau-Prompts.
