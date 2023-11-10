using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetworkUtil;

namespace TankWars
{
    /// <summary>
    /// <Author> Kyla Ryan and Nicholas Provenzano</Author>
    /// 
    /// Form1 (View in MVC) which starts and is the client applicaton. Communicates through the Controller to the
    /// server and Model. Uses events and handlers to communication to the Controller based on player input. 
    /// First, creates the application visual then prompts the user for imputs. If inputs of 
    /// IP address and player name are valid, and connect button clicked, begins the event to 
    /// connec the player to the server. Controller then handles the event and attempts to connect
    /// through the NetworkingController. If connection sucessfully made, draws the world and updates it
    /// to given parameters from the server. User is now able to play the game!
    /// </summary>
    public partial class Form1 : Form
    {
        //create an instance of the Controller
        private Controller controller;

        //game world to hold values and instances of players and objects
        private World gameWorld;

        //player ID of the current player who started the application
        private int playerID;

        //drawing panel to hold the world
        private DrawingPanel drawingPanel; 

        /// <summary>
        /// Constructor for the Form1 TankWars application. Creates the events and links them to their 
        /// handlers. Also creates the drawing panel and connects the Controller world to this one.
        /// </summary>
        /// <param name="ctl">Controller to sync to the game for communication</param>
        public Form1(Controller ctl)
        {
            InitializeComponent();
            //set our controller to the one created on startup
            controller = ctl; 

            //handlers to draw the updates sent by the server to the controller, handles and shows errors,
            //and updates world parameters based on if we successfully connected to the server.
            controller.UpdateArrived+= OnFrame;
            controller.Error += HandleError;
            controller.gameConnect += gameConnected;

            //handlers for key presses and key releases for player control
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;

            //updates the game world to this one from the controller, sets the window and drawingPanel size 
            //for the game world. Adds the menu to the window. Adds drawing panel as a receiver of inputs.
            gameWorld = controller.GetWorld();
            this.Size = new Size(900, 940);
            drawingPanel = new DrawingPanel(gameWorld);
            drawingPanel.Location = new Point(0, 40);
            drawingPanel.Size = new Size(900, 900);
            this.Controls.Add(drawingPanel);

            //handlers for mouse movement and cicks within the drawing panel for firing and aiming of the turret
            drawingPanel.MouseMove += HandleMouseMove;
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;
            controller.beamFired += drawingPanel.AddBeamAnimation;
        }

        /// <summary>
        /// Gets and returns the player ID of the current player who started the application
        /// </summary>
        /// <returns></returns>
        public int getPLayerID()
        {
            return playerID;
        }

        /// <summary>
        /// Handles and shows any errors received on connection to server attempt.
        /// </summary>
        /// <param name="error"></param>
        private void HandleError(string error)
        {
            MessageBox.Show(error);
            this.Invoke(new MethodInvoker(() => { 
            connectButton.Enabled = true; serverAddressText.Enabled = true; playerNameText.Enabled = true;
            }));
        }

        /// <summary>
        /// Invokes the world to be updated if data received from server
        /// </summary>
        private void OnFrame()
        {
            try
            {
                MethodInvoker invalidator = new MethodInvoker(() => this.Invalidate(true));
                this.Invoke(invalidator);
            }
            catch { };
        }

        /// <summary>
        /// Updates world information and player identification received from server on game connection
        /// </summary>
        /// <param name="world"></param>
        /// <param name="playerIdentification"></param>
        private void gameConnected(World world, int playerIdentification)
        {
            gameWorld = world;
            playerID = playerIdentification;
            drawingPanel.SetPlayerID(playerIdentification);
            StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// If the connect button was clicked, make sure proper IP address and name were entered.
        /// Sets text boxes and buttons to be void if data properly made. BEgins connection process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (this.serverAddressText.Text == "")
            {
                MessageBox.Show("Enter a valid server address");
            }
            else if (this.playerNameText.Text == "")
            {
                MessageBox.Show("Please enter a player name");
            }
            else if (this.playerNameText.Text.Length > 16)
            {
                MessageBox.Show("Player name needs to be less than 16 characters.");
            }
            else
            {
                this.connectButton.Enabled = false;
                this.serverAddressText.Enabled = false;
                this.playerNameText.Enabled = false;

                //start the conneciton process to the server
                controller.Connect(serverAddressText.Text, playerNameText.Text);
            }
        }

        /// <summary>
        /// Handles if a movement key was released and sends it to the controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            //CancelMoveRequest determines what action to be taken and for what movement.
            if (e.KeyCode == Keys.W)
            {
                controller.CancelMoveRequest("up");
            }
            if (e.KeyCode == Keys.A)
            {
                controller.CancelMoveRequest("left");
            }
            if (e.KeyCode == Keys.S)
            {
                controller.CancelMoveRequest("down");
            }
            if (e.KeyCode == Keys.D)
            {
                controller.CancelMoveRequest("right");
            }
        }

        /// <summary>
        /// Handles if a movememnt key was pressed, or if esc was pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                //exit program if esc pressed
                Application.Exit();
            }

            //HandleMoveRequest is a method that determiens what action to be taken for what movement
            //Parameters for the above method are what string is to be sent to the JSON for server interpretation.
            //SuppressKeyPress ensures no other key event will be handled
            if(e.KeyCode == Keys.W)
            {
                controller.HandleMoveRequest("up");
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == Keys.A)
            {
                controller.HandleMoveRequest("left");
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == Keys.S)
            {
                controller.HandleMoveRequest("down");
                e.SuppressKeyPress = true;
            }
            if (e.KeyCode == Keys.D)
            {
                controller.HandleMoveRequest("right");
                e.SuppressKeyPress = true;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles if a mouse button was pressed and determines which button for firing and projectile choice.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            //HandleMouseRequest determines what action to be taken for firing
            //projectile fire button
            if (e.Button == MouseButtons.Left)
            {
                controller.HandleMouseRequest("main");
            }
            //beam fire button
            if (e.Button == MouseButtons.Right)
            {
                controller.HandleMouseRequest("alt");
            }
            
        }

        /// <summary>
        /// Handles if a mouse button was released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            //CancelMouseRequest determines what to do if a button was released
            //sets fire to none for no firing
            if(e.Button == MouseButtons.Left) //&& e.Button == MouseButtons.Right)
            {
                controller.CancelMouseRequest("none");
            }
            if(e.Button == MouseButtons.Right)
            {
                controller.CancelMouseRequest("none");
            }
        }

        /// <summary>
        /// Handles if the mouse moved and gets the position it moves to for the turret aim
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            double x = e.X - drawingPanel.Size.Width / 2;
            double y = e.Y - drawingPanel.Size.Height / 2;

            //HandleMouseMoveRequest determines what do do with the x,y point of the mouse
            controller.HandleMouseMoveRequest(x, y);
        }

        /// <summary>
        /// Drawing panel that draws all objects currently in and on the world object.
        /// This is essentially the playing field for the player.
        /// <Author> Kyla Ryan and Nicholas Provenzano</Author>
        /// </summary>
        private class DrawingPanel : Panel
        {
            //world to connect to world received by 
            private World drawingWorld; 

            //If the player ID is still after server sends information back,
            //the world will not load.
            private int playerIDCheck=-1;

            //Below are all the images to be loaded when creating the DrawingPanel object.
            private Image backgroundImage;
            private Image blueTank;
            private Image blueTurret;
            private Image darkTank;
            private Image darkTurret;
            private Image greenTank;
            private Image greenTurret;
            private Image lightGreenTank;
            private Image lightGreenTurret;
            private Image orangeTank;
            private Image orangeTurret;
            private Image purpleTank;
            private Image purpleTurret;
            private Image redTank;
            private Image redTurret;
            private Image yellowTank;
            private Image yellowTurret;
            private Image blueShot;
            private Image brownShot;
            private Image greenShot;
            private Image greyShot;
            private Image redShot;
            private Image violetShot;
            private Image whiteShot;
            private Image yellowShot;
            private Image wallSprite;
            private string filePath;
            private int explosionTimer;

            //Holds the beams so they will be drawn.
            private HashSet<BeamAnimation> beamAnimations = new HashSet<BeamAnimation>();

            //Holds all beams that have been fired for removal from the beamAnimations hash-set.
            private List<BeamAnimation> firedBeams = new List<BeamAnimation>();

            /// <summary>
            /// DrawingPanel constructor that takes in a world object to generate the world from.
            /// </summary>
            /// <param name="w"></param>
            public DrawingPanel(World w)
            {
                //For smooth drawing...
                DoubleBuffered = true;

                //Set the private world parameter to the world received from the server.
                drawingWorld = w;

                //Need to be able to access the files in the resources project so we preset the filepath.
                filePath = "..\\..\\..\\Resources\\Images\\";

                //Set all field parameters to the files they coorespond with.
                backgroundImage = Image.FromFile(filePath + "Background.png");
                blueTank = Image.FromFile(filePath + "BlueTank.png");
                blueTurret = Image.FromFile(filePath + "BlueTurret.png");
                darkTank = Image.FromFile(filePath + "DarkTank.png");
                darkTurret = Image.FromFile(filePath + "DarkTurret.png");
                greenTank = Image.FromFile(filePath + "GreenTank.png");
                greenTurret = Image.FromFile(filePath + "GreenTurret.png");
                lightGreenTank = Image.FromFile(filePath + "LightGreenTank.png");
                lightGreenTurret = Image.FromFile(filePath + "LightGreenTurret.png");
                orangeTank = Image.FromFile(filePath + "OrangeTank.png");
                orangeTurret = Image.FromFile(filePath + "OrangeTurret.png");
                purpleTank = Image.FromFile(filePath + "PurpleTank.png");
                purpleTurret = Image.FromFile(filePath + "PurpleTurret.png");
                redTank = Image.FromFile(filePath + "RedTank.png");
                redTurret = Image.FromFile(filePath + "RedTurret.png");
                yellowTank = Image.FromFile(filePath + "YellowTank.png");
                yellowTurret = Image.FromFile(filePath + "YellowTurret.png");
                blueShot = Image.FromFile(filePath + "shot-blue.png");
                brownShot = Image.FromFile(filePath + "shot-brown.png");
                greenShot = Image.FromFile(filePath + "shot-green.png");
                greyShot = Image.FromFile(filePath + "shot-grey.png");
                redShot = Image.FromFile(filePath + "shot-red.png");
                violetShot = Image.FromFile(filePath + "shot-violet.png");
                whiteShot = Image.FromFile(filePath + "shot-white.png");
                yellowShot = Image.FromFile(filePath + "shot-yellow.png");
                wallSprite = Image.FromFile(filePath + "WallSprite.png");
            }
            
            /// <summary>
            /// Sets the current player's ID to the parameter.
            /// </summary>
            /// <param name="playerIdentification"></param>
            public void SetPlayerID (int playerIdentification)
            {
                playerIDCheck = playerIdentification;
            }

            //Delegate for the object drawers passed into the DrawObjectWithTransform method.
            public delegate void ObjectDrawer(object o, PaintEventArgs e);

            /// <summary>
            /// Provided code from Professor Kopta in Lab10 for us to use.
            /// Takes in account the present orientation of the panel with the origin <0,0> being the 
            /// top left corner and changes it so the orientation origin is in the center of the world and 
            /// draws objects with this in mind.
            /// </summary>
            /// <param name="e"></param>
            /// <param name="o"></param>
            /// <param name="worldX"></param>
            /// <param name="worldY"></param>
            /// <param name="angle"></param>
            /// <param name="drawer"></param>
            private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
            {
                System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();
                e.Graphics.TranslateTransform((int)worldX, (int)worldY);
                e.Graphics.RotateTransform((float)angle);
                drawer(o, e);
                e.Graphics.Transform = oldMatrix;
            }

            /// <summary>
            /// Method with Delegate structure to draw tanks according to specifications
            /// on size and images.
            /// </summary>
            /// <param name="o"></param>
            /// <param name="e"></param>
            private void TankDrawer(object o, PaintEventArgs e)
            {
                // object o is the tank, get the width and the height (60 x 60)
                Tank tank = o as Tank;
                int tWidth = 60;
                int tHeight = 60;
                
                //If tank dead, do not draw.
                if(tank.GetHitPoints() == 0)
                {
                    return;
                }

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                //Rectangle that the images are attached to.
                Rectangle tankRect = new Rectangle(-tWidth / 2, -tHeight / 2, tWidth, tHeight);
                Image tankColor;

                //Depending on the ID of the player, determine what color tank body we will use.
                switch (tank.GetID() % 8)
                {
                    case 0: //red
                        tankColor = redTank;
                        e.Graphics.DrawImage(tankColor, tankRect);
                        break;

                    case 1: //blue
                        tankColor = blueTank;
                        e.Graphics.DrawImage(tankColor, tankRect);
                        break;

                    case 2: //dark
                        tankColor = darkTank;
                        e.Graphics.DrawImage(tankColor, tankRect);
                        break;

                    case 3: //green
                        tankColor = greenTank;
                        e.Graphics.DrawImage(tankColor, tankRect);
                        break;

                    case 4: //L. green
                        tankColor = lightGreenTank;
                        e.Graphics.DrawImage(tankColor, tankRect);
                        break;

                    case 5: //orange
                        tankColor = orangeTank;
                        e.Graphics.DrawImage(tankColor, tankRect);
                        break;

                    case 6: //purple
                        tankColor = purpleTank;
                        e.Graphics.DrawImage(tankColor, tankRect);
                        break;

                    case 7: //yellow
                        tankColor = yellowTank;
                        e.Graphics.DrawImage(tankColor, tankRect);
                        break;
                }
            }

            /// <summary>
            /// Drawer with Delegate structure to draw the "explosion hole" on tank death.
            /// </summary>
            /// <param name="o"></param>
            /// <param name="e"></param>
            private void ExplosionHoleDrawer(object o, PaintEventArgs e)
            {
                Tank tank = o as Tank;
                if (tank.GetHitPoints() == 0)
                {
                    int width = 60;
                    int height = 60;
                    Rectangle ExplosionRect = new Rectangle(0, 0, width, height);
                    using (System.Drawing.SolidBrush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
                    {
                        e.Graphics.FillEllipse(blackBrush, ExplosionRect);
                    }
                }
            }

            /// <summary>
            /// Drawer with Delegate structure to draw the health bar above the tank object.
            /// Depending on the health of the player, a different bar will be drawn.
            /// Doesn't draw if player dies.
            /// </summary>
            /// <param name="o"></param>
            /// <param name="e"></param>
            private void HPDrawer(object o, PaintEventArgs e)
            {
                Tank tankHP = o as Tank;
                int barHeight = 8;
                int barWidth = 60;
                
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                //make health more generic rather than hard-coded
                //Green health bar for full health
                if (tankHP.GetHitPoints() == 3)
                {
                    Rectangle fullRect = new Rectangle(0, 0, barWidth, barHeight);
                    using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
                    {
                        e.Graphics.FillRectangle(greenBrush, fullRect);
                    }
                }

                //Yellow health bar for getting hit once by projectile
                if (tankHP.GetHitPoints() == 2)
                {
                    barWidth -= 20;
                    Rectangle partRect = new Rectangle(0, 0, barWidth, barHeight);
                    using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
                    {
                        e.Graphics.FillRectangle(yellowBrush, partRect);
                    }
                }

                //Red health bar for getting hit twice by projectile
                if (tankHP.GetHitPoints() == 1)
                {
                    barWidth -= 20;
                    Rectangle smallRect = new Rectangle(0, 0, barWidth-=20, barHeight);
                    using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
                    {
                        e.Graphics.FillRectangle(redBrush, smallRect);
                    }
                }

                //If tank is dead, don't draw.
                if (tankHP.GetHitPoints() == 0 || tankHP.GetTankDisconnected())
                {
                    return;
                }
            }

            /// <summary>
            /// Drawer with delegate structure to draw name and score string on the tank.
            /// </summary>
            /// <param name="o"></param>
            /// <param name="e"></param>
            private void TankNameScoreDrawer(object o, PaintEventArgs e)
            {
                Tank tank = o as Tank;
                //If tank is daed, don't draw.
                if (tank.GetHitPoints()==0)
                {
                    return;
                }
                e.Graphics.DrawString(tank.GetTankName() + " : " + tank.GetScore(), new Font("Comic Sans", 10), new SolidBrush(Color.White), 0,0);
            }

            /// <summary>
            /// Drawer with delegate structure to draw turret on tank.
            /// </summary>
            /// <param name="o"></param>
            /// <param name="e"></param>
            private void TurretDrawer (object o, PaintEventArgs e)
            {
                Tank tankTurret = o as Tank;
                int tuWidth = 50;
                int tuHeight = 50;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                //If tank dead, do not draw.
                if (tankTurret.GetHitPoints() == 0)
                {
                    return;
                }

                //Rectangle is used to link the turret image to.
                Rectangle turretRect = new Rectangle(-tuWidth / 2, -tuHeight / 2, tuWidth, tuHeight);
                Image turretColor;

                //Turret image (color) is determined by the player's ID and drawn with proper sizing conventions.
                switch (tankTurret.GetID() % 8)
                {
                        //red tank = red turret
                    case 0:
                        turretColor = redTurret;
                        e.Graphics.DrawImage(turretColor, turretRect);
                        break;

                        //blue tank = blue turret
                    case 1:
                        turretColor = blueTurret;
                        e.Graphics.DrawImage(turretColor, turretRect);
                        break;

                        //dark tank = dark turret
                    case 2:
                        turretColor = darkTurret;
                        e.Graphics.DrawImage(turretColor, turretRect);
                        break;

                        //green tank = green turret
                    case 3:
                        turretColor = greenTurret;
                        e.Graphics.DrawImage(turretColor, turretRect);
                        break;

                        //L. green tank = L. green turret
                    case 4:
                        turretColor = lightGreenTurret;
                        e.Graphics.DrawImage(turretColor, turretRect);
                        break;

                        //orange tank = orange turret
                    case 5:
                        turretColor = orangeTurret;
                        e.Graphics.DrawImage(turretColor, turretRect);
                        break;

                        //purple tank = purple turret
                    case 6:
                        turretColor = purpleTurret;
                        e.Graphics.DrawImage(turretColor, turretRect);
                        break;

                        //yellow tank = yellow turret
                    case 7:
                        turretColor = yellowTurret;
                        e.Graphics.DrawImage(turretColor, turretRect);
                        break;
                }
            }
            
            /// <summary>
            /// Event handler for when a beam in created inside the world.
            /// </summary>
            /// <param name="beam"></param>
            public void AddBeamAnimation(Beam beam)
            {
                BeamAnimation beamAnimation = new BeamAnimation(beam.GetOriginOfBeam(),
                    beam.GetDirectionOfBeam());
                
                //Invokes add method when this AddBeamAnimation is ran.
                this.Invoke(new MethodInvoker(
                    () => { beamAnimations.Add(beamAnimation);
                    }));
            }

            /// <summary>
            /// Drawer with delegate structure to draw projectiles.
            /// </summary>
            /// <param name="o"></param>
            /// <param name="e"></param>
            private void ProjectileDrawer(object o, PaintEventArgs e)
            {
                Projectile projectile = o as Projectile;
                int projWidth = 30;
                int projHeight = 30;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                //Rectangle to attach the image to.
                Rectangle projRect = new Rectangle(-(projWidth / 2), -(projHeight / 2), projWidth, projHeight);
                Image projColor;

                //If projectile dead, do not draw.
                if (projectile.ProjDied())
                {
                    return;
                }

                //Projectile image (color) is determined by player ID.
                switch (projectile.GetOwnerOfProjectile() % 8)
                {
                    //red tank = red shot
                    case 0:
                        projColor = redShot;
                        e.Graphics.DrawImage(projColor,projRect);
                        break;

                    //blue tank = blue shot
                    case 1:
                        projColor = blueShot;
                        e.Graphics.DrawImage(projColor, projRect);
                        break;

                    //dark tank = dark but not dark (grey) shot
                    case 2:
                        projColor = greyShot;
                        e.Graphics.DrawImage(projColor, projRect);
                        break;

                    //green tank = green shot
                    case 3:
                        projColor = greenShot;
                        e.Graphics.DrawImage(projColor, projRect);
                        break;

                    //L. green tank = VERY L. green (white) shot 
                    case 4:
                        projColor = whiteShot;
                        e.Graphics.DrawImage(projColor, projRect);
                        break;

                    //orange tank = basically dark orange (brown) shot
                    case 5:
                        projColor = brownShot;
                        e.Graphics.DrawImage(projColor, projRect);
                        break;

                    //purple tank = violet
                    case 6:
                        projColor = violetShot;
                        e.Graphics.DrawImage(projColor, projRect);
                        break;

                    //yellow tank = yellow show
                    case 7:
                        projColor = yellowShot;
                        e.Graphics.DrawImage(projColor, projRect);
                        break;
                }
            }

            /// <summary>
            /// Drawer with Delegate structure to draw powerups.
            /// </summary>
            /// <param name="o"></param>
            /// <param name="e"></param>
            private void PowerupDrawer (object o, PaintEventArgs e)
            {
                Powerup powerup = o as Powerup;
                int width = 10;
                int height = 10;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                //If powerup dead, don't draw.
                if (powerup.GetPowerUpDied())
                {
                    return;
                }

                using (System.Drawing.SolidBrush pinkBrush = new System.Drawing.SolidBrush(System.Drawing.Color.DeepPink))
                {
                    //Rectangle to fill with color and make an ellipse.
                    Rectangle pwr = new Rectangle(-(width / 2), -(height / 2), width, height);
                    e.Graphics.FillEllipse(pinkBrush, pwr);
                }
            }

            /// <summary>
            /// Drawer with delegate structure to draw walls.
            /// </summary>
            /// <param name="o"></param>
            /// <param name="e"></param>
            private void WallDrawer (object o , PaintEventArgs e)
            {
                Wall wall = o as Wall;
                int height = 50;
                int width = 50;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                //Rectangle to pin wall image to.
                Rectangle wallRect = new Rectangle(-(width / 2), -(height / 2), width, height);
                e.Graphics.DrawImage(wallSprite, wallRect);
            }

            /// <summary>
            /// Overridden OnPaint method which, based on how many objects are in their allotted collection objects,
            /// draws the object based on the data received by the server, using the object's specified drawing method.
            /// This is invoked every frame.
            /// </summary>
            /// <param name="e"></param>
            protected override void OnPaint(PaintEventArgs e)
            {
                // need to check if the world exists and we received a player ID from the server (established connection)
                if (drawingWorld != null && this.playerIDCheck!=-1)
                {
                    // need to lock the world to prevent crossthreading
                    lock (drawingWorld)
                    {
                        //get position of player to center view
                        if (drawingWorld.getTankList().ContainsKey(playerIDCheck)) {
                            Tank player = drawingWorld.getTankList()[playerIDCheck];
                            double playerX = player.GetLocationOfTank().GetX();
                            double playerY = player.GetLocationOfTank().GetY();

                            //Centers the player's view on their tank.
                            e.Graphics.TranslateTransform((float)-playerX + (900 / 2), (float)-playerY + (900 / 2));

                            //Draws the background image in the allotted space of the drawing panel and centers it.
                            e.Graphics.DrawImage(backgroundImage, -drawingWorld.size / 2, -drawingWorld.size / 2, drawingWorld.GetWorldSize(), drawingWorld.GetWorldSize());

                        }

                        //Draws object (tanks) for everyone currently connected to the server.
                        foreach (Tank tank in drawingWorld.getTankList().Values)
                        {
                            //draw tank body
                            DrawObjectWithTransform(e, tank,
                                tank.GetLocationOfTank().GetX(), tank.GetLocationOfTank().GetY(),
                                tank.GetOrientationOfTank().ToAngle(), TankDrawer);

                            //draw tank turret
                            DrawObjectWithTransform(e, tank,
                                tank.GetLocationOfTank().GetX(), tank.GetLocationOfTank().GetY(),
                                tank.GetTurretDirection().ToAngle(), TurretDrawer);

                            //draw tank health bar
                            DrawObjectWithTransform(e, tank, tank.GetLocationOfTank().GetX()-30,
                                tank.GetLocationOfTank().GetY()-45, 0, HPDrawer);

                            //draw tank score and name string
                            DrawObjectWithTransform(e, tank, tank.GetLocationOfTank().GetX() -30,
                                tank.GetLocationOfTank().GetY() + 45, 0, TankNameScoreDrawer);

                            //draw tank hole on death
                            DrawObjectWithTransform(e, tank, tank.GetLocationOfTank().GetX(),
                                tank.GetLocationOfTank().GetY(), 0, ExplosionHoleDrawer);
                        }

                        //Draws the projectiles that are alive in the world.
                        foreach (Projectile projectile in drawingWorld.GetProjectileList().Values)
                        {
                            DrawObjectWithTransform(e, projectile,
                                projectile.GetLocationOfProjectile().GetX(), projectile.GetLocationOfProjectile().GetY(),
                                projectile.GetDirectionOfTank().ToAngle(), ProjectileDrawer);
                        }

                        //Draws the powerups currently alive in the world.
                        foreach (Powerup powerup in drawingWorld.GetPowerUpList().Values)
                        {
                            DrawObjectWithTransform(e, powerup,
                                powerup.GetLocationOfPowerUp().GetX(), powerup.GetLocationOfPowerUp().GetY(),
                                0, PowerupDrawer);
                        }

                        // Draws the walls received from the server on connection.
                        foreach (Wall wall in drawingWorld.getWallList().Values)
                        {
                            double p1XCoord = wall.GetPointOne().GetX();
                            double p1YCoord = wall.GetPointOne().GetY();

                            double p2XCoord = wall.GetPointTwo().GetX();
                            double p2YCoord = wall.GetPointTwo().GetY();

                            //The below if statements account for vertical and horizontal walls and drawing from
                            //left to right, right to left, up to down, and down to up.

                            //vertical wall
                            if (p1XCoord.Equals(p2XCoord))
                            {
                                double currentPosition = p1YCoord;
                                //up to down
                                if (p1YCoord < p2YCoord)
                                {
                                    while (currentPosition <= p2YCoord)
                                    {
                                        DrawObjectWithTransform(e, wall, p1XCoord, currentPosition, 0, WallDrawer);
                                        currentPosition += 50;
                                    }
                                }
                                else
                                {
                                    //down to up
                                    while (currentPosition >= p2YCoord)
                                    {
                                        DrawObjectWithTransform(e, wall, p1XCoord, currentPosition, 0, WallDrawer);
                                        currentPosition -= 50;
                                    }
                                }

                            }

                            //horizontal wall
                            else if (p1YCoord.Equals(p2YCoord))
                            {
                                double currentPosition = p1XCoord;
                                //left to right
                                if (p1XCoord <= p2XCoord)
                                {
                                    while (currentPosition <= p2XCoord)
                                    {
                                        DrawObjectWithTransform(e, wall, currentPosition, p1YCoord, 0, WallDrawer);
                                        currentPosition += 50;
                                    }
                                }
                                else
                                {
                                    //right to left
                                    while (currentPosition >= p2XCoord)
                                    {
                                        DrawObjectWithTransform(e, wall, currentPosition, p1YCoord, 0, WallDrawer);
                                        currentPosition -= 50;
                                    }
                                }
                            }
                        }

                        //Draws the bream animations currently active in the world.
                        foreach (BeamAnimation beam in beamAnimations)
                        {
                            double originX = beam.GetOrigin().GetX();
                            double originY = beam.GetOrigin().GetY();
                            double direction = beam.GetOrientation().ToAngle();

                            DrawObjectWithTransform(e, beam, originX, originY, direction, beam.BeamDrawer);

                            //Slowly reduces beam size.
                            if (beam.GetFrames() > 30)
                            {
                                firedBeams.Add(beam);
                            }
                        }

                        //For all beams fired, removes them from the beamAnimations collection and the firedBeams collection
                        foreach (BeamAnimation beam in firedBeams)
                        {
                            beamAnimations.Remove(beam);
                        }
                        firedBeams.Clear();
                    }
                }
                //Essentially an infinite loop
                base.OnPaint(e);
            }
        }
        
        /// <summary>
        /// Help menu item that shows the controls when clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void controlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("W: Move tank upwards" + Environment.NewLine
                                + "S: Move tank downwards" + Environment.NewLine
                                + "A: Move tank to the left" + Environment.NewLine
                                + "D: Move tank to the right" + Environment.NewLine
                                + "Mouse: Use to aim" + Environment.NewLine
                                + "Left Click: Shoot Projectile" + Environment.NewLine
                                + "Right Click: Shoot Beam");
        }

        /// <summary>
        /// About menu item that shows the game information when clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutTheGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This is Tank War. Drive around as a model tank and fire projectiles at other tanks. " + 
                "You can shoot other tanks and if you destory them, you score goes higher. " + 
                "But be careful, other tanks can shoot you and destroy your tank as well. " +  
                "The player with the higher score wins. " + Environment.NewLine + 
                "A game coded by Kyla Ryan and Nicholas Provenzano using the tools provided by Professor Kopta.");
        }
    }
}