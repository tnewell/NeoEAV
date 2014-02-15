using NeoEAV.Data.DataClasses;
using NeoEAV.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Attribute = NeoEAV.Data.DataClasses.Attribute;
using Container = NeoEAV.Data.DataClasses.Container;


namespace NeoEAV.Web.UI
{
    using NeoEAV.Special;

    public partial class EAVContextController
    {
        private EAVEntityContext context = new EAVEntityContext();

        private static T GetDataItem<T>(IEAVContextControl control) where T : class
        {
            return (control != null ? control.DataItem as T : null);
        }

        private Value AcquireValue(Attribute attribute, ContainerInstance instance, bool createIfMissing)
        {
            Value value = instance != null ? instance.Values.SingleOrDefault(it => it.Attribute == attribute) : null;

            if (value == null && attribute != null && instance != null && createIfMissing)
            {
                value = new Value() { Attribute = attribute, ContainerInstance = instance };
                instance.Values.Add(value);
            }

            return (value);
        }

        private void UpdateValue(IEAVValueControl control, ContainerInstance dbInstance, Attribute dbAttribute)
        {
            if (control != null)
            {
                Value value = AcquireValue(dbAttribute, dbInstance, false);

                if (value != null)
                {
                    if (String.IsNullOrWhiteSpace(control.RawValue))
                        context.Values.Remove(value);
                    else if (value.RawValue != control.RawValue)
                        value.RawValue = control.RawValue;
                }
                else if (!String.IsNullOrWhiteSpace(control.RawValue))
                {
                    value = AcquireValue(dbAttribute, dbInstance, true);
                    value.RawValue = control.RawValue;
                }
            }
        }

        private ContainerInstance AcquireContainerInstance(IEAVContextControl control, ContainerInstance parentInstance, string repeatInstance, bool createIfMissing)
        {
            Container container = GetDataItem<Container>(control.ContextParent);
            Subject subject = parentInstance != null ? parentInstance.Subject : container != null ? GetDataItem<Subject>(control.ContextParent.ContextParent) : null;

            ContainerInstance instance = subject != null ? subject.ContainerInstances.SingleOrDefault(it => it.Container == container && it.ParentContainerInstance == parentInstance && it.RepeatInstance.ToString() == repeatInstance) : null;
            bool parentInstanceCorrect = container == null || (container.ParentContainer != null ^ parentInstance == null);

            if (instance == null && container != null && subject != null && parentInstanceCorrect && createIfMissing)
            {
                if (String.IsNullOrWhiteSpace(repeatInstance))
                {
                    var instances = subject.ContainerInstances.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance);

                    if (!instances.Any() || container.IsRepeating)
                    {
                        int newRepeatInstance = instances.Any() ? instances.Max(it => it.RepeatInstance) + 1 : 0;

                        instance = new ContainerInstance() { Container = container, Subject = subject, ParentContainerInstance = parentInstance, RepeatInstance = newRepeatInstance };

                        if (parentInstance == null)
                            subject.ContainerInstances.Add(instance);
                        else
                            parentInstance.ChildContainerInstances.Add(instance);
                    }
                    else
                    {
                        throw (new ApplicationException(String.Format("Attempt to acquire container instance failed. Container '{0}' is non-repeating and already has repeat instance {1} for subject '{2}'.", container.Name, instances.First().RepeatInstance, subject.MemberID)));
                    }
                }
                else
                {
                    throw (new ApplicationException(String.Format("Attempt to acquire container instance failed. No container instance found for container '{0}' and subject '{1}' having repeat instance {2}.", container.Name, subject.MemberID, repeatInstance)));
                }
            }

            return (instance);
        }

        private void FillContextSet(IEAVContextControl control, ContainerInstance dbInstance)
        {
            switch (control.ContextControlType)
            {
                case ContextControlType.Project:
                    foreach (IEAVContextControl child in control.ContextChildren)
                    {
                        FillContextSet(child, null);
                    }
                    break;
                case ContextControlType.Subject:
                    foreach (IEAVContextControl child in control.ContextChildren)
                    {
                        FillContextSet(child, null);
                    }
                    break;
                case ContextControlType.Container:
                    foreach (IEAVContextControl child in control.ContextChildren)
                    {
                        FillContextSet(child, dbInstance);
                    }
                    break;
                case ContextControlType.Instance:
                    ContainerInstance instance = AcquireContainerInstance(control, dbInstance, control.ContextKey, true);

                    if (String.IsNullOrWhiteSpace(control.ContextKey))
                        control.ContextKey = instance.RepeatInstance.ToString();

                    foreach (IEAVContextControl child in control.ContextChildren)
                    {
                        FillContextSet(child, instance);
                    }

                    if (!instance.Values.Any() && !instance.ChildContainerInstances.Any())
                    {
                        context.ContainerInstances.Remove(instance);
                        control.ContextKey = null;
                    }
                    break;
                case ContextControlType.Attribute:
                    Attribute attribute = GetDataItem<Attribute>(control);

                    if (control is IEAVValueControlContainer)
                    {
                        foreach (IEAVValueControl child in ((IEAVValueControlContainer)control).ValueControls)
                        {
                            UpdateValue(child, dbInstance, attribute);
                        }
                    }
                    break;
            }
        }

        public IEnumerable<Project> Projects { get { return (context.Projects); } }

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

        public void Save(IEAVContextControl contextControl)
        {
            //FillContextSet(contextControl, null, null, null, null, null, null, null);
            FillContextSet(contextControl, null);

            context.SaveChanges();
        }
    }

    public abstract class EAVContextControl : Control, IEAVContextControl, IDataItemContainer
    {
        protected bool myBind = false;

        public static IEAVContextControl FindAncestor(Control control, ContextControlType ancestorContextType)
        {
            return (control.GetParent<Control>(ancestorContextType));
        }

        public static T FindAncestorDataItem<T>(Control control, ContextControlType ancestorContextType) where T : class
        {
            IEAVContextControl container = control.GetParent<Control>(ancestorContextType);

            return (container != null ? container.DataItem as T : null);
        }

        public abstract IEAVContextControl ContextParent { get; }

        public IEnumerable<IEAVContextControl> ContextChildren
        {
            get
            {
                return (this.GetControls<Control, IEAVContextControl>());
            }
        }

        public abstract ContextControlType ContextControlType { get; }

        public abstract object DataItem { get; }

        public abstract ContextType ContextType { get; }

        public ContextType BindingType
        {
            get
            {
                return (myBind ? ContextType : ContextParent != null ? ContextParent.BindingType : ContextType.Unknown);
            }
        }

        public virtual int DataItemIndex { get { return (0); } }

        public virtual int DisplayIndex { get { return (0); } }

        private object dataSource;
        public object DataSource
        {
            get {return(dataSource);}
            set
            {
                dataSource = value;

                myBind = true;
            }
        }

        public string ContextKey
        {
            get { return (ViewState["ContextKey"] as string); }
            set
            {
                ViewState["ContextKey"] = value;

                myBind = true;
            }
        }

        protected abstract void RefreshDataSource();

        protected override void OnDataBinding(EventArgs e)
        {
            if (!myBind && (this.ContextType.HasFlag(ContextType.Data) || this.BindingType.HasFlag(ContextType.Metadata)))
            {
                RefreshDataSource();
            }

            base.OnDataBinding(e);
        }
    }

    public partial class EAVProjectContextControl : EAVContextControl
    {
        public override IEAVContextControl ContextParent { get { return (null); } }

        public override ContextControlType ContextControlType { get { return (ContextControlType.Project); } }

        public override ContextType ContextType { get { return(ContextType.Metadata | ContextType.Data); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<Project> dataSource = DataSource as IEnumerable<Project>;
                
                return(dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextKey) : null);
            }
        }

        protected override void RefreshDataSource()
        {
        }
    }

    public partial class EAVSubjectContextControl : EAVContextControl
    {
        public override IEAVContextControl ContextParent { get { return (FindAncestor(this, ContextControlType.Project)); } }

        public override ContextControlType ContextControlType { get { return (ContextControlType.Subject); } }

        public override ContextType ContextType { get { return (ContextType.Data); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<Subject> dataSource = DataSource as IEnumerable<Subject>;

                if (dataSource == null)
                {
                    RefreshDataSource();
                    dataSource = DataSource as IEnumerable<Subject>;
                }

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.MemberID == ContextKey) : null);
            }
        }

        protected override void RefreshDataSource()
        {
            Project project = FindAncestorDataItem<Project>(this, ContextControlType.Project);
            
            DataSource = project != null ? project.Subjects : null;
        }
    }

    public partial class EAVContainerContextControl : EAVContextControl
    {
        public override IEAVContextControl ContextParent { get { return (FindAncestor(this, ContextControlType.Instance) ?? FindAncestor(this, ContextControlType.Subject)); } }

        public override ContextControlType ContextControlType { get { return (ContextControlType.Container); } }

        public override ContextType ContextType { get { return (ContextType.Metadata); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<Container> dataSource = DataSource as IEnumerable<Container>;

                if (dataSource == null)
                {
                    RefreshDataSource();
                    dataSource = DataSource as IEnumerable<Container>;
                }

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextKey) : null);
            }
        }

        protected override void RefreshDataSource()
        {
            Project project = FindAncestorDataItem<Project>(this, ContextControlType.Project);
            Container parentContainer = FindAncestorDataItem<Container>(this, ContextControlType.Container);

            DataSource = project != null ? project.Containers.Where(it => it.ParentContainer == parentContainer) : null;
        }
    }

    public partial class EAVInstanceContextControl : EAVContextControl
    {
        public override IEAVContextControl ContextParent { get { return (FindAncestor(this, ContextControlType.Container)); } }

        public override ContextControlType ContextControlType { get { return (ContextControlType.Instance); } }

        public override ContextType ContextType { get { return (ContextType.Data); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<ContainerInstance> dataSource = DataSource as IEnumerable<ContainerInstance>;

                {
                    RefreshDataSource();
                    dataSource = DataSource as IEnumerable<ContainerInstance>;
                }

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.RepeatInstance.ToString() == ContextKey) : null);
            }
        }

        protected override void RefreshDataSource()
        {
            Subject subject = FindAncestorDataItem<Subject>(this, ContextControlType.Subject);
            Container container = FindAncestorDataItem<Container>(this, ContextControlType.Container);
            ContainerInstance parentInstance = FindAncestorDataItem<ContainerInstance>(this, ContextControlType.Instance);
            
            DataSource = subject != null ? subject.ContainerInstances.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance) : null;
        }
    }

    public partial class EAVAttributeContextControl : EAVContextControl, IEAVValueControlContainer
    {
        public override IEAVContextControl ContextParent { get { return (FindAncestor(this, ContextControlType.Instance)); } }

        public override ContextControlType ContextControlType { get { return (ContextControlType.Attribute); } }
            
        public override ContextType ContextType { get { return (ContextType.Metadata); } }

        public override object DataItem
        {
            get
            {
                IEnumerable<Attribute> dataSource = DataSource as IEnumerable<Attribute>;

                {
                    RefreshDataSource();
                    dataSource = DataSource as IEnumerable<Attribute>;
                }

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextKey) : null);
            }
        }

        protected override void RefreshDataSource()
        {
            Container container = FindAncestorDataItem<Container>(this, ContextControlType.Container);
        
            DataSource = container != null ? container.Attributes : null;
        }

        private void GetChildrenRecursive(Control ctl, IList<IEAVValueControl> children)
        {
            if (ctl is IEAVValueControl)
            {
                children.Add(ctl as IEAVValueControl);
            }
            else
            {
                foreach (Control child in ctl.Controls)
                    GetChildrenRecursive(child, children);
            }
        }

        public IEnumerable<IEAVValueControl> ValueControls
        {
            get
            {
                List<IEAVValueControl> valueControls = new List<IEAVValueControl>();

                foreach (Control child in Controls)
                    GetChildrenRecursive(child, valueControls);

                return (valueControls);
            }
        }
    }

    public partial class EAVTextBox : TextBox, IEAVValueControl
    {
        public IEAVContextControl ContextParent
        {
            get
            {
                return (EAVContextControl.FindAncestor(this, ContextControlType.Attribute));
            }
        }

        public string RawValue { get { return (Text); } set { Text = value; } }

        public override bool Enabled
        {
            get
            {
                return(base.Enabled && EAVContextControl.FindAncestorDataItem<Subject>(this, ContextControlType.Subject) != null);
            }
            set
            {
                base.Enabled = value;
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            ContainerInstance instance = EAVContextControl.FindAncestorDataItem<ContainerInstance>(this, ContextControlType.Instance);
            Attribute attribute = EAVContextControl.FindAncestorDataItem<Attribute>(this, ContextControlType.Attribute);

            RawValue = instance != null ? instance.Values.Where(it => it.Attribute == attribute).Select(it => it.RawValue).SingleOrDefault() : null;

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
                ctl = new AutoGen.EAVAutoInstanceContextControl();
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
            Subject subject = EAVContextControl.FindAncestorDataItem<Subject>(this, ContextControlType.Subject);
            DataSource = subject != null ? subject.ContainerInstances : null;
            IEnumerable<ContainerInstance> dataSource = DataSource as IEnumerable<ContainerInstance> ?? Enumerable.Empty<ContainerInstance>();

            Container container = EAVContextControl.FindAncestorDataItem<Container>(this, ContextControlType.Container);
            ContainerInstance parentInstance = EAVContextControl.FindAncestorDataItem<ContainerInstance>(this, ContextControlType.Instance);

            dataSource = dataSource.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance);

            if (!dataSource.Any() || container.IsRepeating)
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
                        Control subjectControl = FindAncestor(this, ContextControlType.Subject) as Control;
                        if (subjectControl != null && DataBinder.GetPropertyValue(subjectControl, "DataSource") == null)
                            subjectControl.DataBind();

                        if (container.IsRepeating)
                        {
                            EAVInstanceContextRepeater instanceRepeater = new EAVInstanceContextRepeater();
                            Controls.Add(instanceRepeater);
                            instanceRepeater.DataBind();
                        }
                        else
                        {
                            Subject subject = FindAncestorDataItem<Subject>(this, ContextControlType.Subject);

                            Controls.Add(new EAVAutoInstanceContextControl() { ContextKey = subject != null ? subject.ContainerInstances.Where(it => it.Container == container).Select(it => it.RepeatInstance.ToString()).FirstOrDefault() : null });
                        }
                    }
                }
            }

            protected override void OnDataBinding(EventArgs e)
            {
                if (myBind)
                    ChildControlsCreated = false;

                base.OnDataBinding(e);
            }
        }

        public partial class EAVAutoInstanceContextControl : EAVInstanceContextControl
        {
            protected override void CreateChildControls()
            {
                if (DataSource == null)
                    DataBind();

                Container container = FindAncestorDataItem<Container>(this, ContextControlType.Container);
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

                if (myBind)
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

                if (myBind)
                    ChildControlsCreated = false;
            }
        }
    }
}
