using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Zamboch.Cube21.Work
{
    public class WorkQueue
    {
        #region Data

        public int SourceLevel = 0;
        public List<ShapePair> ThisLevelWork = new List<ShapePair>();

        #endregion

        #region Helpers

        private static readonly XmlSerializer databaseSerializer = new XmlSerializer(typeof(WorkQueue));
        public static string workFile
        {
            get
            {
                return Path.Combine(DatabaseManager.DatabasePath, "Work.xml");
            }
        }

        #endregion

        #region Loader

        public static bool CanLoad()
        {
            return File.Exists(workFile);
        }

        public void Save()
        {
            string workDir = Path.GetDirectoryName(workFile);
            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            string bak = workFile + DatabaseManager.SourceLevel;
            while (File.Exists(bak))
            {
                bak = bak + DatabaseManager.SourceLevel;
            }
            if (File.Exists(workFile))
            {
                File.Move(workFile, bak);
            }


            StreamWriter sw = new StreamWriter(workFile);
            databaseSerializer.Serialize(sw, this);
            sw.Close();
        }

        public static WorkQueue Load()
        {
            using (StreamReader sr = new StreamReader(workFile))
            {
                return (WorkQueue)databaseSerializer.Deserialize(sr);
            }
        }

        #endregion
    }
}