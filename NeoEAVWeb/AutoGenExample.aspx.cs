using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NeoEAV.Data.DataClasses;
using NeoEAV.Web.UI;
using NeoEAV.Web.UI.AutoGen;


namespace NeoEAVWeb
{
    public partial class AutoGenExample : System.Web.UI.Page
    {
        private EAVContextController myContextController = new EAVContextController();

        protected override void OnInitComplete(EventArgs e)
        {
            Debug.WriteLine(null);
            Debug.WriteLine(String.Format("OnInitComplete {{ IsPostBack = {0} }}", IsPostBack));

            base.OnInitComplete(e);

            ctlProjectContext.DataSource = myContextController.Projects;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Debug.WriteLine(String.Format("Page_Load"));

            if (!IsPostBack)
            {
                BindProjects();
            }
        }

        protected override void CreateChildControls()
        {
            Debug.WriteLine(String.Format("CreateChildControls"));
            Debug.Indent();

            base.CreateChildControls();

            Debug.Unindent();
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Debug.WriteLine(String.Format("OnDataBinding"));
            Debug.Indent();

            base.OnDataBinding(e);

            Debug.Unindent();
        }

        protected override void OnPreRender(EventArgs e)
        {
            Debug.WriteLine(String.Format("OnPreRender"));
            Debug.Indent();

            ctlProjectContext.DataBind();

            base.OnPreRender(e);

            Debug.Unindent();
        }

        protected override void OnPreRenderComplete(EventArgs e)
        {
            Debug.WriteLine(String.Format("OnPreRenderComplete"));
            Debug.Indent();

            base.OnPreRenderComplete(e);

            Debug.Unindent();
        }

        private void BindProjects()
        {
            ctlProjectContext.DataSource = myContextController.Projects;
            ctlProjectContext.DataBind();

            BindSubjects();
            BindContainers();
        }

        private void BindSubjects()
        {
            ctlSubjectContext.ContextKey = null;

            Project project = ctlProjectContext.DataItem as Project;
            if (project != null)
            {
                List<string> members = project.Subjects.Select(it => it.MemberID).ToList();

                members.Insert(0, String.Empty);

                ctlSubjects.DataSource = members;
                ctlSubjects.DataBind();
            }
        }

        private void BindContainers()
        {
            ctlContainerContext.ContextKey = null;

            Project project = ctlProjectContext.DataItem as Project;
            if (project != null)
            {
                List<string> members = project.Containers.Where(it => it.ParentContainer == null).Select(it => it.Name).ToList();

                members.Insert(0, String.Empty);

                ctlContainers.DataSource = members;
                ctlContainers.DataBind();
            }
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine(String.Format("ctlSubjects_SelectedIndexChanged {{ SelectedValue = '{0}' }}", ctlSubjects.SelectedValue));

            ctlSubjectContext.ContextKey = ctlSubjects.SelectedValue;
            ctlSubjectContext.DataBind();
        }

        protected void ctlContainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine(String.Format("ctlContainers_SelectedIndexChanged {{ SelectedValue = '{0}' }}", ctlContainers.SelectedValue));

            ctlContainerContext.ContextKey = ctlContainers.SelectedValue;
            ctlContainerContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            myContextController.Save(this);
        }
    }
}