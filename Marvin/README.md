# Police Behavior Tree Demo

The sheriff patrols the town and reacts to suspicious player behavior.

## Behavior

* The sheriff chases the player if he sees them running.
* The sheriff also chases the player if they enter forbidden areas, such as the Grand Hotel or the Barber Shop.
* If the sheriff gets close enough, he arrests / attacks the player.
* If the player escapes and the sheriff loses sight, he investigates the last known position and then returns to patrol.
* If `Low Health Demo Toggle` is enabled on the sheriff, he enters emergency mode and runs toward the church.

## Controls

* `WASD` — Move
* `Shift` — Run
* `R` — Restart after being caught

## Debug

Use `Police BT Tree Debug UI` in the Hierarchy for the Behavior Tree debug window shown in the presentation.

The other debug UI is legacy.

## Other Information

My whole chat with ChatGPT 5.5:
https://chatgpt.com/share/6a29a199-8208-83ed-bf46-1fdf8fa06a55

I put the whole chat with codex (GPT 5.5 (High reasoning)) in a file CHATWITHCODEX.md right next to this readme.md :)

Every commit that actually changes code has its prompts in the description aswell.

AI is crazy and it was a blast :)
