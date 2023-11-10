using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars

/// <summary>
/// <Author> Kyla Ryan and Nicholas Provenzano</Author>
/// 
/// Class that handles the control events sent by the Form1->Controller->ControlCommands.
/// Constructs the JSON to be sent to the server that determines player actions: moving, firing.
/// </summary>
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommands
    {       
        [JsonProperty]
        public string moving { get; set; }
        //none, up, left, down, right

        [JsonProperty]
        public string fire { get; set; }
        //none, main (proj), alt (beam)

        [JsonProperty]
        public Vector2D tdir { get; set; }
        //must normalize for turret diretion

        //List to hold the controls sent by the player for movement.
        //Allows us to see the previous movement button for record.
        List<string> controls = new List<string>();

        /// <summary>
        /// Constructor for the ControlCommands object; sets basic JSON action fields.
        /// </summary>
       public ControlCommands()
        {
            moving = "none";
            fire = "none";
            tdir = new Vector2D();
            tdir.Normalize();
        }

        /// <summary>
        /// If a movement button was pressed, and is not a duplicate press, add to controls string.
        /// </summary>
        /// <param name="direction"></param>
        public void SetMove(string direction)
        {
            if (!controls.Contains(direction)) 
            { 
                controls.Add(direction); 
            }
        }

        /// <summary>
        /// Returns the action that will be sent to the server.
        /// </summary>
        /// <returns></returns>
        public string Move()
        {
            if (controls.Count == 0)
            {
                return "none";
            }
            moving = controls[controls.Count - 1];
            return controls[controls.Count - 1];
        }

        /// <summary>
        /// If a button was released from being pressed, find it in the list and remove it.
        /// </summary>
        /// <param name="direction"></param>
        public void SetCancelMove(string direction)
        {
            controls.Remove(direction);
        }

        /// <summary>
        /// Determine what firing action is being used based on button click and set field parameter to that string.
        /// </summary>
        /// <param name="button"></param>
        public void SetFire(string button)
        {
            //normal projectile
            if (button.Equals("main"))
            {
                fire = "main";
            }

            //beam attack
            if (button.Equals("alt"))
            {
                fire = "alt";
            }
        }

        /// <summary>
        /// If a mouse button was released, set firing to none based on that button.
        /// </summary>
        /// <param name="button"></param>
        public void SetCancelFire(string button)
        {
            //parameter will always return "none" as no buttons are pressed
            fire = button;
            
        }

        /// <summary>
        /// Sets the direction the turret is aiming in based on the mouse position given through the parameters.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetTurretPosition(double x, double y)
        {
            tdir = new Vector2D(x, y);
            //must normalize
            tdir.Normalize();
        }

        /// <summary>
        /// Sets up the JSON string to send to the server.
        /// </summary>
        /// <returns></returns>
        public string JsonStringFromEvents()
        {
            string movement = JsonConvert.SerializeObject(this);
            return movement;
            //need to figure out how to get mouse position RELATIVE to tank and normalize it for the tdir
            //return "{\"moving\":\""+ Move() +"\",\"fire\":\"" + fire + "\",\"tdir\":{\"x\":"+tdir.GetX()+",\"y\":"+tdir.GetY()+"}}";
        }
    }
}
