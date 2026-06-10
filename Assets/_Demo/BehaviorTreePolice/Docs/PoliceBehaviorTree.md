# Police Behavior Tree Demo - Aktuelle Tree-Struktur

## Kurze Erklaerung

Diese Dokumentation beschreibt die aktuell in `PoliceBehaviorTreeRunner.cs` gebaute Behavior-Tree-Struktur. Der Sheriff prueft pro Tick seine Perception, schreibt zentrale Werte ins `PoliceBlackboard` und tickt danach den Tree in einer konfigurierbaren Tickrate.

Der Root ist ein reaktiver Selector (`rememberRunningChild: false`). Dadurch startet die Prioritaetspruefung bei jedem Tick wieder beim ersten Child. Eine laufende Patrol kann also sofort durch Emergency, Arrest, Chase oder Investigate unterbrochen werden.

## Vollstaendige Tree-Struktur

```text
Root Selector (Reactive, rememberRunningChild: false)
├── EmergencyBehavior (Sequence)
│   ├── C_OfficerHealthLow
│   └── Emergency Selector (Selector, memory-based)
│       ├── Call Backup Sequence (Sequence)
│       │   ├── C_NotBackupCalled
│       │   └── A_CallBackup
│       └── A_FleeToSafePoint
│
├── ArrestBehavior (Sequence)
│   ├── C_PlayerVisible
│   ├── C_PlayerSuspicious
│   ├── C_PlayerInArrestRange
│   └── A_ArrestPlayer
│
├── ChaseBehavior (Sequence)
│   ├── C_PlayerVisible
│   ├── C_PlayerSuspicious
│   ├── Not In Arrest Range (Inverter)
│   │   └── C_PlayerInArrestRange
│   └── Chase Parallel (Parallel, requiredChildIndex: 0)
│       ├── A_ChasePlayer        [required/main child]
│       └── A_LookAtPlayer       [support child]
│
├── InvestigateBehavior (Sequence)
│   ├── C_HasLastKnownPlayerPosition
│   ├── Move To Last Known Position Timeout (Timeout, 5s)
│   │   └── A_MoveToLastKnownPosition
│   ├── A_LookAround
│   └── A_ClearLastKnownPlayerPosition
│
└── PatrolBehavior (Repeater, infinite)
    └── Patrol Sequence (Sequence)
        ├── A_SelectNextPatrolPoint
        ├── Move To Patrol Point Retry (Retry, 2 attempts)
        │   └── A_MoveToPatrolPoint
        └── BTWaitAction ("Patrol Wait", 1s)
```

## Mermaid-Diagramm

```mermaid
flowchart TD
    root["Root Selector<br/>(Reactive)"]:::root

    emergency["EmergencyBehavior<br/>Sequence"]:::composite
    arrest["ArrestBehavior<br/>Sequence"]:::composite
    chase["ChaseBehavior<br/>Sequence"]:::composite
    investigate["InvestigateBehavior<br/>Sequence"]:::composite
    patrol["PatrolBehavior<br/>Repeater"]:::decorator

    root --> emergency
    root --> arrest
    root --> chase
    root --> investigate
    root --> patrol

    emergencyHealth{"OfficerHealthLow?"}:::condition
    emergencySelector["Emergency Selector<br/>Selector"]:::composite
    callBackupSequence["Call Backup Sequence<br/>Sequence"]:::composite
    notBackupCalled{"Backup not called?"}:::condition
    callBackup["CallBackup"]:::action
    fleeSafe["FleeToSafePoint"]:::action

    emergency --> emergencyHealth --> emergencySelector
    emergencySelector --> callBackupSequence
    emergencySelector --> fleeSafe
    callBackupSequence --> notBackupCalled --> callBackup

    arrestVisible{"PlayerVisible?"}:::condition
    arrestSuspicious{"PlayerSuspicious?"}:::condition
    arrestRange{"PlayerInArrestRange?"}:::condition
    arrestPlayer["ArrestPlayer"]:::action

    arrest --> arrestVisible --> arrestSuspicious --> arrestRange --> arrestPlayer

    chaseVisible{"PlayerVisible?"}:::condition
    chaseSuspicious{"PlayerSuspicious?"}:::condition
    notArrestRange["Inverter<br/>Not In Arrest Range"]:::decorator
    chaseRange{"PlayerInArrestRange?"}:::condition
    chaseParallel["Chase Parallel<br/>requiredChildIndex: 0"]:::composite
    chasePlayer["ChasePlayer<br/>determines status"]:::action
    lookAtPlayer["LookAtPlayer<br/>support only"]:::action
    chaseNote["A_ChasePlayer determines<br/>Parallel status"]:::note

    chase --> chaseVisible --> chaseSuspicious --> notArrestRange --> chaseRange
    notArrestRange --> chaseParallel
    chaseParallel --> chasePlayer
    chaseParallel --> lookAtPlayer
    chaseParallel -.-> chaseNote

    hasLastKnown{"HasLastKnownPlayerPosition?"}:::condition
    timeout["Timeout(5s)"]:::decorator
    moveLastKnown["MoveToLastKnownPosition"]:::action
    lookAround["LookAround"]:::action
    clearLastKnown["ClearLastKnownPlayerPosition"]:::action

    investigate --> hasLastKnown --> timeout --> moveLastKnown
    timeout --> lookAround --> clearLastKnown

    patrolSequence["Patrol Sequence<br/>Sequence"]:::composite
    selectPatrol["SelectNextPatrolPoint"]:::action
    retryPatrol["Retry(2)"]:::decorator
    movePatrol["MoveToPatrolPoint"]:::action
    patrolWait["Wait(1s)"]:::action

    patrol --> patrolSequence
    patrolSequence --> selectPatrol --> retryPatrol --> movePatrol
    retryPatrol --> patrolWait

    blackboard["Blackboard<br/>Perception, LastKnownPosition,<br/>CurrentBehaviorName, LastTreeStatus"]:::note
    root -. reads/writes .-> blackboard

    classDef root fill:#ffb454,stroke:#b45309,stroke-width:3px,color:#1f1300;
    classDef composite fill:#93c5fd,stroke:#1d4ed8,stroke-width:2px,color:#07152f;
    classDef condition fill:#fde68a,stroke:#b45309,stroke-width:2px,color:#2d1b00;
    classDef action fill:#bbf7d0,stroke:#15803d,stroke-width:2px,color:#052e16;
    classDef decorator fill:#ddd6fe,stroke:#7c3aed,stroke-width:2px,color:#24104f;
    classDef note fill:#e5e7eb,stroke:#6b7280,stroke-width:1px,color:#111827;
```

## Legende

- Orange/Rot: Root Selector. Er ist reaktiv und prueft jeden Tick wieder von oben.
- Blau: Composite Nodes wie `Sequence`, `Selector` und `Parallel`.
- Gelb: Conditions. Sie beantworten Fragen aus dem Blackboard, z. B. `PlayerVisible?`.
- Gruen: Actions. Sie fuehren Demo-Verhalten aus, z. B. `ChasePlayer`.
- Violett: Decorators wie `Inverter`, `Timeout(5s)`, `Retry(2)` und `Repeater`.
- Grau: Blackboard-/Debug-Hinweise.

## Graphviz

Die DOT-Version liegt in `PoliceBehaviorTree.dot`. Wenn Graphviz installiert ist, kann daraus ein SVG oder PNG erzeugt werden:

```powershell
dot -Tsvg Assets/_Demo/BehaviorTreePolice/Docs/PoliceBehaviorTree.dot -o Assets/_Demo/BehaviorTreePolice/Docs/PoliceBehaviorTree.svg
dot -Tpng Assets/_Demo/BehaviorTreePolice/Docs/PoliceBehaviorTree.dot -o Assets/_Demo/BehaviorTreePolice/Docs/PoliceBehaviorTree.png
```
