# Custom Approach Demo

## Ziel der Demo

Diese Demo zeigt eine kleine Behavior-Tree-KI fuer NPC-Polizisten in einer Western-Stadt. Der Spieler bewegt sich durch die Szene, Sheriffs patrouillieren, erkennen verdachtig wirkendes Verhalten, verfolgen den Spieler, nehmen ihn in Reichweite fest oder untersuchen seine letzte bekannte Position.

Alles liegt unter `Assets/CustomApproachDemo/` und ist optional. Die Demo veraendert keine Game-Creator-Dateien, Packages, Synty-Prefabs oder Szenen automatisch.

## Warum eine eigene Lightweight-BT-Runtime?

Game Creator 2 Core und Game Creator 2 Behavior Trees sind im Projekt vorhanden. Fuer diese Demo ist eine eigene sehr kleine Runtime robuster, weil sie ohne direkte Abhaengigkeit auf interne Game-Creator-APIs funktioniert, leicht lesbar bleibt und ohne Risiko fuer bestehende Game-Creator-Setups erweitert werden kann.

Die Demo kann neben Game Creator existieren. Sie ersetzt Game Creator nicht, sondern dient als didaktische, isolierte KI-Demo.

## Geeignete Szene und Charaktere

Nutze eine vorhandene Western-Demo-Szene aus den Synty-Assets, zum Beispiel aus `PolygonWestern` oder `PolygonWesternFrontier`. Geeignet sind einfache Humanoid-Charaktere wie Sheriff-, Cowboy- oder Townsfolk-Modelle. Ein Sheriff/NPC braucht fuer die Demo einen `NavMeshAgent`, einen gebackenen NavMesh und die Police-Demo-Komponenten.

## Setup-Schritte

1. Western-Demo-Szene oeffnen.
2. Spieler-Charakter auswaehlen.
3. `DemoPlayerState` oder per ContextMenu `Add Simple Player Controller To Selected` hinzufuegen.
4. Sheriff/NPC auswaehlen.
5. Per ContextMenu `Add Police Components To Selected` ausfuehren.
6. Per ContextMenu `Create Patrol Points Around Selected Police` ausfuehren.
7. Per ContextMenu `Create Restricted Area Trigger` erzeugen und bewusst platzieren.
8. Per ContextMenu `Create BehaviorTree Debug UI` erzeugen.
9. NavMesh pruefen oder backen.
10. Play druecken.

Optional kann `Create Full Demo Helpers For Selected Police` die Police-Komponenten, Patrol Points, Safe Point und Player-Verknuepfung in einem Schritt vorbereiten. Die Restricted Area wird absichtlich nicht automatisch erstellt, weil ihre Position fuer das Verhalten wichtig ist.

## Third-Person-Kamera

Fuer eine spielbare Demo kann die Main Camera den optionalen `DemoThirdPersonCamera` bekommen. Der schnellste Weg:

1. Spieler mit `DemoPlayerState` vorbereiten.
2. GameObject mit `CustomApproachSceneSetup` auswaehlen.
3. Per ContextMenu `Add Third Person Camera To Main Camera` ausfuehren.
4. Play druecken.

Die Kamera sucht automatisch den ersten `DemoPlayerState`, folgt dem Player und schaut auf ihn. Standardmaessig rotiert die Maus dauerhaft um den Player. Wenn `rotateOnlyWhileRightMouseHeld` aktiv ist, rotiert die Kamera nur bei gedrueckter rechter Maustaste. Das Mausrad zoomt zwischen `minDistance` und `maxDistance`.

## Synty Animation Setup

Fuer einfache Synty-Animationen gibt es einen sicheren Demo-Workflow, der nur Assets unter `Assets/CustomApproachDemo/` erzeugt:

1. In der Unity-Menueleiste `Tools/Custom Approach Demo/Create Animator Controllers` ausfuehren.
2. Ein GameObject mit `CustomApproachSceneSetup` auswaehlen und im Component-ContextMenu `Custom Approach Demo/Setup Synty Animations` ausfuehren.

Der erste Schritt sucht Idle-, Walk- und Run-Clips aus `Assets/Synty/AnimationBaseLocomotion` und erstellt `Player.controller` sowie `Sheriff.controller` im Demo-Ordner. Wenn passende Spezialclips gefunden werden, werden zusaetzlich einfache States fuer Investigate, Emergency und Arrest angelegt.

Der zweite Schritt sucht den ersten `DemoPlayerState` und den ersten `PoliceAIContext` in der Szene, verdrahtet `BasicAnimationDriver`, weist die Demo-Controller zu und setzt `Animator.applyRootMotion = false`. Szene-Instanzen werden nur geaendert, wenn du diesen ContextMenu-Punkt manuell ausfuehrst.

`BasicAnimationDriver` nutzt einfache Animator-Parameter und keine Positionsdelta-Speed-Berechnung. Beim Sheriff kommt `Speed` aus `NavMeshAgent.velocity.magnitude`; beim Player werden feste visuelle Werte fuer Gehen und Rennen genutzt.

## NavMesh-Hinweise

- Der Sheriff braucht einen `NavMeshAgent`.
- Der Boden muss Teil des NavMesh sein.
- Das Package `AI Navigation` ist vorhanden.
- Wenn kein `NavMeshSurface` existiert, muss eines in der Szene angelegt und gebacken werden.
- Wenn der Agent beim Start nicht auf dem NavMesh steht, schlagen Bewegungs-Actions fehl.

## Erwarteter Demo-Ablauf

- Der Sheriff startet in `Patrol`.
- Ein normal sichtbarer Spieler wird nicht verfolgt, solange er nicht verdachtig ist.
- Rennen oder Betreten einer Restricted Area setzt `Suspicious`.
- Bei Sichtkontakt und Verdacht wechselt der Tree in `Chase`.
- In Arrest-Reichweite wird `A_ArrestPlayer` ausgefuehrt.
- Bei Sichtverlust bewegt sich der Sheriff zur letzten bekannten Position und schaut sich um.
- Danach wird die letzte bekannte Position geloescht und der Sheriff kehrt zur Patrouille zurueck.

## Gezeigte Behavior-Tree-Features

- Root
- Selector
- Sequence
- Condition
- Action
- Running
- Success/Failure
- Inverter
- Timeout
- Retry
- Repeater
- Parallel
- Blackboard
- Subtrees

## Blackboard

`PoliceBlackboard` speichert die gemeinsamen Demo-Daten, unter anderem Spieler-Referenz, letzte bekannte Spielerposition, Sichtkontakt, Verdachtsstatus, Arrest-Reichweite, Backup-Status, Patrol-Ziel, aktuellen Behavior-Namen, aktuellen Node-Namen und den letzten Tree-Status.

## Troubleshooting

### Sheriff bewegt sich nicht

Pruefe, ob ein `NavMeshAgent` vorhanden ist, der Sheriff auf dem NavMesh steht und ein NavMesh gebacken wurde. Pruefe ausserdem, ob Patrol Points im `PoliceAIContext` gesetzt sind.

### Player wird nicht erkannt

Pruefe `PoliceAIContext.PlayerState`, `PoliceBlackboard.Player`, `EyePoint`, `viewDistance`, `viewAngle` und `obstacleMask`. Eine zu breite Obstacle-Maske kann den Sicht-Ray blockieren.

### Restricted Area funktioniert nicht

Der Trigger braucht einen `BoxCollider` mit `isTrigger = true` und `RestrictedAreaTrigger`. Der Spieler braucht `DemoPlayerState` am Collider-Objekt oder in einem Parent.

### UI bleibt leer

Pruefe, ob `CustomApproachBehaviorTreeDebugUI.target` gesetzt ist oder ein `PoliceBehaviorTreeRunner` in der Szene existiert. Die UI erzeugt Canvas/Text zur Laufzeit, wenn nichts zugewiesen ist.

### Kamera bewegt sich nicht

Pruefe, ob `DemoThirdPersonCamera.target` gesetzt ist oder ein `DemoPlayerState` in der Szene existiert. Pruefe ausserdem, ob die Kamera den Tag `MainCamera` hat, damit der Player Controller kamera-relative Bewegung verwenden kann.

### Cursor locked

`DemoThirdPersonCamera.lockCursorOnPlay` sperrt den Cursor im Play Mode. Deaktiviere das Feld im Inspector, wenn du den Cursor frei behalten willst.

### Sheriff verfolgt immer oder nie

Pruefe `suspiciousIfRunning`, `suspiciousIfInRestrictedArea`, `DemoPlayerState.IsRunning`, `DemoPlayerState.IsInRestrictedArea`, `viewDistance`, `viewAngle` und `arrestRange`.

### Unity Input funktioniert nicht

Die Demo nutzt `UnityEngine.InputSystem.Keyboard.current` fuer den optionalen Fallback-Controller. Pruefe, ob das Input System Package installiert und eine Tastatur im Play Mode verfuegbar ist.

### NavMeshAgent not on NavMesh

Setze den Sheriff auf eine gebackene NavMesh-Flaeche. Falls noetig, verschiebe den NPC leicht ueber den Boden und backe den NavMesh erneut.

### Animation bleibt Idle

Fuehre zuerst `Tools/Custom Approach Demo/Create Animator Controllers` aus und danach `Custom Approach Demo/Setup Synty Animations`. Pruefe, ob `BasicAnimationDriver` am Player und Sheriff vorhanden ist und ob im Animator der passende Demo-Controller zugewiesen wurde.

### Animator Controller fehlt

Die Demo-Controller liegen nach dem Erzeugen unter `Assets/CustomApproachDemo/Animations/`. Wenn sie fehlen, konnte der Editor-Builder keine passenden Idle/Walk/Run-Clips finden oder wurde noch nicht ausgefuehrt.

### Speed bleibt 0

Beim Player muss `DemoPlayerState.CurrentSpeed` ueber `movingThreshold` liegen. Beim Sheriff muss der `NavMeshAgent` tatsaechlich laufen und im `BasicAnimationDriver.agent` Feld referenziert sein.

### Falsche Clips gefunden

Der Editor-Builder bevorzugt masculine Polygon-Clips aus `Assets/Synty/AnimationBaseLocomotion`. Die verwendeten Clip-Pfade werden nach dem Erzeugen in der Console geloggt.
