using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    /// <summary>
    /// <Author>Kyla Ryan and Nicholas Provenzano(PS8)</Author>
    /// <Author>Kyla Ryan and Elizabeth Lukner (PS9)</Author>
    /// Creates a powerup object with field parameters to create JSON objects from.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        [JsonProperty(PropertyName = "power")]
        private int powerUpID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "died")]
        private bool died;

        //int to hold amount of time for spanwning in of powerup
        private int timeTillSpawn; 

        /// <summary>
        /// Default constructor
        /// </summary>
        public Powerup()
        {
        }

        /// <summary>
        /// Constructor with parameters.
        /// </summary>
        /// <param name="powerupID"></param>
        /// <param name="locationOfPowerUp"></param>
        /// <param name="powerupDied"></param>
        public Powerup(int powerupID, Vector2D locationOfPowerUp, bool powerupDied)
        {
            this.powerUpID = powerupID;
            this.location = new Vector2D(locationOfPowerUp);
            this.died = powerupDied;
        }

        /// <summary>
        /// Get powerup ID
        /// </summary>
        /// <returns></returns>
        public int GetPowerUpID()
        {
            return powerUpID;
        }

        /// <summary>
        /// Get location of powerup
        /// </summary>
        /// <returns></returns>
        public Vector2D GetLocationOfPowerUp()
        {
            return location;
        }

        /// <summary>
        /// Get if powerup has died
        /// </summary>
        /// <returns></returns>
        public bool GetPowerUpDied()
        {
            return died;
        }

        /// <summary>
        /// Sets the field determining if a powerup is dead
        /// </summary>
        /// <param name="dead"></param>
        public void SetDead(bool dead)
        {
            this.died = dead;
        }

        /// <summary>
        /// Updates the information of a powerup
        /// </summary>
        /// <param name="locationOfPowerUp"></param>
        /// <param name="powerupDied"></param>
        public void UpdatePowerup(Vector2D locationOfPowerUp,bool powerupDied)
        {
            this.location = new Vector2D(locationOfPowerUp);
            this.died = powerupDied;
        }

        /// <summary>
        /// Overrides the to string method and serializes the object to send to all clients
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string powerup = JsonConvert.SerializeObject(this);
            return powerup;
        }
    }
}
