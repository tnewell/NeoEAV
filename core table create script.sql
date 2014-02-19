
-- Terms

CREATE TABLE [dbo].[Terms] (
    [Term_ID] INT            IDENTITY (1, 1) NOT NULL,
    [Name]    NVARCHAR (512) NOT NULL,
    CONSTRAINT [PK_dbo.Terms] PRIMARY KEY CLUSTERED ([Term_ID] ASC)
);

-- Entities

CREATE TABLE [dbo].[Entities] (
    [Entity_ID] INT            IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (512) NOT NULL,
    CONSTRAINT [PK_dbo.Entities] PRIMARY KEY CLUSTERED ([Entity_ID] ASC)
);

CREATE TABLE [dbo].[DataTypes] (
    [Data_Type_ID] INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.DataTypes] PRIMARY KEY CLUSTERED ([Data_Type_ID] ASC)
);

-- DataTypes

CREATE TABLE [dbo].[DataTypes] (
    [Data_Type_ID] INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.DataTypes] PRIMARY KEY CLUSTERED ([Data_Type_ID] ASC)
);

-- Projects

CREATE TABLE [dbo].[Projects] (
    [Project_ID]  INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (512) NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Data_Name]   NVARCHAR (64)  DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_dbo.Projects] PRIMARY KEY CLUSTERED ([Project_ID] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Projects_Name]
    ON [dbo].[Projects]([Name] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Projects_Data_Name]
    ON [dbo].[Projects]([Data_Name] ASC);


-- Containers

CREATE TABLE [dbo].[Containers] (
    [Container_ID]        INT            IDENTITY (1, 1) NOT NULL,
    [Name]                NVARCHAR (512) NOT NULL,
    [Display_Name]        NVARCHAR (512) NOT NULL,
    [Sequence]            INT            NOT NULL,
    [Is_Repeating]        BIT            NOT NULL,
    [Project_ID]          INT            NOT NULL,
    [Parent_Container_ID] INT            NULL,
    [Data_Name]           NVARCHAR (64)  NOT NULL,
    CONSTRAINT [PK_dbo.Containers] PRIMARY KEY CLUSTERED ([Container_ID] ASC),
    CONSTRAINT [FK_dbo.Containers_dbo.Containers_Parent_Container_ID] FOREIGN KEY ([Parent_Container_ID]) REFERENCES [dbo].[Containers] ([Container_ID]),
    CONSTRAINT [FK_dbo.Containers_dbo.Projects_Project_ID] FOREIGN KEY ([Project_ID]) REFERENCES [dbo].[Projects] ([Project_ID])
);

GO
CREATE NONCLUSTERED INDEX [IX_Parent_Container_ID]
    ON [dbo].[Containers]([Parent_Container_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Project_ID]
    ON [dbo].[Containers]([Project_ID] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Containers_Name_Project_ID]
    ON [dbo].[Containers]([Name] ASC, [Project_ID] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Containers_Data_Name_Project_ID]
    ON [dbo].[Containers]([Data_Name] ASC, [Project_ID] ASC);

-- Attributes

CREATE TABLE [dbo].[Attributes] (
    [Attribute_ID]       INT            IDENTITY (1, 1) NOT NULL,
    [Name]               NVARCHAR (512) NOT NULL,
    [Display_Name]       NVARCHAR (512) NOT NULL,
    [Sequence]           INT            NOT NULL,
    [Container_ID]       INT            NOT NULL,
    [Term_ID]            INT            NOT NULL,
    [Data_Type_ID]       INT            DEFAULT ((0)) NOT NULL,
    [Has_Variable_Units] BIT            DEFAULT ((0)) NOT NULL,
    [Data_Name]          NVARCHAR (64)  NOT NULL,
    CONSTRAINT [PK_dbo.Attributes] PRIMARY KEY CLUSTERED ([Attribute_ID] ASC),
    CONSTRAINT [FK_dbo.Attributes_dbo.Containers_Container_ID] FOREIGN KEY ([Container_ID]) REFERENCES [dbo].[Containers] ([Container_ID]),
    CONSTRAINT [FK_dbo.Attributes_dbo.Terms_Term_ID] FOREIGN KEY ([Term_ID]) REFERENCES [dbo].[Terms] ([Term_ID]),
    CONSTRAINT [FK_dbo.Attributes_dbo.DataTypes_Data_Type_ID] FOREIGN KEY ([Data_Type_ID]) REFERENCES [dbo].[DataTypes] ([Data_Type_ID])
);

GO
CREATE NONCLUSTERED INDEX [IX_Container_ID]
    ON [dbo].[Attributes]([Container_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Term_ID]
    ON [dbo].[Attributes]([Term_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Data_Type_ID]
    ON [dbo].[Attributes]([Data_Type_ID] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Attributes_Name_Container_ID]
    ON [dbo].[Attributes]([Name] ASC, [Container_ID] ASC);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Attributes_Data_Name_Container_ID]
    ON [dbo].[Attributes]([Data_Name] ASC, [Container_ID] ASC);

-- Subject

CREATE TABLE [dbo].[Subjects] (
    [Subject_ID] INT            IDENTITY (1, 1) NOT NULL,
    [Member_ID]  NVARCHAR (128) NOT NULL,
    [Project_ID] INT            NOT NULL,
    [Entity_ID]  INT            NOT NULL,
    CONSTRAINT [PK_dbo.Subjects] PRIMARY KEY CLUSTERED ([Subject_ID] ASC),
    CONSTRAINT [FK_dbo.Subjects_dbo.Projects_Project_ID] FOREIGN KEY ([Project_ID]) REFERENCES [dbo].[Projects] ([Project_ID]),
    CONSTRAINT [FK_dbo.Subjects_dbo.Entities_Entity_ID] FOREIGN KEY ([Entity_ID]) REFERENCES [dbo].[Entities] ([Entity_ID])
);

GO
CREATE NONCLUSTERED INDEX [IX_Project_ProjectID]
    ON [dbo].[Subjects]([Project_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Entity_EntityID]
    ON [dbo].[Subjects]([Entity_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Project_ID]
    ON [dbo].[Subjects]([Project_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Entity_ID]
    ON [dbo].[Subjects]([Entity_ID] ASC);

-- Instances

CREATE TABLE [dbo].[ContainerInstances] (
    [Container_Instance_ID]        INT IDENTITY (1, 1) NOT NULL,
    [Repeat_Instance]              INT NOT NULL,
    [Container_ID]                 INT NOT NULL,
    [Subject_ID]                   INT NOT NULL,
    [Parent_Container_Instance_ID] INT NULL,
    CONSTRAINT [PK_dbo.ContainerInstances] PRIMARY KEY CLUSTERED ([Container_Instance_ID] ASC),
    CONSTRAINT [FK_dbo.ContainerInstances_dbo.Containers_Container_ID] FOREIGN KEY ([Container_ID]) REFERENCES [dbo].[Containers] ([Container_ID]),
    CONSTRAINT [FK_dbo.ContainerInstances_dbo.Subjects_Subject_ID] FOREIGN KEY ([Subject_ID]) REFERENCES [dbo].[Subjects] ([Subject_ID]),
    CONSTRAINT [FK_dbo.ContainerInstances_dbo.ContainerInstances_Parent_Container_Instance_ID] FOREIGN KEY ([Parent_Container_Instance_ID]) REFERENCES [dbo].[ContainerInstances] ([Container_Instance_ID])
);

GO
CREATE NONCLUSTERED INDEX [IX_Container_ID]
    ON [dbo].[ContainerInstances]([Container_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Subject_ID]
    ON [dbo].[ContainerInstances]([Subject_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Parent_Container_Instance_ID]
    ON [dbo].[ContainerInstances]([Parent_Container_Instance_ID] ASC);

-- Values

CREATE TABLE [dbo].[Values] (
    [Container_Instance_ID] INT            NOT NULL,
    [Attribute_ID]          INT            NOT NULL,
    [Raw_Value]             NVARCHAR (MAX) NOT NULL,
    [Units]                 NVARCHAR (8)   NULL,
    CONSTRAINT [PK_dbo.Values] PRIMARY KEY CLUSTERED ([Container_Instance_ID] ASC, [Attribute_ID] ASC),
    CONSTRAINT [FK_dbo.Values_dbo.ContainerInstances_Container_Instance_ID] FOREIGN KEY ([Container_Instance_ID]) REFERENCES [dbo].[ContainerInstances] ([Container_Instance_ID]),
    CONSTRAINT [FK_dbo.Values_dbo.Attributes_Attribute_ID] FOREIGN KEY ([Attribute_ID]) REFERENCES [dbo].[Attributes] ([Attribute_ID])
);

GO
CREATE NONCLUSTERED INDEX [IX_Container_Instance_ID]
    ON [dbo].[Values]([Container_Instance_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Attribute_ID]
    ON [dbo].[Values]([Attribute_ID] ASC);

-- Units

CREATE TABLE [dbo].[Units] (
    [Unit_ID] INT            IDENTITY (1, 1) NOT NULL,
    [Name]    NVARCHAR (128) NOT NULL,
    [Symbol]  NVARCHAR (8)   NOT NULL,
    CONSTRAINT [PK_dbo.Units] PRIMARY KEY CLUSTERED ([Unit_ID] ASC)
);

-- AttributeUnits

CREATE TABLE [dbo].[AttributeUnits] (
    [Attribute_ID] INT NOT NULL,
    [Unit_ID]      INT NOT NULL,
    CONSTRAINT [PK_dbo.AttributeUnits] PRIMARY KEY CLUSTERED ([Attribute_ID] ASC, [Unit_ID] ASC),
    CONSTRAINT [FK_dbo.AttributeUnits_dbo.Attributes_Attribute_AttributeID] FOREIGN KEY ([Attribute_ID]) REFERENCES [dbo].[Attributes] ([Attribute_ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.AttributeUnits_dbo.Units_Unit_UnitID] FOREIGN KEY ([Unit_ID]) REFERENCES [dbo].[Units] ([Unit_ID]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_Attribute_AttributeID]
    ON [dbo].[AttributeUnits]([Attribute_ID] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Unit_UnitID]
    ON [dbo].[AttributeUnits]([Unit_ID] ASC);






