using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NeoEAV.Data.DataClasses;
using NeoEAV.Web.UI;


namespace NeoEAVWeb
{
    public partial class RecurringExample : System.Web.UI.Page
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
            myContextController.ActiveContainerInstance = ctlInstanceContext.ContextKey;

            if (!IsPostBack)
            {
                BindProjects();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ctlSaveButton.Enabled = ctlSubjects.SelectedIndex > 0;
        }

        private void BindProjects()
        {
            ctlProjectContext.DataBind();

            BindSubjects();
        }

        private void BindSubjects()
        {
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
            List<string> instances = myContextController.GetContainerInstancesForActiveSubjectAndContainer().Select(it => it.RepeatInstance.ToString()).ToList();

            if (instances.Any())
                instances.Insert(0, String.Empty);

            ctlInstances.DataSource = instances;
            ctlInstances.DataBind();
            ctlInstances.Enabled = ctlInstances.Items.Count > 0;

            if (myContextController.ActiveContainerInstance != ctlInstanceContext.ContextKey)
                myContextController.ActiveContainerInstance = ctlInstanceContext.ContextKey;
            
            ctlInstances.SelectedValue = myContextController.ActiveContainerInstance;
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            myContextController.ActiveSubject = ctlSubjects.SelectedValue;
            myContextController.ActiveContainerInstance = null;

            ctlSubjectContext.ContextKey = myContextController.ActiveSubject;
            ctlInstanceContext.ContextKey = myContextController.ActiveContainerInstance;
            
            ctlProjectContext.DataBind();

            BindInstances();
        }

        protected void ctlInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            myContextController.ActiveContainerInstance = ctlInstances.SelectedValue;
            
            ctlInstanceContext.ContextKey = myContextController.ActiveContainerInstance;

            ctlProjectContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            //myContextController.Save(this);
            myContextController.Save(ctlProjectContext);

            ctlProjectContext.DataBind();

            BindInstances();
        }
    }
}
