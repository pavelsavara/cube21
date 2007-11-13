using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Server.Properties;
using Zamboch.Cube21.Work;

namespace Server
{
    public class Global : System.Web.HttpApplication
    {

        private static DatabaseManager manager;

        protected void Application_Start(object sender, EventArgs e)
        {
            if (manager == null)
            {
                manager = new DatabaseManager(Settings.Default.DatabasePath);
                manager.Initialize();
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}