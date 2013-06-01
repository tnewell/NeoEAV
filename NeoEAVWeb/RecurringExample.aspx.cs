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

            if (!IsPostBack)
            {
                BindProjects();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ctlSaveButton.Enabled = ctlInstances.Items.Count == 0 || ctlInstances.SelectedIndex > 0;
        }

        private void BindProjects()
        {
            ctlProjectContext.DataSource = myContextController.Projects;
            ctlProjectContext.DataBind();

            BindSubjects();
        }

        private void BindSubjects()
        {
            ctlSubjectContext.ContextKey = null;

            myContextController.ActiveSubject = null;

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
            ctlSubjectContext.ContextKey = ctlSubjects.SelectedValue;
            ctlProjectContext.DataBind();

            myContextController.ActiveSubject = ctlSubjects.SelectedValue;

            BindInstances();
        }

        protected void ctlInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            ctlInstanceContext.ContextKey = ctlInstances.SelectedValue;
            ctlProjectContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            var oldList = myContextController.GetContainerInstancesForActiveSubjectAndContainer().ToList();

            myContextController.Save(this);
            ctlProjectContext.DataBind();

            var newList = myContextController.GetContainerInstancesForActiveSubjectAndContainer().ToList();

            ContainerInstance oldInstance = oldList.Except(newList).FirstOrDefault();
            ContainerInstance newInstance = newList.Except(oldList).FirstOrDefault();
            if (oldInstance != null || newInstance != null)
            {
                BindInstances();

                if (newInstance != null)
                {
                    ctlInstances.SelectedValue = newInstance.RepeatInstance.ToString();
                    ctlInstanceContext.ContextKey = newInstance.RepeatInstance.ToString();
                }
            }
        }
    }
}
