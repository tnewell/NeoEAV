using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NeoEAV.Data.DataClasses;
using NeoEAV.Web.UI;


namespace NeoEAVWeb
{
    public partial class OneShotExample : System.Web.UI.Page
    {
        private EAVContextController myContextController = new EAVContextController();

        protected override void OnInitComplete(EventArgs e)
        {
            base.OnInitComplete(e);

            ctlProjectContext.DataSource = myContextController.Projects;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
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
            ctlProjectContext.DataBind();

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

            BindInstances();
        }

        private void BindInstances()
        {
            ctlInstanceContext.ContextKey = null;

            List<string> instances = myContextController.GetContainerInstancesForActiveSubjectAndContainer().Select(it => it.RepeatInstance.ToString()).ToList(); //subject.ContainerInstances.Where(it => it.Container == container).Select(it => it.RepeatInstance.ToString()).ToList();

            if (instances.Any())
                instances.Insert(0, String.Empty);

            ctlInstances.DataSource = instances;
            ctlInstances.DataBind();
            ctlInstances.Enabled = ctlInstances.Items.Count > 0;
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            myContextController.ActiveSubject = ctlSubjects.SelectedValue;
            ctlSubjectContext.ContextKey = ctlSubjects.SelectedValue;
            
            BindInstances();
        }

        protected void ctlInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            ctlInstanceContext.ContextKey = ctlInstances.SelectedValue;
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            myContextController.Save(this);
        }
    }
}