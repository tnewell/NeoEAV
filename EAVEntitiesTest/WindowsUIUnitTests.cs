﻿using System;
using System.Linq;
using System.Web;
using System.Web.UI;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;

using NeoEAV.Data.DataClasses;
using NeoEAV.Objects;
using NeoEAV.Web.UI;
using NeoEAV.Web.UI.AutoGen;

using Attribute = NeoEAV.Data.DataClasses.Attribute;


namespace EAVEntitiesTest
{
    [TestClass]
    public class WindowsUIUnitTests
    {
        private void BuildContainerControlsRecursive(Container dbContainer, IEAVContextControl ctl)
        {
            EAVContainerContextControl ctlContainer = new EAVContainerContextControl() { ContextKey = dbContainer.Name };
            ((Control)ctl).Controls.Add(ctlContainer);

            EAVInstanceContextControl ctlInstance = new EAVInstanceContextControl();
            ((Control)ctlContainer).Controls.Add(ctlInstance);

            // Recurse here
            foreach (Container dbChildContainer in dbContainer.ChildContainers)
            {
                BuildContainerControlsRecursive(dbChildContainer, ctlInstance);
            }

            foreach (Attribute dbAttribute in dbContainer.Attributes)
            {
                EAVAttributeContextControl ctlAttribute = new EAVAttributeContextControl() { ContextKey = dbAttribute.Name };
                ((Control)ctlInstance).Controls.Add(ctlAttribute);

                EAVTextBox ctlValue = new EAVTextBox();
                ((Control)ctlAttribute).Controls.Add(ctlValue);

            }
        }

        private EAVProjectContextControl BuildContextControls(string project, string subject = null)
        {
            EAVProjectContextControl ctlProject = new EAVProjectContextControl() { ContextKey = project };

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Project dbProject = ctx.Projects.Single(it => it.Name == project);

                EAVSubjectContextControl ctlSubject = new EAVSubjectContextControl() { ContextKey = String.IsNullOrWhiteSpace(subject) ? null : subject };
                ((Control)ctlProject).Controls.Add(ctlSubject);

                foreach (Container dbContainer in dbProject.Containers.Where(it => it.ParentContainer == null))
                {
                    BuildContainerControlsRecursive(dbContainer, ctlSubject);
                }
            }

            return (ctlProject);
        }

        private void RemoveInstanceDataRecursive(EAVEntityContext ctx, ContainerInstance instance)
        {
            while (instance.ChildContainerInstances.Any())
            {
                RemoveInstanceDataRecursive(ctx, instance.ChildContainerInstances.First());
            }

            while (instance.Values.Any())
            {
                ctx.Values.Remove(instance.Values.First());
            }

            ctx.ContainerInstances.Remove(instance);
        }

        private void RemoveSubjectData(string project, string subject)
        {
            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Subject dbSubject = ctx.Subjects.SingleOrDefault(it => it.Project.Name == project && it.MemberID == subject);

                if (dbSubject != null)
                {
                    while (dbSubject.ContainerInstances.Any())
                    {
                        RemoveInstanceDataRecursive(ctx, dbSubject.ContainerInstances.First());
                    }

                    ctx.SaveChanges();
                }
            }
        }

        private void RemoveContainerMetadataRecursive(EAVEntityContext ctx, Container dbContainer)
        {
            while (dbContainer.ChildContainers.Any())
            {
                RemoveContainerMetadataRecursive(ctx, dbContainer.ChildContainers.First());
            }

            while (dbContainer.Attributes.Any())
            {
                ctx.Attributes.Remove(dbContainer.Attributes.First());
            }

            ctx.Containers.Remove(dbContainer);
        }

        private void RemoveProjectMetadata(string project)
        {
            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Project dbProject = ctx.Projects.SingleOrDefault(it => it.Name == project);

                if (dbProject != null)
                {
                    while (dbProject.Subjects.Any())
                    {
                        RemoveSubjectData(project, dbProject.Subjects.First().MemberID);

                        ctx.Subjects.Remove(dbProject.Subjects.First());
                    }

                    while (dbProject.Containers.Any())
                    {
                        RemoveContainerMetadataRecursive(ctx, dbProject.Containers.First());
                    }

                    ctx.Projects.Remove(dbProject);

                    ctx.SaveChanges();
                }
            }
        }

        private Project AddProjectMetadata(string project, string parentContainer, string container, bool isRepeating, string subject = null, params string[] attributes)
        {
            Project dbProject = null;

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                dbProject = ctx.Projects.SingleOrDefault(it => it.Name == project);

                if (dbProject == null)
                {
                    dbProject = ctx.Projects.Create();

                    dbProject.Name = project;
                    dbProject.DataName = project;
                    dbProject.Description = String.Format("Project: {0}", project);

                    ctx.Projects.Add(dbProject);
                }

                if (!String.IsNullOrWhiteSpace(container))
                {
                    bool rootContainer = String.IsNullOrWhiteSpace(parentContainer);
                    Container dbParentContainer = rootContainer ? null : dbProject.Containers.SingleOrDefault(it => it.Name == parentContainer);
                    Container dbContainer = rootContainer ? dbProject.Containers.SingleOrDefault(it => it.Name == container && it.ParentContainer == null) : dbParentContainer.ChildContainers.SingleOrDefault(it => it.Name == container);

                    if (dbContainer == null)
                    {
                        dbContainer = ctx.Containers.Create();

                        dbContainer.Name = container;
                        dbContainer.Project = dbProject;
                        dbContainer.ParentContainer = dbParentContainer;
                        dbContainer.DataName = container;
                        dbContainer.DisplayName = container;
                        dbContainer.IsRepeating = isRepeating;
                        dbContainer.Sequence = 0;

                        ctx.Containers.Add(dbContainer);
                    }

                    foreach (string attribute in attributes)
                    {
                        Attribute dbAttribute = dbContainer.Attributes.SingleOrDefault(it => it.Name == attribute);

                        if (dbAttribute == null)
                        {
                            dbAttribute = ctx.Attributes.Create();

                            dbAttribute.Name = attribute;
                            dbAttribute.Container = dbContainer;
                            dbAttribute.DataName = attribute;
                            dbAttribute.DisplayName = attribute;
                            dbAttribute.HasFixedValues = false;
                            dbAttribute.HasVariableUnits = false;
                            dbAttribute.DataType = ctx.DataTypes.Single(it => it.Name == "String");
                            dbAttribute.Term = ctx.Terms.First();

                            ctx.Attributes.Add(dbAttribute);
                        }
                    }
                }

                if (!String.IsNullOrWhiteSpace(subject))
                {
                    Subject dbSubject = ctx.Subjects.SingleOrDefault(it => it.Project.Name == project && it.MemberID == subject);

                    if (dbSubject == null)
                    {
                        dbSubject = ctx.Subjects.Create();

                        dbSubject.MemberID = subject;
                        dbSubject.Project = dbProject;
                        dbSubject.Entity = ctx.Entities.First();

                        ctx.Subjects.Add(dbSubject);
                    }
                }

                ctx.SaveChanges();
            }

            return (dbProject);
        }

        private void SingleContainerTest(bool repeating, bool noData)
        {
            string project = "Unit Test Project";
            string subject = "Unit Test Subject";

            AddProjectMetadata(project, null, "Unit Test Root Container", repeating, subject, "Unit Test Attribute 1", "Unit Test Attribute 2", "Unit Test Attribute 3");

            EAVProjectContextControl ctlProject = BuildContextControls(project, subject);
            EAVContextController controller = new EAVContextController();

            ctlProject.DataSource = controller.Projects;
            ctlProject.DataBind();

            EAVSubjectContextControl ctlSubject = ctlProject.ContextChildren.ElementAt(0) as EAVSubjectContextControl;
            EAVInstanceContextControl ctlInstance = ctlSubject.ContextChildren.ElementAt(0).ContextChildren.ElementAt(0) as EAVInstanceContextControl;

            EAVTextBox ctlValue0 = ((IEAVValueControlContainer)ctlInstance.ContextChildren.ElementAt(0)).ValueControls.ElementAt(0) as EAVTextBox;
            EAVTextBox ctlValue1 = ((IEAVValueControlContainer)ctlInstance.ContextChildren.ElementAt(1)).ValueControls.ElementAt(0) as EAVTextBox;
            EAVTextBox ctlValue2 = ((IEAVValueControlContainer)ctlInstance.ContextChildren.ElementAt(2)).ValueControls.ElementAt(0) as EAVTextBox;

            try
            {
                // Insert
                ctlValue0.RawValue = "Insert Value 0";
                ctlValue1.RawValue = "Insert Value 1";
                ctlValue2.RawValue = "Insert Value 2";

                Assert.IsTrue(String.IsNullOrWhiteSpace(ctlInstance.ContextKey));

                controller.Save(ctlProject);

                Assert.IsFalse(String.IsNullOrWhiteSpace(ctlInstance.ContextKey));
                int repeatInstance = Int32.Parse(ctlInstance.ContextKey);

                using (EAVEntityContext ctx = new EAVEntityContext())
                {
                    ContainerInstance dbInstance = ctx.ContainerInstances.Where(it => it.RepeatInstance == repeatInstance && it.Subject.MemberID == subject && it.Container.Name == ctlInstance.ContextParent.ContextKey).SingleOrDefault();

                    Assert.IsNotNull(dbInstance);
                    Assert.IsTrue(dbInstance.Values.Any());

                    Assert.AreEqual(ctlValue0.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue0.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                    Assert.AreEqual(ctlValue1.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue1.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                    Assert.AreEqual(ctlValue2.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue2.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                }

                if (noData)
                {
                    // Update
                    ctlValue0.RawValue = "Update Value 0";
                    ctlValue1.RawValue = "Update Value 1";
                    ctlValue1.RawValue = "Update Value 2";

                    controller.Save(ctlProject);

                    using (EAVEntityContext ctx = new EAVEntityContext())
                    {
                        ContainerInstance dbInstance = ctx.ContainerInstances.Where(it => it.RepeatInstance == repeatInstance && it.Subject.MemberID == subject && it.Container.Name == ctlInstance.ContextParent.ContextKey).SingleOrDefault();

                        Assert.IsNotNull(dbInstance);
                        Assert.IsTrue(dbInstance.Values.Any());

                        Assert.AreEqual(ctlValue0.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue0.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                        Assert.AreEqual(ctlValue1.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue1.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                        Assert.AreEqual(ctlValue2.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue2.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                    }

                    // Partial Delete
                    ctlValue0.RawValue = null;

                    controller.Save(ctlProject);

                    using (EAVEntityContext ctx = new EAVEntityContext())
                    {
                        ContainerInstance dbInstance = ctx.ContainerInstances.Where(it => it.RepeatInstance == repeatInstance && it.Subject.MemberID == subject && it.Container.Name == ctlInstance.ContextParent.ContextKey).SingleOrDefault();

                        Assert.IsNotNull(dbInstance);
                        Assert.IsTrue(dbInstance.Values.Any());

                        Assert.IsNull(dbInstance.Values.Where(it => it.Attribute.Name == ctlValue0.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                        Assert.AreEqual(ctlValue1.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue1.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                        Assert.AreEqual(ctlValue2.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue2.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                    }

                    // Delete All
                    ctlValue1.RawValue = null;
                    ctlValue2.RawValue = null;

                    controller.Save(ctlProject);

                    Assert.IsTrue(String.IsNullOrWhiteSpace(ctlInstance.ContextKey));

                    using (EAVEntityContext ctx = new EAVEntityContext())
                    {
                        ContainerInstance dbInstance = ctx.ContainerInstances.Where(it => it.RepeatInstance == repeatInstance && it.Subject.MemberID == subject && it.Container.Name == ctlInstance.ContextParent.ContextKey).SingleOrDefault();

                        Assert.IsNull(dbInstance);
                    }
                }
                else
                {
                    // Second Insert
                    ctlInstance.ContextKey = null;
                    ctlProject.DataBind();

                    ctlValue0.RawValue = "Insert Value 3";
                    ctlValue1.RawValue = "Insert Value 4";
                    ctlValue2.RawValue = "Insert Value 5";

                    Assert.IsTrue(String.IsNullOrWhiteSpace(ctlInstance.ContextKey));

                    controller.Save(ctlProject);

                    Assert.IsFalse(String.IsNullOrWhiteSpace(ctlInstance.ContextKey));
                    repeatInstance = Int32.Parse(ctlInstance.ContextKey);

                    using (EAVEntityContext ctx = new EAVEntityContext())
                    {
                        ContainerInstance dbInstance = ctx.ContainerInstances.Where(it => it.RepeatInstance == repeatInstance && it.Subject.MemberID == subject && it.Container.Name == ctlInstance.ContextParent.ContextKey).SingleOrDefault();

                        Assert.IsNotNull(dbInstance);
                        Assert.IsTrue(dbInstance.Values.Any());

                        Assert.AreEqual(ctlValue0.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue0.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                        Assert.AreEqual(ctlValue1.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue1.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                        Assert.AreEqual(ctlValue2.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue2.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                    }
                }
            }
            finally
            {
                RemoveProjectMetadata(project);
            }
        }

        [TestMethod]
        public void SingleRepeatingContainerCRUD()
        {
            SingleContainerTest(true, false);
        }

        [TestMethod]
        public void SingleRepeatingContainerInsertWithData()
        {
            SingleContainerTest(true, true);
        }

        [TestMethod]
        public void SingleNonRepeatingContainerCRUD()
        {
            SingleContainerTest(false, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void SingleNonRepeatingContainerInsertWithData()
        {
            SingleContainerTest(false, false);
        }
    }
}