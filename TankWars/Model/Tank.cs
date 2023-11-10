using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace TankWars
{
    /// <summary>
    /// <Author>Kyla Ryan and Nicholas Provenzano(PS8)</Author>
    /// <Author>Kyla Ryan  and Elizabeth Lukner (PS9)</Author>
    /// Class to create tank objects with set parameters to create JSON objects from.
    /// TODO need method to respawn (set location, orientation, last fired, hp, aiming)
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        //All fields of a tnak object to be set and determines the current state of the tank.
        [JsonProperty(PropertyName = "tank")]
        private int ID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D aiming = new Vector2D(0.0, -1.0);

        [JsonProperty(PropertyName = "name")]
        private string name;

        [JsonProperty(PropertyName = "hp")]
        private int hitPoints = 3; //Constants.MaxHP;

        [JsonProperty(PropertyName = "score")]
        private int score = 0;

        [JsonProperty(PropertyName = "died")]
        private bool died = false;

        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;

        [JsonProperty(PropertyName = "join")]
        private bool joined = false;

        //field to determine firing rate - # of frames since last shot
        //need field to determine respawn rate - # of frames since last death
        private int _timeTillRespawn;

        //checks if the tank is dead and needs to be respawned
        private bool waitingToRespawn = false;

        //number of powerups the tank has picked up
        private int powerupTotal = 0;

        /// <summary>
        /// Default constructor for the tank
        /// </summary>
        public Tank()
        {
        }

        /// <summary>
        /// Constructor for tank with parameters
        /// </summary>
        /// <param name="tankName"></param>
        /// <param name="tankID"></param>
        /// <param name="location"></param>
        /// <param name="aiming"></param>
        /// <param name="orientation"></param>
        /// <param name="hp"></param>
        /// <param name="scoreOfTank"></param>
        /// <param name="tankDied"></param>
        /// <param name="tankDisconnected"></param>
        public Tank(string tankName, int tankID, Vector2D location, Vector2D aiming, Vector2D orientation, int hp, int scoreOfTank, bool tankDied, bool tankDisconnected)
        {
            this.ID = tankID;
            this.name = tankName;
            this.orientation = new Vector2D(orientation);
            this.location = new Vector2D(location);
            this.aiming = new Vector2D(aiming);
            this.hitPoints = hp;
            this.score = scoreOfTank;
            this.died = tankDied;
            this.disconnected = tankDisconnected;
            this.joined = false;
        }

        /// <summary>
        /// Increments the number of powerups the tank has picked up
        /// </summary>
        public void IncPowerupTotal()
        {
            powerupTotal++;
        }

        /// <summary>
        /// Gets the number of powerups the tank has picked up
        /// </summary>
        /// <returns></returns>
        public int GetPowerupTotal()
        {
            return powerupTotal;
        }

        /// <summary>
        /// Decrements the number of powerups for when a beam has been fired.
        /// </summary>
        /// <returns></returns>
        public int DecrementPowerupTotal()
        {
            return this.powerupTotal--;
        }

        /// <summary>
        /// Returns the time till a respawn
        /// </summary>
        /// <returns></returns>
        public int GetTimeTillRespawn()
        {
            return this._timeTillRespawn;
        }

        /// <summary>
        /// Gets turret direction
        /// </summary>
        /// <returns></returns>
        public Vector2D GetTurretDirection()
        {
            return this.aiming;
        }

        /// <summary>
        /// Gets Tank ID
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.ID;
        }

        /// <summary>
        /// Gets tank name (player)
        /// </summary>
        /// <returns></returns>
        public string GetTankName()
        {
            return name;
        }

        /// <summary>
        /// Gets score of tank
        /// </summary>
        /// <returns></returns>
        public int GetScore()
        {
            return score;
        }

        /// <summary>
        /// Adds to the score if the tank has killed another tank
        /// </summary>
        public void IncrementScore()
        {
            score++;
        }

        /// <summary>
        /// Gets hit points of tank
        /// </summary>
        /// <returns></returns>
        public int GetHitPoints()
        {
            return hitPoints;
        }

        /// <summary>
        /// Gets if tank has died
        /// </summary>
        /// <returns></returns>
        public bool GetDied()
        {
            return died;
        }

        /// <summary>
        /// Gets if tank has disconnected
        /// </summary>
        /// <returns></returns>
        public bool GetTankDisconnected()
        {
            return disconnected;
        }

        /// <summary>
        /// Gets location of tank
        /// </summary>
        /// <returns></returns>
        public Vector2D GetLocationOfTank()
        {
            return location;
        }

        /// <summary>
        /// Gets orientation of tank
        /// </summary>
        /// <returns></returns>
        public Vector2D GetOrientationOfTank()
        {
            return orientation;
        }

        /// <summary>
        /// sets the orientation of the tank based on the movement json received
        /// </summary>
        public void SetOrientation(string movement)
        {
            Vector2D orient = this.orientation;
            if (movement == "up")
            {
                orient = new Vector2D(0, -1);
            }
            else if (movement == "down")
            {
                orient = new Vector2D(0, 1);
            }
            else if (movement == "left")
            {
                orient = new Vector2D(-1, 0);
            }
            else if (movement == "right")
            {
                orient = new Vector2D(1, 0);
            }
            else
            {
                orient = new Vector2D(0, 0);
            }

            this.orientation = orient;
        }

        /// <summary>
        /// Updates the location of the tank to account for the movement velocity
        /// </summary>
        /// <param name="velocity"></param>
        public void UpdateLocation(Vector2D velocity)
        {
            this.location += velocity;
        }

        /// <summary>
        /// updates the turret vector based on the json received
        /// </summary>
        /// <param name="aim"></param>
        public void SetAiming(Vector2D aim)
        {
            aim.Normalize();
            this.aiming = aim;
        }

        /// <summary>
        /// Sets the hit points of the tank
        /// </summary>
        /// <param name="hp"></param>
        public void SetHp(int hp)
        {
            this.hitPoints = hp;
        }

        /// <summary>
        /// sets the respawn frame of the tank
        /// </summary>
        /// <param name="dead"></param>
        /// <param name="frame"></param>
        public void SetRespawnFrame(int timeTillRespawn)
        {
            _timeTillRespawn = timeTillRespawn;
        }


        /// <summary>
        /// sets died to true or false
        /// </summary>
        /// <param name="dead"></param>
        /// <param name="frame"></param>
        public void SetDied(bool dead)
        {

            this.died = dead;

            if (!died)
            {
                //sets respawn frame to a max value if not dead
                SetRespawnFrame(int.MaxValue);
            }
        }

        /// <summary>
        /// Sets disconnected to true or false depending on the status of the client.
        /// </summary>
        /// <param name="discon"></param>
        public void SetDisconnected(bool discon)
        {
            this.disconnected = discon;
        }

        /// <summary>
        /// overrides the to string to serialize the tank object to sent to clients for processing
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string tank = JsonConvert.SerializeObject(this);
            return tank;
        }

        /// <summary>
        /// Sets the wraparound location to the opposite x coordinate of the map if the end of the map was reached
        /// </summary>
        public void xWrapAround()
        {
            this.location = new Vector2D(this.GetLocationOfTank().GetX() * -1.0, this.GetLocationOfTank().GetY());
        }

        /// <summary>
        /// Sets the wraparound location to the opposite y coordinate of the map if the end of the map was reached
        /// </summary>
        public void yWrapAround()
        {
            this.location = new Vector2D(this.GetLocationOfTank().GetX(), this.GetLocationOfTank().GetY() * -1.0);
        }


        /// <summary>
        /// Sets waiting to respawn to true if the tank is dead
        /// </summary>
        public void SetwaitingToRespawn()
        {
            waitingToRespawn = true;
        }

        /// <summary>
        /// Gets the waiting time to respawn of a tank
        /// </summary>
        /// <returns></returns>
        public bool GetWaitingToRespawn()
        {
            return waitingToRespawn;
        }


        /// <summary>
        /// Updates the tank information to that of the parameters.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="aiming"></param>
        /// <param name="orientation"></param>
        /// <param name="hp"></param>
        /// <param name="scoreOfTank"></param>
        /// <param name="tankDied"></param>
        /// <param name="tankDisconnected"></param>
        public void UpdateTank(Vector2D location, Vector2D aiming, Vector2D orientation, int hp, int scoreOfTank, bool tankDied, bool tankDisconnected)
        {
            this.orientation = new Vector2D(orientation);
            this.location = new Vector2D(location);
            this.aiming = new Vector2D(aiming);
            this.hitPoints = hp;
            this.score = scoreOfTank;
            this.died = tankDied;
            this.disconnected = tankDisconnected;
            this.joined = false;
            this.waitingToRespawn = false;
            this.powerupTotal = 0;

        }
    }
}