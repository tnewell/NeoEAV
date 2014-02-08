using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NeoEAV.Web.UI;


namespace NeoEAVWeb
{
    public partial class AutoGenExample : System.Web.UI.Page
    {
        private EAVContextController myContextController = new EAVContextController();

        protected override void OnInitComplete(EventArgs e)
        {
            base.OnInitComplete(e);

            ctlProjectContext.DataSource = myContextController.Projects;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Restore our controller after view state loaded.
            myContextController.ActiveProject = ctlProjectContext.ContextKey;
            myContextController.ActiveSubject = ctlSubjectContext.ContextKey;
            myContextController.ActiveContainer = ctlContainerContext.ContextKey;

            if (!IsPostBack)
            {
                BindProjects();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ctlSaveButton.Enabled = ctlSubjects.SelectedIndex > 0 && ctlContainers.SelectedIndex > 0;
        }

        private void BindProjects()
        {
            List<string> projects = myContextController.Projects.Select(it => it.Name).ToList();

            if (projects.Any())
                projects.Insert(0, String.Empty);

            ctlProjects.DataSource = projects;
            ctlProjects.DataBind();
            ctlProjects.Enabled = ctlProjects.Items.Count > 0;
        }

        private void BindSubjects()
        {
            List<string> members = myContextController.GetSubjectsForActiveProject().Select(it => it.MemberID).ToList();

            if (members.Any())
                members.Insert(0, String.Empty);

            ctlSubjects.DataSource = members;
            ctlSubjects.DataBind();
            ctlSubjects.Enabled = ctlSubjects.Items.Count > 0;
        }

        private void BindContainers()
        {
            List<string> members = myContextController.GetContainersForActiveProject().Select(it => it.Name).ToList();

            if (members.Any())
                members.Insert(0, String.Empty);

            ctlContainers.DataSource = members;
            ctlContainers.DataBind();
            ctlContainers.Enabled = ctlContainers.Items.Count > 0;
        }

        protected void ctlProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            myContextController.ActiveProject = ctlProjects.SelectedValue;

            ctlProjectContext.ContextKey = ctlProjects.SelectedValue;

            ctlProjectContext.DataBind();

            BindSubjects();
            BindContainers();
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            myContextController.ActiveSubject = ctlSubjects.SelectedValue;

            ctlSubjectContext.ContextKey = ctlSubjects.SelectedValue;
            
            ctlProjectContext.DataBind();
        }

        protected void ctlContainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            myContextController.ActiveContainer = ctlContainers.SelectedValue;
            
            ctlContainerContext.ContextKey = ctlContainers.SelectedValue;
            
            ctlProjectContext.DataBind();            
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            //myContextController.Save(this);
            myContextController.Save(ctlProjectContext);

            ctlProjectContext.DataBind();
        }
    }
}
