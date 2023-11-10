using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars

{
    /// <summary>
    /// <Author>Kyla Ryan and Nicholas Provenzano(PS8)</Author>
    /// <Author>Kyla Ryan and Elizabeth Lukner (PS9)</Author>
    /// Creates a beam object with set field parameters to create JSON objects from.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        //Field paramaters linked to beam objects.
        [JsonProperty(PropertyName = "beam")]
        private int beam;

        [JsonProperty(PropertyName = "org")]
        private Vector2D origin;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;

        [JsonProperty(PropertyName = "owner")]
        private int owner;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Beam()
        {
        }

        /// <summary>
        /// Constructor for beam with parameters.
        /// </summary>
        /// <param name="beamName"></param>
        /// <param name="originOfBeam"></param>
        /// <param name="directionOfBeam"></param>
        /// <param name="ownerOfBeam"></param>
        public Beam(int beamName, Vector2D originOfBeam, Vector2D directionOfBeam, int ownerOfBeam)
        {
            this.beam = beamName;
            this.owner = ownerOfBeam;
            this.origin = originOfBeam;
            this.direction = directionOfBeam;
        }

        /// <summary>
        /// Sets the owner of the beam to the parameter.
        /// </summary>
        /// <param name="ownerOfBeam"></param>
        public void SetOwnerBeam(int ownerOfBeam)
        {
            this.owner = ownerOfBeam;
        }
        
        /// <summary>
        /// Sets the origin of the beam to the parameter (typically the tank location)
        /// </summary>
        /// <param name="originOfBeam"></param>
        public void SetOriginOfBeam(Vector2D originOfBeam)
        {
            this.origin = originOfBeam;
        }

        /// <summary>
        /// SEts the direction of the beam (typically the tank's turret direction).
        /// </summary>
        /// <param name="directionOfBeam"></param>
        public void SetDirectionOfBeam(Vector2D directionOfBeam)
        {
            this.direction = directionOfBeam;
        }

        /// <summary>
        /// Get origin of the beam
        /// </summary>
        /// <returns></returns>
        public Vector2D GetOriginOfBeam()
        {
            return origin;
        }

        /// <summary>
        /// Get direction of the beam
        /// </summary>
        /// <returns></returns>
        public Vector2D GetDirectionOfBeam()
        {
            return direction;
        }

        /// <summary>
        /// Sets the name of the beam to the paramter (client ID)
        /// </summary>
        /// <param name="beamID"></param>
        public void SetBeamName(int beamID)
        {
            beam = beamID;
        }

        /// <summary>
        /// Get name of the beam
        /// </summary>
        /// <returns></returns>
        public int GetBeamName()
        {
            return beam;
        }

        /// <summary>
        /// Get owner of the beam
        /// </summary>
        /// <returns></returns>
        public int GetBeamOwner()
        {
            return owner;
        }

        /// <summary>
        /// Overrides the to string method to serialize the beam to send to clients for processing.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string beam = JsonConvert.SerializeObject(this);
            return beam;
        }
    }
}
