# FISH AND FISHER - GAME CONTROLS

## 🐟 FISH PLAYER CONTROLS

### Movement
- **W / Up Arrow** - Swim forward (maintain direction)
- **A / Left Arrow** - Turn left 60° (or 30° when pressing forward)
- **D / Right Arrow** - Turn right 60° (or 30° when pressing forward)
- **Space** - Boost speed (press rapidly for combo acceleration)

### Movement Mechanics
- The fish automatically swims forward at base speed
- You can only turn left/right - no backward movement
- Rapid Space tapping builds combo levels (max 3x) for higher speed
- Speed gradually decreases when not boosting
- Turning at high speed reduces your momentum

### Objective
**SURVIVE** until the timer runs out. Stay away from the fisher's hook!

### Game Area
- 50×50 unit rectangular boundary
- You cannot leave this area

---

## 🎣 FISHER PLAYER CONTROLS

### Aiming
- **Move Mouse** - Control the fishing hook crosshair position
- The crosshair can move anywhere within the game area

### Action
- **Left Mouse Button / E Key** - Swing the fishing rod and attempt to catch
- Cooldown: 1 second between swings
- Hook detection radius: 0.5 units around the crosshair

### Aiming System
- **Red crosshair (on water surface)** - Visual guide showing where you're aiming
- **Blue marker (fish level)** - Actual hook position (same XZ coordinates as red crosshair)
- The hook operates on the same plane as the fish (Y=0)

### Objective
**CATCH THE FISH** before time runs out by positioning your hook and swinging when close!

### Game Area
- 25×25 unit rectangular boundary (same as fish)
- Your hook can reach any location the fish can reach

---

## 🏆 WIN CONDITIONS

- **Fish Wins:** Survive until the timer reaches 00:00
- **Fisher Wins:** Successfully hook the fish before time runs out

**Game immediately ends** when the fish is caught - no need to wait for the timer!

---

## 💡 TIPS

### For Fish Players
- Use boost combos to escape quickly when the fisher is close
- Sharp turns help dodge hooks but reduce your speed
- Watch the timer - you only need to survive!

### For Fisher Players
- Predict the fish's movement path
- Wait for the right moment - don't waste swings during cooldown
- The fish moves faster when boosting - aim ahead of their position

---

Good luck, and may the best player win! 🐟 vs 🎣
