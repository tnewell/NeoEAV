using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NeoEAV.Data.DataClasses;
using Attribute = NeoEAV.Data.DataClasses.Attribute;
using Container = NeoEAV.Data.DataClasses.Container;


namespace NeoEAV.Web.UI
{
    public partial class EAVContextController
    {
        private EAVEntityContext context = new EAVEntityContext();

        public IEnumerable<Project> Projects { get { return(context.Projects); } }

        protected string BuildContextValue(Control control)
        {
            if (control == null)
                return (String.Empty);

            Control parent = control.Parent;
            while (parent != null && !(parent is IEAVContextControl))
                parent = parent.Parent;

            if (!(control is IEAVContextControl))
                return (BuildContextValue(parent));
            else if (parent == null)
                return (((IEAVContextControl)control).ContextSelector);
            else
                return (String.Concat(BuildContextValue(parent), "|", ((IEAVContextControl)control).ContextSelector));
        }

        protected IEnumerable<IEAVDataControl> FindEAVValueControls(Control control)
        {
            List<UI.IEAVDataControl> controls = new List<UI.IEAVDataControl>();

            controls.AddRange(control.Controls.OfType<UI.IEAVDataControl>());

            foreach (Control childControl in control.Controls)
            {
                controls.AddRange(FindEAVValueControls(childControl));
            }

            return (controls);
        }

        private Subject FindSubject(Project project, string memberID, bool createIfMissing)
        {
            Subject subject = project != null ? project.Subjects.SingleOrDefault(it => it.MemberID == memberID) : null;

            if (subject == null && project != null && !String.IsNullOrEmpty(memberID) && createIfMissing)
            {
                throw (new ApplicationException(String.Format("Attempt to create new Subject when Member ID not found disallowed. Member ID = '{0}'.", memberID)));
            }

            return (subject);
        }

        private ContainerInstance FindContainerInstance(Container container, Subject subject, ContainerInstance parentInstance, string repeatInstance, bool createIfMissing)
        {
            ContainerInstance instance = container != null ? container.ContainerInstances.SingleOrDefault(it => it.Subject == subject && it.ParentContainerInstance == parentInstance && it.RepeatInstance.ToString() == repeatInstance) : null;
            bool parentInstanceCorrect = container == null || (container.ParentContainer != null ^ parentInstance == null);

            if (instance == null && container != null && subject != null && parentInstanceCorrect && createIfMissing)
            {
                if (String.IsNullOrWhiteSpace(repeatInstance))
                {
                    instance = new ContainerInstance() { Subject = subject, ParentContainerInstance = parentInstance };
                    container.ContainerInstances.Add(instance);
                }
                else
                {
                    throw (new ApplicationException(String.Format("Attempt to create new Container Instance when Repeat Instance not found disallowed. Repeat Instance = '{0}'.", repeatInstance)));
                }
            }

            return (instance);
        }

        private Value FindValue(Attribute attribute, ContainerInstance instance, bool createIfMissing)
        {
            Value value = attribute != null ? attribute.Values.SingleOrDefault(it => it.ContainerInstance == instance) : null;

            if (value == null && attribute != null && instance != null && createIfMissing)
            {
                value = new Value() { ContainerInstance = instance };
                attribute.Values.Add(value);
            }

            return (value);
        }

        public void Bind(Control contextControl, bool bindToSource = false)
        {
            // Need to finesse this so that we don't keep looking up stuff
            // we don't need to. Order by context maybe.
            foreach (IEAVDataControl control in FindEAVValueControls(contextControl))
            {
                string[] contextValues = BuildContextValue(control as Control).Split('|');
                if (contextValues.Length >= 5)
                {
                    string strProject = contextValues[0];
                    string strSubject = contextValues[1];
                    string strAttribute = contextValues[contextValues.Length - 1];

                    Project project = context.Projects.Single(it => it.Name == strProject);
                    Subject subject = FindSubject(project, strSubject, bindToSource);

                    Container container = project.Containers.Single(it => it.Name == contextValues[2]);
                    ContainerInstance instance = FindContainerInstance(container, subject, null, contextValues[3], bindToSource);

                    for (int index = 4; index < (contextValues.Length - 2) && container != null; index += 2)
                    {
                        container = container.ChildContainers.Single(it => it.Name == contextValues[index]);

                        if (instance != null)
                            instance = FindContainerInstance(container, subject, instance, contextValues[index + 1], bindToSource);
                    }

                    Attribute attribute = container.Attributes.Single(it => it.Name == strAttribute);
                    Value value = FindValue(attribute, instance, false);

                    if (bindToSource)
                    {
                        if (value != null)
                        {
                            if (String.IsNullOrWhiteSpace(control.RawValue))
                                context.Values.Remove(value);
                            else
                                value.RawValue = control.RawValue;
                        }
                        else if (!String.IsNullOrWhiteSpace(control.RawValue))
                        {
                            value = FindValue(attribute, instance, true);
                            value.RawValue = control.RawValue;
                        }
                    }
                    else
                    {
                        control.RawValue = (value == null ? null : value.RawValue);
                    }
                }
            }

            // TODO: Need to prune empty bits out before saving

            if (bindToSource)
                context.SaveChanges();
        }
    }

    public enum ContextType { Unknown = 0, Project = 1, Subject = 2, Container = 3, Instance = 4, Attribute = 5, Value = 6 }

    public interface IEAVContextControl
    {
        ContextType ContextType { get; }
        string ContextSelector { get; set; }
    }

    public interface IEAVDataControl
    {
        string RawValue { get; set; }
    }

    public abstract class EAVContextControl : Control, IEAVContextControl, IDataItemContainer
    {
        public virtual string ContextSelector { get { return (ViewState["ContextSelector"] as string); } set { ViewState["ContextSelector"] = value; DataSource = null; } }

        public abstract ContextType ContextType { get; }

        public abstract object DataItem { get; }

        public virtual int DataItemIndex { get { return (0); } }

        public virtual int DisplayIndex { get { return (0); } }

        public virtual object DataSource { get; set; }

        public static T FindAncestorDataItem<T>(Control control, ContextType ancestorContextType) where T : class
        {
            Control container = control != null ? control.Parent : null;
            while (container != null)
            {
                if (container is IEAVContextControl && ((IEAVContextControl)container).ContextType == ancestorContextType)
                {
                    return(DataBinder.GetDataItem(container) as T);
                }

                container = container.Parent;
            }

            return (null);
        }
    }

    public partial class EAVProjectContextControl : EAVContextControl
    {
        public override ContextType ContextType { get { return (ContextType.Project); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<Project> dataSource = DataSource as IEnumerable<Project>;
                
                return(dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextSelector) : null);
            }
        }
    }

    public partial class EAVSubjectContextControl : EAVContextControl
    {
        public override ContextType ContextType { get { return (ContextType.Subject); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<Subject> dataSource = DataSource as IEnumerable<Subject>;

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.MemberID == ContextSelector) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Project project = FindAncestorDataItem<Project>(this, UI.ContextType.Project);
            DataSource = project != null ? DataBinder.GetPropertyValue(project, "Subjects") : null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVContainerContextControl : EAVContextControl
    {
        public override ContextType ContextType { get { return (ContextType.Container); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<Container> dataSource = DataSource as IEnumerable<Container>;

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextSelector) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Project project = FindAncestorDataItem<Project>(this, UI.ContextType.Project);
            DataSource = project != null ? DataBinder.GetPropertyValue(project, "Containers") : null;

            FilterDataSource();

            base.OnDataBinding(e);
        }

        public void FilterDataSource()
        {
            IEnumerable<Container> dataSource = DataSource as IEnumerable<Container>;
            if (dataSource != null)
            {
                Container parentContainer = FindAncestorDataItem<Container>(this, ContextType.Container);

                DataSource = dataSource.Where(it => it.ParentContainer == parentContainer);
            }
        }
    }

    public partial class EAVInstanceContextControl : EAVContextControl
    {
        public override ContextType ContextType { get { return (ContextType.Instance); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<ContainerInstance> dataSource = DataSource as IEnumerable<ContainerInstance>;

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.RepeatInstance.ToString() == ContextSelector) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Subject subject = FindAncestorDataItem<Subject>(this, UI.ContextType.Subject);
            DataSource = subject != null ? DataBinder.GetPropertyValue(subject, "ContainerInstances") : null;

            FilterDataSource();

            base.OnDataBinding(e);
        }
        
        public void FilterDataSource()
        {
            IEnumerable<ContainerInstance> dataSource = DataSource as IEnumerable<ContainerInstance>;
            if (dataSource != null)
            {
                Container container = FindAncestorDataItem<Container>(this, ContextType.Container);
                ContainerInstance parentInstance = FindAncestorDataItem<ContainerInstance>(this, ContextType.Instance);

                DataSource = dataSource.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance);
            }
        }
    }

    public partial class EAVAttributeContextControl : EAVContextControl
    {
        public override ContextType ContextType { get { return (ContextType.Attribute); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<Attribute> dataSource = DataSource as IEnumerable<Attribute>;

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextSelector) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Container project = FindAncestorDataItem<Container>(this, UI.ContextType.Container);
            DataSource = project != null ? DataBinder.GetPropertyValue(project, "Attributes") : null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVTextBox : TextBox, IEAVDataControl, IDataItemContainer
    {
        public string RawValue { get { return (Text); } set { Text = value; } }

        public object DataItem
        {
            get
            {
                ContainerInstance instance = EAVContextControl.FindAncestorDataItem<ContainerInstance>(this, ContextType.Instance);
                if (instance != null)
                {
                    Attribute attribute = EAVContextControl.FindAncestorDataItem<Attribute>(this, ContextType.Attribute);

                    return (instance.Values.SingleOrDefault(it => it.Attribute == attribute));
                }

                return (null);
            }
        }

        public int DataItemIndex
        {
            get { return (0); }
        }

        public int DisplayIndex
        {
            get { return (0); }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Value value = DataItem as Value;

            RawValue = value != null ? value.RawValue : null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVInstanceContextRepeaterItem : RepeaterItem, IEAVContextControl
    {
        public string ContextSelector { get { return (ViewState["ContextSelector"] as string); } set { ViewState["ContextSelector"] = value; } }

        public ContextType ContextType { get { return ((ContextType)(ViewState["ContextType"] ?? ContextType.Unknown)); } set { ViewState["ContextType"] = value; } }

        public EAVInstanceContextRepeaterItem(int itemIndex, ListItemType itemType, ContextType contextType) : base(itemIndex, itemType) { ContextType = contextType; }

        protected override void OnDataBinding(EventArgs e)
        {
            if (DataItem != null)
                ContextSelector = ((ContainerInstance)DataItem).RepeatInstance.ToString();
            else
                ContextSelector = null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVInstanceContextRepeater : Repeater
    {
        protected override RepeaterItem CreateItem(int itemIndex, ListItemType itemType)
        {
            if (itemType == ListItemType.AlternatingItem || itemType == ListItemType.EditItem || itemType == ListItemType.Item || itemType == ListItemType.SelectedItem)
                return (new EAVInstanceContextRepeaterItem(itemIndex, itemType, ContextType.Instance));
            else
                return base.CreateItem(itemIndex, itemType);
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Subject subject = EAVContextControl.FindAncestorDataItem<Subject>(this, ContextType.Subject);
            DataSource = subject != null ? DataBinder.GetPropertyValue(subject, "ContainerInstances") : null;

            FilterDataSource();

            base.OnDataBinding(e);
        }

        public void FilterDataSource()
        {
            IEnumerable<ContainerInstance> dataSource = DataSource as IEnumerable<ContainerInstance>;
            if (dataSource != null)
            {
                Container container = EAVContextControl.FindAncestorDataItem<Container>(this, ContextType.Container);
                ContainerInstance parentInstance = EAVContextControl.FindAncestorDataItem<ContainerInstance>(this, ContextType.Instance);

                DataSource = dataSource.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance);
            }
        }
    }
}
