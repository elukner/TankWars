using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using TankWars;

namespace Server
{
    /// <summary>
    /// Game Settings object to hold settings read by a file. Also has a reader to read the file.
    /// <Author>Kyla Ryan and Elizabeth Lukner (PS9)</Author>
    /// </summary>
    public class GameSettings
    {
        //gets and set the default information read from the settings file
        public int UniverseSize { get; protected set; }

        public int MSPerFrame { get; protected set; }

        public int FramesPerShot { get; protected set; }

        public int RespawnRate { get; protected set; }

        public List<Wall> WallList { get; protected set; }

        public int WallID { get; protected set; }

        /// <summary>
        /// Default game settings constructor to use if a settings file has not been found
        /// </summary>
        public GameSettings()
        {
            UniverseSize = 2000;
            MSPerFrame = 17;
            FramesPerShot = 80;
            RespawnRate = 300;
            WallList = new List<Wall>();
        }


        /// <summary>
        /// Reads the settings file if it has been found and updates the game information based
        /// on what data was read.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static GameSettings ReadSettings(string filePath)
        {
            //new game settings object to hold all game settings found
            GameSettings serverSettings = new GameSettings();
            serverSettings.WallID = 0;

            //new wall to handle all wall objects found inside the file
            Wall wall = new Wall();

            //helper objects to set when an object has been found in the file
            bool p1PointFound = false;
            bool p2PointFound = false;
            Vector2D p1Point = new Vector2D();
            Vector2D p2Point = new Vector2D();
            double x = 0;
            double y = 0;

            try
            {
                XmlReaderSettings readSettings = new XmlReaderSettings() { IgnoreWhitespace = true };
                using (XmlReader reader = XmlReader.Create(filePath, readSettings))
                {
                    //read the file
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "UniverseSize":
                                    reader.Read();
                                    serverSettings.UniverseSize = int.Parse(reader.Value);
                                    break;

                                case "MSPerFrame":
                                    reader.Read();
                                    serverSettings.MSPerFrame = int.Parse(reader.Value);
                                    break;

                                case "FramesPerShot":
                                    reader.Read();
                                    serverSettings.FramesPerShot = int.Parse(reader.Value);
                                    break;

                                case "RespawnRate":
                                    reader.Read();
                                    serverSettings.RespawnRate = int.Parse(reader.Value);
                                    break;

                                //all below items are linked to the wall object
                                case "Wall":
                                    wall = new Wall();
                                    break;

                                case "p1":
                                    p1PointFound = true; 
                                    break;

                                case "p2":
                                    p1PointFound = false;
                                    break;

                                case "x":
                                    reader.Read();
                                    x = double.Parse(reader.Value);
                                    break;

                                case "y":
                                    reader.Read();
                                    y = double.Parse(reader.Value);
                                    //if we have found a p1, allow for p1 to be set
                                    if (p1PointFound == true)
                                    {
                                        p1Point = new Vector2D(x, y);
                                    }
                                    //otherwise, if we haven't found a p1, set p2
                                    else if(p1PointFound == false)
                                    {
                                        p2Point = new Vector2D(x, y);
                                        //create the wall from the points found
                                        wall = new Wall(serverSettings.WallID, p1Point, p2Point);
                                        serverSettings.WallList.Add(wall);
                                        serverSettings.WallID++;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return serverSettings;
        }
    }
}
