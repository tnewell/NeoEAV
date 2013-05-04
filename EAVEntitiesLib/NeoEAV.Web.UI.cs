using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private ContainerInstance FindContainerInstance(Subject subject, Container container, ContainerInstance parentInstance, string repeatInstance, bool createIfMissing)
        {
            ContainerInstance instance = subject != null ? subject.ContainerInstances.SingleOrDefault(it => it.Container == container && it.ParentContainerInstance == parentInstance && it.RepeatInstance.ToString() == repeatInstance) : null;
            bool parentInstanceCorrect = container == null || (container.ParentContainer != null ^ parentInstance == null);

            if (instance == null && container != null && subject != null && parentInstanceCorrect && createIfMissing)
            {
                if (String.IsNullOrWhiteSpace(repeatInstance))
                {
                    int newRepeatInstance = subject.ContainerInstances.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance).Max(it => it.RepeatInstance) + 1;

                    instance = new ContainerInstance() { Container = container, ParentContainerInstance = parentInstance, RepeatInstance = newRepeatInstance };
                    subject.ContainerInstances.Add(instance);
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
            Value value = instance != null ? instance.Values.SingleOrDefault(it => it.Attribute == attribute) : null;

            if (value == null && attribute != null && instance != null && createIfMissing)
            {
                value = new Value() { Attribute = attribute };
                instance.Values.Add(value);
            }

            return (value);
        }

        private void FillContextSet(ContextControlSet contextSet, Control container, ContextType contextType, Container parentContainer, ContainerInstance parentInstance)
        {
            if (container is IEAVDataControl && contextType == ContextType.Value)
            {
                contextSet.ValueControl = container as IEAVDataControl;
                contextSet.Value = FindValue(contextSet.Attribute, contextSet.Instance, false);
                if (contextSet.Value != null)
                {
                    if (String.IsNullOrWhiteSpace(contextSet.ValueControl.RawValue))
                        context.Values.Remove(contextSet.Value);
                    else if (contextSet.Value.RawValue != contextSet.ValueControl.RawValue)
                        contextSet.Value.RawValue = contextSet.ValueControl.RawValue;
                }
                else if (!String.IsNullOrWhiteSpace(contextSet.ValueControl.RawValue))
                {
                    contextSet.Value = FindValue(contextSet.Attribute, contextSet.Instance, true);
                    contextSet.Value.RawValue = contextSet.ValueControl.RawValue;
                }
                contextSet.Value = null;
                contextSet.ValueControl = null;
            }
            else if (container is IEAVContextControl && ((IEAVContextControl) container).ContextType == contextType)
            {
                switch (contextType)
                {
                    case ContextType.Project:
                        contextSet.ProjectControl = container as IEAVContextControl;
                        contextSet.Project = context.Projects.SingleOrDefault(it => it.Name == contextSet.ProjectControl.ContextSelector);
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Subject, parentContainer, parentInstance);
                        }
                        contextSet.Project = null;
                        contextSet.ProjectControl = null;
                        break;
                    case ContextType.Subject:
                        contextSet.SubjectControl = container as IEAVContextControl;
                        contextSet.Subject = contextSet.Project.Subjects.SingleOrDefault(it => it.MemberID == contextSet.SubjectControl.ContextSelector);
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Container, parentContainer, parentInstance);
                        }
                        contextSet.Subject = null;
                        contextSet.SubjectControl = null;
                        break;
                    case ContextType.Container:
                        contextSet.ContainerControl = container as IEAVContextControl;
                        contextSet.Container = contextSet.Project.Containers.SingleOrDefault(it => it.ParentContainer == parentContainer && it.Name == contextSet.ContainerControl.ContextSelector);
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Instance, parentContainer, parentInstance);
                        }
                        contextSet.Container = null;
                        contextSet.ContainerControl = null;
                        break;
                    case ContextType.Instance:
                        contextSet.InstanceControl = container as IEAVContextControl;
                        contextSet.Instance = FindContainerInstance(contextSet.Subject, contextSet.Container, parentInstance, contextSet.InstanceControl.ContextSelector, true);
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Attribute, parentContainer, parentInstance);
                        }
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Container, contextSet.Container, contextSet.Instance);
                        }
                        contextSet.Instance = null;
                        contextSet.InstanceControl = null;
                        break;
                    case ContextType.Attribute:
                        contextSet.AttributeControl = container as IEAVContextControl;
                        contextSet.Attribute = contextSet.Container.Attributes.SingleOrDefault(it => it.Name == contextSet.AttributeControl.ContextSelector);
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Value, parentContainer, parentInstance);
                        }
                        contextSet.Attribute = null;
                        contextSet.AttributeControl = null;
                        break;
                }
            }
            else if (!(container is IEAVContextControl) && !(container is IEAVDataControl))
            {
                // If we don't find what we're looking for on this level
                // keep going with the child controls
                foreach (Control child in container.Controls)
                {
                    FillContextSet(contextSet, child, contextType, parentContainer, parentInstance);
                }
            }
        }

        public void Save(Control contextControl)
        {
            ContextControlSet set = new ContextControlSet();

            FillContextSet(set, contextControl, ContextType.Project, null, null);

            context.SaveChanges();
        }
    }

    public partial class ContextControlSet
    {
        public Project Project { get; set; }
        public IEAVContextControl ProjectControl { get; set; }

        public Subject Subject { get; set; }
        public IEAVContextControl SubjectControl { get; set; }

        public Container Container { get; set; }
        public IEAVContextControl ContainerControl { get; set; }

        public ContainerInstance Instance { get; set; }
        public IEAVContextControl InstanceControl { get; set; }

        public Attribute Attribute { get; set; }
        public IEAVContextControl AttributeControl { get; set; }

        public Value Value { get; set; }
        public IEAVDataControl ValueControl { get; set; }
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
            DataSource = project != null ? project.Subjects : null;

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
            DataSource = project != null ? project.Containers : null;

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
            DataSource = subject != null ? subject.ContainerInstances : null;

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
            Container container = FindAncestorDataItem<Container>(this, UI.ContextType.Container);
            DataSource = container != null ? container.Attributes : null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVTextBox : TextBox, IEAVDataControl
    {
        public string RawValue { get { return (Text); } set { Text = value; } }

        protected override void OnDataBinding(EventArgs e)
        {
            Value value = null;

            ContainerInstance instance = EAVContextControl.FindAncestorDataItem<ContainerInstance>(this, ContextType.Instance);
            if (instance != null)
            {
                Attribute attribute = EAVContextControl.FindAncestorDataItem<Attribute>(this, ContextType.Attribute);

                value = instance.Values.SingleOrDefault(it => it.Attribute == attribute);
            }

            RawValue = value != null ? value.RawValue : null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVContextRepeaterItem : RepeaterItem, IEAVContextControl
    {
        public string ContextSelector { get { return (ViewState["ContextSelector"] as string); } set { ViewState["ContextSelector"] = value; } }

        public ContextType ContextType { get { return ((ContextType)(ViewState["ContextType"] ?? ContextType.Unknown)); } set { ViewState["ContextType"] = value; } }

        public EAVContextRepeaterItem(int itemIndex, ListItemType itemType, ContextType contextType) : base(itemIndex, itemType) { ContextType = contextType; }

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
                return (new EAVContextRepeaterItem(itemIndex, itemType, ContextType.Instance));
            else
                return base.CreateItem(itemIndex, itemType);
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Subject subject = EAVContextControl.FindAncestorDataItem<Subject>(this, ContextType.Subject);
            DataSource = subject != null ? subject.ContainerInstances : null;

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
