using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            myContextController.ActiveContainerInstance = ctlInstanceContext.ContextKey;

            if (!IsPostBack)
            {
                BindProjects();
            }
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
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            myContextController.ActiveSubject = ctlSubjects.SelectedValue;
            myContextController.ActiveContainerInstance = myContextController.GetContainerInstancesForActiveSubjectAndContainer().Select(it => it.RepeatInstance.ToString()).FirstOrDefault();

            ctlSubjectContext.ContextKey = myContextController.ActiveSubject;
            ctlInstanceContext.ContextKey = myContextController.ActiveContainerInstance;

            ctlProjectContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            //myContextController.Save(this);
            myContextController.Save2(ctlProjectContext);

            ctlProjectContext.DataBind();
        }
    }
}