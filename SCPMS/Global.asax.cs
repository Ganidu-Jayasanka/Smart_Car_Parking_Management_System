using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;

namespace SCPMS
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            ScriptResourceDefinition jqueryDef = new ScriptResourceDefinition();
            jqueryDef.Path = "~/Scripts/jquery-3.6.0.min.js";
            jqueryDef.DebugPath = "~/Scripts/jquery-3.6.0.js";
            jqueryDef.CdnPath = "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.6.0.min.js";
            jqueryDef.CdnDebugPath = "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.6.0.js";
            jqueryDef.CdnSupportsSecureConnection = true;
            jqueryDef.LoadSuccessExpression = "window.jQuery";

            ScriptManager.ScriptResourceMapping.AddDefinition("jquery", null, jqueryDef);
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
           RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}