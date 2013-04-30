using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NeoEAV.Data.DataClasses;
using Attribute = NeoEAV.Data.DataClasses.Attribute;
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
            if (!IsPostBack)
            {
                BindProjects();
            }
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
            
            Project project = ctlProjectContext.DataItem as Project;
            if (project != null)
            {
                List<string> members = project.Subjects.Select(it => it.MemberID).ToList();

                members.Insert(0, String.Empty);

                ctlSubjects.DataSource = members;
                ctlSubjects.DataBind();
            }

            if (!String.IsNullOrWhiteSpace(ctlSubjectContext.ContextSelector))
            {
                ctlSubjectContext.ContextSelector = null;
                ctlSubjectContext.DataBind();
            }

            BindInstances();
        }

        private void BindInstances()
        {
            Debug.WriteLine("BindInstances");
            
            Subject subject = ctlSubjectContext.DataItem as Subject;
            if (subject != null)
            {
                Container container = ctlContainerContext.DataItem as Container;
                List<string> members = subject.ContainerInstances.Where(it => it.Container == container).Select(it => it.RepeatInstance.ToString()).ToList();

                members.Insert(0, String.Empty);

                ctlInstances.DataSource = members;
                ctlInstances.DataBind();
            }

            if (!String.IsNullOrWhiteSpace(ctlInstanceContext.ContextSelector))
            {
                ctlInstanceContext.ContextSelector = null;
                ctlInstanceContext.DataBind();
            }
        }

        protected void ctlSubjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("ctlSubjects_SelectedIndexChanged");

            ctlSubjectContext.ContextSelector = ctlSubjects.SelectedValue;

            ctlProjectContext.DataBind();

            BindInstances();
        }

        protected void ctlInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("ctlInstances_SelectedIndexChanged");
            
            ctlInstanceContext.ContextSelector = ctlInstances.SelectedValue;
            
            ctlProjectContext.DataBind();
        }

        protected void ctlChildInstance1List_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("ctlChildInstance1List_SelectedIndexChanged");

            ctlChildInstanceContext1.ContextSelector = ctlChildInstance1List.SelectedValue;

            ctlProjectContext.DataBind();
        }

        protected void ctlSaveButton_Click(object sender, EventArgs e)
        {
            myContextController.Bind(this, true);
        }
    }
}

namespace NeoEAV.Web.UI
{
    #region Autogen Controls
    //public partial class ContainerRepeaterDataItem : RepeaterItem
    //{
    //    #region Recast Properties
    //    // Cast of Parent property
    //    public ContainerRepeater MyParent { get { return (Parent as ContainerRepeater); } }

    //    // Cast of DataItem property
    //    public Container MyDataItem { get { return (DataItem as Container); } }

    //    public ITextControl MyControl { get { return (Controls.Count > 0 ? Controls[0] as ITextControl : null); } }
    //    #endregion

    //    public ContainerRepeaterDataItem(int itemIndex, ListItemType itemType) : base(itemIndex, itemType) { }

    //    #region Base Method Overrides
    //    protected override void OnDataBinding(EventArgs e)
    //    {
    //        Debug.Indent();
    //        Debug.WriteLine("ContainerRepeaterDataItem::OnDataBinding");

    //        base.OnDataBinding(e);

    //        if (MyDataItem != null && MyControl != null)
    //        {
    //            MyControl.Text = MyDataItem.Name;
    //        }
    //        Debug.Unindent();
    //    }
    //    #endregion
    //}

    //public partial class ContainerItemTemplate : ITemplate
    //{
    //    public void InstantiateIn(Control container)
    //    {
    //        ContainerRepeaterDataItem dataItem = container as ContainerRepeaterDataItem;

    //        switch (dataItem.ItemType)
    //        {
    //            case ListItemType.Header:
    //                dataItem.Controls.Add(new Label() { ID = "ctlHeader", Text = "Header--" });
    //                break;
    //            case ListItemType.Item:
    //            case ListItemType.AlternatingItem:
    //                dataItem.Controls.Add(new Label() { ID = "ctlItem", Text = "Item" });
    //                break;
    //            case ListItemType.Separator:
    //                dataItem.Controls.Add(new Label() { ID = "ctlSeparator", Text = "--Separator--" });
    //                break;
    //            case ListItemType.Footer:
    //                dataItem.Controls.Add(new Label() { ID = "ctlFooter", Text = "--Footer" });
    //                break;
    //        }
    //    }
    //}

    //public partial class ContainerRepeater : Repeater
    //{
    //    #region Recast Properties
    //    // Cast of Parent property
    //    //public ContainerInstanceRepeaterDataItem MyParent { get { return (Parent as ContainerInstanceRepeaterDataItem); } }

    //    // Cast of DataSource property
    //    public IEnumerable<Container> MyDataSource
    //    {
    //        get { return (DataSource as IEnumerable<Container>); }
    //        set { DataSource = value; }
    //    }

    //    // Cast of Items property
    //    //public IEnumerable<ContainerInstanceRepeater> ContainerInstances
    //    //{
    //    //    get
    //    //    {
    //    //        return (Items.OfType<ContainerRepeaterDataItem>().Select(it => it.InstanceRepeater));
    //    //    }
    //    //}
    //    #endregion

    //    public HtmlTextWriterTag TagKey
    //    {
    //        get
    //        {
    //            if (ViewState["TagKey"] == null)
    //                TagKey = HtmlTextWriterTag.Table;

    //            return ((HtmlTextWriterTag)ViewState["TagKey"]);
    //        }
    //        set
    //        {
    //            if (value != HtmlTextWriterTag.Table && value != HtmlTextWriterTag.Div)
    //                throw (new ArgumentException("Legal values are 'Table' or 'Div'."));

    //            ViewState["TagKey"] = value;
    //        }
    //    }

    //    #region Base Method Overrides
    //    protected override void OnInit(EventArgs e)
    //    {
    //        base.OnInit(e);

    //        if (HeaderTemplate == null)
    //            HeaderTemplate = new ContainerItemTemplate();

    //        if (ItemTemplate == null)
    //            ItemTemplate = new ContainerItemTemplate();

    //        if (AlternatingItemTemplate == null)
    //            AlternatingItemTemplate = new ContainerItemTemplate();

    //        if (SeparatorTemplate == null)
    //            SeparatorTemplate = new ContainerItemTemplate();

    //        if (FooterTemplate == null)
    //            FooterTemplate = new ContainerItemTemplate();
    //    }

    //    protected override RepeaterItem CreateItem(int itemIndex, ListItemType itemType)
    //    {
    //        return (new ContainerRepeaterDataItem(itemIndex, itemType)
    //        {
    //        });
    //    }
    //    #endregion
    //}
    #endregion
}
