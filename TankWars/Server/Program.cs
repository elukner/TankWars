using System;
using System.Collections.Generic;
using NetworkUtil;
using System.Text.RegularExpressions;
using TankWars;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;

namespace Server
{
    /// <summary>
    /// <Author> Kyla Ryan and Elizabeth Lukner (PS9)</Author>
    /// This is the controller
    /// The server controller is separate from the client's GameController. 
    /// It should update the model according to the game mechanics, keep track of
    /// clients, and broadcast the world to them.
    /// </summary>
    class Program
    {
        //Holds the game settings received from a file or default constructor
        private static GameSettings gameSettings;

        //world object held by the server
        private static World worldObject;

        //watch to determine number of milliseconds per frame
        private static Stopwatch watch;

        //holds the current frame
        private static int FrameCounter = 0;

        //holds when the next powerup will spawn
        private static int nextPowerupTime = 0;

        //a map of clients that are connected, each with an ID
        private static Dictionary<long, SocketState> connectedClients;

        //a map of clients that are disconnected, each with an ID
        private static Dictionary<long, SocketState> disconnectedClients;

        //keeps track of all of the commands associated with each client
        private static Dictionary<long, ControlCommands> userCommands = new Dictionary<long, ControlCommands>();

        //keeps track of all projectiles that have been fired
        private static List<Projectile> projUsed = new List<Projectile>();

        //determines when the client has fired last to be consistent with fire rate of game settings
        private static Dictionary<long, int> lastFireClient = new Dictionary<long, int>();


        /// <summary>
        /// Main method that starts the server and handles when a client connects and all data within the game
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Find game settings file
            string file = "..\\..\\..\\..\\Resources\\settings.xml";

            //if file doesn't exist, use default game data
            if (!(File.Exists(file)))
            {
                file = "settings.xml";

            }

            //read the file to get the game settings
            gameSettings = GameSettings.ReadSettings(file);

            //dictionaries to hold connected clients and disconnected clients
            connectedClients = new Dictionary<long, SocketState>();
            disconnectedClients = new Dictionary<long, SocketState>();

            //watch to keep track of time
            watch = new Stopwatch();

            //starts the server to begin accepting clients
            StartServer();

            //thread to continuously accept clients and update the world data for all clients
            Thread mainThread = new Thread(UpdateLoop);
            mainThread.Start();

            //create the world object to account for the game settings
            worldObject = new World(gameSettings.UniverseSize, gameSettings.WallList,
                gameSettings.FramesPerShot, gameSettings.RespawnRate, gameSettings.MSPerFrame);

            //keeps the server from closing after all settings have been set up.
            Console.Read();
        }

        /// <summary>
        /// Starts the server and begins connection over the server and port number.
        /// </summary>
        public static void StartServer()
        {
            //  1. Client connects
            Networking.StartServer(new Action<SocketState>(HandleClientConnected), 11000);
            Console.WriteLine("Server is running. Accepting clients.");
        }

        /// <summary>
        /// Loop to be consistently ran by the server to accept data
        /// </summary>
        private static void UpdateLoop()
        {
            watch.Start();
            while (true)
            {
                Update();

                //counting frames passed by
                FrameCounter++;
            }
        }


       /// <summary>
       /// Updates the world information every second for every client.
       /// </summary>
        private static void Update()
        {
            //string that holds the world data to send to every client
            string worldData = "";
            while (watch.ElapsedMilliseconds < gameSettings.MSPerFrame)
            { //do nothing if not enough time went by
            }
            watch.Restart();
            lock (worldObject)
            {
                //ensures the world objects have been updated (new location, fired, etc)
                UpdateWorldObjects();

                //creates a string based on the world objects to send to clients
                worldData = WorldObjectStringCreator();

                //clean up dead objects so they are no longer drawn
                worldObject.RemoveDeadWorldObjects();
            }

            lock (connectedClients)
            {
                //completely removes disconnected clients from the game
                disconnectedClients.Clear();

                //sends the world data to every client if possible
                foreach (SocketState client in connectedClients.Values)
                {
                    //if cannot send...
                    if (!Networking.Send(client.TheSocket, worldData))
                    {
                        //if cannot send, the client has disconnected
                        disconnectedClients[client.ID] = client;
                    }

                    //otherwise, do nothing as the data has been sent and continue
                    else{ }
                }

                //go and remove the clients that do not work
                foreach (SocketState client in disconnectedClients.Values)
                {
                    RemoveClient(client.ID);
                }
            }       
        }

        /// <summary>
        /// If a client wants to connect, begin connection process to load the game
        /// </summary>
        /// <param name="socketState"></param>
        private static void HandleClientConnected(SocketState socketState)
        {
            if (socketState.ErrorOccured)
            {
                return;
            }
            socketState.OnNetworkAction = ReceiveStartupName;
            Networking.GetData(socketState);
        }

        /// <summary>
        /// Gets the name of the player from the client and creates a tank and sends that and the walls of the game to the client's world
        /// </summary>
        /// <param name="socketState"></param>
        private static void ReceiveStartupName(SocketState socketState)
        {
            if (socketState.ErrorOccured)
            {
                return;
            }

            //Try to get the player name from the client and establish a connection
            try
            {
                string totalData = socketState.GetData();

                string[] parts = Regex.Split(totalData, @"(?<=[\n])");

                foreach (string p in parts)
                {
                    if (p.Length == 0)
                    {
                        continue;
                    }

                    if (p[p.Length - 1] != '\n')
                    {
                        break;
                    }
                    //name received from teh client
                    string name = p.Substring(0, p.Length - 1);

                    //adds the client to the dictionary of connected clients
                    lock (connectedClients)
                    {
                        connectedClients.Add((int)socketState.ID, socketState);
                    }

                    //sends the client ID and world size to connected player
                    SendStartupData(socketState, (int)socketState.ID);

                    //tells server what client has connected and what their name is.
                    Console.WriteLine("Player(" + socketState.ID + ") " + "\"" + name + "\" has joined.");
                   
                    lock (worldObject)
                    {
                        //link a new tank to that client as they have connected and add it to the tanks dictionary
                        worldObject.AddRandomTank(name, (int)socketState.ID);

                        //send the walls to the client
                        foreach (Wall wall in gameSettings.WallList)
                        {
                            worldObject.AddWall(wall.GetWallName(), wall);
                        }
                    }

                    //removes already handled data
                    socketState.RemoveData(0, p.Length);

                    //adds walls the first time on connection
                    lock (socketState.TheSocket)
                    {
                        StringBuilder walls = new StringBuilder();
                        foreach (Wall wall in worldObject.getWallList().Values)
                        {
                            walls.Append(wall + "\n");
                        }
                        Networking.Send(socketState.TheSocket, walls.ToString());
                    }
                }
            }
            catch { }

            //client should be fully connected and all world data should be sent to the client
            //now handle any other objects that are currently on the server, send those to client
            //and receive information sent by the client as they play the game
            socketState.OnNetworkAction = JsonObjectProcess;
            Networking.GetData(socketState);
        }

        /// <summary>
        /// Process information and objects currently on the server and received by other clients and
        /// update the game for every client based on this new data.
        /// </summary>
        /// <param name="socketState"></param>
        private static void JsonObjectProcess(SocketState socketState)
        {
            if (socketState.ErrorOccured)
            {
                return;
            }

            //lock to prevent cross-threading
            lock (worldObject)
            {
                try
                {
                    string totalData = socketState.GetData();
                    string[] parts = Regex.Split(totalData, @"(?<=[\n])");
                    foreach (string rMessage in parts)
                    {
                        if (rMessage.Length == 0)
                        {
                            continue;
                        }
                        if (rMessage[rMessage.Length - 1] != '\n')
                        {
                            break;
                        }

                        ControlCommands ctrl = JsonConvert.DeserializeObject<ControlCommands>(rMessage);

                        if ((int)socketState.ID == -1) //check if dead
                        {
                            return;
                        }

                        //receives user commands from each individual client and add that to the 
                        //dictionary based on what client sent the request
                        userCommands[socketState.ID] = ctrl;

                        socketState.RemoveData(0, rMessage.Length);
                    }
                }
                catch{ }

                //continue handling and processing data surrounding the client's gameplay
                socketState.OnNetworkAction = new Action<SocketState>(JsonObjectProcess);
                Networking.GetData(socketState);
            }
        }

        /// <summary>
        /// Adds all current objects that are not dead to the string to send to all clients.
        /// This is essentially the string that every client reads that determines what is 
        /// happening in the world/game during that frame, including movement, firing, and spawning.
        /// </summary>
        /// <returns></returns>
        private static string WorldObjectStringCreator()
        {
            StringBuilder worldData = new StringBuilder();

            //checks if a tank is dead, and or, waiting to respawn in the game
            foreach (Tank tank in worldObject.getTankList().Values)
            {
                if (!tank.GetWaitingToRespawn())
                {
                    worldData.Append(tank.ToString() + "\n");
                }
                if (tank.GetDied())
                {
                    tank.SetwaitingToRespawn();
                }
            }

            //checks all projectiles that have been fired by clients in that frame
            foreach (Projectile proj in worldObject.GetProjectileList().Values)
            {
                worldData.Append(proj.ToString() + "\n");
            }

            //checks all current powerups still "alive" in the world
            foreach (Powerup power in worldObject.GetPowerUpList().Values)
            {
                worldData.Append(power.ToString() + "\n");
            }

            //checks all beams that have been fired in that frame
            foreach (Beam beam in worldObject.beams.Values)
            {
                worldData.Append(beam.ToString() + "\n");
            }
            
            //all data to be sent to all clients is inside this string
            return worldData.ToString();
        }

        /// <summary>
        /// Update the world objects to be sent to the clients using the WorldObjectStringCreator method.
        /// </summary>
        private static void UpdateWorldObjects()
        {
            //for every tank, ensure they can wrap around the world and handle if they have died
            foreach (Tank tank in worldObject.getTankList().Values)
            {
                WrapAround(tank);

                if (tank.GetDied())
                {
                    worldObject.RandomRespawnTank(tank, FrameCounter);
                }
            }

            //For every projectile, check if it collides with another object, update the velocity of the projectile
            //and check if the projectile has or hasn't died and update the location or eligibility of the projectile.
            foreach (Projectile proj in worldObject.GetProjectileList().Values)
            {
                worldObject.ProjectileCollision(proj, FrameCounter);

                Vector2D velocityProj = proj.GetDirectionOfTank() * 25.0;
                Vector2D newLocationProj = proj.GetLocationOfProjectile() + velocityProj;

                if (!proj.ProjDied())
                {
                    proj.UpdateLocationProj(newLocationProj);
                }
                else
                {
                    projUsed.Add(proj);
                }

            }

            //for each powerup, check if it has collided with a tank after spawn, if so, set it to dead
            foreach (Powerup power in worldObject.GetPowerUpList().Values)
            {
                if (worldObject.TankPowerupCollisions(power))
                {
                    power.SetDead(true);
                }
            }

            //Handle all commands sent by the user. These determine the location and firing of a tank.
            foreach (long id in userCommands.Keys)
            {
                HandleCommand(id, userCommands[id]);
            }
        }

        /// <summary>
        /// Determines a tank's position and allows it to wrap around the world.
        /// </summary>
        /// <param name="tank"></param>
        private static void WrapAround(Tank tank)
        {
            int halfWorldSize = worldObject.GetWorldSize() / 2;
            //check -x,x position is less than half of the world othwrwise flip 
            if (tank.GetLocationOfTank().GetX() > halfWorldSize || tank.GetLocationOfTank().GetX() < -halfWorldSize)
            {
                //if true for -x or x
                tank.xWrapAround();
            }
            //check x,y position is less than half of the world othwrwise flip 
            if (tank.GetLocationOfTank().GetY() > halfWorldSize || tank.GetLocationOfTank().GetY() < -halfWorldSize)
            {
                //if true for y- or y
                tank.yWrapAround();
            }
        }

        /// <summary>
        /// Handles the information received from the movement json and updates the tank accordingly
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        private static void HandleCommand(long id, ControlCommands command)
        {
            //determines if a command exists or if a tank can be affected by the command
            if (command is null)
            {
                return;
            }

            worldObject.getTankList().TryGetValue((int)id, out Tank tank);
            if (tank is null || tank.GetDied())
            {
                return;
            }

            lock (worldObject)
            {
                //determines the spawning of powerups and the next time a powerup can spawn in
                Random rand = new Random();

                if (nextPowerupTime == 0)
                {
                    nextPowerupTime = FrameCounter + (int)rand.Next(0, 1650);
                }
                if (nextPowerupTime <= FrameCounter)
                {
                    if (worldObject.GetPowerUpList().Count < 2)
                    {
                        //adds a powerup to the world and updates its data
                        worldObject.AddPowerups();
                    }
                    nextPowerupTime = FrameCounter + (int)rand.Next(0, 1650);
                }

                //update tank information for movement and aiming depending on the frame
                tank.SetAiming(command.tdir);
                tank.SetOrientation(command.moving);
                Vector2D velocity = tank.GetOrientationOfTank() * 3.0;
                Vector2D newLocationTank = tank.GetLocationOfTank() + velocity;
                if (worldObject.TankWallCollision(newLocationTank) == false)
                {
                    tank.UpdateLocation(velocity);
                }

                //check if proj is "main" and checks if the client is eligible to fire since their last fire
                if (command.fire == "main")
                {
                    lock (worldObject)
                    {
                        if (lastFireClient.TryGetValue(id, out int frameFiredOn))
                        {
                            if (worldObject.framesPerShot + frameFiredOn > FrameCounter)
                            {
                                return;
                            }
                        }
                        //if the client can fire, create the projectile and update its direction data to fire
                        Projectile projObj = new Projectile();
                        projObj.SetProjectileName(tank.GetID());
                        worldObject.AddProj(projObj.GetProjectileName(), projObj);
                        worldObject.UpdateProjData(projObj.GetProjectileName(), tank);

                        //set the last time the client has fired to the current frame
                        lastFireClient[id] = FrameCounter;
                    }
                }

                //Check if the client wants to fire a beam and adds the beam if a client has picked up a powerup
                if (command.fire == "alt")
                {
                    //create beam check for intersections
                    if (tank.GetPowerupTotal() > 0)
                    {
                        worldObject.AddBeam(tank, FrameCounter);
                        tank.DecrementPowerupTotal();
                    }
                }
                userCommands.Remove(id);
            }
        }

        /// <summary>
        /// Sned player ID and universe size to player
        /// </summary>
        /// <param name="socketState"></param>
        /// <param name="playerID"></param>
        private static void SendStartupData(SocketState socketState, int playerID)
        {
            lock (socketState.TheSocket)
            {
                Networking.Send(socketState.TheSocket, playerID.ToString() + "\n" + gameSettings.UniverseSize + "\n");
            }
        }

        /// <summary>
        /// Removes the client from the dictionary if they have disconnected
        /// </summary>
        /// <param name="id"></param>
        private static void RemoveClient(long id)
        {
            Console.WriteLine("Client " + id + " disconnected");
            lock (connectedClients)
            {
                worldObject.TankSetDisconnected((int)id);
                connectedClients.Remove(id);
            }
        }
    }
}