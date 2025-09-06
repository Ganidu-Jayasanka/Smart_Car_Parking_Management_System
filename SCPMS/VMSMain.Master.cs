using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SCPMS
{
    public partial class VMSMain : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedInUserId"] == null)
            {
                // User is not logged in, redirect to login page
                Response.Redirect("~/VMS_Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                string currentDate = DateTime.Now.ToString("dd/MM/yyyy");
                string currentTime = DateTime.Now.ToString("HH:mm:ss");



                // Display the logged-in username in the navigation bar
                lblUserName.Text = Session["LoggedInUserName"]?.ToString();
            }
        }
    }
}