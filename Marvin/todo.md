TODO:

Demo Using AI Approach done (polish that)

Demo using git amend approach
Demo Using unity built in behavior Tree package
Demo Using Behavior Designer Pro
Demo Using Game Creator 2


Polish game:
1. Wir spawnen bei der lock (punkt a), neben uns liegt eine kiste. 
2. mit "E" kann man die kiste aufnehmen oder an dem aktuellen ort droppen
3. mit der kiste in der hand läuft man sehr langsam
4. Es ist nachts
5. wir wollen die kiste zu lydia (anderer npc bei punkt b) beim brunnen bringen
6. es pattroulieren überall sherriffs mit lampen diese sollen einen lichtkegel haben der gleich ihrem vision kegel ist
7. im lichtkegel wird man gesehen
8. sherrifs laufen schneller als wir
9. wenn uns ein sherrif mit der kiste sieht rennt er zu uns und nimmt uns fest
10. sherrifs haben ansonsten beim pattroulieren eine normale lauf animation kein rennen
11. ziel ist es mit der kiste zu lydia zu laufen ohne gesehen zu werden
12. am anfang soll ein overlay kommen was sagt "bring die kiste zu Lydia beim brunnen ohne dich erwischen zu lassen"
13. bei der festnahme soll ein verhafted screen kommen, danach spawnt man im gefängnis und die kiste ist an einem punkt C auf der karte






| Status     | Feature                                                                          |
| ---------- | -------------------------------------------------------------------------------- |
| ✅          | Level-Punkte platziert: PlayerSpawn, DeliveryPoint, CrateReset, JailSpawn, Lydia |
| ✅          | MissionCrate platziert                                                           |
| ✅          | Nacht-Look                                                                       |
| ✅          | Sheriff-Licht-/Vision-Cone                                                       |
| ⏳          | Kiste mit `E` aufnehmen / ablegen                                                |
| ⏳          | Mit Kiste deutlich langsamer laufen, kein Sprint                                 |
| ⏳          | Start-Overlay mit Missionsziel                                                   |
| ⏳          | Delivery bei Lydia                                                               |
| ⏳          | Mission-Success-Zustand/UI                                                       |
| ⏳          | Arrest-Präsentation / „Verhaftet“-Screen                                         |
| ⏳          | Fade Out / Jail-Teleport / Fade In                                               |
| ⏳          | Kiste nach Arrest zu `CrateReset`                                                |
| ⏳          | sauberer Reset aller Gameplay-Zustände                                           |
| ⏳          | zusätzliche Sheriffs aus deinen Spawnpunkten                                     |
| ⏳          | Patrol = Walk, Chase = Run sauber darstellen                                     |
| ⏳          | finales UI-/Animations-/Presentation-Polish                                      |
| **später** | eigentliche Patrol/Chase/Investigate/Arrest-Entscheidungen der fünf BT-Systeme   |



Okay, nun möchte ich dass das visionlightcone vom sheriff doch etwas subtiler ist. ausserdem habe ich eben an dem mission crate rumgefummelt und wieder rückgängig gemacht bitte nochmal prüfen ob da alles so ist wie es sein soll

Okay dann soll er jetzt mal machen dass es so aussieht als würde man die kiste durch die gegend tragen, bevor du einen prompt schreibst gib mir ideen wie du das sinnvoll zu generieren findest, ich stell mir das nicht so leicht vor codex zu sagen er soll jetzt eine animation bauen und die sieht am ende nicht kacke aus. 


Nun soll die logik so umgebaut werden: Bisher wird man arrested fürs rumrennen, das ist natürlich quatsch, man soll erwischt/verhaftet werden wenn man die kiste durch die gegend trägt und dabei erwischt wird, also einfach rennen als arrest grund ersetzen durch das rumtragen. 

Nun möchte ich einen ausführlichen analysepart starten bevor ich dann in den teil übergehe die anderen behavior tree ansätze einzubauen. Erstmal habe ich eigene vorschläge und ideen. Ich möchte dabei die projektstruktur wirklich sauber halten das ist mir sehr sehr wichtig zum beispiel fallen mir folgende sachen auf: 
1. unter assets/synty liegen diverse synty packages aber polygonwesternfrontier liegt daneben obwohl es auch ein synty package ist, kann das nicht einfach da rein? 
2. Ich finde Synty als verzeichnis direkt in assets irgendwie verrückt sollte da nicht eine ebene ThridpartyPackages oder ähnliches drüber das ist doch käse.. was sind vernünftige namen dafür und dann pack das da rein, esseidenn ich irre mich. 
3. Dann fällt mir auf dass das hauptspiel in customapproachdemo liegt einschliesslich des behvior tree parts. Ich finde für den sinn und zweck dieses projektes zur darstellung der unterschiedlichen behavior tree ansätze sollte das spiel von CustomApprochDemo zu BehaviorTreeDemo umbenannt werden und jegliche scripts wie animation gameplay ui usw sollten darin liegen, 
den ordner unter scripts der behavior tree heisst würde ich gerne in CustomApproachBehaviorTree umbenennen. Im weiteren verlauf stelle ich mir dann vor das daneben die ordner liegen wie z.b. GameCreatorBehaviorTree usw... gib mir mal feedback ob du das für sinnvoll hälst, was noch sinnvoll ist und danach wäge ich ab und bitte dich um den prompt für codex.

Finaler prompt, ich baller jetzt gerne richtig tokens raus mit astra. Er soll das ganze projekt durchgehen, logiklücken, uncleanes coding, unkonventionelle strukturen, verbesserungen finden. vorallem namingconvention, saubere projektstruktur, nachhaltige logik/implementation usw sind mir dabei sehr wichtig. Sehr sehr wichtig ist auch dass alles was zum behavior Tree gehört isoliert ist am liebsten hätte ich dass alles vom game völlig seperat vom behavior tree ist und man denn dan gleich durch die anderen ansäte ersetzen kann ohne viel rumbasteln. ich weiß es gibt das konzept der interfaces in der informatik, es soll jetzt nicht völlig absurd overkill werden aber wenn das einfach machbar ist hätte ich das sehr gerne so clean wie möglich. Er soll das gesammt Projekt ausführlich scannen und detailiertes feedback geben, das gebe ich dann dir hier im chat zurück und wir gucken uns alle punkte an und entscheiden wie wir aufräumen.



