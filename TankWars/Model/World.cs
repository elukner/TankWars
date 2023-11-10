using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// <Author>Kyla Ryan and Nicholas Provenzano (PS8)</Author>
    ///<Author> Kyla Ryan and Elizabeth Lukner (PS9)</Author>
    ///World class which holds all current objects to be drawn in the game.
    ///This also updates the physics and collisions of each object.
    /// </summary>
    public class World
    {
        //Dictionaries to hold all specified objects currently in the world.
        private Dictionary<int, Tank> tanks;

        private Dictionary<int, Wall> walls;

        private Dictionary<int, Powerup> powerups;

        private Dictionary<int, Projectile> projectiles;

        public Dictionary<int, Beam> beams { get; set; }

        //Fields to get the current spawn and frame rates of the game for
        //tank respawning.
        public int respawnRate { get; set; }

        public int framesPerShot { get; set; }

        public int MSPerFrame { get; set; }

        /// <summary>
        /// Gets/Sets size of the world
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// Creates world object and Dictionaries.
        /// </summary>
        /// <param name="_size"></param>
        public World()
        {
            beams = new Dictionary<int, Beam>();
            tanks = new Dictionary<int, Tank>();
            walls = new Dictionary<int, Wall>();
            powerups = new Dictionary<int, Powerup>();
            projectiles = new Dictionary<int, Projectile>();
        }

        /// <summary>
        /// World object from the base constructor of the world (line 44) which
        /// holds additional data that pertains to the server such as:
        /// world size, frames, and respawn rates.
        /// This information is assigned from a settings file in the server.
        /// </summary>
        /// <param name="worldSize"></param>
        /// <param name="Walls"></param>
        /// <param name="fps"></param>
        /// <param name="respawnRate"></param>
        /// <param name="MSPerFrame"></param>
        public World(int worldSize, IEnumerable<Wall> Walls, int fps, int respawnRate, int MSPerFrame) : this()
        {
            this.size = worldSize;
            this.respawnRate = respawnRate;
            this.framesPerShot = fps;
            this.MSPerFrame = MSPerFrame;

            //get all walls from the settings file and store them in the dictionary
            foreach (Wall wall in Walls)
            {
                walls[wall.GetWallName()] = wall;
            }
        }

        /// <summary>
        /// For random respawn of objects inside of the world object.
        /// </summary>
        /// <returns></returns>
        public Vector2D RandomLocation()
        {
            Random rand = new Random();
            Vector2D randLoc;
            bool flag;
            //checks if collision if true makes a new location; does while a location has not been found
            do
            {
                //2.0-1.0* worldSize/2 is used because the vector2D clamps the numbers
                randLoc = new Vector2D(rand.NextDouble() * 2.0 - 1.0, rand.NextDouble() * 2.0 - 1.0) * ((double)this.GetWorldSize() / 2);

                flag = true;

            } while (!flag);
            return randLoc;
        }

        /// <summary>
        /// Checks if the projectile collides with any other objects or goes outside the map scope.
        /// </summary>
        /// <param name="proj"></param>
        /// <returns></returns>
        public void ProjectileCollision(Projectile proj, int currentFrameNumber)
        {
            //check if projectiles and tanks collide
            foreach (Tank tank in getTankList().Values)
            {
                //if tank that fired projectile is the current one in the dictionary
                if (proj.GetOwnerOfProjectile() == tank.GetID())
                {
                    continue;
                }
                //gets the position information of the projectile and tank using a rectangle's points
                double projX1 = proj.GetLocationOfProjectile().GetX();
                double projY1 = proj.GetLocationOfProjectile().GetY();

                //add or subtract 30 as it is half the tank size
                double t1XCoord = tank.GetLocationOfTank().GetX() - 30;
                double t2XCoord = tank.GetLocationOfTank().GetX() + 30;

                double t1YCoord = tank.GetLocationOfTank().GetY() - 30;
                double t2YCoord = tank.GetLocationOfTank().GetY() + 30;

                //if the projectile is within the frame of the tank
                if ((projX1 >= t1XCoord && projX1 <= t2XCoord) && (projY1 >= t1YCoord && projY1 <= t2YCoord))
                {
                    proj.SetDied(true);
                    tank.SetHp(tank.GetHitPoints() - 1);

                    //if the tank has died from the projectile hit
                    if (tank.GetHitPoints() == 0)
                    {
                        //determines the respawn frame of the tank that was killed
                        int respawnFrame = currentFrameNumber + respawnRate;
                        tank.SetRespawnFrame(respawnFrame);
                        tank.SetDied(true);
                        tanks[proj.GetOwnerOfProjectile()].IncrementScore();
                    }
                }
            }

            //check if projectile hits edge of world and destroy if true
            int halfWorld = GetWorldSize() / 2;
            double projX2 = proj.GetLocationOfProjectile().GetX();
            double projY2 = proj.GetLocationOfProjectile().GetY();
            if (projX2 > halfWorld || projX2 < -halfWorld)
            {
                proj.SetDied(true);
            }

            if (projY2 > halfWorld || projY2 < -halfWorld)
            {
                proj.SetDied(true);
            }

            //check if projectile hits wall
            foreach (Wall wall in getWallList().Values)
            {
                //gets the position of the wall using a rectangle's points
                double p1XCoord = wall.GetPointOne().GetX();
                double p2XCoord = wall.GetPointTwo().GetX();

                double p1YCoord = wall.GetPointOne().GetY();
                double p2YCoord = wall.GetPointTwo().GetY();

                //used to determine frame around the wall for collision
                double x1 = 0;
                double x2 = 0;

                double y1 = 0;
                double y2 = 0;

                //vertical wall
                if (p1XCoord.Equals(p2XCoord))
                {
                    x1 = p1XCoord - 30;
                    x2 = p1XCoord + 30;

                    //up to down
                    if (p1YCoord < p2YCoord)
                    {
                        y1 = p1YCoord - 30;
                        y2 = p2YCoord + 30;
                    }

                    //down to up
                    else
                    {
                        y1 = p2YCoord - 30;
                        y2 = p1YCoord + 30;
                    }
                }

                //horizontal wall
                else if (p1YCoord.Equals(p2YCoord))
                {
                    y1 = p1YCoord - 30;
                    y2 = p1YCoord + 30;

                    //left to right
                    if (p1XCoord <= p2XCoord)
                    {
                        x1 = p1XCoord - 30;
                        x2 = p2XCoord + 30;
                    }

                    //right to left
                    else
                    {
                        x1 = p2XCoord - 30;
                        x2 = p1XCoord + 30;
                    }
                }

                //check if proj is inside of a wall
                if ((projX2 >= x1 && projX2 <= x2) && (projY2 >= y1 && projY2 <= y2))
                {
                    proj.SetDied(true);
                }
            }
        }

        /// <summary>
        /// determine if the tank will collide with the walls
        /// </summary>
        /// <param name="tank"></param>
        public bool TankWallCollision(Vector2D locationToCheck)
        {
            //Gets the location of the tank for reference
            double tankX = locationToCheck.GetX();
            double tankY = locationToCheck.GetY();

            foreach (Wall wall in getWallList().Values)
            {
                //gets the location of the wall using a rectangle's points
                double p1XCoord = wall.GetPointOne().GetX();
                double p2XCoord = wall.GetPointTwo().GetX();

                double p1YCoord = wall.GetPointOne().GetY();
                double p2YCoord = wall.GetPointTwo().GetY();

                //used to determine frame around the wall
                double x1 = 0;
                double x2 = 0;

                double y1 = 0;
                double y2 = 0;

                //vertical wall
                if (p1XCoord.Equals(p2XCoord))
                {
                    x1 = p1XCoord - 55;
                    x2 = p1XCoord + 55;

                    //up to down
                    if (p1YCoord < p2YCoord)
                    {
                        y1 = p1YCoord - 55;
                        y2 = p2YCoord + 55;
                    }

                    //down to up
                    else
                    {
                        y1 = p2YCoord - 55;
                        y2 = p1YCoord + 55;
                    }
                }
                //horizontal wall
                else if (p1YCoord.Equals(p2YCoord))
                {
                    y1 = p1YCoord - 55;
                    y2 = p1YCoord + 55;

                    //left to right
                    if (p1XCoord <= p2XCoord)
                    {
                        x1 = p1XCoord - 55;
                        x2 = p2XCoord + 55;
                    }

                    //right to left
                    else
                    {
                        x1 = p2XCoord - 55;
                        x2 = p1XCoord + 55;
                    }
                }
                //check if tank is inside of a wall
                if ((tankX >= x1 && tankX <= x2) && (tankY >= y1 && tankY <= y2))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Updates the data linked to a projectile, such as location and direction
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="tank"></param>
        public void UpdateProjData(int playerID, Tank tank)
        {
            Projectile proj = projectiles[playerID];
            proj.SetLocation(new Vector2D(tank.GetLocationOfTank()));
            Vector2D newDir = new Vector2D(tank.GetTurretDirection());
            newDir.Normalize();
            proj.SetDirection(newDir);
            proj.SetOwner(playerID);
            proj.SetDied(false);
        }

        /// <summary>
        /// Adds a tank in a random location.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public int AddRandomTank(string name, int ID)
        {
            Vector2D orientation = new Vector2D(0, 0);
            Vector2D aiming = new Vector2D(0, -1);
            Vector2D location = RandomLocation();

            while (TankWallCollision(location))
            {
                location = RandomLocation();
            }
            //Creates tank to be added with new data for spawn
            Tank newTank = new Tank(name, ID, location, aiming, orientation, 3, 0, false, false);
            AddTank(newTank.GetID(), newTank);
            return newTank.GetID();
        }

        /// <summary>
        /// Gets world size;
        /// </summary>
        /// <returns></returns>
        public int GetWorldSize()
        {
            return size;
        }

        /// <summary>
        /// Gets tank dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Tank> getTankList()
        {
            return tanks;
        }

        /// <summary>
        /// Gets wall dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Wall> getWallList()
        {
            return walls;
        }

        /// <summary>
        /// Gets powerup dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Powerup> GetPowerUpList()
        {
            return powerups;
        }

        /// <summary>
        /// Gets projectile dictionary.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Projectile> GetProjectileList()
        {
            return projectiles;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="tankID"></param>
        /// <param name="tank"></param>
        private void AddTank(int tankID, Tank tank)
        {
            if (tank != null)
            {
                tanks[tankID] = tank;
            }
        }

        /// <summary>
        /// Adds a projectile into the dictionary.
        /// </summary>
        /// <param name="projID"></param>
        /// <param name="projectile"></param>
        public void AddProj(int projID, Projectile projectile)
        {
            if (projectile != null)
            {
                projectiles[projID] = projectile;
            }
        }

        /// <summary>
        /// Adds a wall into the dictionary.
        /// </summary>
        /// <param name="wallID"></param>
        /// <param name="wall"></param>
        public void AddWall(int wallID, Wall wall)
        {
            if (wall != null)
            {
                walls[wallID] = wall;
            }

        }

        /// <summary>
        /// Add no more then 2 powerups as defualt to world
        /// </summary>
        /// <param name="pwrupID"></param>
        /// <param name="pwrups"></param>
        public void AddPowerups()
        {
            //Need to check no more then 2 power ups
            if (powerups.Count < 2)
            {
                Vector2D location = RandomLocation();

                while (WallPowerupCollision(location))
                {
                    location = RandomLocation();
                }
                Powerup powerup = new Powerup();
                //Vector2D powerupLoc = new Vector2D(30, 30);
                powerup.UpdatePowerup(location, false);
                //powerup.SetLocation(location);
                powerups[powerup.GetPowerUpID()] = powerup;
            }
        }

        /// <summary>
        /// Determines if a powerup collides with a wall or outside of the world on creation
        /// </summary>
        /// <param name="powerup"></param>
        public bool WallPowerupCollision(Vector2D location)
        {
            //make sure the powerup cannot spawn outside of the world
            int halfWorld = GetWorldSize() / 2;
            double pwrX = location.GetX();
            double pwrY = location.GetY();

            if (pwrX > halfWorld || pwrX < -halfWorld)
            {
                return true;
            }

            if (pwrY > halfWorld || pwrY < -halfWorld)
            {
                return true;
            }

            //make sure the powerup cannot spawn inside a wall
            foreach (Wall wall in getWallList().Values)
            {
                //get wall points to determine collision
                double p1XCoord = wall.GetPointOne().GetX();
                double p2XCoord = wall.GetPointTwo().GetX();

                double p1YCoord = wall.GetPointOne().GetY();
                double p2YCoord = wall.GetPointTwo().GetY();

                //used to properly determine rectangle around the wall for collision
                double x1 = 0;
                double x2 = 0;

                double y1 = 0;
                double y2 = 0;

                //vertical wall
                if (p1XCoord.Equals(p2XCoord))
                {
                    x1 = p1XCoord - 35;
                    x2 = p1XCoord + 35;

                    //up to down
                    if (p1YCoord < p2YCoord)
                    {
                        y1 = p1YCoord - 35;
                        y2 = p2YCoord + 35;
                    }
                    else
                    {
                        y1 = p2YCoord - 35;
                        y2 = p1YCoord + 35;
                    }
                }
                //horizontal wall
                else if (p1YCoord.Equals(p2YCoord))
                {
                    y1 = p1YCoord - 35;
                    y2 = p1YCoord + 35;

                    //left to right
                    if (p1XCoord <= p2XCoord)
                    {
                        x1 = p1XCoord - 35;
                        x2 = p2XCoord + 35;
                    }
                    else
                    {
                        x1 = p2XCoord - 35;
                        x2 = p1XCoord + 35;
                    }
                }

                //check if powerup is inside of a wall
                if ((pwrX >= x1 && pwrX <= x2) && (pwrY >= y1 && pwrY <= y2))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a tank collides with a wall and sets the powerup to be removed from the dictionary if true.
        /// </summary>
        /// <param name="powerup"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public bool TankPowerupCollisions(Powerup powerup)
        {
            //Random rand = new Random();
           
            //int pwrRespawnRate = rand.Next(100, 1650); 
            int halfPwrTankWidth = 35; 

            foreach (Tank tank in getTankList().Values)
            {
                //gets the location of the powerup
                double projX1 = powerup.GetLocationOfPowerUp().GetX();
                double projY1 = powerup.GetLocationOfPowerUp().GetY();

                //gets the four points of the rectangle surrounding the tank to determine collision
                double t1XCoord = tank.GetLocationOfTank().GetX() - halfPwrTankWidth;
                double t2XCoord = tank.GetLocationOfTank().GetX() + halfPwrTankWidth;

                double t1YCoord = tank.GetLocationOfTank().GetY() - halfPwrTankWidth;
                double t2YCoord = tank.GetLocationOfTank().GetY() + halfPwrTankWidth;

                //checks if the tank has collided with the wall based on rectangle locations.
                if ((projX1 >= t1XCoord && projX1 <= t2XCoord) && (projY1 >= t1YCoord && projY1 <= t2YCoord))
                {
                    powerup.SetDead(true);
                    tank.IncPowerupTotal();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Used to respawn tanks in after they have died.
        /// </summary>
        /// <param name="tank"></param>
        /// <param name="currentFrameNumber"></param>
        public void RandomRespawnTank(Tank tank,int currentFrameNumber)
        {
            if (tank.GetHitPoints()==0)
            {
                //if the number of frames have passed for proper respawning
                if (currentFrameNumber >= tank.GetTimeTillRespawn())
                {
                    Vector2D orientation = new Vector2D(0, 0);
                    Vector2D aiming = new Vector2D(0, -1);
                    Vector2D location = RandomLocation();

                    while (TankWallCollision(location))
                    {
                        location = RandomLocation();
                    }
                    //respawns with same score as before and updated data
                    tank.UpdateTank(location, aiming, orientation, 3, tank.GetScore(), false, false);
                }
            }
        }

        /// <summary>
        /// Adds a beam for firing.
        /// </summary>
        /// <param name="tank"></param>
        /// <param name="currentFrameNumber"></param>
        public void AddBeam(Tank tank, int currentFrameNumber)
        {
            int halfTankSize = 30;

            foreach (Tank otherTank in tanks.Values)
            {
                if (otherTank != tank)
                {
                    //determins if the beam intersects the location of another tank aside from the one who fired it
                    if (Intersects(tank.GetLocationOfTank(), tank.GetTurretDirection(), otherTank.GetLocationOfTank(), halfTankSize) == true)
                    {
                        //kill the tank will save frame later
                        otherTank.SetDied(true);
                        otherTank.SetRespawnFrame(currentFrameNumber + respawnRate);
                        otherTank.SetHp(0);
                        tank.IncrementScore();
                    }
                }
            }

            //Create the beam and set its data for world interaction.
            Beam beam = new Beam();
            beam.SetBeamName(tank.GetID());
            beam.SetOwnerBeam(tank.GetID());
            beam.SetOriginOfBeam(new Vector2D(tank.GetLocationOfTank()));
            beam.SetDirectionOfBeam(new Vector2D(tank.GetTurretDirection()));
            beams[beam.GetBeamName()] = beam;
        }

        ///// <summary>
        ///// Determines if a ray interescts a circle. Code given by Daniel Kopta.
        ///// </summary>
        ///// <param name="rayOrig">The origin of the ray which is the tank location</param>
        ///// <param name="rayDir">The direction of the ray which is the turret direction</param>
        ///// <param name="center">The center of the circle center of the other tank</param>
        ///// <param name="r">The radius of the circle half the with other tank</param>
        ///// <returns></returns>
        public static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        /// <summary>
        /// Removes all dead(projectiles and powerups) or disconnected(tank) world objects.
        /// This method is called at the end of every frame to keep consistency.
        /// </summary>
        public void RemoveDeadWorldObjects()
        {
            //Removes tanks whos player has disconnected
            foreach (Tank tank in getTankList().Values)
            {
                if (tank.GetTankDisconnected())
                {
                    tanks.Remove(tank.GetID());
                }
            }
            //Removes if projecitle has died (collided with another object)
            foreach (Projectile proj in GetProjectileList().Values)
            {
                if (proj.ProjDied())
                {
                    projectiles.Remove(proj.GetProjectileName());
                }
            }
            //Removes powerup if died (collided with a tank)
            foreach (Powerup power in GetPowerUpList().Values)
            {
                if (power.GetPowerUpDied())
                {
                    powerups.Remove(power.GetPowerUpID());
                }
            }
            //forget about all of the beams that have fired
            beams.Clear();
        }

        /// <summary>
        /// Sets data of the tank if a client has disconnected.
        /// </summary>
        /// <param name="id"></param>
        public void TankSetDisconnected(int id)
        {
            tanks[id].SetDisconnected(true);
            tanks[id].SetDied(true);
            tanks[id].SetHp(0);
        }
    }
}