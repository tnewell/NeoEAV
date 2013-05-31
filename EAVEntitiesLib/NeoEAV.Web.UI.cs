using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
                    var instances = subject.ContainerInstances.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance);
                    int newRepeatInstance = instances.Any() ? instances.Max(it => it.RepeatInstance) + 1 : 0;

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
            if (container is IEAVContextControl && ((IEAVContextControl) container).ContextType == contextType)
            {
                switch (contextType)
                {
                    case ContextType.Project:
                        contextSet.ProjectControl = container as IEAVContextControl;
                        contextSet.Project = context.Projects.SingleOrDefault(it => it.Name == contextSet.ProjectControl.ContextKey);

                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Subject, parentContainer, parentInstance);
                        }
                        
                        contextSet.Project = null;
                        contextSet.ProjectControl = null;
                        break;
                    case ContextType.Subject:
                        contextSet.SubjectControl = container as IEAVContextControl;
                        contextSet.Subject = contextSet.Project.Subjects.SingleOrDefault(it => it.MemberID == contextSet.SubjectControl.ContextKey);
                        
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Container, parentContainer, parentInstance);
                        }
                        
                        contextSet.Subject = null;
                        contextSet.SubjectControl = null;
                        break;
                    case ContextType.Container:
                        contextSet.ContainerControl = container as IEAVContextControl;
                        contextSet.Container = contextSet.Project.Containers.SingleOrDefault(it => it.ParentContainer == parentContainer && it.Name == contextSet.ContainerControl.ContextKey);
                        
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Instance, parentContainer, parentInstance);
                        }
                        
                        contextSet.Container = null;
                        contextSet.ContainerControl = null;
                        break;
                    case ContextType.Instance:
                        contextSet.InstanceControl = container as IEAVContextControl;
                        contextSet.Instance = FindContainerInstance(contextSet.Subject, contextSet.Container, parentInstance, contextSet.InstanceControl.ContextKey, true);

                        if (String.IsNullOrWhiteSpace(contextSet.InstanceControl.ContextKey))
                            contextSet.InstanceControl.ContextKey = contextSet.Instance.RepeatInstance.ToString();

                        foreach (Control child in container.Controls)
                        {
                            if (!(child is IEAVContextControl) || ((IEAVContextControl) child).ContextType == ContextType.Attribute)
                                FillContextSet(contextSet, child, ContextType.Attribute, parentContainer, parentInstance);
                        }

                        parentContainer = contextSet.Container;
                        parentInstance = contextSet.Instance;

                        foreach (Control child in container.Controls)
                        {
                            if (!(child is IEAVContextControl) || ((IEAVContextControl)child).ContextType == ContextType.Container)
                                FillContextSet(contextSet, child, ContextType.Container, parentContainer, parentInstance);
                        }

                        contextSet.Container = parentContainer;
                        contextSet.Instance = parentInstance;

                        if (!contextSet.Instance.Values.Any() && !contextSet.Instance.ChildContainerInstances.Any())
                            context.ContainerInstances.Remove(contextSet.Instance);

                        contextSet.Instance = null;
                        contextSet.InstanceControl = null;
                        break;
                    case ContextType.Attribute:
                        contextSet.AttributeControl = container as IEAVContextControl;
                        contextSet.Attribute = contextSet.Container.Attributes.SingleOrDefault(it => it.Name == contextSet.AttributeControl.ContextKey);
                        
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Value, parentContainer, parentInstance);
                        }
                        
                        contextSet.Attribute = null;
                        contextSet.AttributeControl = null;
                        break;
                    case ContextType.Value:
                        contextSet.ValueControl = container as IEAVContextControl;
                        contextSet.Value = FindValue(contextSet.Attribute, contextSet.Instance, false);
                        
                        if (contextSet.Value != null)
                        {
                            if (String.IsNullOrWhiteSpace(contextSet.ValueControl.ContextKey))
                                context.Values.Remove(contextSet.Value);
                            else if (contextSet.Value.RawValue != contextSet.ValueControl.ContextKey)
                                contextSet.Value.RawValue = contextSet.ValueControl.ContextKey;
                        }
                        else if (!String.IsNullOrWhiteSpace(contextSet.ValueControl.ContextKey))
                        {
                            contextSet.Value = FindValue(contextSet.Attribute, contextSet.Instance, true);
                            contextSet.Value.RawValue = contextSet.ValueControl.ContextKey;
                        }
                        
                        contextSet.Value = null;
                        contextSet.ValueControl = null;
                        break;
                }
            }
            else
            {
                foreach (Control child in container.Controls)
                {
                    FillContextSet(contextSet, child, contextType, parentContainer, parentInstance);
                }
            }
        }

        private Project activeProject;
        public string ActiveProject
        {
            get { return(activeProject != null ? activeProject.Name : null); }
            set { activeProject = context.Projects.FirstOrDefault(it => it.Name == value); }
        }

        private Subject activeSubject;
        public string ActiveSubject
        {
            get { return (activeSubject != null ? activeSubject.MemberID : null); }
            set { activeSubject = GetSubjectsForActiveProject().FirstOrDefault(it => it.MemberID == value); }
        }

        private Container activeContainer;
        public string ActiveContainer
        {
            get { return (activeContainer != null ? activeContainer.Name : null); }
            set { activeContainer = GetContainersForActiveProject().FirstOrDefault(it => it.Name == value); }
        }

        private ContainerInstance activeContainerInstance;
        public string ActiveContainerInstance
        {
            get { return (activeContainerInstance != null ? activeContainerInstance.RepeatInstance.ToString() : null); }
            set { activeContainerInstance = GetContainerInstancesForActiveSubjectAndContainer().FirstOrDefault(it => it.RepeatInstance.ToString() == value); }
        }

        public IEnumerable<Subject> GetSubjectsForActiveProject() { return (activeProject != null ? activeProject.Subjects : Enumerable.Empty<Subject>()); }

        public IEnumerable<Container> GetContainersForActiveProject() { return (activeProject != null ? activeProject.Containers.Where(it => it.ParentContainer == null) : Enumerable.Empty<Container>()); }

        public IEnumerable<ContainerInstance> GetContainerInstancesForActiveSubjectAndContainer() { return (activeSubject != null ? activeSubject.ContainerInstances.Where(it => it.Container == activeContainer && it.ParentContainerInstance == null) : Enumerable.Empty<ContainerInstance>()); }

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
        public IEAVContextControl ValueControl { get; set; }
    }

    public enum ContextType { Unknown = 0, Project = 1, Subject = 2, Container = 3, Instance = 4, Attribute = 5, Value = 6 }

    [Flags]
    public enum BindingType { Unknown = 0, Data = 1, Metadata = 2 }

    // TODO: Move IDataItemContainer here?
    public interface IEAVContextControl
    {
        // TODO: Turn this into DataParent and MetadataParent
        IEAVContextControl ParentContextControl { get; }

        ContextType ContextType { get; }
        
        string ContextKey { get; set; }
        
        BindingType BindingType { get; }
    }

    public abstract class EAVContextControl : Control, IEAVContextControl, IDataItemContainer
    {
        public static IEAVContextControl FindAncestor(Control control, ContextType ancestorContextType)
        {
            Control container = control != null ? control.Parent : null;
            while (container != null)
            {
                if (container is IEAVContextControl && ((IEAVContextControl)container).ContextType == ancestorContextType)
                {
                    return (container as IEAVContextControl);
                }

                container = container.Parent;
            }

            return (null);
        }

        public static T FindAncestorDataItem<T>(Control control, ContextType ancestorContextType) where T : class
        {
            IEAVContextControl container = FindAncestor(control, ancestorContextType);

            if (container != null)
            {
                return (DataBinder.GetDataItem(container) as T);
            }

            return (null);
        }

        protected bool ContextKeyChanged { get; set; }

        public abstract IEAVContextControl ParentContextControl { get; }

        public abstract ContextType ContextType { get; }

        public abstract object DataItem { get; }

        public abstract BindingType BindingType { get; }

        public virtual string ContextKey
        {
            get { return (ViewState["ContextKey"] as string); }
            set
            {
                ViewState["ContextKey"] = value;
                ContextKeyChanged = true;
            }
        }

        public virtual int DataItemIndex { get { return (0); } }

        public virtual int DisplayIndex { get { return (0); } }

        public virtual object DataSource { get; set; }

        public virtual bool DynamicContextKey { get; set; }

        protected override void OnInit(EventArgs e)
        {
            // This resets our status after object creation
            ContextKeyChanged = false;

            base.OnInit(e);
        }
    }

    public partial class EAVProjectContextControl : EAVContextControl
    {
        public override IEAVContextControl ParentContextControl { get { return (null); } }

        public override ContextType ContextType { get { return (ContextType.Project); } }

        public override BindingType BindingType
        {
            get
            {
                return(ContextKeyChanged ? UI.BindingType.Metadata | UI.BindingType.Data : UI.BindingType.Unknown);
            }
        }

        public override object DataItem
        {
            get
            {
                IEnumerable<Project> dataSource = DataSource as IEnumerable<Project>;
                
                return(dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextKey) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);
        }
    }

    public partial class EAVSubjectContextControl : EAVContextControl
    {
        public override IEAVContextControl ParentContextControl { get { return (FindAncestor(this, UI.ContextType.Project)); } }

        public override ContextType ContextType { get { return (ContextType.Subject); } }

        public override BindingType BindingType
        {
            get
            {
                return (ParentContextControl.BindingType | (ContextKeyChanged ? UI.BindingType.Data : UI.BindingType.Unknown));
            }
        }

        public override object DataItem
        {
            get
            {
                IEnumerable<Subject> dataSource = DataSource as IEnumerable<Subject>;

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.MemberID == ContextKey) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Project project = FindAncestorDataItem<Project>(this, UI.ContextType.Project);
            DataSource = project != null ? project.Subjects : null;

            if (ParentContextControl.BindingType != UI.BindingType.Unknown && DynamicContextKey)
                ContextKey = null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVContainerContextControl : EAVContextControl
    {
        public override IEAVContextControl ParentContextControl { get { return (FindAncestor(this, UI.ContextType.Instance) ?? FindAncestor(this, UI.ContextType.Subject)); } }

        public override ContextType ContextType { get { return (ContextType.Container); } }

        public override BindingType BindingType
        {
            get
            {
                return (ParentContextControl.BindingType | (ContextKeyChanged ? UI.BindingType.Metadata : UI.BindingType.Unknown));
            }
        }

        public override object DataItem
        {
            get
            {
                IEnumerable<Container> dataSource = DataSource as IEnumerable<Container>;

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextKey) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Project project = FindAncestorDataItem<Project>(this, UI.ContextType.Project);
            DataSource = project != null ? project.Containers : null;

            IEnumerable<Container> dataSource = DataSource as IEnumerable<Container>;
            if (dataSource != null)
            {
                Container parentContainer = FindAncestorDataItem<Container>(this, ContextType.Container);

                DataSource = dataSource.Where(it => it.ParentContainer == parentContainer);
            }

            if (ParentContextControl.BindingType == UI.BindingType.Metadata && DynamicContextKey)
                ContextKey = null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVInstanceContextControl : EAVContextControl
    {
        public override IEAVContextControl ParentContextControl { get { return (FindAncestor(this, UI.ContextType.Container)); } }

        public override ContextType ContextType { get { return (ContextType.Instance); } }

        public override BindingType BindingType
        {
            get
            {
                return (ParentContextControl.BindingType | (ContextKeyChanged ? UI.BindingType.Data : UI.BindingType.Unknown));
            }
        }

        public override object DataItem
        {
            get
            {
                IEnumerable<ContainerInstance> dataSource = DataSource as IEnumerable<ContainerInstance>;

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.RepeatInstance.ToString() == ContextKey) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Subject subject = FindAncestorDataItem<Subject>(this, UI.ContextType.Subject);
            DataSource = subject != null ? subject.ContainerInstances : null;

            IEnumerable<ContainerInstance> dataSource = DataSource as IEnumerable<ContainerInstance>;
            if (dataSource != null)
            {
                Container container = FindAncestorDataItem<Container>(this, ContextType.Container);
                ContainerInstance parentInstance = FindAncestorDataItem<ContainerInstance>(this, ContextType.Instance);

                DataSource = dataSource.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance);
            }

            if (ParentContextControl.BindingType != UI.BindingType.Unknown && DynamicContextKey)
            {
                Container container = FindAncestorDataItem<Container>(this, UI.ContextType.Container);

                if (subject != null && container != null && !container.IsRepeating)
                    ContextKey = subject.ContainerInstances.Where(it => it.Container == container).Select(it => it.RepeatInstance.ToString()).SingleOrDefault();
                else
                    ContextKey = null;
            }

            base.OnDataBinding(e);
        }
    }

    public partial class EAVAttributeContextControl : EAVContextControl
    {
        public override IEAVContextControl ParentContextControl { get { return (FindAncestor(this, UI.ContextType.Instance)); } }

        public override ContextType ContextType { get { return (ContextType.Attribute); } }

        public override BindingType BindingType
        {
            get
            {
                return (ParentContextControl.BindingType | (ContextKeyChanged ? UI.BindingType.Metadata : UI.BindingType.Unknown));
            }
        }

        public override object DataItem
        {
            get
            {
                IEnumerable<Attribute> dataSource = DataSource as IEnumerable<Attribute>;

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextKey) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Container container = FindAncestorDataItem<Container>(this, UI.ContextType.Container);
            DataSource = container != null ? container.Attributes : null;

            if (ParentContextControl.BindingType == UI.BindingType.Metadata && DynamicContextKey)
                ContextKey = null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVTextBox : TextBox, IEAVContextControl
    {
        public IEAVContextControl ParentContextControl { get { return (EAVContextControl.FindAncestor(this, UI.ContextType.Attribute)); } }

        public string ContextKey { get { return (Text); } set { Text = value; } }

        public ContextType ContextType { get { return (ContextType.Value); } }

        public BindingType BindingType
        {
            get
            {
                return(ParentContextControl.BindingType);
            }
        }

        public override bool Enabled
        {
            get
            {
                return(base.Enabled && EAVContextControl.FindAncestorDataItem<Subject>(this, ContextType.Subject) != null);
            }
            set
            {
                base.Enabled = value;
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Value value = null;

            ContainerInstance instance = EAVContextControl.FindAncestorDataItem<ContainerInstance>(this, ContextType.Instance);
            if (instance != null)
            {
                Attribute attribute = EAVContextControl.FindAncestorDataItem<Attribute>(this, ContextType.Attribute);

                value = instance.Values.SingleOrDefault(it => it.Attribute == attribute);
            }

            ContextKey = value != null ? value.RawValue : null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVInstanceContextRepeaterItemTemplate : ITemplate
    {
        public void InstantiateIn(Control container)
        {
            EAVInstanceContextControl ctl = container.Controls.OfType<EAVInstanceContextControl>().FirstOrDefault();

            if (ctl == null)
            {
                ctl = new AutoGen.EAVAutoInstanceContextControl() { DynamicContextKey = true };
                container.Controls.Add(ctl);
                container.Controls.Add(new LiteralControl("<br/>"));
            }
        }
    }

    public partial class EAVInstanceContextRepeater : Repeater
    {
        protected override RepeaterItem CreateItem(int itemIndex, ListItemType itemType)
        {
            RepeaterItem item = base.CreateItem(itemIndex, itemType);

            item.DataBinding += RepeaterItem_DataBinding;

            return (item);
        }

        protected void RepeaterItem_DataBinding(object sender, EventArgs e)
        {
            RepeaterItem item = sender as RepeaterItem;
            EAVInstanceContextControl ctl = item.Controls.OfType<EAVInstanceContextControl>().FirstOrDefault();

            if (item.DataItem != null && ctl != null)
            {
                ctl.ContextKey = ((ContainerInstance)item.DataItem).RepeatInstance.ToString();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (ItemTemplate == null)
            {
                ItemTemplate = new EAVInstanceContextRepeaterItemTemplate();
            }

            base.OnInit(e);
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Subject subject = EAVContextControl.FindAncestorDataItem<Subject>(this, ContextType.Subject);
            DataSource = subject != null ? subject.ContainerInstances : null;
            IEnumerable<ContainerInstance> dataSource = DataSource as IEnumerable<ContainerInstance> ?? Enumerable.Empty<ContainerInstance>();

            Container container = EAVContextControl.FindAncestorDataItem<Container>(this, ContextType.Container);
            ContainerInstance parentInstance = EAVContextControl.FindAncestorDataItem<ContainerInstance>(this, ContextType.Instance);

            dataSource = dataSource.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance);

            if (subject != null && (!dataSource.Any() || container.IsRepeating))
                dataSource = dataSource.Concat(new ContainerInstance[] { null });

            DataSource = dataSource;

            base.OnDataBinding(e);
        }
    }

    namespace AutoGen
    {
        public partial class EAVAutoContainerContextControl : EAVContainerContextControl
        {
            protected override void CreateChildControls()
            {
                if (!String.IsNullOrWhiteSpace(ContextKey))
                {
                    if (DataSource == null)
                        DataBind();

                    Container container = DataItem as Container;
                    if (container != null)
                    {
                        if (container.IsRepeating)
                        {
                            EAVInstanceContextRepeater rptr = new EAVInstanceContextRepeater();
                            Controls.Add(rptr);
                            rptr.DataBind();
                        }
                        else
                        {
                            Control ctl = FindAncestor(this, UI.ContextType.Subject) as Control;
                            if (ctl != null && DataBinder.GetPropertyValue(ctl, "DataSource") == null)
                                ctl.DataBind();

                            Subject subject = FindAncestorDataItem<Subject>(this, UI.ContextType.Subject);

                            Controls.Add(new EAVAutoInstanceContextControl() { DynamicContextKey = true, ContextKey = subject != null ? subject.ContainerInstances.Where(it => it.Container == container).Select(it => it.RepeatInstance.ToString()).FirstOrDefault() : null });
                        }
                    }
                }
            }

            protected override void OnDataBinding(EventArgs e)
            {
                base.OnDataBinding(e);

                if (ContextKeyChanged)
                    ChildControlsCreated = false;
            }
        }

        public partial class EAVAutoInstanceContextControl : EAVInstanceContextControl
        {
            protected override void CreateChildControls()
            {
                if (DataSource == null)
                    DataBind();

                Container container = FindAncestorDataItem<Container>(this, UI.ContextType.Container);
                if (container != null)
                {
                    foreach (Attribute attribute in container.Attributes)
                    {
                        Controls.Add(new EAVAutoAttributeContextControl() { ContextKey = attribute.Name });
                    }

                    foreach (Container childContainer in container.ChildContainers)
                    {
                        Controls.Add(new LiteralControl("<br/>"));
                        Controls.Add(new EAVAutoContainerContextControl() { ContextKey = childContainer.Name });
                    }
                }
            }

            protected override void OnDataBinding(EventArgs e)
            {
                base.OnDataBinding(e);

                if (ContextKeyChanged)
                    ChildControlsCreated = false;
            }
        }

        public partial class EAVAutoAttributeContextControl : EAVAttributeContextControl
        {
            protected override void CreateChildControls()
            {
                if (!String.IsNullOrWhiteSpace(ContextKey))
                {
                    if (DataSource == null)
                        DataBind();

                    Attribute attribute = DataItem as Attribute;
                    if (attribute != null)
                    {
                        Controls.Add(new Label() { Text = " [" + attribute.DisplayName + "] " });
                        EAVTextBox ctl = new EAVTextBox();
                        Controls.Add(ctl);
                        ctl.DataBind();
                    }
                }
            }

            protected override void OnDataBinding(EventArgs e)
            {
                base.OnDataBinding(e);

                if (ContextKeyChanged)
                    ChildControlsCreated = false;
            }
        }
    }
}
