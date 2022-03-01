SELECT 
OrderId
,ProductId
,p.[Name] AS ProductName
,Quantity
,UnitPriceInclTax
,UnitPriceExclTax
,PriceInclTax
,PriceExclTax
,DiscountAmountInclTax
,DiscountAmountExclTax
,OriginalProductCost
FROM OrderItem oi
INNER JOIN Product p on oi.ProductId = p.Id
ORDER By OrderId