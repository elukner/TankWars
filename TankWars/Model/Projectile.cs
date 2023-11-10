using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TankWars
{
    /// <summary>
    /// <Author>Kyla Ryan and Nicholas Provenzano(PS8)</Author>
    /// <Author>Kyla Ryan and Elizabeth Lukner (PS9)</Author>
    /// Projectile class with set field parameters to create JSON objects from.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        //Fields of the projectile object
        [JsonProperty(PropertyName = "proj")]
        private int projectile;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D dir;

        [JsonProperty(PropertyName = "died")]
        private bool died;

        [JsonProperty(PropertyName = "owner")]
        private int owner;

        //Holds the next available name ID of the projectile
        private static int nextName =0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Projectile()
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="projectileName"></param>
        /// <param name="locationOfProj"></param>
        /// <param name="directionOfTank"></param>
        /// <param name="ownerOfProjectile"></param>
        /// <param name="projDied"></param>
        public Projectile(int projectileName, Vector2D locationOfProj, Vector2D directionOfTank, int ownerOfProjectile, bool projDied)
        {
            this.projectile = projectileName;
            this.owner = ownerOfProjectile;
            this.location = locationOfProj;
            this.dir = directionOfTank;
            this.died = projDied;
        }

        /// <summary>
        /// Sets the field of the projectile to a boolean to show if it has died
        /// </summary>
        /// <param name="dead"></param>
        public void SetDied(bool dead)
        {
            this.died = dead;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void SetOwner(int name)
        {
            this.owner = name;
        }

        /// <summary>
        /// Sets the direction of the projectile
        /// </summary>
        /// <param name="dir"></param>
        public void SetDirection(Vector2D dir)
        {
            this.dir = dir;
        }

        /// <summary>
        /// Get projectile location
        /// </summary>
        /// <returns></returns>
        public Vector2D GetLocationOfProjectile()
        {
            return location;
        }

        /// <summary>
        /// Get tank direction
        /// </summary>
        /// <returns></returns>
        public Vector2D GetDirectionOfTank()
        {
            return dir;
        }

        /// <summary>
        /// Get owner of projectile
        /// </summary>
        /// <returns></returns>
        public int GetOwnerOfProjectile()
        {
            return owner;
        }

        /// <summary>
        /// Get projectile ID
        /// </summary>
        /// <returns></returns>
        public int GetProjectileName()
        {
            return projectile;
        }

        /// <summary>
        /// Set next projectile ID
        /// </summary>
        /// <returns></returns>
        public int SetNextProjectileName()
        {  
           return nextName++;
        }

        /// <summary>
        /// Set projectile ID
        /// </summary>
        /// <returns></returns>
        public void SetProjectileName(int proj)
        {
            this.projectile = proj;
        }


        /// <summary>
        /// Get if projectile has died
        /// </summary>
        /// <returns></returns>
        public bool ProjDied()
        {
            return died;
        }

        /// <summary>
        /// Sets the location of the projectile
        /// </summary>
        /// <param name="newLocation"></param>
        public void SetLocation(Vector2D newLocation)
        {
            this.location = newLocation;
        }

        /// <summary>
        /// Updates the location of the tank to account for the movement velocity
        /// </summary>
        /// <param name="position"></param>
        public void UpdateLocationProj(Vector2D position)
        {
            this.location = position;
        }

        /// <summary>
        /// Overrides the to string to serialize objects to send to the clients
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string proj = JsonConvert.SerializeObject(this);
            return proj;
        }
    }
}
