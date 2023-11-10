Authors: Kyla Ryan, Nicholas Provenzano (PS8), Elizabeth Lukner (PS9)
PS9 Tank Wars Client README Information

Date: Mar 28th - 31st, 2021
Initially set up of the PS8 solution: Project created for model, view, controller, resources, and Vector2D
Added the JSON functionality through the NewtonSoft Json addon
First, we decided to take on the GameController first as that would allow us to process the information from the server
and get a connection started. 
Creation of initial textboxes, labels, and connect button were created in the view. 
Then, we decided to use a delegate to send the player name and server address to the controller
Initially tests were done to make sure we could connect to the server.

Date: April 1st - 3rd, 2021
Realize that we need to rebuild the solution due to wrong configuring settings in the project creator.
Once the project was rebuilt, we tackled the GameController. First, we made sure that it could connect to the game
server. Next, we tackled receiving the data from the server and parsing the string from the server. Then, we parsed
the second string sent from the server using the provided JSON addon. Once we attained JSON functionality, we began to
work on the View portion of the solution. Needing some help, we got in touch with TAs and added a lock around the parsing
for JSON info. At this point, we were noticing that the client would start, connect to a server, and then freeze after
a few seconds.

Date: April 4th - 5th, 2021
At this point, we decided to take care of the view and getting the background drawn. We added the correct images to the
resources project. We began working on the logic for the walls, tanks, powerups, projectiles, and other model classes. 
We established a way to edit those model class through lists in the specified world. We were still having the issue where
connecting to the server would result in a complete freeze a few seconds after connecting. 

Date: April 6th - 7th, 2021
More progress was made on the View and Model projects. In addition, bugs fixes were applied to the Game Controller class that
fixed that freezing issue that occurred. Once the background and walls were being drawn, the logic for drawing the tanks 
taken care of. Tanks were still not able to be drawn at this point. Then, basic controls were implemented. A simpler way to
updated the model classes was implemented as well as bugs fixes for the Game Controller. 

Date: April 8th, 2021
A help menu was created that detailed the controls of the game and a little bit of detail about the game. Tanks were now 
being drawn, and a fix was applied to get random colors for each tank to appear. Next, controls were finalized and tanks 
were able to be moved using the arrow keys. However, functionality was a bit choppy when moving side to side. Next, a fix
was applied to the tanks to make moving around smoother. A bug was noticed where AI tanks could move through walls. This
issue was addressed as well. Another issue arose with the turrets not disappearing when a tank died. This issue was fixed
through a check on if the tank was still alive. 

Date: April 9th, 2021
The final details of the project were completed. A health bar was added so that the player can visualize their health. In
addition, the player name was added below the tank with their score to the right. Powerup sizing was fixed and a different 
color was added for better visual clarity. A bug was fixed where when a player died the player's name would remain on screen.
Another bug was fixed where the health bar disappeared for the player. Then, the beam and projectile animations were added
to the game. Later, the frame rate was adjusted for a smoother beam animation. A bug was found where if a player right clicked 
in the game, a context menu would pop up. This was due to the creation of a wrong menu item, and was removed. Now, it was time 
for us to test the game and make sure there are no other bugs. One final touch was adding an "explosion". Instead of an 
explosion, the tank is removed from the world using a black hole.

Date: April 12th, 2021
First initially added the needed additions for the PS9 solution: Server controller, game settings file, and game settings xml reader. Then finished game settings reader first. 
Then planned for the next stage which is creating the server controller and model logic. Next, finished the handshake part of server controller and added on the timer to save how many milliseconds have passed.
Continued handshake, added getters/setters for tank. Began update world. Added Game Setting object to handshake. Fixed structure of Server Controller and Game Settings. Added constructor 
for settings from file & default settings. Finished getting the server to update the world objects so that it will show up on the view. 
Added Setters for all model objects. Added private helper methods in the world class for the update world.
Finally ending the week by moving beam intersection to world class. 


Date: April 19th, 2021
Week two started off by adding some changes to the classes to try to get a tank to spawn in. 
Adjusted Program to have all methods from Server Controller. Added methods in the server controller to process data to send to 
client thus having have a world and tank drawing on view. Had problems with walls not spawning fixed by fixing the game settings reader. 
The fix was to read for the exact match of p1, p2 and saving it only then. 
Finished working on determining movements in the server controller by processing the control commands. 
Finished turret and tank movement. Added and altered connection methods that send and receive data, process, and update objects within the world
Finished world wrap around. Originally added if statements to world for removing and adding data at the same time in the server controller update world information. 
Starting on collisions of tanks with walls. Originally had problems with collision with tank and walls not colliding and then if it did collide the tank would then freeze but fixed 
by using the new location to determine if there was a collision instead of the current tank location. Then finished projectiles firing. 
Finally finishing the week by implementing the beam animations to work at anytime for now until powerups are fully implemented. 

Date: April 26th, 2021
Started this week by finishing the implementation of client disconnections. 
Started next projectile tank collisions
Worked on implementing randomRespawnTank
projectile firing and collision with tanks is working
Fixed projectiles collisions with walls because the projectiles would not delete after colliding into wall and this is 
where we had to separate removing dead world objects and updating current world objects because there were inconsistencies with how all of the information for all of
the world objects were being saved or the world object was removed before the information was being sent to the server. 
Added died checks for handling commands. 
Added projectile tank health decrementing.
Finished tank respawning and changed the update world to only send the json string where the tank is dead once instead of 
multiple times so that the tank death animation is only ruined through once instead of multiple times because the json is being 
Sent multiple times about the tank being dead. 
Added the power up respawn implementation so that now the power ups will respawn. 

//TODO left to comment: leaving now for the beams to only be able to be used when power up is picked up by tank. 



