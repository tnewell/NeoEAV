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

            myContextController.ActiveProject = ctlProjectContext.ContextKey;
            myContextController.ActiveSubject = ctlSubjectContext.ContextKey;
            myContextController.ActiveContainer = ctlContainerContext.ContextKey;

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
            myContextController.ActiveSubject = null;
            ctlSubjectContext.ContextKey = null;

            List<string> members = myContextController.GetSubjectsForActiveProject().Select(it => it.MemberID).ToList();

            if (members.Any())
                members.Insert(0, String.Empty);

            ctlSubjects.DataSource = members;
            ctlSubjects.DataBind();
            ctlSubjects.Enabled = ctlSubjects.Items.Count > 0;
        }

        private void BindContainers()
        {
            myContextController.ActiveContainer = null;
            ctlContainerContext.ContextKey = null;

            List<string> members = myContextController.GetContainersForActiveProject().Select(it => it.Name).ToList();

            if (members.Any())
                members.Insert(0, String.Empty);

            ctlContainers.DataSource = members;
            ctlContainers.DataBind();
            ctlContainers.Enabled = ctlContainers.Items.Count > 0;
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine(String.Format("ctlSubjects_SelectedIndexChanged {{ SelectedValue = '{0}' }}", ctlSubjects.SelectedValue));

            myContextController.ActiveSubject = ctlSubjects.SelectedValue;
            ctlSubjectContext.ContextKey = ctlSubjects.SelectedValue;
        }

        protected void ctlContainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine(String.Format("ctlContainers_SelectedIndexChanged {{ SelectedValue = '{0}' }}", ctlContainers.SelectedValue));

            myContextController.ActiveContainer = ctlContainers.SelectedValue;
            ctlContainerContext.ContextKey = ctlContainers.SelectedValue;
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            myContextController.Save(this);
        }
    }
}
