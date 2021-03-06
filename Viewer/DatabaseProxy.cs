﻿// This file is part of project Cube21
// Whole solution including its LGPL license could be found at
// http://cube21.sf.net/
// 2007 Pavel Savara, http://zamboch.blogspot.com/

using System;
using System.ServiceModel;
using System.Windows;
using Viewer.Properties;
using Viewer.ServiceReference;
using Zamboch.Cube21;
using Zamboch.Cube21.Actions;

namespace Viewer
{
    class DatabaseProxy
    {
        public static Path FindWayHome(Cube cube)
        {
            if (Settings.Default.UseLocal)
            {
                return cube.FindWayHome();
            }
            else
            {
                try
                {
                    BasicHttpBinding binding = new BasicHttpBinding();
                    binding.Name = "binding1";
                    binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
                    binding.Security.Mode = BasicHttpSecurityMode.None;

                    EndpointAddress a = new EndpointAddress(Settings.Default.WebService);
                    Cube21ServiceClient client = new Cube21ServiceClient(binding, a);
                    SmartStep[] result = client.FindWayHome(cube);
                    client.Close();
                    return new Path(result);
                }
                catch(Exception e)
                {
                    MessageBox.Show(
                        "Problems with server, probably offline. Drop email to pavel.savara@gmail.com, thanks!");
                    return new Path();
                }
            }
        }
    }
}
