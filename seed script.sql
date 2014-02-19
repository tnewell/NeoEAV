
set nocount on

delete from [Values]
delete from [ContainerInstances] where Parent_Container_Instance_ID is not null
delete from [ContainerInstances]
dbcc checkident('ContainerInstances', reseed, 0) with no_infomsgs
delete from [Subjects]
dbcc checkident('Subjects', reseed, 0) with no_infomsgs
delete from [Attributes]
dbcc checkident('Attributes', reseed, 0) with no_infomsgs
delete from [Containers] where Parent_Container_ID is not null
delete from [Containers]
dbcc checkident('Containers', reseed, 0) with no_infomsgs
delete from [Projects]
dbcc checkident('Projects', reseed, 0) with no_infomsgs
delete from [Terms]
dbcc checkident('Terms', reseed, 0) with no_infomsgs
delete from [Entities]
dbcc checkident('Entities', reseed, 0) with no_infomsgs


declare @i int
declare @pid int
declare @eid int
declare @iid int
declare @str nvarchar(128)


-- 3 projects
set @i = 0
while @i < 3
begin
	insert into Projects (Name, Description) values ('Test Project ' + cast(@i + 1 as nvarchar), 'Test Project ' + cast(@i + 1 as nvarchar))
	set @i = @i + 1
end

-- 3 subjects per project
-- 9 entities
set @i = 0
while @i < 3
begin
	select @pid = project_id from projects where name = 'Test Project ' + cast(@i + 1 as nvarchar)

	set @str = cast(@i * 3 + 1 as nvarchar)
	insert into Entities (Name) values ('Test Entity ' + @str)
	insert into subjects (member_id, project_id, Entity_ID) values ('Test Member ' + @str, @pid, SCOPE_IDENTITY())

	set @str = cast(@i * 3 + 2 as nvarchar)
	insert into Entities (Name) values ('Test Entity ' + @str)
	insert into subjects (member_id, project_id, Entity_ID) values ('Test Member ' + @str, @pid, SCOPE_IDENTITY())

	set @str = cast(@i * 3 + 3 as nvarchar)
	insert into Entities (Name) values ('Test Entity ' + @str)
	insert into subjects (member_id, project_id, Entity_ID) values ('Test Member ' + @str, @pid, SCOPE_IDENTITY())

	set @i = @i + 1
end

-- 3 forms (singleton, running, complicated recurring)
select @pid = project_id from projects where name = 'Test Project 1'

-- One-Shot
insert into Containers (Project_ID, Parent_Container_ID, Name, DisplayName, Is_Repeating, Has_Fixed_Instances, Sequence)
values (@pid, null, 'Test Root Container 1', 'One-Shot Form', 0, 0, 1)
select @eid = SCOPE_IDENTITY()

set @i = 1
while @i <= 5
begin
	insert into Terms (Name) values ('Test Term ' + cast(@i as nvarchar))

	insert into Attributes (Container_ID, Term_ID, Name, Display_Name, Data_Type_ID, Has_Variable_Units, Has_Fixed_Values, Sequence)
	values (@eid, SCOPE_IDENTITY(), 'Test Attribute ' + cast(@i as nvarchar), 'Attribute ' + cast(@i as nvarchar), @i, 0, 0, @i)	
	set @i = @i + 1
end

-- Running
insert into Containers (Project_ID, Parent_Container_ID, Name, DisplayName, Is_Repeating, Has_Fixed_Instances, Sequence)
values (@pid, null, 'Test Root Container 2', 'Running Form', 1, 0, 2)
select @eid = SCOPE_IDENTITY()

set @i = 1
while @i <= 5
begin
	insert into Terms (Name) values ('Test Term ' + cast(@i + 5 as nvarchar))
	insert into Attributes (Container_ID, Term_ID, Name, Display_Name, Data_Type_ID, Has_Variable_Units, Has_Fixed_Values, Sequence)
	values (@eid, SCOPE_IDENTITY(), 'Test Attribute ' + cast(@i + 5 as nvarchar), 'Attribute ' + cast(@i + 5 as nvarchar), @i, 0, 0, @i)
	set @i = @i + 1
end

-- Complex Recurring
insert into Containers (Project_ID, Parent_Container_ID, Name, DisplayName, Is_Repeating, Has_Fixed_Instances, Sequence)
values (@pid, null, 'Test Root Container 3', 'Complex Form', 0, 0, 3)
select @eid = SCOPE_IDENTITY()

set @i = 1
while @i <= 5
begin
	insert into Terms (Name) values ('Test Term ' + cast(@i + 10 as nvarchar))
	insert into Attributes (Container_ID, Term_ID, Name, Display_Name, Data_Type_ID, Has_Variable_Units, Has_Fixed_Values, Sequence)
	values (@eid, SCOPE_IDENTITY(), 'Test Attribute ' + cast(@i + 10 as nvarchar), 'Attribute ' + cast(@i + 10 as nvarchar), @i, 0, 0, @i)
	set @i = @i + 1
end

insert into Containers (Project_ID, Parent_Container_ID, Name, DisplayName, Is_Repeating, Has_Fixed_Instances, Sequence)
values (@pid, @eid, 'Test Child Container 1', 'Non-Running Child', 0, 0, 1)

set @i = 1
while @i <= 5
begin
	insert into Terms (Name) values ('Test Term ' + cast(@i + 15 as nvarchar))
	insert into Attributes (Container_ID, Term_ID, Name, Display_Name, Data_Type_ID, Has_Variable_Units, Has_Fixed_Values, Sequence)
	values (@eid, SCOPE_IDENTITY(), 'Test Attribute ' + cast(@i + 15 as nvarchar), 'Attribute ' + cast(@i + 15 as nvarchar), @i, 0, 0, @i)
	set @i = @i + 1
end

insert into Containers (Project_ID, Parent_Container_ID, Name, DisplayName, Is_Repeating, Has_Fixed_Instances, Sequence)
values (@pid, @eid, 'Test Child Container 2', 'Running Child', 1, 0, 2)

set @i = 1
while @i <= 5
begin
	insert into Terms (Name) values ('Test Term ' + cast(@i + 20 as nvarchar))
	insert into Attributes (Container_ID, Term_ID, Name, Display_Name, Data_Type_ID, Has_Variable_Units, Has_Fixed_Values, Sequence)
	values (@eid, SCOPE_IDENTITY(), 'Test Attribute ' + cast(@i + 20 as nvarchar), 'Attribute ' + cast(@i + 20 as nvarchar), @i, 0, 0, @i)
	set @i = @i + 1
end






