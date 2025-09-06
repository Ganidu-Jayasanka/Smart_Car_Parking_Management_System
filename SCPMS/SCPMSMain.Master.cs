using System;
using System.Web.UI;

namespace SCPMS
{
    public partial class SCPMSMain : MasterPage
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedInUserId"] == null)
            {
                // User is not logged in, redirect to login page
                Response.Redirect("~/Login.aspx", false);
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
