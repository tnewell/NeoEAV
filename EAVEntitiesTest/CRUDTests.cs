using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Validation;

using NeoEAV.Data.DataClasses;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Attribute = NeoEAV.Data.DataClasses.Attribute;


namespace EAVEntitiesTest
{
    [TestClass]
    public class CRUDTests
    {
        [TestMethod]
        public void ProjectCRUD()
        {
            int id = 0;
            string tmpProjectName = Guid.NewGuid().ToString();

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Project p = new Project() { Name = tmpProjectName };

                ctx.Projects.Add(p);
                ctx.SaveChanges();

                id = p.ProjectID;
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Project p = ctx.Projects.SingleOrDefault(it => it.ProjectID == id);

                Assert.IsNotNull(p, "Project cannot be modified. Project was not created.");
                Assert.IsTrue(String.IsNullOrWhiteSpace(p.Description), "Project modification cannot be verified. Description property is not empty.");

                p.Description = tmpProjectName;

                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Project p = ctx.Projects.SingleOrDefault(it => it.ProjectID == id);

                Assert.AreEqual(tmpProjectName, p.Description, "Description property was not modified as expected.");

                // Clean up
                ctx.Projects.Remove(p);
                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Project p = ctx.Projects.SingleOrDefault(it => it.ProjectID == id);

                Assert.IsNull(p, "Project was not deleted.");
            }
        }

        [TestMethod]
        public void ChildlessRootContainerCRUD()
        {
            int id = 0;
            string tmpProjectName = Guid.NewGuid().ToString();
            string tmpContainerName = Guid.NewGuid().ToString();

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Container c = new Container() { Name = tmpContainerName, DisplayName = tmpContainerName, Project = new Project() { Name = tmpProjectName } };

                ctx.Containers.Add(c);
                ctx.SaveChanges();

                id = c.ContainerID;
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Container c = ctx.Containers.SingleOrDefault(it => it.ContainerID == id);

                Assert.IsNotNull(c, "Container cannot be modified. Container was not created.");
                Assert.AreNotEqual(tmpProjectName, c.DisplayName, "Container modification cannot be verified. DisplayName property is already set to test value.");

                c.DisplayName = tmpProjectName;

                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Container c = ctx.Containers.SingleOrDefault(it => it.ContainerID == id);

                Assert.AreEqual(tmpProjectName, c.DisplayName, "DisplayName property was not modified as expected.");

                // Clean up
                Project p = c.Project;
                ctx.Containers.Remove(c);
                ctx.Projects.Remove(p);
                ctx.SaveChanges();
            }

            // Test for deletion
            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Container c = ctx.Containers.SingleOrDefault(it => it.ContainerID == id);

                Assert.IsNull(c, "Container was not deleted.");
            }
        }

        [TestMethod]
        public void AttributeCRUD()
        {
            int id = 0;
            string tmpProjectName = Guid.NewGuid().ToString();
            string tmpContainerName = Guid.NewGuid().ToString();
            string tmpAttributeName = Guid.NewGuid().ToString();
            string tmpTermName = Guid.NewGuid().ToString();

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Attribute a = new Attribute()
                {
                    Name = tmpAttributeName,
                    DisplayName = tmpAttributeName,
                    DataType = ctx.DataTypes.Single(it => it.Name == "Integer"),
                    Container = new Container()
                    {
                        Name = tmpContainerName,
                        DisplayName = tmpContainerName,
                        Project = new Project() { Name = tmpProjectName }
                    },
                    Term = new Term() { Name = tmpTermName }
                };
                ctx.Attributes.Add(a);
                ctx.SaveChanges();

                id = a.AttributeID;
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Attribute a = ctx.Attributes.SingleOrDefault(it => it.AttributeID == id);

                Assert.IsNotNull(a, "Attribute cannot be modified. Attribute was not created.");
                Assert.AreNotEqual(tmpContainerName, a.DisplayName, "Attribute modification cannot be verified. DisplayName property is already set to test value.");

                a.DisplayName = tmpContainerName;

                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Attribute a = ctx.Attributes.SingleOrDefault(it => it.AttributeID == id);

                Assert.AreEqual(tmpContainerName, a.DisplayName, "DisplayName property was not modified as expected.");

                // Clean up
                Term t = a.Term;
                Container c = a.Container;
                Project p = c.Project;
                ctx.Attributes.Remove(a);
                ctx.Terms.Remove(t);
                ctx.Containers.Remove(c);
                ctx.Projects.Remove(p);
                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Attribute a = ctx.Attributes.SingleOrDefault(it => it.AttributeID == id);

                Assert.IsNull(a, "Attribute was not deleted.");
            }
        }

        [TestMethod]
        public void SubjectCRUD()
        {
            int id = 0;
            string tmpEntityName = Guid.NewGuid().ToString();
            string tmpProjectName = Guid.NewGuid().ToString();
            string tmpSubjectName = Guid.NewGuid().ToString();

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Subject s = new Subject() { MemberID = tmpSubjectName, Project = new Project() { Name = tmpProjectName }, Entity = new Entity() { Name = tmpEntityName } };

                ctx.Subjects.Add(s);
                ctx.SaveChanges();

                id = s.SubjectID;
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Subject s = ctx.Subjects.SingleOrDefault(it => it.SubjectID == id);

                Assert.IsNotNull(s, "Subject cannot be modified. Subject was not created.");
                Assert.AreNotEqual(tmpProjectName, s.MemberID, "Subject modification cannot be verified. MemberID property is already set to test value.");

                s.MemberID = tmpProjectName;

                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Subject s = ctx.Subjects.SingleOrDefault(it => it.SubjectID == id);

                Assert.IsNotNull(s, "Subject modification cannot be verified. Subject was not created.");
                Assert.AreEqual(tmpProjectName, s.MemberID, "MemberID property was not modified as expected.");

                // Clean up
                Project p = s.Project;
                Entity e = s.Entity;
                ctx.Subjects.Remove(s);
                ctx.Projects.Remove(p);
                ctx.Entities.Remove(e);
                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Subject s = ctx.Subjects.SingleOrDefault(it => it.SubjectID == id);

                Assert.IsNull(s, "Subject was not deleted.");
            }
        }

        [TestMethod]
        public void ChildlessRootContainerInstanceCRUD()
        {
            int id = 0;
            string tmpProjectName = Guid.NewGuid().ToString();
            string tmpSubjectName = Guid.NewGuid().ToString();
            string tmpEntityName = Guid.NewGuid().ToString();
            string tmpContainerName = Guid.NewGuid().ToString();

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Project p = new Project() { Name = tmpProjectName };
                ContainerInstance ci = new ContainerInstance() { Subject = new Subject() { MemberID = tmpSubjectName, Entity = new Entity() { Name = tmpEntityName }, Project = p }, Container = new Container() { Name = tmpContainerName, DisplayName = tmpContainerName, Project = p } };

                ctx.ContainerInstances.Add(ci);
                ctx.SaveChanges();

                id = ci.ContainerInstanceID;
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                ContainerInstance ci = ctx.ContainerInstances.SingleOrDefault(it => it.ContainerInstanceID == id);

                Assert.IsNotNull(ci, "ContainerInstance cannot be modified. ContainerInstance was not created.");
                Assert.AreNotEqual(42, ci.RepeatInstance, "ContainerInstance modification cannot be verified. RepeatInstance property is already set to test value.");

                ci.RepeatInstance = 42;

                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                ContainerInstance ci = ctx.ContainerInstances.SingleOrDefault(it => it.ContainerInstanceID == id);

                Assert.IsNotNull(ci, "ContainerInstance modification cannot be verified. ContainerInstance was not created.");
                Assert.AreEqual(42, ci.RepeatInstance, "RepeatInstance property was not modified as expected.");

                // Clean up
                Subject s = ci.Subject;
                Entity e = s.Entity;
                Container c = ci.Container;
                Project p = c.Project;
                ctx.ContainerInstances.Remove(ci);
                ctx.Subjects.Remove(s);
                ctx.Containers.Remove(c);
                ctx.Entities.Remove(e);
                ctx.Projects.Remove(p);
                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                ContainerInstance ci = ctx.ContainerInstances.SingleOrDefault(it => it.ContainerInstanceID == id);

                Assert.IsNull(ci, "ContainerInstance was not deleted.");
            }
        }

        [TestMethod]
        public void ValueCRUD()
        {
            int aid = 0;
            int ciid = 0;
            string tmpProjectName = Guid.NewGuid().ToString();
            string tmpSubjectName = Guid.NewGuid().ToString();
            string tmpEntityName = Guid.NewGuid().ToString();
            string tmpContainerName = Guid.NewGuid().ToString();
            string tmpTermName = Guid.NewGuid().ToString();
            string tmpAttributeName = Guid.NewGuid().ToString();

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Project p = new Project() { Name = tmpProjectName };
                Container c = new Container() { Name = tmpContainerName, DisplayName = tmpContainerName, Project = p };
                ContainerInstance ci = new ContainerInstance() { Subject = new Subject() { MemberID = tmpSubjectName, Entity = new Entity() { Name = tmpEntityName }, Project = p }, Container = c };
                Value v = new Value() { ContainerInstance = ci, Attribute = new Attribute() { Name = tmpAttributeName, DisplayName = tmpAttributeName, DataType = ctx.DataTypes.Single(it => it.Name == "Integer"), Term = new Term() { Name = tmpTermName } }, RawValue = "Raw Value" };

                ctx.Values.Add(v);
                ctx.SaveChanges();

                ciid = v.ContainerInstanceID;
                aid = v.AttributeID;
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Value v = ctx.Values.SingleOrDefault(it => it.ContainerInstanceID == ciid && it.AttributeID == aid);

                Assert.IsNotNull(v, "Value cannot be modified. Value was not created.");
                Assert.AreNotEqual("42", v.RawValue, "Value modification cannot be verified. RawValue property is already set to test value.");

                v.RawValue = "42";

                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Value v = ctx.Values.SingleOrDefault(it => it.ContainerInstanceID == ciid && it.AttributeID == aid);

                Assert.IsNotNull(v, "Value modification cannot be verified. Value was not created.");
                Assert.AreEqual("42", v.RawValue, "RawValue property was not modified as expected.");

                // Clean up
                Attribute a = v.Attribute;
                Term t = a.Term;
                ContainerInstance ci = v.ContainerInstance;
                Subject s = ci.Subject;
                Entity e = s.Entity;
                Container c = ci.Container;
                Project p = c.Project;
                ctx.Values.Remove(v);
                ctx.Attributes.Remove(a);
                ctx.Terms.Remove(t);
                ctx.ContainerInstances.Remove(ci);
                ctx.Subjects.Remove(s);
                ctx.Entities.Remove(e);
                ctx.Containers.Remove(c);
                ctx.Projects.Remove(p);
                ctx.SaveChanges();
            }

            using (EAVEntityContext ctx = new EAVEntityContext())
            {
                Value v = ctx.Values.SingleOrDefault(it => it.ContainerInstanceID == ciid && it.AttributeID == aid);

                Assert.IsNull(v, "Value was not deleted.");
            }
        }
    }
}
