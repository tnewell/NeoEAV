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

                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Attribute, parentContainer, parentInstance);
                        }
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Container, contextSet.Container, contextSet.Instance);
                        }

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

    public interface IEAVContextControl
    {
        ContextType ContextType { get; }
        string ContextKey { get; set; }
    }

    public abstract class EAVContextControl : Control, IEAVContextControl, IDataItemContainer
    {
        public static T FindAncestorDataItem<T>(Control control, ContextType ancestorContextType) where T : class
        {
            Control container = control != null ? control.Parent : null;
            while (container != null)
            {
                if (container is IEAVContextControl && ((IEAVContextControl)container).ContextType == ancestorContextType)
                {
                    return (DataBinder.GetDataItem(container) as T);
                }

                container = container.Parent;
            }

            return (null);
        }

        public virtual string ContextKey { get { return (ViewState["ContextKey"] as string); } set { ViewState["ContextKey"] = value; } }

        public abstract ContextType ContextType { get; }

        public abstract object DataItem { get; }

        public virtual int DataItemIndex { get { return (0); } }

        public virtual int DisplayIndex { get { return (0); } }

        public virtual object DataSource { get; set; }
    }

    public partial class EAVProjectContextControl : EAVContextControl
    {
        public override ContextType ContextType { get { return (ContextType.Project); } }

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
            Debug.WriteLine(String.Format("Project::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Indent();

            base.OnDataBinding(e);

            Debug.WriteLine(String.Format("Project::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Unindent();
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

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.MemberID == ContextKey) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Debug.WriteLine(String.Format("Subject::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Indent();

            Project project = FindAncestorDataItem<Project>(this, UI.ContextType.Project);
            DataSource = project != null ? project.Subjects : null;

            base.OnDataBinding(e);

            Debug.WriteLine(String.Format("Subject::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Unindent();
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

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextKey) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Debug.WriteLine(String.Format("Container::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Indent();

            Project project = FindAncestorDataItem<Project>(this, UI.ContextType.Project);
            DataSource = project != null ? project.Containers : null;

            FilterDataSource();

            base.OnDataBinding(e);

            Debug.WriteLine(String.Format("Container::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Unindent();
        }

        private void FilterDataSource()
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

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.RepeatInstance.ToString() == ContextKey) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Debug.WriteLine(String.Format("Instance::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Indent();

            Subject subject = FindAncestorDataItem<Subject>(this, UI.ContextType.Subject);
            DataSource = subject != null ? subject.ContainerInstances : null;

            FilterDataSource();

            base.OnDataBinding(e);

            Debug.WriteLine(String.Format("Instance::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Unindent();
        }
        
        private void FilterDataSource()
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

                return (dataSource != null ? dataSource.SingleOrDefault(it => it.Name == ContextKey) : null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            Debug.WriteLine(String.Format("Attribute::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Indent();

            Container container = FindAncestorDataItem<Container>(this, UI.ContextType.Container);
            DataSource = container != null ? container.Attributes : null;

            base.OnDataBinding(e);

            Debug.WriteLine(String.Format("Attribute::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
            Debug.Unindent();
        }
    }

    public partial class EAVTextBox : TextBox, IEAVContextControl
    {
        public string ContextKey { get { return (Text); } set { Text = value; } }

        public ContextType ContextType { get { return (ContextType.Value); } }

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

    public partial class EAVInstanceContextRepeaterItem : RepeaterItem, IEAVContextControl
    {
        public string ContextKey { get { return (ViewState["ContextKey"] as string); } set { ViewState["ContextKey"] = value; } }

        public ContextType ContextType { get { return (ContextType.Instance); } }

        public EAVInstanceContextRepeaterItem(int itemIndex, ListItemType itemType) : base(itemIndex, itemType) { }

        protected override void OnDataBinding(EventArgs e)
        {
            if (DataItem != null)
                ContextKey = ((ContainerInstance)DataItem).RepeatInstance.ToString();
            else
                ContextKey = null;

            base.OnDataBinding(e);
        }
    }

    public partial class EAVInstanceContextRepeater : Repeater
    {
        protected override RepeaterItem CreateItem(int itemIndex, ListItemType itemType)
        {
            if (itemType == ListItemType.AlternatingItem || itemType == ListItemType.EditItem || itemType == ListItemType.Item || itemType == ListItemType.SelectedItem)
                return (new EAVInstanceContextRepeaterItem(itemIndex, itemType));
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

        private void FilterDataSource()
        {
            IEnumerable<ContainerInstance> dataSource = DataSource as IEnumerable<ContainerInstance>;
            if (dataSource != null)
            {
                Container container = EAVContextControl.FindAncestorDataItem<Container>(this, ContextType.Container);
                ContainerInstance parentInstance = EAVContextControl.FindAncestorDataItem<ContainerInstance>(this, ContextType.Instance);

                dataSource = dataSource.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance);

                if (!dataSource.Any() || container.IsRepeating)
                    dataSource = dataSource.Concat(new ContainerInstance[] { null } );

                DataSource = dataSource;
            }
        }
    }

    namespace AutoGen
    {
        public partial class EAVAutoContainerContextControl : EAVContainerContextControl
        {
            public override string ContextKey
            {
                get
                {
                    return base.ContextKey;
                }
                set
                {
                    base.ContextKey = value;
                    ChildControlsCreated = false;
                }
            }

            protected override void CreateChildControls()
            {
                Debug.WriteLine(String.Format("AutoContainer::CreateChildControls {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Indent();

                if (!String.IsNullOrWhiteSpace(ContextKey))
                {
                    if (DataSource == null)
                        DataBind();

                    Container container = DataItem as Container;
                    if (container != null)
                    {
                        if (container.IsRepeating)
                        {
                            Controls.Add(new EAVInstanceContextRepeater());
                        }
                        else
                        {
                            Controls.Add(new EAVAutoInstanceContextControl() { ContextKey = container.ContainerInstances.Select(it => it.RepeatInstance.ToString()).FirstOrDefault() });
                        }
                    }
                }

                Debug.WriteLine(String.Format("AutoContainer::CreateChildControls {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Unindent();
            }

            protected override void OnDataBinding(EventArgs e)
            {
                Debug.WriteLine(String.Format("AutoContainer::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Indent();

                base.OnDataBinding(e);

                Debug.WriteLine(String.Format("AutoContainer::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Unindent();
            }
        }

        public partial class EAVAutoInstanceContextControl : EAVInstanceContextControl
        {
            public override string ContextKey
            {
                get
                {
                    return base.ContextKey;
                }
                set
                {
                    base.ContextKey = value;
                    ChildControlsCreated = false;
                }
            }

            protected override void CreateChildControls()
            {
                Debug.WriteLine(String.Format("AutoInstance::CreateChildControls {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Indent();

                if (!String.IsNullOrWhiteSpace(ContextKey))
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
                    }
                }

                Debug.WriteLine(String.Format("AutoInstance::CreateChildControls {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Unindent();
            }

            protected override void OnDataBinding(EventArgs e)
            {
                Debug.WriteLine(String.Format("AutoInstance::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Indent();

                base.OnDataBinding(e);

                Debug.WriteLine(String.Format("AutoInstance::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Unindent();
            }
        }

        public partial class EAVAutoAttributeContextControl : EAVAttributeContextControl
        {
            public override string ContextKey
            {
                get
                {
                    return base.ContextKey;
                }
                set
                {
                    base.ContextKey = value;
                    ChildControlsCreated = false;
                }
            }
            
            protected override void CreateChildControls()
            {
                Debug.WriteLine(String.Format("AutoAttribute::CreateChildControls {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Indent();

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

                Debug.WriteLine(String.Format("AutoAttribute::CreateChildControls {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Unindent();
            }

            protected override void OnDataBinding(EventArgs e)
            {
                Debug.WriteLine(String.Format("AutoAttribute::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Indent();

                base.OnDataBinding(e);

                Debug.WriteLine(String.Format("AutoAttribute::OnDataBinding {{ DataSource = {0}, ContextKey = '{1}', DataItem = {2} }}", DataSource != null, ContextKey, DataItem != null));
                Debug.Unindent();
            }
        }
    }
}
