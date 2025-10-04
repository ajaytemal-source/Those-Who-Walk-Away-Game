# C# Scripts for *Those Who Walk Away*

This folder contains all of the C# scripts for *Those Who Walk Away*.

The scripts are organized by their role in the overall game architecture:

### **Core Scripts**
Contains the 'manager' scripts for each app, major dialogue/data parsing systems, data containers, and a global controller that coordinates execution of the game.

### **Important Scripts**
Contains essential scripts for smaller but key systems, such as drag/drop, dialogue choice selection, the copy/paste clipboard, and manager scripts for essential prefabs.

### **Minor Fixes or Prefab Scripts**
Contains lightweight scripts for prefabs and low-maintenance systems, such as updating the time or maintaining UI elements.

---

**Note:**
These scripts have been 'battle-tested' and mostly work reliably, though some are still in a draft-like state. Certain scripts already demonstrate strong coding practices, such as IRYSDialogueManager and SaveFileManager (Core Scripts), WindowResizerScript (Important Scripts), and ScrollRectVisibility (Minor Scripts or Script Prefabs). Many of the other scripts are still evolving, and I am actively improving their readability, modularity, and maintainability as part of ongoing development.
