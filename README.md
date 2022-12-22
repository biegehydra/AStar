#  C# AStar Pathfinding Command Line Interface
 The following CLI allows you create a rectangular terrain that the A* algorithm will be tasked with navigating
 
 You supply the heigh and width of the terrain, the position of the start and end node, and the positions of the obstacles.
 
 With that information the A Star algorithm will find the fastest path to the end node. It does this by looking at all the visitable nodes and assigning them a balue
 based on their distance from the starting node and their distance from the end node, then choosing the one with the smallest total.
 
 The terrain positions are in the following format
 
![Format](./Images/TerrainExample.png)

Here is an example input and solution

![Example](./Images/ExampleIO.png)

Xs represent vistitable nodes. S represents the starting node. E represents the end node. W represents Walls.
