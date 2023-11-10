using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    /// <summary>
    /// Class that handles creating and drawing beams on the DrawingPanel in Form1.
    /// </summary>
    class BeamAnimation
    {
        //data linked to start and angle of the beam.
        private Vector2D origin;
        private Vector2D orientation;

        //Number of frames to allow the beam animation to be "alive"
        private int numFrames;


        /// <summary>
        /// Constructor for creating a BeamAnimation object.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dir"></param>
        public BeamAnimation(Vector2D position, Vector2D dir)
        {
            origin = new Vector2D(position);
            orientation = new Vector2D(dir);
        }

        /// <summary>
        /// Gets number of frames
        /// </summary>
        /// <returns></returns>
        public int GetFrames()
        {
            return numFrames;
        }

        /// <summary>
        /// Gets origin of beam
        /// </summary>
        /// <returns></returns>
        public Vector2D GetOrigin()
        {
            return origin;
        }

        /// <summary>
        /// gets angle of beam
        /// </summary>
        /// <returns></returns>
        public Vector2D GetOrientation()
        {
            return orientation;
        }

        /// <summary>
        /// Drawer with delegate (from DrawingPanel) to draw beams.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void BeamDrawer(object o, PaintEventArgs e)
        {
            Beam beam = o as Beam;
            using (System.Drawing.Pen beamPen = new System.Drawing.Pen(System.Drawing.Color.White, 15f -numFrames))
            {
                e.Graphics.DrawLine(beamPen, 0, 0, 0, -2000);
            }
            numFrames++;
        }
    }
}
