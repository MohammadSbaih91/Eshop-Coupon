begin tran
alter table Address
	add CivilityId int default 0 not null
go
alter table Address
	add NationalityId int default 0 not null
go
alter table Address
	add NationalityTypeId int default 0 not null
go

alter table Address
	add IdentityCardOrPassport nvarchar(40)
go
--commit