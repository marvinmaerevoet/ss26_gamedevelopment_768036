# ss26_gamedevelopment_768036

# Behavior Tree Demo – Cowboy Stealth & Bossfight

## Überblick
Diese Demo zeigt den Einsatz von Behavior Trees in zwei Szenarien:
1. Stealth-Gameplay mit NPCs (Bürger & Sheriffs)
2. Optionaler Bosskampf mit Phasen-Logik

---

## Part 1: Stealth in der Stadt

### Setting
- Spieler: Cowboy
- Startpunkt: Stadtrand
- Ziel: Den Saloon unentdeckt erreichen
- Umgebung: Stadt mit patrouillierenden NPCs

---

### NPC-Typen & Verhalten

#### Bürger (Civilians)
- Patrouillieren auf festen Routen
- Sehen Spieler:
  - → Schreien nach Hilfe
- Danach:
  - → Kehren zur Route zurück

---

#### Sheriffs
- Patrouillieren auf festen Routen
- Hören Hilferuf:
  - → Bewegen sich zur Quelle
- Sehen Spieler:
  - → Verfolgung beginnt
- Verlieren Spieler:
  - → Suchen im Gebiet
- Erfolglos:
  - → Rückkehr zur Route

---

### Behavior Tree Konzepte

**Selector Nodes**
- Priorisierung von Verhalten:
  - Verfolgung > Untersuchung > Patrouille

**Sequenzen**
- Klare Abläufe:
  - Hilferuf hören → untersuchen
  - Spieler verlieren → suchen → abbrechen

---

## Part 2: Bossfight im Saloon (Optional)

### Setting
- Ort: Saloon
- Gegner: Bad Guy (Boss)
- Ziel: Boss besiegen

---

### Boss-Phasen

#### Phase 1 – Defensiv
- Blocken / Ausweichen
- Wenige Angriffe

#### Phase 2 – Aggressiv
- Häufige Angriffe
- Höherer Druck auf Spieler

#### Phase 3 – Spezialattacken
- Kombiniert aggressive Angriffe mit Spezialfähigkeiten

---

### Behavior Tree Konzepte

**Subtrees**
- Wiederverwendbare Verhaltensblöcke (z. B. Angriff, Bewegung)

**Reusability**
- Gleiche Logik kann in mehreren Phasen genutzt werden
- Flexible Erweiterbarkeit

---

## Ziel der Demo
- Veranschaulichung von Behavior Trees in realistischen Spielsituationen
- Vergleich von einfachem NPC-Verhalten vs. komplexer Boss-Logik
- Fokus auf:
  - Modularität
  - Erweiterbarkeit
  - Klar strukturierte Entscheidungslogik



  Ich lese zuerst die relevanten Skills durch, bevor ich dir einen Fahrplan erstelle.
  Hier ist dein strukturierter Implementierungsfahrplan – angepasst für Unity-Beginner mit Game Creator 2:VvisualizeHier ist dein Fahrplan als Übersicht – ich erkläre jede Phase direkt:

  Phase 0 – Projekt kennenlernen (~2–3h)
  Bevor du irgendwas baust: NavMesh backen (Window → AI → Navigation → Bake), einen Test-NPC aus deinem Asset-Pack in die Szene ziehen, und ihn per Game Creator 2 Behavior Graph zu einem einzigen Waypoint laufen lassen. Nur um das System zu verstehen – kein "echter" Code, nur ausprobieren.

  Phase 1 – Civilian NPC: Patrol & Alert (~3–4h)
  Das ist dein einfachster NPC und der perfekte Einstieg in Behavior Trees.
  Behavior Tree Aufbau:

  Root: Selector (prüft Prioritäten von oben nach unten)

  Branch 1: Bedingung "Spieler in Sichtweite?" → Alert-Sequenz (Schrei-Animation + Audio spielen → kurz warten → zurück)
  Branch 2: Default → Patrol-Sequenz (Loop durch Waypoints)



  Nutze die GC2 Perception-Komponente (Vision Sensor) mit dem Tag "Player". Wenn der Civilian schreit, setzt du eine globale Variable AlertTriggered = true + AlertPosition – die braucht der Sheriff später.

  Phase 2 – Sheriff NPC: Chase, Investigate, Search (~4–5h)
  Das ist das Herzstück des Projekts und zeigt die BT-Konzepte am deutlichsten.
  Behavior Tree Aufbau (Prioritäts-Selector):

  Spieler direkt gesehen? → Chase-Sequenz (NavMesh Agent folgt Spieler)
  Chase verloren (kein Sight für X Sekunden)? → Search-Sequenz (gehe zu letzter bekannter Position, schaue um, Timer → dann abbrechen)
  AlertTriggered == true? → Investigate-Sequenz (gehe zur AlertPosition, Variable zurücksetzen)
  Default → Patrol (gleiche Logik wie Civilian – das zeigt Reusability!)

  Nutze GC2 Blackboard-Variablen als "Gedächtnis": LastKnownPosition, AlertTriggered, AlertPosition.

  Phase 3 – Spieler & Stealth-Mechaniken (~2–3h)

  Spieler-Charakter bekommt Tag "Player"
  Spawn-Punkt am Stadtrand, Ziel-Trigger an der Saloon-Tür (bei Betreten: "Du hast es geschafft!"-UI)
  Optional: "!"-Symbol über NPCs wenn sie den Spieler entdecken (gut für Demos)


  Phase 4 – Boss-Fight mit Phasen (~4–5h)
  Hier zeigst du Subtrees und Reusability.
  Root-Selector prüft HP-Schwellen:

  HP > 66% → Phasen-Subtree 1: Defensiv (blocken, selten angreifen)
  HP 33–66% → Phasen-Subtree 2: Aggressiv (häufige Angriffe, auf Spieler zugehen)
  HP < 33% → Phasen-Subtree 3: Spezialattacken (AoE oder Projektil + normale Angriffe wechselnd)

  Speichere jeden Subtree als eigenen GC2-Subgraph → das ist dein Beweis für Modularität in der Präsentation.

  Phase 5 – Polish & Präsentation (~2h)

  GC2 Behavior Graph Debugger im Play-Mode nutzen: zeigt live welcher Node aktiv ist → perfektes Präsentationsbild
  Sichtfeld-Gizmos einschalten damit das Publikum die Logik sieht
  Screenshots der Behavior Trees für deine Folien


  Empfohlene Reihenfolge: Phase 0 → 1 → 2 → 3 → dann erst 4 (Boss ist optional). Wenn die Zeit knapp wird, ist Phase 1+2 allein schon eine vollständige Demo für Behavior Trees.
  Soll ich für einen bestimmten Teil tiefer gehen – z.B. wie genau der GC2 Behavior Graph für den Sheriff aufgebaut wird?
