using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NeoEAV.Data.DataClasses;
using NeoEAV.Web.UI;


namespace NeoEAVWeb
{
    public partial class RunningExample : System.Web.UI.Page
    {
        EAVContextController myContextController = new EAVContextController();

        protected override void OnInitComplete(EventArgs e)
        {
            ctlProjectContext.DataSource = myContextController.Projects;

            base.OnInitComplete(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindProjects();
            }
        }

        private void BindProjects()
        {
            ctlProjectContext.DataSource = myContextController.Projects;
            ctlProjectContext.DataBind();

            BindSubjects();
        }

        private void BindSubjects()
        {
            Project project = ctlProjectContext.DataItem as Project;
            if (project != null)
            {
                List<string> members = project.Subjects.Select(it => it.MemberID).ToList();

                members.Insert(0, String.Empty);

                ctlSubjects.DataSource = members;
                ctlSubjects.DataBind();
            }

            if (!String.IsNullOrWhiteSpace(ctlSubjectContext.ContextKey))
            {
                ctlSubjectContext.ContextKey = null;
                ctlSubjectContext.DataBind();
            }
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            ctlSubjectContext.ContextKey = ctlSubjects.SelectedValue;

            ctlProjectContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            myContextController.Save(this);

            ctlProjectContext.DataBind();
        }
    }
}