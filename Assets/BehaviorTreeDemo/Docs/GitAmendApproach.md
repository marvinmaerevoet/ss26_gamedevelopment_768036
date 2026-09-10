# GitAmend Approach

## Source

Die lokale Read-only-Referenz liegt unter `gitamend Approach/`. Ihr README nennt die GitAmend-/Adam-Myhre-Tutorials **Behaviour Trees** und **Blackboard Architecture**. Die beigefuegte `LICENSE` stellt den Referenzcode unter die Unlicense. Diese Dokumentation trifft keine darueber hinausgehende Aussage zu Urheberschaft oder Lizenz.

## Original Architecture

Die relevante Behavior-Tree-Runtime der Referenz besteht aus:

- `Assets/_Project/Scripts/BehaviourTrees/Node.cs`
  - `Node` mit verschachteltem `Node.Status` (`Success`, `Failure`, `Running`)
  - `BehaviourTree`
  - `Selector`, `PrioritySelector`, `RandomSelector`, `Sequence`
  - `Inverter`, `UntilFail`
  - `Leaf`
  - `IPolicy` und `Policies`
- `Assets/_Project/Scripts/BehaviourTrees/Strategies.cs`
  - `IStrategy`
  - `ActionStrategy`
  - `Condition`
  - die tutorialspezifischen Strategien `PatrolStrategy` und `MoveToTarget`
- `Assets/_Project/Scripts/Utilities/ListExtensions.cs`
  - Fisher-Yates-Shuffle fuer `RandomSelector`
- `Assets/_Project/Scripts/Hero.cs`
  - baut den Beispielbaum in `Start()` imperativ mit Konstruktoren und `AddChild()` auf
  - ruft `tree.Process()` in jedem `Update()` auf

`Node` speichert seine Children in einer oeffentlichen Liste. `Sequence` und der normale `Selector` merken ihren Fortschritt ueber `currentChild`. Eine `Sequence` verarbeitet pro `Process()` hoechstens ein Child und liefert zwischen zwei erfolgreichen Children `Running`. Ein `Selector` bleibt bei einem laufenden Child; nach einem Fehlschlag wechselt er beim naechsten Aufruf zum folgenden Child.

`PrioritySelector` ist anders: Er sortiert seine Children beim ersten Zugriff nach absteigender Priority und prueft diese sortierte Liste bei jedem `Process()` wieder von vorne. Die sortierte Liste wird bis zum naechsten `Reset()` gecacht. Er besitzt keinen separaten Abort-Mechanismus und setzt beim Wechsel zu einem hoeher priorisierten laufenden Child einen zuvor laufenden niedrigeren Branch nicht automatisch zurueck.

`Leaf` delegiert an ein `IStrategy`. Damit liegen fachlicher Zustand und mehrphasige Actions in der Strategy. Das Interface bietet `Process()` sowie ein standardmaessig leeres `Reset()`. `Node.Reset()` setzt `currentChild` auf null und ruft rekursiv `Reset()` fuer alle Children auf.

`BehaviourTree` ist selbst ein `Node`. Seine Standard-Policy ist `RunForever`. Nach dem Verarbeiten seines aktuellen Root-Child wechselt er zyklisch zum naechsten Tree-Child und liefert weiter `Running`, solange die Policy kein Ende fordert. Das Tutorial nutzt einen einzigen `PrioritySelector` unter dem Tree und tickt ihn framebasiert aus `Hero.Update()`.

Die Referenz enthaelt zusaetzlich ein eigenstaendiges Blackboard-System mit `BlackboardKey`, typisierten `BlackboardEntry<T>`, `Blackboard`, `BlackboardData`, `BlackboardController`, `Arbiter` und `IExpert`. Die BT-Runtime referenziert dieses System nicht direkt. `Hero` bindet Blackboard-Werte ueber Closures in `Condition` und ueber Strategies an den Baum. Der Arbiter ist ein weiteres Tutorialsystem und kein Composite oder Lifecycle-Bestandteil des Behavior Trees.

## Police Tree

Der GitAmend-Adapter baut den Police Tree mit den originalen GitAmend-Composites auf:

```text
BehaviourTree (RunForever)
└── PrioritySelector
    ├── Emergency (Sequence, priority 500)
    ├── Arrest (Sequence, priority 400)
    ├── Chase (Sequence, priority 300)
    ├── Investigate (Sequence, priority 200)
    └── Patrol (Sequence, priority 100)
```

Conditions sind originale `Leaf`-/`Condition`-Kombinationen und lesen nur den bestehenden `PoliceBlackboard`-Snapshot. Mehrphasige Police-Actions implementieren das originale `IStrategy` und delegieren ihre fachliche Arbeit an `PoliceAIContext`.

Der Adapter ruft `BehaviourTree.Process()` wie das Original in jedem `Update()` auf. Shared Perception bleibt davon unabhaengig bei 10 Hz autoritativ; weder Adapter noch Strategy rufen `RefreshPerception()` auf oder fuehren eigene Raycasts aus.

## Important Differences to CustomApproach

| Thema | CustomApproach | GitAmend |
|---|---|---|
| Tree construction | Konstruktoren erhalten Child-Collections | Imperativer Aufbau mit Konstruktoren und `AddChild()` |
| Node lifecycle | `Tick()` ruft geschuetztes `OnTick()` auf und speichert Debugstatus | Virtuelles `Process()` ohne separate Tick-Huelle |
| Status | Eigenstaendiges `BTStatus` | In `Node` verschachteltes `Node.Status` |
| Selector | Wahlweise reaktiv oder Running-Child-basiert; der reaktive Root resettiert gewechselte/lower-priority Branches | Normaler `Selector` merkt `currentChild`; `PrioritySelector` prueft Prioritaeten erneut, besitzt aber keinen Branch-Abort |
| Sequence | Kann mehrere sofort erfolgreiche Children in einem Tick durchlaufen | Verarbeitet pro `Process()` hoechstens ein Child und liefert dazwischen `Running` |
| State sharing | Nodes halten Context-Referenzen; neutraler `PoliceBlackboard` | Original nutzt Closures/Strategies; Police-Integration schliesst ebenfalls den neutralen Context/Blackboard ein |
| Ticking | Konfigurierbares 0,1-s-Intervall | Originalgetreu framebasiert in `Update()` |
| Reset | Jeder Custom-Node besitzt explizite Reset-Semantik, der reaktive Selector resettiert Branches beim Wechsel | Rekursives `Node.Reset()` plus optionales `IStrategy.Reset()`; kein eigener Abort-Lifecycle |
| Debugging | Node-Event, letzter Status, Tick-Zeit und Pfad | Runtime bietet `PrintTree()`; nur der Adapter merkt letzten Tree-Status und Tick-Zeit |

## Suitability

**YES WITH ADAPTER.**

`Running`, `Success`, `Failure`, zustandsbehaftete Actions, Conditions, Prioritaeten, Investigation und ein ueber die Jail-Sequenz laufender Arrest lassen sich darstellen. Die Police-spezifischen Strategies ueberbruecken fehlende Composites, ohne die GitAmend-Runtime umzuschreiben:

- Chase kombiniert die gemeinsamen Movement- und Facing-Aufrufe in einer Strategy, weil die Original-Runtime keinen Parallel-Node besitzt.
- Das bestehende 5-s-Investigation-Limit liegt in der Police-Movement-Strategy, weil die Original-Runtime keinen Timeout-Decorator besitzt.
- Die zwei Patrol-Movement-Versuche liegen in der Patrol-Strategy, weil die Original-Runtime keinen Retry-Decorator besitzt.
- Endloses Patrol entsteht durch den originalen `RunForever`-Tree und den Reset des erfolgreichen Priority-Selectors; ein neuer Repeater wurde nicht eingefuehrt.

Die native GitAmend-Eigenschaft, nicht ausgewaehlte laufende Branches nicht aktiv abzubrechen, bleibt erhalten. Neue aktive Police-Strategies setzen ihre Ziele und Modi ueber die gemeinsame API; Arrest und Investigation nutzen deren bestehende Cancel-/Lifecycle-Funktionen.

## Integration Boundary

```text
Shared Police API
        ↑
GitAmendPoliceDecisionController
        ↓
GitAmend BehaviourTree / Node / Leaf / IStrategy
```

`GitAmendPoliceDecisionController` leitet von `PoliceDecisionController` ab, holt den lokalen `PoliceAIContext`, baut den GitAmend-Baum, ruft `Process()` auf und reicht `ResetDecisionState()` an das originale rekursive Tree-Reset weiter. Police-Strategies lesen den vorhandenen Snapshot und rufen ausschliesslich gemeinsame Context-APIs fuer Movement, Facing, Investigation, Arrest und Patrol auf.

Shared Gameplay und Shared Police referenzieren keine GitAmend-Typen.

## Copied and Adapted

| Neue Datei | Kategorie | Herkunft/Anpassung |
|---|---|---|
| `Runtime/Node.cs` | Core Runtime | GitAmend `Node.cs`; Klassen, Felder, Child-Struktur, `Process()`- und Reset-Semantik beibehalten; Namespace angepasst |
| `Runtime/Strategies.cs` | Core Runtime | `IStrategy`, `ActionStrategy` und `Condition` aus GitAmend; Namespace angepasst |
| `Runtime/ListExtensions.cs` | Runtime Helper | GitAmend-Shuffle fuer `RandomSelector`; Namespace angepasst |
| `Nodes/PoliceNodeSupport.cs` | Police Bridge | Neu; validiert lokale Shared-Referenzen und mappt `PoliceMovementStatus` auf `Node.Status` |
| `Nodes/PoliceStrategies.cs` | Police Bridge | Neu; duenne GitAmend-`IStrategy`-Implementierungen ueber der Shared Police API |
| `GitAmendPoliceDecisionController.cs` | Adapter | Neu; verbindet `PoliceDecisionController` mit dem originalen GitAmend-Tree-Lifecycle |
| `Debug/GitAmendBehaviorTreeDebugUI.cs` | Adapter-Diagnose | Neu; liest Tree-Struktur, Adapterstatus und Shared Police-State, ohne den Core zu erweitern |

Nicht uebernommen wurden `Hero`, `Scout`, Input-, Character-, Animation- und Beispielwelt-Code sowie die tutorialspezifischen `PatrolStrategy`- und `MoveToTarget`-Klassen. Das separate Blackboard-/Arbiter-Tutorialsystem wurde ebenfalls nicht kopiert, weil die gemeinsame Demo bereits den autoritativen `PoliceBlackboard` besitzt und die originale BT-Runtime selbst keine direkte Blackboard-Abhaengigkeit hat.

## Architectural Deviations

1. Der Namespace wurde von `Pathfinding.BehaviourTrees` zu `BehaviorTreeDemo.AI.GitAmend.Runtime` geaendert, damit Referenzprojekt, CustomApproach und neue Kopie kollisionsfrei nebeneinander liegen.
2. Der global definierte `ListExtensions`-Helper liegt im projektsicheren Runtime-Namespace.
3. Aus `Strategies.cs` wurden nur die Runtime-Bestandteile `IStrategy`, `ActionStrategy` und `Condition` uebernommen. Die beiden Tutorial-Gameplay-Strategies wurden durch Police-spezifische Strategies ersetzt.
4. Das eigenstaendige GitAmend-Blackboard-/Arbiter-System wurde nicht dupliziert. Die Police-Strategies greifen entsprechend dem originalen Closure-/Strategy-Prinzip auf den vorhandenen Shared State zu.
5. Der neue MonoBehaviour-Adapter erbt vom neutralen `PoliceDecisionController`, validiert den lokalen Context und exponiert letzten Status/Tick-Zeit fuer spaetere Integration.
6. Chase-Facing und Chase-Movement liegen gemeinsam in einer Strategy, weil GitAmend keinen Parallel-Composite anbietet.
7. Investigation-Timeout und Patrol-Retry liegen in ihren Police-Strategies, weil GitAmend keine entsprechenden Decorators anbietet.
8. `OnDisable()` resettiert den GitAmend-Tree und beendet ueber den Shared Context laufende Operationen, damit spaeteres Deaktivieren des Adapters keinen Movement-State hinterlaesst.
9. Der neutrale Scene-Host aktiviert den GitAmend-Adapter anhand einer Approach-ID. Diese Integrationsschicht kennt nur `PoliceDecisionController` und veraendert die GitAmend-Runtime nicht.
10. Das GitAmend-Debug-UI traversiert die bereits oeffentliche `Node.children`-Struktur und zeigt vorhandene Adapter-/Shared-Werte. Es fuegt keinen Active Path, Node-Tick-Event oder Abort-Lifecycle hinzu.

Die GitAmend-Composites, ihr Statusmodell, die `Process()`-Reihenfolge, das Running-Child-Verhalten und das rekursive Reset wurden nicht erweitert oder korrigiert.

## Scene Integration

Alle vier Sheriffs besitzen einen eigenen `GitAmendPoliceDecisionController`, einen eigenen Custom-Runner und einen neutralen `PoliceDecisionHost`. Der Host aktiviert beim Scene-Start nur den durch `BehaviorTreeApproachSelection` gewaehlten lokalen Adapter. CustomApproach ist der Default eines neuen App-/Play-Starts; `F1` und `F2` setzen die Auswahl und laden die Hauptszene vollstaendig neu.

`DemoReset` referenziert die vier Hosts. Ein `R`-Reset behaelt die Auswahl bei und delegiert den Tree-Reset nur an den aktiven Adapter. Das GitAmend-Debug-UI wird nur bei GitAmend aktiviert, cached die vier Controller beim Start und verwendet `1` bis `4` zur Sheriff-Auswahl. Die Hauptszene ist als einzige aktive Build-Szene eingetragen.
