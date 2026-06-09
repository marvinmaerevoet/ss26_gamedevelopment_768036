# Police Behavior Tree Demo

## Ziel der Demo

Diese Demo soll eine kleine, robuste Behavior-Tree-Grundlage fuer NPC-Polizisten bereitstellen. Der Fokus liegt auf gut lesbarem Demo-Code, der spaeter Patrol, Sichtkontakt, Verfolgung, Festnahme und Untersuchung einer letzten bekannten Position abbilden kann.

Die Runtime ist bewusst unabhaengig von Game Creator und veraendert keine Szene automatisch.

## BTStatus

Jeder Node gibt genau einen Status zurueck:

- `Success`: Der Node ist erfolgreich abgeschlossen.
- `Failure`: Der Node ist fehlgeschlagen.
- `Running`: Der Node arbeitet weiter und soll im naechsten Tick erneut ausgefuehrt werden.

Alle Nodes speichern ihren letzten Status in `LastStatus`.

## Selector

Ein Selector probiert seine Child-Nodes der Reihe nach. Er liefert `Success`, sobald ein Child erfolgreich ist. Er liefert `Running`, wenn das aktuelle Child noch laeuft. Erst wenn alle Children fehlschlagen, liefert er `Failure`.

Der laufende Child-Index wird gemerkt, damit laufende Aktionen nicht bei jedem Tick neu starten.

## Sequence

Eine Sequence fuehrt ihre Child-Nodes der Reihe nach aus. Sie liefert `Failure`, sobald ein Child fehlschlaegt. Sie liefert `Running`, wenn das aktuelle Child noch laeuft. Erst wenn alle Children erfolgreich waren, liefert sie `Success`.

Der laufende Child-Index wird gemerkt, damit laufende Aktionen nicht bei jedem Tick neu starten.

## Decorator

Decorator-Nodes kapseln genau einen Child-Node und veraendern dessen Ergebnis oder Laufzeitverhalten.

Enthalten sind:

- `BTInverter`: dreht `Success` und `Failure` um; `Running` bleibt `Running`.
- `BTRepeater`: wiederholt einen Child-Node, aber maximal einmal pro Tick.
- `BTRetry`: versucht einen fehlgeschlagenen Child-Node erneut bis zur maximalen Versuchszahl.
- `BTTimeout`: bricht einen laufenden Child-Node nach einer Zeitgrenze mit `Failure` ab.

## Parallel

`BTParallel` tickt alle Children in einem Frame.

Wenn `requiredChildIndex` gesetzt ist, bestimmt dieser Child den Hauptstatus. Ohne `requiredChildIndex` gilt:

- `Success`, wenn alle Children `Success` liefern.
- `Failure`, wenn ein Child `Failure` liefert.
- sonst `Running`.

## Blackboard

Ein Blackboard ist fuer den naechsten Ausbauschritt vorgesehen. Es soll gemeinsame Demo-Daten halten, zum Beispiel Spieler-Referenz, letzte bekannte Spielerposition, Verdachtswert, Sichtkontakt und aktuelle Patrol-Ziele.

In dieser Grundstruktur ist noch keine Polizeilogik implementiert.
