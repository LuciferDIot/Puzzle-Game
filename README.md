# Recruitment-Task
Match 3 programming assignment

It's really awesome of you to apply for a position at Envision Studio Pvt Ltd.
As part of the evaluation process, we let candidates make a small game in Unity.

Please read the following instructions carefully before starting to work on it. Read them again before submitting.

Preparations

The very first thing you should do is read through these instructions. After that, send an e-mail to the email mentioned in the invite, with your suggested deadline for submission.
This means you choose your own deadline. There is no need to ask us if the deadline is OK. After that, start working on the game and submit it no later than the deadline.

Instructions

Make a simple Match 3 game based on this Unity project.
See the movie to get an idea of what it can look like.
https://youtu.be/DItjr0D_fCI

Unity version

Make sure the Unity version you use matches the one in the project. You can see that in Unity Hub, or by looking in the file ProjectSettings/ProjectVersion.txt.

Game mechanics

The game mechanics are as follows:

* The game consists of m x n tiles (configurable in editor or code).
* If three or more of the same tile type are in a horizontal row, the game removes them.
* A tile falls down if there is no tile below it (unless it's on the bottom row).
* The user can click on a tile. The game then removes the tile.
* When the game starts, all the positions should be filled with tiles and nothing should fall down.
* Do not add more features than this.

Version control

Start with cloning this repository. Keep it local. Do not create a public repository! Work with Git locally as usual. Do not push, as you have no write permissions. If you haven't worked with Git before, this is an opportunity to learn it.

Other guidelines

Make it simple, but write the code to a quality that's good enough for professional developers to continue working on. As a performance guideline for the code, design it so it runs well on relatively limited devices such as mobiles.

Requirements

* Follow Envision Studio's Style guide.
* Do not use the physics engine to move the tiles.
* Do not use raycasting for anything except if required to detect what tile the player clicked on.
* Apart from Unity's built-in functionality, do not use any plugin, external library, or code you didn't write yourself.

Submission

Make sure you have committed the final version.
Then create a zip file of the .git directory. The zip file should only contain the .git directory, nothing else.
Send the zip file to the suggested email (see your invite to the test).
