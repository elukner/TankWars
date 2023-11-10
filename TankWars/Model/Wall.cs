using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace TankWars
{
    /// <summary>
    /// <Author>Kyla Ryan and Nicholas Provenzano</Author>
    /// Creates wall object with set field parameters to create JSON objects from.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        //Fields linked to wall objects
        [JsonProperty(PropertyName = "wall")]
        private int wall;

        [JsonProperty(PropertyName = "p1")]
        private Vector2D ePointOne;

        [JsonProperty(PropertyName = "p2")]
        private Vector2D ePointTwo;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Wall()
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="wallName"></param>
        /// <param name="pointOne"></param>
        /// <param name="pointTwo"></param>
        public Wall(int wallName, Vector2D pointOne, Vector2D pointTwo)
        {
            this.wall = wallName;
            this.ePointOne = new Vector2D(pointOne);
            this.ePointTwo = new Vector2D(pointTwo);
        }

        /// <summary>
        /// Get point one of the wall.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetPointOne()
        {
            return ePointOne;
        }

        /// <summary>
        /// Get point two of the wall
        /// </summary>
        /// <returns></returns>
        public Vector2D GetPointTwo()
        {
            return ePointTwo;
        }

        /// <summary>
        /// Get wall ID
        /// </summary>
        /// <returns></returns>
        public int GetWallName()
        {
            return wall;
        }

        /// <summary>
        /// Overrides the to string to serialize walls to send to clients for processing.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string wall = JsonConvert.SerializeObject(this);
            return wall;
        }
    }
}
