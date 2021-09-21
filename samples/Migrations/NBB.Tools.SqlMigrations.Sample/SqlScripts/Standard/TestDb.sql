if not exists(select top 1 1 from sysobjects where xtype='U' and name='Test')
create table Test(Id int, Name nvarchar(255))

GO