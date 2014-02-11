using System;
using System.Linq;
using System.Web.UI;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NeoEAV.Data.DataClasses;
using NeoEAV.Objects;
using NeoEAV.Web.UI;

using Attribute = NeoEAV.Data.DataClasses.Attribute;


namespace EAVEntitiesTest
{
    [TestClass]
    public class WebUIUnitTests
    {
        private EAVProjectContextControl BuildContextControls(string project, string subject = null)
        {
            EAVProjectContextControl ctlProject = new EAVProjectContextControl() { ContextKey = project };

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Project dbProject = ctx.Projects.Single(it => it.Name == project);

                EAVSubjectContextControl ctlSubject = new EAVSubjectContextControl() { ContextKey = String.IsNullOrWhiteSpace(subject) ? null : subject };
                ((Control)ctlProject).Controls.Add(ctlSubject);

                foreach (Container dbContainer in dbProject.Containers)
                {
                    EAVContainerContextControl ctlContainer = new EAVContainerContextControl() { ContextKey = dbContainer.Name };
                    ((Control)ctlSubject).Controls.Add(ctlContainer);

                    EAVInstanceContextControl ctlInstance = new EAVInstanceContextControl();
                    ((Control)ctlContainer).Controls.Add(ctlInstance);

                    foreach (Attribute dbAttribute in dbContainer.Attributes)
                    {
                        EAVAttributeContextControl ctlAttribute = new EAVAttributeContextControl() { ContextKey = dbAttribute.Name };
                        ((Control)ctlInstance).Controls.Add(ctlAttribute);

                        EAVTextBox ctlValue = new EAVTextBox();
                        ((Control)ctlAttribute).Controls.Add(ctlValue);

                    }
                }
            }

            return (ctlProject);
        }

        [TestMethod]
        public void CRUD_SingleContainer()
        {
            EAVContextController controller = new EAVContextController();

            EAVProjectContextControl ctlProject = BuildContextControls("Test Project 1", "Subject 1");

            ctlProject.DataSource = controller.Projects;
            ctlProject.DataBind();

            EAVSubjectContextControl ctlSubject = ctlProject.ContextChildren.ElementAt(0) as EAVSubjectContextControl;
            EAVInstanceContextControl ctlInstance = ctlSubject.ContextChildren.ElementAt(0).ContextChildren.ElementAt(0) as EAVInstanceContextControl;
            EAVTextBox ctlValue0 = ((IEAVValueControlContainer) ctlInstance.ContextChildren.ElementAt(0)).ValueControls.ElementAt(0) as EAVTextBox;
            EAVTextBox ctlValue1 = ((IEAVValueControlContainer) ctlInstance.ContextChildren.ElementAt(1)).ValueControls.ElementAt(0) as EAVTextBox;
            EAVTextBox ctlValue2 = ((IEAVValueControlContainer) ctlInstance.ContextChildren.ElementAt(2)).ValueControls.ElementAt(0) as EAVTextBox;

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
                ContainerInstance dbInstance = ctx.ContainerInstances.Where(it => it.RepeatInstance == repeatInstance && it.Subject.MemberID == "Subject 1" && it.Container.Name == ctlInstance.ContextParent.ContextKey).SingleOrDefault();

                Assert.IsNotNull(dbInstance);
                Assert.IsTrue(dbInstance.Values.Any());
                Assert.AreEqual(ctlValue0.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue0.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                Assert.AreEqual(ctlValue1.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue1.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
                Assert.AreEqual(ctlValue2.RawValue, dbInstance.Values.Where(it => it.Attribute.Name == ctlValue2.ContextParent.ContextKey).Select(it => it.RawValue).SingleOrDefault());
            }

            // Update
            ctlValue0.RawValue = "Update Value 0";
            ctlValue1.RawValue = "Update Value 1";
            ctlValue1.RawValue = "Update Value 2";

            controller.Save(ctlProject);

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                ContainerInstance dbInstance = ctx.ContainerInstances.Where(it => it.RepeatInstance == repeatInstance && it.Subject.MemberID == "Subject 1" && it.Container.Name == ctlInstance.ContextParent.ContextKey).SingleOrDefault();

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
                ContainerInstance dbInstance = ctx.ContainerInstances.Where(it => it.RepeatInstance == repeatInstance && it.Subject.MemberID == "Subject 1" && it.Container.Name == ctlInstance.ContextParent.ContextKey).SingleOrDefault();

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
                ContainerInstance dbInstance = ctx.ContainerInstances.Where(it => it.RepeatInstance == repeatInstance && it.Subject.MemberID == "Subject 1" && it.Container.Name == ctlInstance.ContextParent.ContextKey).SingleOrDefault();

                Assert.IsNull(dbInstance);
            }
        }
    }
}
