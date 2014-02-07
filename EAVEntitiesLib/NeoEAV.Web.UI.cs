using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using NeoEAV.Data.DataClasses;
using NeoEAV.Objects;

using Attribute = NeoEAV.Data.DataClasses.Attribute;
using Container = NeoEAV.Data.DataClasses.Container;


namespace NeoEAV.Web.UI
{
    public partial class EAVContextController
    {
        private EAVEntityContext context = new EAVEntityContext();

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

                    instance = new ContainerInstance() { Container = container, Subject = subject, ParentContainerInstance = parentInstance, RepeatInstance = newRepeatInstance };

                    if (parentInstance == null)
                        subject.ContainerInstances.Add(instance);
                    else
                        parentInstance.ChildContainerInstances.Add(instance);
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
                value = new Value() { Attribute = attribute, ContainerInstance = instance };
                instance.Values.Add(value);
            }

            return (value);
        }

        private void UpdateValue(Control control, ContainerInstance dbInstance, Attribute dbAttribute)
        {
            IEAVValueControl valueControl = control as IEAVValueControl;

            if (valueControl != null)
            {
                Value value = FindValue(dbAttribute, dbInstance, false);

                if (value != null)
                {
                    if (String.IsNullOrWhiteSpace(valueControl.RawValue))
                        context.Values.Remove(value);
                    else if (value.RawValue != valueControl.RawValue)
                        value.RawValue = valueControl.RawValue;
                }
                else if (!String.IsNullOrWhiteSpace(valueControl.RawValue))
                {
                    value = FindValue(dbAttribute, dbInstance, true);
                    value.RawValue = valueControl.RawValue;
                }
            }
            else
            {
                foreach (Control child in control.Controls)
                    UpdateValue(child, dbInstance, dbAttribute);
            }
        }

        private void UpdateValue2(IEAVValueControl control, ContainerInstance dbInstance, Attribute dbAttribute)
        {
            if (control != null)
            {
                Value value = FindValue(dbAttribute, dbInstance, false);

                if (value != null)
                {
                    if (String.IsNullOrWhiteSpace(control.RawValue))
                        context.Values.Remove(value);
                    else if (value.RawValue != control.RawValue)
                        value.RawValue = control.RawValue;
                }
                else if (!String.IsNullOrWhiteSpace(control.RawValue))
                {
                    value = FindValue(dbAttribute, dbInstance, true);
                    value.RawValue = control.RawValue;
                }
            }
        }

        private void FillContextSet(Control control, ContextControlType contextType, Container parentContainer, ContainerInstance parentInstance, Project dbProject, Subject dbSubject, Container dbContainer, ContainerInstance dbInstance, Attribute dbAttribute)
        {
            if (control is IEAVContextControl && ((IEAVContextControl) control).ContextControlType == contextType)
            {
                switch (contextType)
                {
                    case ContextControlType.Project:
                        IEAVContextControl projectControl = control as IEAVContextControl;
                        Project project = context.Projects.SingleOrDefault(it => it.Name == projectControl.ContextKey);

                        foreach (Control child in control.Controls)
                        {
                            FillContextSet(child, ContextControlType.Subject, parentContainer, parentInstance, project, null, null, null, null);
                        }
                        break;
                    case ContextControlType.Subject:
                        IEAVContextControl subjectControl = control as IEAVContextControl;
                        Subject subject = dbProject.Subjects.SingleOrDefault(it => it.MemberID == subjectControl.ContextKey);

                        foreach (Control child in control.Controls)
                        {
                            FillContextSet(child, ContextControlType.Container, parentContainer, parentInstance, dbProject, subject, null, null, null);
                        }
                        break;
                    case ContextControlType.Container:
                        IEAVContextControl containerControl = control as IEAVContextControl;
                        Container container = dbProject.Containers.SingleOrDefault(it => it.ParentContainer == parentContainer && it.Name == containerControl.ContextKey);

                        foreach (Control child in control.Controls)
                        {
                            FillContextSet(child, ContextControlType.Instance, parentContainer, parentInstance, dbProject, dbSubject, container, null, null);
                        }
                        break;
                    case ContextControlType.Instance:
                        IEAVContextControl instanceControl = control as IEAVContextControl;
                        ContainerInstance instance = FindContainerInstance(dbSubject, dbContainer, parentInstance, instanceControl.ContextKey, true);

                        if (String.IsNullOrWhiteSpace(instanceControl.ContextKey))
                            instanceControl.ContextKey = instance.RepeatInstance.ToString();

                        foreach (Control child in control.Controls)
                        {
                            if (!(child is IEAVContextControl) || ((IEAVContextControl) child).ContextControlType == ContextControlType.Attribute)
                                FillContextSet(child, ContextControlType.Attribute, parentContainer, parentInstance, dbProject, dbSubject, dbContainer, instance, null);
                        }

                        foreach (Control child in control.Controls)
                        {
                            if (!(child is IEAVContextControl) || ((IEAVContextControl)child).ContextControlType == ContextControlType.Container)
                                FillContextSet(child, ContextControlType.Container, dbContainer, instance, dbProject, dbSubject, null, null, null);
                        }

                        if (!instance.Values.Any() && !instance.ChildContainerInstances.Any())
                        {
                            context.ContainerInstances.Remove(instance);

                            instanceControl.ContextKey = null;
                        }
                        break;
                    case ContextControlType.Attribute:
                        IEAVContextControl attributeControl = control as IEAVContextControl;
                        Attribute attribute = dbContainer.Attributes.SingleOrDefault(it => it.Name == attributeControl.ContextKey);

                        foreach (Control child in control.Controls)
                        {
                            UpdateValue(child, dbInstance, attribute);
                        }
                        break;
                }
            }
            else
            {
                foreach (Control child in control.Controls)
                {
                    FillContextSet(child, contextType, parentContainer, parentInstance, dbProject, dbSubject, dbContainer, dbInstance, dbAttribute);
                }
            }
        }

        private void FillContextSet2(IEAVContextControl control, Container dbParentContainer, ContainerInstance dbParentInstance, Project dbProject, Subject dbSubject, Container dbContainer, ContainerInstance dbInstance, Attribute dbAttribute)
        {
            // TODO: Do we need db items? Can we use DataItem property somehow?
            switch (control.ContextControlType)
            {
                case ContextControlType.Project:
                    Project project = context.Projects.SingleOrDefault(it => it.Name == control.ContextKey);

                    foreach (IEAVContextControl child in control.ContextChildren)
                    {
                        FillContextSet2(child, dbParentContainer, dbParentInstance, project, null, null, null, null);
                    }
                    break;
                case ContextControlType.Subject:
                    Subject subject = dbProject.Subjects.SingleOrDefault(it => it.MemberID == control.ContextKey);

                    foreach (IEAVContextControl child in control.ContextChildren)
                    {
                        FillContextSet2(child, dbParentContainer, dbParentInstance, dbProject, subject, null, null, null);
                    }
                    break;
                case ContextControlType.Container:
                    Container container = dbProject.Containers.SingleOrDefault(it => it.ParentContainer == dbParentContainer && it.Name == control.ContextKey);

                    foreach (IEAVContextControl child in control.ContextChildren)
                    {
                        FillContextSet2(child, dbParentContainer, dbParentInstance, dbProject, dbSubject, container, null, null);
                    }
                    break;
                case ContextControlType.Instance:
                    ContainerInstance instance = FindContainerInstance(dbSubject, dbContainer, dbParentInstance, control.ContextKey, true);

                    if (String.IsNullOrWhiteSpace(control.ContextKey))
                        control.ContextKey = instance.RepeatInstance.ToString();

                    foreach (IEAVContextControl child in control.ContextChildren)
                    {
                        bool isAttribute = child.ContextControlType == ContextControlType.Attribute;

                        FillContextSet2(child, isAttribute ? dbParentContainer : dbContainer, isAttribute ? dbParentInstance : instance, dbProject, dbSubject, isAttribute ? dbContainer : null, isAttribute ? instance : null, null);
                    }

                    if (!instance.Values.Any() && !instance.ChildContainerInstances.Any())
                    {
                        context.ContainerInstances.Remove(instance);
                        control.ContextKey = null;
                    }
                    break;
                case ContextControlType.Attribute:
                    Attribute attribute = dbContainer.Attributes.SingleOrDefault(it => it.Name == control.ContextKey);

                    foreach (IEAVValueControl child in ((IEAVValueControlContainer)control).ValueControls)
                    {
                        UpdateValue2(child, dbInstance, attribute);
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

        public void Save(Control contextControl)
        {
            FillContextSet(contextControl, ContextControlType.Project, null, null, null, null, null, null, null);

            context.SaveChanges();
        }

        public void Save2(IEAVContextControl contextControl)
        {
            FillContextSet2(contextControl, null, null, null, null, null, null, null);

            context.SaveChanges();
        }
    }

    public abstract class EAVContextControl : Control, IEAVContextControl, IDataItemContainer
    {
        protected bool inBind = false;
        protected bool myBind = false;

        public static IEAVContextControl FindAncestor(Control control, ContextControlType ancestorContextType)
        {
            Control container = control != null ? control.Parent : null;
            while (container != null)
            {
                if (container is IEAVContextControl && ((IEAVContextControl)container).ContextControlType == ancestorContextType)
                {
                    return (container as IEAVContextControl);
                }

                container = container.Parent;
            }

            return (null);
        }

        public static T FindAncestorDataItem<T>(Control control, ContextControlType ancestorContextType) where T : class
        {
            IEAVContextControl container = FindAncestor(control, ancestorContextType);

            if (container != null)
            {
                return (DataBinder.GetDataItem(container) as T);
            }

            return (null);
        }

        public abstract IEAVContextControl ContextParent { get; }

        private void GetChildrenRecursive(Control ctl, IList<IEAVContextControl> children)
        {
            if (ctl is IEAVContextControl)
            {
                children.Add(ctl as IEAVContextControl);
            }
            else
            {
                foreach (Control child in ctl.Controls)
                    GetChildrenRecursive(child, children);
            }
        }

        public IEnumerable<IEAVContextControl> ContextChildren
        {
            get
            {
                List<IEAVContextControl> children = new List<IEAVContextControl>();

                foreach (Control ctl in Controls)
                {
                    GetChildrenRecursive(ctl, children);
                }

                return (children);
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

                myBind = !inBind;
            }
        }

        public string ContextKey
        {
            get { return (ViewState["ContextKey"] as string); }
            set
            {
                ViewState["ContextKey"] = value;

                myBind = !inBind;
            }
        }

        public bool DynamicContextKey { get; set; }

        protected abstract void RefreshDataSource();

        protected override void OnDataBinding(EventArgs e)
        {
            if (!myBind && (this.ContextType.HasFlag(ContextType.Data) || this.BindingType.HasFlag(ContextType.Metadata)))
            {
                inBind = true;

                RefreshDataSource();

                if (DynamicContextKey)
                    ContextKey = null;

                inBind = false;
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
                    RefreshDataSource();

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
                    RefreshDataSource();

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

                if (dataSource == null)
                    RefreshDataSource();

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

                if (dataSource == null)
                    RefreshDataSource();

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
