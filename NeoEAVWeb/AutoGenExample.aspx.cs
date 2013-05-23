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
            Debug.WriteLine(null);
            Debug.WriteLine(String.Format("OnInitComplete {{ IsPostBack = {0} }}", IsPostBack));
            Debug.Indent();

            base.OnInitComplete(e);

            // This needs to be here before we load view state to give meaning
            // to any restored context key attributes
            ctlProjectContext.DataSource = myContextController.Projects;

            Debug.Unindent();
            Debug.WriteLine(String.Format("OnInitComplete {{ IsPostBack = {0} }}", IsPostBack));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Debug.WriteLine(String.Format("Page_Load"));
            Debug.Indent();

            // Restore our controller after view state loaded.
            myContextController.ActiveProject = ctlProjectContext.ContextKey;
            myContextController.ActiveSubject = ctlSubjectContext.ContextKey;
            myContextController.ActiveContainer = ctlContainerContext.ContextKey;

            if (!IsPostBack)
            {
                BindProjects();
            }

            Debug.Unindent();
            Debug.WriteLine(String.Format("Page_Load"));
        }

        protected override void CreateChildControls()
        {
            Debug.WriteLine(String.Format("CreateChildControls"));
            Debug.Indent();

            base.CreateChildControls();

            Debug.Unindent();
            Debug.WriteLine(String.Format("CreateChildControls"));
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Debug.WriteLine(String.Format("OnDataBinding"));
            Debug.Indent();

            base.OnDataBinding(e);

            Debug.Unindent();
            Debug.WriteLine(String.Format("OnDataBinding"));
        }

        protected override void OnPreRender(EventArgs e)
        {
            Debug.WriteLine(String.Format("OnPreRender"));
            Debug.Indent();

            base.OnPreRender(e);

            Debug.Unindent();
            Debug.WriteLine(String.Format("OnPreRender"));
        }

        protected override void OnPreRenderComplete(EventArgs e)
        {
            Debug.WriteLine(String.Format("OnPreRenderComplete"));
            Debug.Indent();

            base.OnPreRenderComplete(e);

            Debug.Unindent();
            Debug.WriteLine(String.Format("OnPreRenderComplete"));
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
            // TODO: Don't appear to need it, but what happens if we bind here?

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
            // TODO: Don't appear to need it, but what happens if we bind here?

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
            ctlProjectContext.DataBind();
        }

        protected void ctlContainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine(String.Format("ctlContainers_SelectedIndexChanged {{ SelectedValue = '{0}' }}", ctlContainers.SelectedValue));

            myContextController.ActiveContainer = ctlContainers.SelectedValue;

            ctlContainerContext.ContextKey = ctlContainers.SelectedValue;
            ctlProjectContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            //myContextController.Save(this);
        }
    }
}
