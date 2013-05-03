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

        private ContainerInstance FindContainerInstance(Container container, Subject subject, ContainerInstance parentInstance, string repeatInstance, bool createIfMissing)
        {
            ContainerInstance instance = subject != null ? subject.ContainerInstances.SingleOrDefault(it => it.Container == container && it.ParentContainerInstance == parentInstance && it.RepeatInstance.ToString() == repeatInstance) : null;
            bool parentInstanceCorrect = container == null || (container.ParentContainer != null ^ parentInstance == null);

            if (instance == null && container != null && subject != null && parentInstanceCorrect && createIfMissing)
            {
                if (String.IsNullOrWhiteSpace(repeatInstance))
                {
                    int newRepeatInstance = subject.ContainerInstances.Where(it => it.Container == container && it.ParentContainerInstance == parentInstance).Max(it => it.RepeatInstance) + 1;

                    instance = new ContainerInstance() { Subject = subject, Container = container, ParentContainerInstance = parentInstance, RepeatInstance = newRepeatInstance };
                    //container.ContainerInstances.Add(instance);
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
                value = new Value() { ContainerInstance = instance, Attribute = attribute };
                //attribute.Values.Add(value);
            }

            return (value);
        }

        private void SaveAttribute(Container container, ContainerInstance instance, IGrouping<string, string[]> attributeGroup)
        {
            Debug.WriteLine(String.Format("Attribute = '{0}'.", attributeGroup.Key));
            Debug.Indent();

            Attribute attribute = container.Attributes.SingleOrDefault(it => it.Name == attributeGroup.Key);
            Value value = FindValue(attribute, instance, false);
            string rawValue = attributeGroup.First().Last();

            Debug.WriteLine(String.Format("Value = '{0}'.", rawValue));

            if (value != null)
            {
                if (String.IsNullOrWhiteSpace(rawValue))
                    context.Values.Remove(value);
                else
                    value.RawValue = rawValue;
            }
            else if (!String.IsNullOrWhiteSpace(rawValue))
            {
                value = FindValue(attribute, instance, true);
                value.RawValue = rawValue;
            }

            Debug.Unindent();
        }

        private void SaveInstance(Project project, Subject subject, Container container, ContainerInstance parentInstance, IGrouping<string, string[]> instanceGroup, int keyIndex)
        {
            Debug.WriteLine(String.Format("Instance = '{0}'.", instanceGroup.Key));
            Debug.Indent();

            ContainerInstance instance = FindContainerInstance(container, subject, parentInstance, instanceGroup.Key, true);

            foreach (var attributeGroup in instanceGroup.GroupBy(it => it[keyIndex + 1]).Where(it => it.Count() - keyIndex <= 3))
            {
                SaveAttribute(container, instance, attributeGroup);
            }

            foreach (var containerGroup in instanceGroup.GroupBy(it => it[keyIndex + 1]).Where(it => it.Count() - keyIndex > 3))
            {
                SaveContainer(project, subject, container, instance, containerGroup, keyIndex + 1);
            }

            if (!instance.Values.Any() && !instance.ChildContainerInstances.Any())
            {
                context.ContainerInstances.Remove(instance);
            }

            Debug.Unindent();
        }

        private void SaveContainer(Project project, Subject subject, Container parentContainer, ContainerInstance parentInstance, IGrouping<string, string[]> containerGroup, int keyIndex)
        {
            Debug.WriteLine(String.Format("Container = '{0}'.", containerGroup.Key));
            Debug.Indent();

            Container container = project.Containers.SingleOrDefault(it => it.Name == containerGroup.Key && it.ParentContainer == parentContainer);
            if (container != null)
            {
                foreach (var instanceGroup in containerGroup.GroupBy(it => it[keyIndex + 1]))
                {
                    SaveInstance(project, subject, container, parentInstance, instanceGroup, keyIndex + 1);
                }
            }
            else
            {
                // ERROR: Container not found
            }

            Debug.Unindent();
        }

        private void SaveSubject(Project project, IGrouping<string, string[]> subjectGroup)
        {
            Debug.WriteLine(String.Format("Subject = '{0}'.", subjectGroup.Key));
            Debug.Indent();

            Subject subject = project.Subjects.SingleOrDefault(it => it.MemberID == subjectGroup.Key);
            if (subject != null)
            {
                foreach (var containerGroup in subjectGroup.GroupBy(it => it[2]))
                {
                    SaveContainer(project, subject, null, null, containerGroup, 2);
                }
            }
            else
            {
                // ERROR: Subject not found
            }

            Debug.Unindent();
        }

        private void SaveProject(IGrouping<string, string[]> projectGroup)
        {
            Debug.WriteLine(String.Format("Project = '{0}'.", projectGroup.Key));
            Debug.Indent();

            Project project = context.Projects.Single(it => it.Name == projectGroup.Key);
            if (project != null)
            {
                foreach (var subjectGroup in projectGroup.GroupBy(it => it[1]))
                {
                    SaveSubject(project, subjectGroup);
                }
            }
            else
            {
                // ERROR: Project not found
            }

            Debug.Unindent();
        }

        public void Save(Control contextControl)
        {
            BuildContextSet(contextControl);

            Debug.WriteLine("Saving...");
            Debug.Indent();

            // Concatenate each data control's context with its value
            var controlData = FindEAVValueControls(contextControl).Select(it => String.Concat(BuildContextValue(it as Control), "|", it.RawValue).Split('|'));

            Debug.WriteLine(String.Format("{0} controls.", controlData.Count()));

            // For each control group with a common project...
            foreach (var projectGroup in controlData.GroupBy(it => it[0]))
            {
                SaveProject(projectGroup);
            }

            //context.SaveChanges();

            Debug.Unindent();
        }

        //public void Bind(Control contextControl, bool bindToSource = false)
        //{
        //    // Need to finesse this so that we don't keep looking up stuff
        //    // we don't need to. Order by context maybe.
        //    foreach (IEAVDataControl control in FindEAVValueControls(contextControl))
        //    {
        //        string[] contextValues = BuildContextValue(control as Control).Split('|');
        //        if (contextValues.Length >= 5)
        //        {
        //            string strProject = contextValues[0];
        //            string strSubject = contextValues[1];
        //            string strAttribute = contextValues[contextValues.Length - 1];

        //            Project project = context.Projects.Single(it => it.Name == strProject);
        //            Subject subject = project.Subjects.Single(it => it.MemberID == strSubject);

        //            Container container = project.Containers.Single(it => it.Name == contextValues[2]);
        //            ContainerInstance instance = FindContainerInstance(container, subject, null, contextValues[3], bindToSource);

        //            for (int index = 4; index < (contextValues.Length - 2) && container != null; index += 2)
        //            {
        //                container = container.ChildContainers.Single(it => it.Name == contextValues[index]);

        //                if (instance != null)
        //                    instance = FindContainerInstance(container, subject, instance, contextValues[index + 1], bindToSource);
        //            }

        //            Attribute attribute = container.Attributes.Single(it => it.Name == strAttribute);
        //            Value value = FindValue(attribute, instance, false);

        //            if (bindToSource)
        //            {
        //                if (value != null)
        //                {
        //                    if (String.IsNullOrWhiteSpace(control.RawValue))
        //                        context.Values.Remove(value);
        //                    else
        //                        value.RawValue = control.RawValue;
        //                }
        //                else if (!String.IsNullOrWhiteSpace(control.RawValue))
        //                {
        //                    value = FindValue(attribute, instance, true);
        //                    value.RawValue = control.RawValue;
        //                }
        //            }
        //            else
        //            {
        //                control.RawValue = (value == null ? null : value.RawValue);
        //            }
        //        }
        //    }

        //    // TODO: Need to prune empty bits out before saving

        //    if (bindToSource)
        //        context.SaveChanges();
        //}

        public void FillContextSet(ContextControlSet contextSet, Control container, ContextType contextType)
        {
            if (container is IEAVDataControl && contextType == ContextType.Value)
            {
                Debug.WriteLine(String.Format("Found Value '{0}'", ((IEAVDataControl) container).RawValue));
                contextSet.Value = container as IEAVDataControl;

                // TODO: Save data here
            }
            else if (container is IEAVContextControl && ((IEAVContextControl) container).ContextType == contextType)
            {
                switch (contextType)
                {
                    case ContextType.Project:
                        Debug.WriteLine(String.Format("Found Project '{0}'", ((IEAVContextControl) container).ContextSelector));
                        Debug.Indent();
                        contextSet.Project = container as IEAVContextControl;
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Subject);
                        }
                        contextSet.Project = null;
                        Debug.Unindent();
                        break;
                    case ContextType.Subject:
                        Debug.WriteLine(String.Format("Found Subject '{0}'", ((IEAVContextControl) container).ContextSelector));
                        Debug.Indent();
                        contextSet.Subject = container as IEAVContextControl;
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Container);
                        }
                        contextSet.Subject = null;
                        Debug.Unindent();
                        break;
                    case ContextType.Container:
                        Debug.WriteLine(String.Format("Found Container '{0}'", ((IEAVContextControl) container).ContextSelector));
                        Debug.Indent();
                        contextSet.Container = container as IEAVContextControl;
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Instance);
                        }
                        contextSet.Container = null;
                        Debug.Unindent();
                        break;
                    case ContextType.Instance:
                        Debug.WriteLine(String.Format("Found Instance '{0}'", ((IEAVContextControl) container).ContextSelector));
                        Debug.Indent();
                        contextSet.Instance = container as IEAVContextControl;
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Attribute);
                        }
                        Debug.WriteLine(null);
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Container);
                        }
                        contextSet.Instance = null;
                        Debug.Unindent();
                        break;
                    case ContextType.Attribute:
                        Debug.WriteLine(String.Format("Found Attribute '{0}'", ((IEAVContextControl) container).ContextSelector));
                        Debug.Indent();
                        contextSet.Attribute = container as IEAVContextControl;
                        foreach (Control child in container.Controls)
                        {
                            FillContextSet(contextSet, child, ContextType.Value);
                        }
                        contextSet.Attribute = null;
                        Debug.Unindent();
                        break;
                }
            }
            else if (!(container is IEAVContextControl) && !(container is IEAVDataControl))
            {
                foreach (Control child in container.Controls)
                {
                    FillContextSet(contextSet, child, contextType);
                }
            }
        }

        public void BuildContextSet(Control container)
        {
            ContextControlSet set = new ContextControlSet();

            FillContextSet(set, container, ContextType.Project);
        }
    }

    public partial class ContextControlSet
    {
        public IEAVContextControl Project { get; set; }
        public IEAVContextControl Subject { get; set; }
        public IEAVContextControl Container { get; set; }
        public IEAVContextControl Instance { get; set; }
        public IEAVContextControl Attribute { get; set; }
        public IEAVDataControl Value{ get; set; }
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
