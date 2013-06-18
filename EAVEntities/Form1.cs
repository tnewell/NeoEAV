using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NeoEAV.Data.DataClasses;

using Attribute = NeoEAV.Data.DataClasses.Attribute;


namespace EAVEntities
{
    public partial class Form1 : Form
    {
        private EAVEntityContext sourceContext;
        private EAVEntityContext targetContext;

        private string SourceConnectionString { get { return(ConfigurationManager.ConnectionStrings["EAVEntityContext"].ConnectionString); } }
        private string TargetConnectionString { get { return (ConfigurationManager.ConnectionStrings["DataTarget"].ConnectionString); } }

        public Form1()
        {
            InitializeComponent();

            sourceContext = new EAVEntityContext(SourceConnectionString);
            targetContext = new EAVEntityContext(TargetConnectionString);
        }

        private SqlDbType SQLTypeFromEAVType(EAVDataType eavType)
        {
            switch (eavType)
            {
                case EAVDataType.Boolean: return (SqlDbType.Bit);
                case EAVDataType.DateTime: return (SqlDbType.DateTime);
                case EAVDataType.Float: return (SqlDbType.Float);
                case EAVDataType.Integer: return (SqlDbType.Int);
                case EAVDataType.String: return (SqlDbType.NVarChar);
                default: throw(new ArgumentException(String.Format("Parameter 'eavType' has an invalid value '{0}'.", eavType), "eavType"));
            }
        }

        private Type SystemTypeFromEAVType(EAVDataType eavType)
        {
            switch (eavType)
            {
                case EAVDataType.Boolean: return (typeof(Boolean));
                case EAVDataType.DateTime: return (typeof(DateTime));
                case EAVDataType.Float: return (typeof(Double));
                case EAVDataType.Integer: return (typeof(Int32));
                case EAVDataType.String: return (typeof(String));
                default: throw (new ArgumentException(String.Format("Parameter 'eavType' has an invalid value '{0}'.", eavType), "eavType"));
            }
        }

        #region Core Table Creation

        private string GenerateTermsTableCreateScript(DataSet dsData)
        {
            DataTable dbTable = new DataTable();

            dbTable.TableName = "Terms";
            dbTable.Columns.Add(new DataColumn("Term_ID", typeof(Int32)) { AllowDBNull = false, Unique = true });
            dbTable.Columns.Add(new DataColumn("Name", typeof(String)) { AllowDBNull = false, MaxLength = 512 });
            dbTable.PrimaryKey = new DataColumn[] { dbTable.Columns["Term_ID"] };

            dsData.Tables.Add(dbTable);

            StringBuilder strSQL = new StringBuilder();

            strSQL.Append("CREATE TABLE [Terms] (");
            strSQL.Append("[Term_ID] INT NOT NULL, [Name] NVARCHAR(512) NOT NULL");
            strSQL.Append(", CONSTRAINT [PK_Terms] PRIMARY KEY CLUSTERED ([Term_ID] ASC)");
            strSQL.Append(");");

            return(strSQL.ToString());
        }
        private void UpdateTermsTable(DataSet dsEAVData, IEnumerable<Term> terms)
        {
            SqlCommandBuilder dbBuilder = new SqlCommandBuilder(new SqlDataAdapter("SELECT * FROM Terms", TargetConnectionString));
            dbBuilder.DataAdapter.Fill(dsEAVData, "Terms");
            foreach (Term term in terms)
            {
                dsEAVData.Tables["Terms"].LoadDataRow(new object[] { term.TermID, term.Name }, false);
            }
            dbBuilder.DataAdapter.Update(dsEAVData, "Terms");
        }

        private string GenerateEntitiesTableCreateScript(DataSet dsData)
        {
            DataTable dbTable = new DataTable();

            dbTable.TableName = "Entities";
            dbTable.Columns.Add(new DataColumn("Entity_ID", typeof(Int32)) { AllowDBNull = false, Unique = true });
            dbTable.Columns.Add(new DataColumn("Name", typeof(String)) { AllowDBNull = false, MaxLength = 512 });
            dbTable.PrimaryKey = new DataColumn[] { dbTable.Columns["Entity_ID"] };

            dsData.Tables.Add(dbTable);

            StringBuilder strSQL = new StringBuilder();

            strSQL.Append("CREATE TABLE [Entities] (");
            strSQL.Append("[Entity_ID] INT NOT NULL, [Name] NVARCHAR(512) NOT NULL");
            strSQL.Append(", CONSTRAINT [PK_Entities] PRIMARY KEY CLUSTERED ([Entity_ID] ASC)");
            strSQL.Append(");");

            return (strSQL.ToString());
        }
        private void UpdateEntitiesTable(DataSet dsEAVData, IEnumerable<Entity> entities)
        {
            SqlCommandBuilder dbBuilder = new SqlCommandBuilder(new SqlDataAdapter("SELECT * FROM Entities", TargetConnectionString));
            dbBuilder.DataAdapter.Fill(dsEAVData, "Entities");
            foreach (Entity entity in entities)
            {
                dsEAVData.Tables["Entities"].LoadDataRow(new object[] { entity.EntityID, entity.Name }, false);
            }
            dbBuilder.DataAdapter.Update(dsEAVData, "Entities");
        }

        private string GenerateProjectsTableCreateScript(DataSet dsData)
        {
            DataTable dbTable = new DataTable();

            dbTable.TableName = "Projects";
            dbTable.Columns.Add(new DataColumn("Project_ID", typeof(Int32)) { AllowDBNull = false, Unique = true });
            dbTable.Columns.Add(new DataColumn("Name", typeof(String)) { AllowDBNull = false, MaxLength = 512 });
            dbTable.Columns.Add(new DataColumn("Description", typeof(String)) { AllowDBNull = true, MaxLength = Int32.MaxValue });
            dbTable.PrimaryKey = new DataColumn[] { dbTable.Columns["Project_ID"] };

            dsData.Tables.Add(dbTable);

            StringBuilder strSQL = new StringBuilder();

            strSQL.Append("CREATE TABLE [Projects] (");
            strSQL.Append("[Project_ID] INT NOT NULL, [Name] NVARCHAR(512) NOT NULL, [Description] NVARCHAR(MAX) NULL");
            strSQL.Append(", CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED ([Project_ID] ASC)");
            strSQL.Append(");");

            return (strSQL.ToString());
        }
        private void UpdateProjectsTable(DataSet dsEAVData, IEnumerable<Project> projects)
        {
            SqlCommandBuilder dbBuilder = new SqlCommandBuilder(new SqlDataAdapter("SELECT * FROM Projects", TargetConnectionString));
            dbBuilder.DataAdapter.Fill(dsEAVData, "Projects");
            foreach (Project project in projects)
            {
                dsEAVData.Tables["Projects"].LoadDataRow(new object[] { project.ProjectID, project.Name, project.Description }, false);
            }
            dbBuilder.DataAdapter.Update(dsEAVData, "Projects");
        }

        private string GenerateSubjectsTableCreateScript(DataSet dsData)
        {
            DataTable dbTable = new DataTable();

            dbTable.TableName = "Subjects";
            dbTable.Columns.Add(new DataColumn("Subject_ID", typeof(Int32)) { AllowDBNull = false, Unique = true });
            dbTable.Columns.Add(new DataColumn("Member_ID", typeof(String)) { AllowDBNull = false, MaxLength = 128 });
            dbTable.Columns.Add(new DataColumn("Project_ID", typeof(Int32)) { AllowDBNull = false });
            dbTable.Columns.Add(new DataColumn("Entity_ID", typeof(Int32)) { AllowDBNull = false });
            dbTable.PrimaryKey = new DataColumn[] { dbTable.Columns["Subject_ID"] };

            dsData.Tables.Add(dbTable);

            dbTable.Constraints.Add(new ForeignKeyConstraint("FK_Projects_Subjects", dsData.Tables["Projects"].Columns["Project_ID"], dbTable.Columns["Project_ID"]));
            dbTable.Constraints.Add(new ForeignKeyConstraint("FK_Entities_Subjects", dsData.Tables["Entities"].Columns["Entity_ID"], dbTable.Columns["Entity_ID"]));

            StringBuilder strSQL = new StringBuilder();

            strSQL.Append("CREATE TABLE [Subjects] (");
            strSQL.Append("[Subject_ID] INT NOT NULL, [Member_ID] NVARCHAR(128) NOT NULL, [Project_ID] INT NOT NULL, [Entity_ID] INT NOT NULL");
            strSQL.Append(", CONSTRAINT [PK_Subjects] PRIMARY KEY CLUSTERED ([Subject_ID] ASC)");
            strSQL.Append(", CONSTRAINT [FK_Subjects_Projects] FOREIGN KEY ([Project_ID]) REFERENCES [Projects] ([Project_ID])");
            strSQL.Append(", CONSTRAINT [FK_Subjects_Entities] FOREIGN KEY ([Entity_ID]) REFERENCES [Entities] ([Entity_ID])");
            strSQL.Append("); GO;");

            return (strSQL.ToString());
        }
        
        #endregion

        private void GenerateContainerTableCreateScript(List<string> scripts, DataSet dsData, Container container, Container parentContainer = null)
        {
            StringBuilder strSQL = new StringBuilder();
            DataTable dbTable = new DataTable();

            strSQL.AppendFormat("CREATE TABLE [{0}] (", container.DataName);
            dbTable.TableName = container.DataName;

            strSQL.Append("[Subject_ID] INT NOT NULL, [Repeat_Instance] INT NOT NULL");
            dbTable.Columns.Add(new DataColumn("Subject_ID", typeof(Int32)) { AllowDBNull = false });
            dbTable.Columns.Add(new DataColumn("Repeat_Instance", typeof(Int32)) { AllowDBNull = false });

            if (parentContainer != null)
            {
                strSQL.Append(", [Parent_Repeat_Instance] INT NULL");
                dbTable.Columns.Add(new DataColumn("Parent_Repeat_Instance", typeof(Int32)) { AllowDBNull = true });
            }

            foreach (Attribute attribute in container.Attributes)
            {
                strSQL.AppendFormat(", [{0}] NVARCHAR(MAX) NOT NULL", attribute.DataName);
                dbTable.Columns.Add(new DataColumn(attribute.DataName, typeof(String)) { AllowDBNull = false, MaxLength = Int32.MaxValue });

                if (attribute.HasVariableUnits || attribute.Units.Any())
                {
                    strSQL.AppendFormat(", [{0} (Units)] NVARCHAR(8) NULL", attribute.DataName);
                    dbTable.Columns.Add(new DataColumn(String.Concat(attribute.DataName, " (Units)"), typeof(String)) { AllowDBNull = true, MaxLength = 8 });
                }

                strSQL.AppendFormat(", [{0} (TYPED)] {1}{2} NULL", attribute.DataName, SQLTypeFromEAVType(attribute.DataType.DataTypeID), attribute.DataType.DataTypeID == EAVDataType.String ? "(MAX)" : String.Empty);
                dbTable.Columns.Add(new DataColumn(String.Concat(attribute.DataName, " (TYPED)"), SystemTypeFromEAVType(attribute.DataType.DataTypeID)) { AllowDBNull = true });
            }

            strSQL.AppendFormat(", CONSTRAINT [PK_{0}_{1}] PRIMARY KEY CLUSTERED ([Subject_ID] ASC, [Repeat_Instance] ASC)", container.Project.DataName.Replace(" ", "_"), container.DataName.Replace(" ", "_"));
            dbTable.PrimaryKey = new DataColumn[] { dbTable.Columns["Subject_ID"], dbTable.Columns["Repeat_Instance"] };

            dsData.Tables.Add(dbTable);

            strSQL.AppendFormat(", CONSTRAINT [FK_{0}_{1}_Subjects] FOREIGN KEY ([Subject_ID]) REFERENCES [Subjects] ([Subject_ID])", container.Project.DataName.Replace(" ", "_"), container.DataName.Replace(" ", "_"));
            dbTable.Constraints.Add(new ForeignKeyConstraint(String.Format("FK_{0}_{1}_Subjects", container.Project.DataName.Replace(" ", "_"), container.DataName.Replace(" ", "_")), dsData.Tables["Subjects"].Columns["Subject_ID"], dbTable.Columns["Subject_ID"]));

            if (parentContainer != null)
            {
                strSQL.AppendFormat(", CONSTRAINT [FK_{0}_{1}] FOREIGN KEY ([Subject_ID], [Parent_Repeat_Instance]) REFERENCES [{2}] ([Subject_ID], [Repeat_Instance])", container.DataName.Replace(" ", "_"), container.ParentContainer.DataName.Replace(" ", "_"), container.ParentContainer.DataName);
                dbTable.Constraints.Add(new ForeignKeyConstraint(String.Format("FK_{0}_{1}", container.DataName.Replace(" ", "_"), container.ParentContainer.DataName.Replace(" ", "_")), new DataColumn[] { dsData.Tables[container.ParentContainer.DataName].Columns["Subject_ID"], dsData.Tables[container.ParentContainer.DataName].Columns["Repeat_Instance"] }, new DataColumn[] { dbTable.Columns["Subject_ID"], dbTable.Columns["Parent_Repeat_Instance"] }));
            }

            strSQL.Append(");");

            scripts.Add(strSQL.ToString());

            foreach (Container childContainer in container.ChildContainers)
            {
                GenerateContainerTableCreateScript(scripts, dsData, childContainer, container);
            }
        }

        private void ExecuteScripts(List<string> scripts)
        {
            using (SqlConnection dbConn = new SqlConnection(TargetConnectionString))
            {
                dbConn.Open();

                SqlTransaction dbTransaction = dbConn.BeginTransaction();
                SqlCommand dbCmd = new SqlCommand("", dbConn) { Transaction = dbTransaction };

                try
                {
                    foreach (var script in scripts)
                    {
                        dbCmd.CommandText = script;
                        dbCmd.ExecuteNonQuery();
                    }

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    ctlResults.AppendText(ex.ToString());

                    dbTransaction.Rollback();
                }
            }
        }

        private void UpdateTable(DataTable table, IEnumerable<object[]> rowData)
        {
            SqlCommandBuilder dbBuilder = new SqlCommandBuilder(new SqlDataAdapter(String.Format("SELECT * FROM [{0}]", table.TableName), TargetConnectionString));
            dbBuilder.DataAdapter.Fill(table);
            foreach (object[] itemArray in rowData)
            {
                table.LoadDataRow(itemArray, false);
            }
            dbBuilder.DataAdapter.Update(table);
        }

        private void UpdateDataTable(DataSet dsEAVData, Container container, IEnumerable<ContainerInstance> instances)
        {
            List<object[]> rowData = new List<object[]>();
            foreach (ContainerInstance instance in instances)
            {
                List<object> itemArray = new List<object>();

                itemArray.Add(instance.SubjectID);
                itemArray.Add(instance.RepeatInstance);

                if (instance.ParentContainerInstance != null)
                    itemArray.Add(instance.ParentContainerInstance.RepeatInstance);

                foreach (Attribute attribute in instance.Container.Attributes)
                {
                    bool hasUnits = attribute.HasVariableUnits || attribute.Units.Any();
                    Value value = instance.Values.SingleOrDefault(it => it.Attribute == attribute);

                    itemArray.Add(value != null ? value.RawValue : null);
                    if (hasUnits) itemArray.Add(value != null ? value.Units : null);
                    itemArray.Add(value != null ? value.ObjectValue : null);
                }

                rowData.Add(itemArray.ToArray());
            }

            UpdateTable(dsEAVData.Tables[container.DataName], rowData);

            ctlResults.AppendText("\tUpdating child data tables...\r\n");
            foreach (var instanceSet in instances.SelectMany(it => it.ChildContainerInstances).GroupBy(key => key.Container))
            {
                UpdateDataTable(dsEAVData, instanceSet.Key, instanceSet);
            }
        }

        private void ExportProjectData(Project project)
        {
            DataSet dsEAVData = new DataSet();
            List<string> scripts = new List<string>();

            scripts.Add(GenerateTermsTableCreateScript(dsEAVData));
            scripts.Add(GenerateEntitiesTableCreateScript(dsEAVData));
            scripts.Add(GenerateProjectsTableCreateScript(dsEAVData));
            scripts.Add(GenerateSubjectsTableCreateScript(dsEAVData)); // Dependent on Entities and Projects tables

            //ExecuteScripts(scripts);
            scripts.Clear();

            ctlResults.AppendText("Updating terms...\r\n");
            UpdateTable(dsEAVData.Tables["Terms"], project.Containers.SelectMany(it => it.Attributes).Select(it => it.Term).Distinct().Select(it => new object[] { it.TermID, it.Name }));

            ctlResults.AppendText("Updating entities...\r\n");
            UpdateTable(dsEAVData.Tables["Entities"], project.Subjects.Select(it => it.Entity).Distinct().Select(it => new object[] { it.EntityID, it.Name }));

            ctlResults.AppendText("Updating projects...\r\n");
            UpdateTable(dsEAVData.Tables["Projects"], new List<object[]>() { new object[] { project.ProjectID, project.Name, project.Description } });

            ctlResults.AppendText("Updating subjects...\r\n");
            UpdateTable(dsEAVData.Tables["Subjects"], project.Subjects.Select(it => new object[] { it.SubjectID, it.MemberID, it.ProjectID, it.EntityID }));

            ctlResults.AppendText("Generating data tables...\r\n");
            foreach (Container container in project.Containers.Where(it => it.ParentContainer == null))
            {
                GenerateContainerTableCreateScript(scripts, dsEAVData, container);
            }

            //ExecuteScripts(scripts);
            scripts.Clear();

            ctlResults.AppendText("Updating root data tables...\r\n");
            foreach (var instanceSet in project.Subjects.SelectMany(it => it.ContainerInstances).Where(it => it.ParentContainerInstance == null).GroupBy(key => key.Container))
            {
                UpdateDataTable(dsEAVData, instanceSet.Key, instanceSet);
            }
        }

        private void ctlGoButton_Click(object sender, EventArgs e)
        {
            ExportProjectData(sourceContext.Projects.OrderBy(it => it.ProjectID).Skip(2).First());

            ctlResults.AppendText("\r\nDone\r\n");
        }
    }
}
