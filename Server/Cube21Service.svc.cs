using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Server.Properties;
using Zamboch.Cube21;
using Zamboch.Cube21.Actions;
using Zamboch.Cube21.Work;

namespace Server
{
    public class Cube21Service : ICube21Service
    {
        static Cube21Service()
        {
            if (manager == null)
            {
                manager = new DatabaseManager(Settings.Default.DatabasePath);
                manager.Initialize();
            }
        }

        private static DatabaseManager manager;

        public Path FindWayHome(Cube cube)
        {
            try
            {
                cube.CheckPieces();
                cube.CheckFlipable();
            }
            catch(CubeException ex)
            {
                int i = 0;
            }

            Path result=cube.FindWayHome();
            return result;
        }
    }
}
