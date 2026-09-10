# Behavior Tree Feature-Parity Contract

Jeder Decision-Adapter muss dieselbe gemeinsame Szene, dieselben `PoliceAIContext`-Instanzen und dieselben Gameplay-/Presentation-Systeme verwenden. Unterschiede duerfen nur aus dem Decision-System selbst entstehen.

## Gemeinsame Regeln

### Perception

- Sichtweite: `12 m`.
- Sichtwinkel: `90°`.
- Alle vier Sheriffs verwenden dieselbe Obstacle-Maske und dieselbe Line-of-Sight-Regel.
- Eigene Sheriff- und Player-Child-Collider blockieren den Ray nicht; andere Treffer der Weltgeometrie blockieren Sicht.
- Jeder Adapter liest ausschliesslich den aktuellen, durch seinen `PoliceAIContext` erzeugten Perception-Snapshot.

## Decision Timing Contract

- Shared Perception wird pro Sheriff durch dessen eigenen `PoliceAIContext` mit einem festen Intervall von `0.1 s` (`10 Hz`) aktualisiert.
- Jeder Refresh schreibt genau einen autoritativen Snapshot fuer Sichtbarkeit, Suspicion, Arrest-Reichweite und letzte relevante Player-Position in das gemeinsame `PoliceBlackboard`.
- Decision-Adapter und einzelne Nodes duerfen Shared Perception weder selbst refreshen noch diese fachlichen Werte erneut aus Live-Weltdaten berechnen.
- Zwischen zwei Refreshes lesen auch schneller evaluierende Adapter denselben Snapshot und erhalten dadurch keine neueren Perception-Daten.
- Tree-/Graph-Evaluation darf weiterhin systemtypisch und nativ laufen. CustomApproach, GitAmend, UnityBehavior, BehaviorDesigner und GameCreatorBehavior muessen intern nicht auf `10 Hz` gezwungen werden.
- Shared zeitabhaengige Operationen verwenden reale Dauer in Sekunden statt Adapter- oder Decision-Tickzahlen.
- NavMesh-Bewegung, Animation, Kamera, UI, Fade und kontinuierliche Presentation duerfen weiterhin framebasiert laufen.

Die native Ausfuehrungsweise gehoert zur Implementierung des jeweiligen Decision-Systems. Fairness entsteht durch identische Shared Inputs, gemeinsame Gameplay-Regeln und gemeinsame Actions; eine kuenstlich identische interne Tickrate wuerde diesen Systemunterschied verdecken.

### Suspicion und Chase

- Suspicion ist ausschliesslich wahr, wenn `DemoPlayerCarryController.CurrentCarryable` exakt die im Context referenzierte `MissionCrate` ist und deren Pickup nicht gesperrt ist.
- Running und Restricted Areas beeinflussen Suspicion nicht.
- Chase erfordert sichtbaren und verdaechtigen Player.
- Wird die Kiste vor dem Arrest-Commit abgelegt, endet der Smuggling-Chase. Die letzte relevante Position darf anschliessend untersucht werden.

### Patrol und Bewegung

- `0` gueltige Patrol Points: Auswahl/Action schlaegt fehl.
- `1` gueltiger Patrol Point: dieser Punkt wird verwendet.
- `n >= 2`: unabhaengige Zufallsauswahl pro Sheriff; aktueller Punkt wird nach Moeglichkeit nicht direkt wiederholt.
- Walk: `3 m/s`; Run: `6 m/s`.
- Patrol und Investigation verwenden Walk. Chase und der bestehende Emergency-Flee verwenden Run.
- Adapter verwenden die neutralen Context-APIs fuer Destination, `PoliceMovementStatus`, Stop und Reset.

### Last Known Position und Investigation

- Die letzte bekannte Position wird nur bei sichtbarem, verdaechtigem Player aktualisiert.
- Investigation geht mit Walk zur Position und startet erst nach Ankunft den gemeinsamen LookAround-Vorgang.
- LookAround dauert ungefaehr `2 s` und ist zeitbasiert, nicht von der Adapter-Tickrate abhaengig.
- Danach wird die letzte bekannte Position geloescht.

### Arrest

- Reihenfolge der normalen fachlichen Prioritaet: Arrest vor Chase, Chase vor Investigate, Investigate vor Patrol. Der bestehende Emergency-Demozweig darf weiterhin darueber liegen.
- Arrest-Approach und Latch sind lokal pro Sheriff.
- Zielband: `1.4 m ± 0.15 m` horizontale Distanz zum Player. Zu nahe Sheriffs korrigieren nach aussen, zu weit entfernte naehern sich; ein NavMesh-Sample muss das Distanzband weiterhin sinnvoll repraesentieren.
- Erst am gueltigen Standpunkt werden Bewegung/Pfad gestoppt, der Sheriff zum Player ausgerichtet und `DemoPlayerState.IsArrested` gesetzt.
- Sheriff-Praesentation: PointHand. Player-Praesentation: HandsOnHips Enter, danach Hold.
- Approach, Commit, Hold, Release, Cancel und Reset laufen ueber die neutrale `PoliceAIContext`-Arrest-API. Nach Commit bleibt der Adapter aktiv, bis die externe Arrest-Sequenz den Player freigibt.

### Delivery, Reset und Presentation

- Delivery und Arrest koennen nicht beide erfolgreich committen. Nach Delivery ist die Kiste gesperrt und nicht mehr suspicious.
- Der gemeinsame `DemoReset` setzt Gameplay, Presentation und Police-State zurueck. Ein Adapter setzt ueber `PoliceDecisionController.ResetDecisionState()` nur seinen Graph-/Tree-State zurueck.
- Vier Sheriffs besitzen je einen eigenen Context, Blackboard, Agent und Decision-Adapter. Sie duerfen keine lokalen Laufzeitwerte gegenseitig schreiben.
- Animator, Mission-/Arrest-UI, Fade, Jail-Sequenz, Carry, Delivery und Reset bleiben fuer alle Adapter identisch.

## Kleiner kuenftiger Testkatalog

Diese Tests sind empfohlen, aber in diesem Refactor nicht implementiert:

1. Ein Selector-Branch-Wechsel darf neu ausgewaehlte Bewegung nicht durch den Reset des alten Branches stoppen.
2. Reset eines nie gestarteten Arrest-Nodes darf keine fremde Bewegung beeinflussen.
3. Arrest-Cancel und Reset loeschen Approach-, Commit-, Facing- und Movement-Flags.
4. Delivery und Arrest koennen im selben Frame nicht beide erfolgreich werden.
5. Crate-Drop beendet Chase gemaess Vertrag und bewahrt nur die relevante LastKnownPosition.
6. Alle Adapter erhalten identische Perception-Snapshots.
7. Patrol-Auswahl behandelt `0`, `1` und `n` gueltige Punkte korrekt.
8. Jeder Adapter erfuellt den neutralen Reset-Vertrag fuer alle vier Sheriffs.
