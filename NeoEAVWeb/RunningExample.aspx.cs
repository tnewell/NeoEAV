using System;
using System.Collections.Generic;
using System.Linq;

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
            myContextController.ActiveProject = ctlProjectContext.ContextKey;
            myContextController.ActiveSubject = ctlSubjectContext.ContextKey;
            myContextController.ActiveContainer = ctlRootContainer.ContextKey;

            if (!IsPostBack)
            {
                BindProjects();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        private void BindProjects()
        {
            ctlProjectContext.DataSource = myContextController.Projects;
            ctlProjectContext.DataBind();

            BindSubjects();
        }

        private void BindSubjects()
        {
            myContextController.ActiveSubject = null;
            ctlSubjectContext.ContextKey = null;

            List<string> members = myContextController.GetSubjectsForActiveProject().Select(it => it.MemberID).ToList();

            if (members.Any())
                members.Insert(0, String.Empty);

            ctlSubjects.DataSource = members;
            ctlSubjects.DataBind();
            ctlSubjects.Enabled = ctlSubjects.Items.Count > 0;
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            myContextController.ActiveSubject = ctlSubjects.SelectedValue;

            ctlSubjectContext.ContextKey = ctlSubjects.SelectedValue;
            ctlProjectContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            //myContextController.Save(this);
        }
    }
}