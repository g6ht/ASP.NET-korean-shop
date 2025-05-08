using System;
using System.Web.UI;

namespace CourseWorkTP
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("index.aspx");
        }
    }
}