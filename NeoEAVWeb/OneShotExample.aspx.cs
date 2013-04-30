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
            ctlSubjectContext.ContextSelector = null;

            Project project = ctlProjectContext.DataItem as Project;
            if (project != null)
            {
                List<string> members = project.Subjects.Select(it => it.MemberID).ToList();

                members.Insert(0, String.Empty);

                ctlSubjects.DataSource = members;
                ctlSubjects.DataBind();
            }

            BindInstances();
        }

        private void BindInstances()
        {
            ctlInstanceContext.ContextSelector = null;

            Subject subject = ctlSubjectContext.DataItem as Subject;
            if (subject != null)
            {
                Container container = ctlContainerContext.DataItem as Container;
                List<string> members = subject.ContainerInstances.Where(it => it.Container == container).Select(it => it.RepeatInstance.ToString()).ToList();

                members.Insert(0, String.Empty);

                ctlInstances.DataSource = members;
                ctlInstances.DataBind();
            }
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            ctlSubjectContext.ContextSelector = ctlSubjects.SelectedValue;

            ctlProjectContext.DataBind();

            BindInstances();
        }

        protected void ctlInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            ctlInstanceContext.ContextSelector = ctlInstances.SelectedValue;

            ctlProjectContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            myContextController.Bind(this, true);
        }
    }
}