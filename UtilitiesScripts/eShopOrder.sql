SELECT 
o.Id AS OrderId
,CustomerId
,a.Email
,a.FirstName + ' ' + a.LastName AS CustomerName
,BillingAddressId
,a.Address1 + ' '+ a.Address2 + ' ZipPostalCode: '+a.ZipPostalCode AS BilingAddress
,a.PhoneNumber
,CASE WHEN PickUpInStore = 0 THEN 'YES' ELSE 'NO' END AS PickupInStore
,CASE WHEN PaymentMethodSystemName = 'Payments.CheckMoneyOrder' THEN 'offline payment' ELSE 'Online Payment' END PaymentMethod
,OrderDiscount
,OrderTotal
,CustomOrderNumber
FROM [Order] o
INNER JOIN Customer c on o.CustomerId = c.Id
INNER JOIN [Address] a on a.Id = o.BillingAddressId
ORDER By o.Id
