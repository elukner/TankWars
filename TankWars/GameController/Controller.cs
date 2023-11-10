using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TankWars
{
    /// <summary>
    /// <Author> Kyla Ryan and Nicholas Provenzano</Author>
    /// Controller to handle/receive and send communication between the Model, View, and server.
    /// </summary>
    public class Controller
    {
        //preset data values for world and player information
        private int worldSize = 0;
        private int playerID = 0;
        private string playerName;

        //world to hold all values in and control commands object to handle player action events.
        private World gameWorld;
        private ControlCommands ctl;

        /// <summary>
        /// Constructor for a controller object that builds the world and control commands object to be used.
        /// </summary>
        public Controller()
        {
            gameWorld = new World();
            ctl = new ControlCommands();
        }

        //Below are event handler delegates to handle data and events triggered from the Form1/DrawingPanel
        public delegate void DataHandler();
        public event DataHandler UpdateArrived;

        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        public delegate void gameConnected(World gameWorld, int playerID);
        public event gameConnected gameConnect;

        public delegate void BeamFired(Beam beam);
        public event BeamFired beamFired;

        /// <summary>
        /// Beginning of connection between client and server.
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="playersName"></param>
        public void Connect(string addr, string playersName)
        {
            playerName = playersName;
            Networking.ConnectToServer(new Action<SocketState>(FirstConnectToServer), addr, 11000);
        }

        /// <summary>
        /// Callback method for the connection between client and server to handle data sent.
        /// </summary>
        /// <param name="socketState"></param>
        private void FirstConnectToServer(SocketState socketState)
        {
            if (socketState.ErrorOccured)
            {
                Error("Error connecting to the server.");
                return;
            }
            else
            {
                // need to process the data of the player's name to send to the server
                socketState.OnNetworkAction = ProcessMessages;

                // need to send the player's name to update the server 
                Networking.Send(socketState.TheSocket, playerName + "\n");
                Networking.GetData(socketState);
            }
        }

        /// <summary>
        /// Used to process the first two messages received by the server: world size and player ID.
        /// If this is received, we must use the information to alter the world of the View and set the player ID.
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
        {
            if (state.ErrorOccured)
            {
                Error("Lost connection to server.  :( ");
                return;
            }
            try
            {
                string totalData = state.GetData();

                //Process the data received by the server.
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");
                List<string> newMessage = new List<string>();
                foreach(string p in parts)
                {
                    if ( p.Length==0)
                    {
                        continue;
                    }
                    if(p[p.Length - 1] != '\n')
                    {
                        break;
                    }
                    newMessage.Add(p);
                }

                //Hold and remove data received.
                playerID = int.Parse(newMessage[0]);
                worldSize = int.Parse(newMessage[1]);
                state.RemoveData(0, parts[0].Length);
                state.RemoveData(0, parts[1].Length);
            }
            catch
            {
            }

            //update game world
            gameWorld.size=(worldSize);

            //Event handler in Form1 that sets the world and player ID in Form1.
            gameConnect(gameWorld, playerID);
            
            //If we get this far, begin to process JSONS sent by the server.
            state.OnNetworkAction = JsonObjectProcess;
            Networking.GetData(state);
            
        }

        /// <summary>
        /// Method to process and send JSONS between the server and determine what objects/actions to take
        /// depending on the data.
        /// </summary>
        /// <param name="socketState"></param>
        private void JsonObjectProcess(SocketState socketState)
        {
            if (socketState.ErrorOccured)
            {
                Error("Error connecting to the server.");
                return;
            }

            //lock to prevent cross-threading
            lock (this.gameWorld)
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
                        
                        //Types of objects we can receive from the server.
                        JObject obj = JObject.Parse(rMessage);
                        JToken tank = obj["tank"];
                        JToken proj = obj["proj"];
                        JToken beam = obj["beam"];
                        JToken power = obj["power"];
                        JToken wall = obj["wall"];

                        //Deserialize walls.
                        if (wall != null)
                        {
                            Wall newWall = JsonConvert.DeserializeObject<Wall>(rMessage);
                            if (newWall != null)
                            {

                                gameWorld.getWallList()[newWall.GetWallName()] = newWall;
                            }
                        }

                        //Deserialize tanks.
                        if (tank != null)
                        {
                            Tank newTank = JsonConvert.DeserializeObject<Tank>(rMessage);
                            if (newTank != null)
                            {
                                gameWorld.getTankList()[newTank.GetID()] = newTank;
                            }
  
                        }

                        //Deserialize projectiles.
                        if (proj != null)
                        {
                            Projectile newProj = JsonConvert.DeserializeObject<Projectile>(rMessage);
                            if (newProj != null)
                            {
                                if (newProj.ProjDied())
                                {
                                    gameWorld.GetProjectileList().Remove(newProj.GetProjectileName());
                                }
                                gameWorld.GetProjectileList()[newProj.GetProjectileName()] = newProj;
                            }
                        }

                        //Deserialize beams.
                        if (beam != null)
                        {
                            Beam newBeam = JsonConvert.DeserializeObject<Beam>(rMessage);
                            if (newBeam != null)
                            {
                                beamFired(newBeam);
                            }
                        }

                        //Deserialize powerups.
                        if (power != null)
                        {
                            Powerup newPower = JsonConvert.DeserializeObject<Powerup>(rMessage);
                            if (newPower != null)
                            {
                                if (newPower.GetPowerUpDied())
                                {
                                    gameWorld.GetPowerUpList().Remove(newPower.GetPowerUpID());
                                }
                                gameWorld.GetPowerUpList()[newPower.GetPowerUpID()] = newPower;
                            }
                        }

                        //Remove the data to empty the socket to receive more data.
                        socketState.RemoveData(0, rMessage.Length);
                    }
                }
                catch
                {
                }

                //Essentially an infinite loop to receive data.
                socketState.OnNetworkAction = new Action<SocketState>(this.JsonObjectProcess);
            }

            //Update and draw changes to world.
            UpdateArrived();

            //Send player action data from the ControlCommands object.
            Networking.Send(socketState.TheSocket, ctl.JsonStringFromEvents() + "\n");

            Networking.GetData(socketState);
        }
        
        /// <summary>
        /// Getter for the world.
        /// </summary>
        /// <returns></returns>
        public World GetWorld()
        {
            return gameWorld;
        }

        /// <summary>
        /// If a button was released, handle it in the ControlCommands.
        /// </summary>
        /// <param name="direction"></param>
        public void CancelMoveRequest(string direction)
        {
            ctl.SetCancelMove(direction);
        }

        /// <summary>
        /// If a button was pressed, handle it in the ControlCommands.
        /// </summary>
        /// <param name="direction"></param>
        public void HandleMoveRequest(string direction)
        {
            ctl.SetMove(direction);
        }

        /// <summary>
        /// If mouse was moved, handle it in the ControlCommands.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void HandleMouseMoveRequest(double x, double y)
        {
            ctl.SetTurretPosition(x, y);
        }

        /// <summary>
        /// If a mouse button was clicked, handle it in the ControlCommands.
        /// </summary>
        /// <param name="button"></param>
        public void HandleMouseRequest(string button)
        {
            ctl.SetFire(button);
        }

        /// <summary>
        /// If a mouse button was released, handle it in the ControlCommands.
        /// </summary>
        /// <param name="button"></param>
        public void CancelMouseRequest(string button)
        {
            ctl.SetCancelFire(button);
        }
    }
}
