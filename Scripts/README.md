This folder contains all of the C# scripts for Those Who Walk Away. 

At the moment, the scripts are seperated by their level of importance relative to the game architecture. 

- Core Scripts contains the 'manager' scripts for each app, major dialogue/data parsing systems, data containers, and a global script that communicates with each to execute the game in order.
- Important Scripts contains essential scripts for smaller systems, such as drag/drop, dialogue choice selection, the copy/paste clipboard, and manager scripts for essential prefabs.
- Minor Fixes or Prefab Scripts contains smaller scripts for prefabs/low-maintence systems, such as updating the time and maintaining small prefabs.  

Note: These scripts have been 'battle-tested' and mostly work reliably, though some are still in a draft-like state. I've decided to keep them visible to demonstrate how I apprached the development of gameplay systems under interation, and I'm actively working on refactoring them for clarity and modularity. 
