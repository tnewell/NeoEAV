using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace NeoEAV.Data.DataClasses
{
    public class EAVEntityContext : DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<Attribute> Attributes { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<ContainerInstance> ContainerInstances { get; set; }
        public DbSet<Value> Values { get; set; }
        public DbSet<Value2> OtherValues { get; set; }
        public DbSet<DataType> DataTypes { get; set; }

        public EAVEntityContext() { }

        public EAVEntityContext(string connectionString) : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Attribute>().HasMany(it => it.Units).WithMany(it => it.Attributes).Map(it =>
            {
                it.MapLeftKey("Attribute_ID");
                it.MapRightKey("Unit_ID");
                it.ToTable("AttributeUnits");
            });

            base.OnModelCreating(modelBuilder);
        }
    }

    public class Project
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("Project_ID")]
        public int ProjectID { get; set; }

        [Required, MaxLength(512)]
        public string Name { get; set; }

        [Required, Column("Data_Name"), MaxLength(64)]
        public string DataName { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Container> Containers { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; }

        public Project()
        {
            Containers = new List<Container>();
            Subjects = new List<Subject>();
        }
    }

    public class Container
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("Container_ID")]
        public int ContainerID { get; set; }

        [Required, MaxLength(512)]
        public string Name { get; set; }

        [Required, Column("Display_Name"), MaxLength(512)]
        public string DisplayName { get; set; }

        [Required, Column("Data_Name"), MaxLength(64)]
        public string DataName { get; set; }

        [Required]
        public int Sequence { get; set; }

        [Required, Column("Is_Repeating")]
        public bool IsRepeating { get; set; }

        [Required, Column("Project_ID")]
        public int ProjectID { get; set; }
        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        [Column("Parent_Container_ID")]
        public int? ParentContainerID { get; set; }
        [ForeignKey("ParentContainerID")]
        public virtual Container ParentContainer { get; set; }

        public virtual ICollection<Container> ChildContainers { get; set; }

        public virtual ICollection<Attribute> Attributes { get; set; }

        public virtual ICollection<ContainerInstance> ContainerInstances { get; set; }

        public Container()
        {
            ChildContainers = new List<Container>();
            Attributes = new List<Attribute>();
            ContainerInstances = new List<ContainerInstance>();
        }
    }

    public class Attribute
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("Attribute_ID")]
        public int AttributeID { get; set; }

        [Required, MaxLength(512)]
        public string Name { get; set; }

        [Required, Column("Display_Name"), MaxLength(512)]
        public string DisplayName { get; set; }

        [Required, Column("Data_Name"), MaxLength(64)]
        public string DataName { get; set; }

        [Required]
        public int Sequence { get; set; }

        [Required, Column("Has_Variable_Units")]
        public bool HasVariableUnits { get; set; }

        [Required, Column("Has_Fixed_Values")]
        public bool HasFixedValues { get; set; }

        [Required, Column("Container_ID")]
        public int ContainerID { get; set; }
        [ForeignKey("ContainerID")]
        public virtual Container Container { get; set; }

        [Required, Column("Term_ID")]
        public int TermID { get; set; }
        [ForeignKey("TermID")]
        public virtual Term Term { get; set; }

        [Required, Column("Data_Type_ID")]
        public EAVDataType DataTypeID { get; set; }
        [ForeignKey("DataTypeID")]
        public virtual DataType DataType { get; set; }

        public virtual ICollection<Value> Values { get; set; }

        public virtual ICollection<Value2> OtherValues { get; set; }

        public virtual ICollection<Unit> Units { get; set; }

        public Attribute()
        {
            Values = new List<Value>();
            OtherValues = new List<Value2>();
            Units = new List<Unit>();
        }

        [NotMapped]
        public ICollection<string> FixedValues { get; set; }
    }

    public class Term
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("Term_ID")]
        public int TermID { get; set; }

        [Required, MaxLength(512)]
        public string Name { get; set; }

        public virtual ICollection<Attribute> Attributes { get; set; }

        public Term()
        {
            Attributes = new List<Attribute>();
        }
    }

    public class Entity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("Entity_ID")]
        public int EntityID { get; set; }
        
        [Required, MaxLength(512)]
        public string Name { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; }

        public Entity()
        {
            Subjects = new List<Subject>();
        }
    }

    public class Subject
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("Subject_ID")]
        public int SubjectID { get; set; }

        [Required, Column("Member_ID"), MaxLength(128)]
        public string MemberID { get; set; }

        [Required, Column("Project_ID")]
        public int ProjectID { get; set; }
        [ForeignKey("ProjectID")]
        public virtual Project Project { get; set; }

        [Required, Column("Entity_ID")]
        public int EntityID { get; set; }
        [ForeignKey("EntityID")]
        public virtual Entity Entity { get; set; }

        public virtual ICollection<ContainerInstance> ContainerInstances { get; set; }

        public Subject()
        {
            ContainerInstances = new List<ContainerInstance>();
        }
    }

    public class Unit
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("Unit_ID")]
        public int UnitID { get; set; }

        [Required, MaxLength(128)]
        public string Name { get; set; }

        [Required, MaxLength(8)]
        public string Symbol { get; set; }

        public virtual ICollection<Attribute> Attributes { get; set; }

        public Unit()
        {
            Attributes = new List<Attribute>();
        }
    }

    public class ContainerInstance
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("Container_Instance_ID")]
        public int ContainerInstanceID { get; set; }

        [Required, Column("Repeat_Instance")]
        public int RepeatInstance { get; set; }

        [Required, Column("Container_ID")]
        public int ContainerID { get; set; }
        [ForeignKey("ContainerID")]
        public virtual Container Container { get; set; }

        [Required, Column("Subject_ID")]
        public int SubjectID { get; set; }
        [ForeignKey("SubjectID")]
        public virtual Subject Subject { get; set; }

        [Column("Parent_Container_Instance_ID")]
        public int? ParentContainerInstanceID { get; set; }
        [ForeignKey("ParentContainerInstanceID")]
        public virtual ContainerInstance ParentContainerInstance { get; set; }

        public virtual ICollection<ContainerInstance> ChildContainerInstances { get; set; }

        public virtual ICollection<Value> Values { get; set; }

        public virtual ICollection<Value2> OtherValues { get; set; }

        public ContainerInstance()
        {
            ChildContainerInstances = new List<ContainerInstance>();
            Values = new List<Value>();
            OtherValues = new List<Value2>();
        }
    }

    [Serializable]
    public class Value
    {
        [Required, Column("Raw_Value")]
        public string RawValue { get; set; }

        [MaxLength(8)]
        public string Units { get; set; }

        [Key, Column("Container_Instance_ID", Order = 0)]
        public int ContainerInstanceID { get; set; }
        [ForeignKey("ContainerInstanceID")]
        public virtual ContainerInstance ContainerInstance { get; set; }

        [Key, Column("Attribute_ID", Order = 1)]
        public int AttributeID { get; set; }
        [ForeignKey("AttributeID")]
        public virtual Attribute Attribute { get; set; }

        public Value()
        {
        }

        [NotMapped]
        public object ObjectValue
        {
            get
            {
                if (Attribute == null)
                    return (null);

                switch (Attribute.DataType.DataTypeID)
                {
                    case EAVDataType.Boolean: return(BooleanValue);
                    case EAVDataType.DateTime: return (DateTimeValue);
                    case EAVDataType.Float: return (FloatValue);
                    case EAVDataType.Integer: return (IntegerValue);
                    case EAVDataType.String: return (StringValue);
                    default: return (null);
                }
            }
        }

        [NotMapped]
        public Boolean? BooleanValue { get { Boolean val; return (Boolean.TryParse(RawValue, out val) ? (Boolean?)val : null); } }

        [NotMapped]
        public DateTime? DateTimeValue { get { DateTime val; return (DateTime.TryParse(RawValue, out val) ? (DateTime?)val : null); } }

        [NotMapped]
        public Single? FloatValue { get { Single val; return (Single.TryParse(RawValue, out val) ? (Single?)val : null); } }

        [NotMapped]
        public Int32? IntegerValue { get { Int32 val; return (Int32.TryParse(RawValue, out val) ? (Int32?)val : null); } }

        [NotMapped]
        public String StringValue { get { return (RawValue); } }
    }

    [Serializable]
    public class Value2
    {
        [Required, Column("Raw_Value")]
        public string RawValue { get; set; }

        [MaxLength(8)]
        public string Units { get; set; }

        [Key, Column("Container_Instance_ID", Order = 0)]
        public int ContainerInstanceID { get; set; }
        [ForeignKey("ContainerInstanceID")]
        public virtual ContainerInstance ContainerInstance { get; set; }

        [Key, Column("Attribute_ID", Order = 1)]
        public int AttributeID { get; set; }
        [ForeignKey("AttributeID")]
        public virtual Attribute Attribute { get; set; }

        public Value2()
        {
        }

        [Column("Boolean_Value")]
        public Boolean? BooleanValue { get; set; }

        [Column("DateTime_Value")]
        public DateTime? DateTimeValue { get; set; }

        [Column("Float_Value")]
        public Single? FloatValue { get; set; }

        [Column("Integer_Value")]
        public Int32? IntegerValue { get; set; }

        [NotMapped]
        public String StringValue { get { return (RawValue); } set { RawValue = value; } }

        [NotMapped]
        public object ObjectValue
        {
            get
            {
                if (Attribute == null)
                    return (null);

                switch (Attribute.DataType.DataTypeID)
                {
                    case EAVDataType.Boolean: return (BooleanValue);
                    case EAVDataType.DateTime: return (DateTimeValue);
                    case EAVDataType.Float: return (FloatValue);
                    case EAVDataType.Integer: return (IntegerValue);
                    case EAVDataType.String: return (StringValue);
                    default: return (null);
                }
            }
        }
    }

    public enum EAVDataType { Boolean = 1, DateTime = 2, Float = 3, Integer = 4, String = 5 }

    public class DataType
    {
        [Key, Column("Data_Type_ID")]
        public EAVDataType DataTypeID { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ICollection<Attribute> Attributes { get; set; }

        public DataType()
        {
            Attributes = new List<Attribute>();
        }
    }
}

// TODO: Eventually these need to be in their own class file
namespace NeoEAV.Objects
{
    public enum ContextControlType { Unknown, Project, Subject, Container, Instance, Attribute }

    [Flags]
    public enum ContextType { Unknown = 0, Data = 1, Metadata = 2 }

    // TODO: Move IDataItemContainer here?
    public interface IEAVContextControl
    {
        // TODO: Turn this into DataParent and MetadataParent
        IEAVContextControl ParentContextControl { get; }

        ContextControlType ContextControlType { get; }

        string ContextKey { get; set; }

        ContextType ContextType { get; }
        ContextType BindingType { get; }
    }

    public interface IEAVValueControl
    {
        string RawValue { get; set; }
    }
}