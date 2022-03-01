begin tran

alter table Product	add SplitAmount decimal(18,4) default 0 not null
go
alter table Product	add SplitAmount2 decimal(18,4) default 0 not null
go
alter table Product	add TaxSplit bit default 0 not null
go
alter table Product	add PaymentTypeId int default 0 not null
go
--alter table Product	add TaxCategory2Id int default 0 not null
--go

alter table ProductAttributeValue add SplitAmount decimal(18,4) default 0 not null
go
alter table ProductAttributeValue add SplitAmount2 decimal(18,4) default 0 not null
go
--select * from product --select * from ProductAttributeValue 
--commit