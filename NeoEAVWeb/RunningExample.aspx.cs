﻿using System;
using System.Collections.Generic;
using System.Linq;

using NeoEAV.Web.UI;


namespace NeoEAVWeb
{
    public partial class RunningExample : System.Web.UI.Page
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
            myContextController.ActiveContainer = ctlRootContainer.ContextKey;
            myContextController.ActiveContainerInstance = null; // Running form doesn't have an active instance

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

            ctlSubjectContext.ContextKey = myContextController.ActiveSubject;
            
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