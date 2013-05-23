using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NeoEAV.Web.UI;


namespace NeoEAVWeb
{
    public partial class RecurringExample : System.Web.UI.Page
    {
        EAVContextController myContextController = new EAVContextController();

        protected override void OnInitComplete(EventArgs e)
        {
            Debug.WriteLine(null);

            ctlProjectContext.DataSource = myContextController.Projects;

            base.OnInitComplete(e);
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
            base.OnPreRender(e);
        }

        private void BindProjects()
        {
            Debug.WriteLine("BindProjects");
            
            ctlProjectContext.DataSource = myContextController.Projects;
            ctlProjectContext.DataBind();

            BindSubjects();
        }

        private void BindSubjects()
        {
            Debug.WriteLine("BindSubjects");

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
            Debug.WriteLine("BindInstances");

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
            Debug.WriteLine("ctlSubjects_SelectedIndexChanged");

            myContextController.ActiveSubject = ctlSubjects.SelectedValue;

            ctlSubjectContext.ContextKey = ctlSubjects.SelectedValue;
            ctlProjectContext.DataBind();

            BindInstances();
        }

        protected void ctlInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("ctlInstances_SelectedIndexChanged");
            
            ctlInstanceContext.ContextKey = ctlInstances.SelectedValue;
            ctlProjectContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            //myContextController.Save(this);
        }
    }
}
