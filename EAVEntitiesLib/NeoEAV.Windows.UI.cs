using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NeoEAV.Data.DataClasses;
using NeoEAV.Objects;

using Attribute = NeoEAV.Data.DataClasses.Attribute;
using Container = NeoEAV.Data.DataClasses.Container;


namespace NeoEAV.Windows.UI
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

        private void FillContextSet(Control control, ContextType contextType, Container parentContainer, ContainerInstance parentInstance, Project dbProject, Subject dbSubject, Container dbContainer, ContainerInstance dbInstance, Attribute dbAttribute)
        {
            if (control is IEAVContextControl && ((IEAVContextControl)control).ContextType == contextType)
            {
                switch (contextType)
                {
                    case ContextType.Project:
                        IEAVContextControl projectControl = control as IEAVContextControl;
                        Project project = context.Projects.SingleOrDefault(it => it.Name == projectControl.ContextKey);

                        foreach (Control child in control.Controls)
                        {
                            FillContextSet(child, ContextType.Subject, parentContainer, parentInstance, project, null, null, null, null);
                        }
                        break;
                    case ContextType.Subject:
                        IEAVContextControl subjectControl = control as IEAVContextControl;
                        Subject subject = dbProject.Subjects.SingleOrDefault(it => it.MemberID == subjectControl.ContextKey);

                        foreach (Control child in control.Controls)
                        {
                            FillContextSet(child, ContextType.Container, parentContainer, parentInstance, dbProject, subject, null, null, null);
                        }
                        break;
                    case ContextType.Container:
                        IEAVContextControl containerControl = control as IEAVContextControl;
                        Container container = dbProject.Containers.SingleOrDefault(it => it.ParentContainer == parentContainer && it.Name == containerControl.ContextKey);

                        foreach (Control child in control.Controls)
                        {
                            FillContextSet(child, ContextType.Instance, parentContainer, parentInstance, dbProject, dbSubject, container, null, null);
                        }
                        break;
                    case ContextType.Instance:
                        IEAVContextControl instanceControl = control as IEAVContextControl;
                        ContainerInstance instance = FindContainerInstance(dbSubject, dbContainer, parentInstance, instanceControl.ContextKey, true);

                        if (String.IsNullOrWhiteSpace(instanceControl.ContextKey))
                            instanceControl.ContextKey = instance.RepeatInstance.ToString();

                        foreach (Control child in control.Controls)
                        {
                            if (!(child is IEAVContextControl) || ((IEAVContextControl)child).ContextType == ContextType.Attribute)
                                FillContextSet(child, ContextType.Attribute, parentContainer, parentInstance, dbProject, dbSubject, dbContainer, instance, null);
                        }

                        foreach (Control child in control.Controls)
                        {
                            if (!(child is IEAVContextControl) || ((IEAVContextControl)child).ContextType == ContextType.Container)
                                FillContextSet(child, ContextType.Container, dbContainer, instance, dbProject, dbSubject, null, null, null);
                        }

                        if (!instance.Values.Any() && !instance.ChildContainerInstances.Any())
                        {
                            context.ContainerInstances.Remove(instance);

                            if (instanceControl != null)
                                instanceControl.ContextKey = null;
                        }
                        break;
                    case ContextType.Attribute:
                        IEAVContextControl attributeControl = control as IEAVContextControl;
                        Attribute attribute = dbContainer.Attributes.SingleOrDefault(it => it.Name == attributeControl.ContextKey);

                        foreach (Control child in control.Controls)
                        {
                            FillContextSet(child, ContextType.Value, parentContainer, parentInstance, dbProject, dbSubject, dbContainer, dbInstance, attribute);
                        }
                        break;
                    case ContextType.Value:
                        IEAVContextControl valueControl = control as IEAVContextControl;
                        Value value = FindValue(dbAttribute, dbInstance, false);

                        if (value != null)
                        {
                            if (String.IsNullOrWhiteSpace(valueControl.ContextKey))
                                context.Values.Remove(value);
                            else if (value.RawValue != valueControl.ContextKey)
                                value.RawValue = valueControl.ContextKey;
                        }
                        else if (!String.IsNullOrWhiteSpace(valueControl.ContextKey))
                        {
                            value = FindValue(dbAttribute, dbInstance, true);
                            value.RawValue = valueControl.ContextKey;
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

        public IEnumerable<Project> Projects { get { return (context.Projects); } }

        private Project activeProject;
        public string ActiveProject
        {
            get { return (activeProject != null ? activeProject.Name : null); }
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
            FillContextSet(contextControl, ContextType.Project, null, null, null, null, null, null, null);

            context.SaveChanges();
        }
    }

    //public abstract class EAVContextControl : Control, IEAVContextControl, IDataItemContainer
    //{
    //    public static IEAVContextControl FindAncestor(Control control, ContextType ancestorContextType)
    //    {
    //        Control container = control != null ? control.Parent : null;
    //        while (container != null)
    //        {
    //            if (container is IEAVContextControl && ((IEAVContextControl)container).ContextType == ancestorContextType)
    //            {
    //                return (container as IEAVContextControl);
    //            }

    //            container = container.Parent;
    //        }

    //        return (null);
    //    }

    //    public static T FindAncestorDataItem<T>(Control control, ContextType ancestorContextType) where T : class
    //    {
    //        IEAVContextControl container = FindAncestor(control, ancestorContextType);

    //        if (container != null)
    //        {
    //            return (DataBinder.GetDataItem(container) as T);
    //        }

    //        return (null);
    //    }

    //    protected bool ContextKeyChanged { get; set; }

    //    public abstract IEAVContextControl ParentContextControl { get; }

    //    public abstract ContextType ContextType { get; }

    //    public abstract object DataItem { get; }

    //    public abstract BindingType BindingType { get; }

    //    public virtual string ContextKey
    //    {
    //        get { return (ViewState["ContextKey"] as string); }
    //        set
    //        {
    //            ViewState["ContextKey"] = value;
    //            ContextKeyChanged = true;
    //        }
    //    }

    //    public virtual int DataItemIndex { get { return (0); } }

    //    public virtual int DisplayIndex { get { return (0); } }

    //    public virtual object DataSource { get; set; }

    //    public virtual bool DynamicContextKey { get; set; }

    //    protected override void OnInit(EventArgs e)
    //    {
    //        // This resets our status after object creation
    //        ContextKeyChanged = false;

    //        base.OnInit(e);
    //    }
    //}
}
