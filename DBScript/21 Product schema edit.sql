alter table Product
	add ValidateLocationBasedService bit default 0
go

update  Product set ValidateLocationBasedService = 0 where 1=1