-- Add fields
ALTER TABLE ShoppingCartItem ADD CustomProductTypeId INT DEFAULT 0 NOT NULL
GO

ALTER TABLE OrderItem ADD CustomProductTypeId INT DEFAULT 0 NOT NULL
GO

ALTER TABLE Product ADD CustomProductTypeId INT DEFAULT 0 NOT NULL
GO