using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Zamboch.Cube21;
using Zamboch.Cube21.Actions;

namespace Server
{
    public partial class BestPath : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (tbTop.Text == "" && tbBot.Text == "")
            {
                Cube c = new Cube();
                tbTop.Text = c.TopToString();
                tbBot.Text = c.BotToString();
            }
        }

        protected void btGetPath_Click(object sender, EventArgs e)
        {
            try
            {
                Cube cube = new Cube(tbTop.Text + tbBot.Text);
                Path result = cube.FindWayHome();
                if (result.Count==0)
                {
                    lbResult.Text = "this is solved cube";
                }
                else
                {
                    lbResult.Text = result.ToString();
                }
            }
            catch(Exception ex)
            {
                string s = "Bad cube permutation \n\n" + ex;
                lbResult.Text = s.Replace("\n", "<br/>");
            }
        }
    }
}
