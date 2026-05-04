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
