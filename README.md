# Those Who Walk Away - Game

*Those Who Walk Away* is a desktop sci-fi simulation with branching narrative and complex interpersonal choices.

![Game Cover](Images/GameCover.png)

---

## Features
- **Interactive desktop simulation:** Navigate multiple desktop apps, each with functional and responsive UI.  
- **Branching dialogue system:** Dialogue choices dynamically affect outcomes and character relationships.  
- **Drag-and-drop mechanics:** Drag and drop is supported for file transfer between apps.  
- **Multi-app integration:** Desktop apps interact seamlessly, simulating a realistic workflow environment.  

---

## Technical Implementation
- **Dialogue system:** Implemented in C# with JSON parsing to drive branching narrative and dynamic character states.  
- **Event-driven architecture:** Custom system for triggering in-game events, interactions, and UI updates based on player actions.  
- **Inter-app communication:** Multiple desktop apps synchronize and exchange data through shared event systems.  
- **Save System:** Implemented in C# using JSON serialization and ScriptableObjects to capture conversations, player choices, and desktop states.

All C# scripts implementing these systems are included in the **Scripts** folder. Review them to see the underlying architecture, data handling, and gameplay logic.

---

## In-Progress Build
A playable Windows build of the game is available here: [Itch.io Link](https://ajaytemal.itch.io/those-who-walk-away)

**Note:** This is an early pre-release version of the game.  
- Includes key features like the interactive desktop, dialogue system, and drag-and-drop interactions.  
- Future updates will expand content and polish the game.  
- Bugs and missing features are expected in this pre-release version.  

**Instructions:**  
1. Extract the ZIP file.  
2. Run `Those Who Walk Away.exe`  
3. Enjoy!  



