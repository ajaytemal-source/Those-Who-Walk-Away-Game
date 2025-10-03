# This folder contains all of the C# scripts for Those Who Walk Away. 

The scripts are organized by their role in the overall game architecture: 

- Core Scripts:
  Contains the 'manager' scripts for each app, major dialogue/data parsing systems, data containers, and a global controller that coordinates execution of the game. 
- Important Scripts:
  Contains essential scripts for smaller but key systems, such as drag/drop, dialogue choice selection, the copy/paste clipboard, and manager scripts for essential prefabs.
- Minor Fixes or Prefab Scripts:
  Contains lightweight scripts for prefabs and low-maintence systems, such as updating the time or maintaining UI elements. 

Note: These scripts have been 'battle-tested' and mostly work reliably, though some are still in a draft-like state. I've decided to keep them visible to demonstrate how I apprached the development of gameplay systems under interation, and I'm actively working on refactoring them for clarity and modularity. 
