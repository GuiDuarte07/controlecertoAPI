SELECT * FROM "Users"

-- Id = 11 test@test.com

SELECT * FROM "Accounts" WHERE "Accounts"."UserId" = (
	SELECT "Id" FROM "Users" WHERE "Users"."Email" = 'test@test.com'
)

-- Id = 4 nubank


SELECT * FROM "Expenses" WHERE "Expenses"."AccountId" = 4

SELECT * FROM "Incomes" WHERE "Incomes"."AccountId" = 4

SELECT * FROM "Categories" WHERE "Categories"."UserId" = 11

INSERT INTO "Categories"
VALUES (0, 'Teste', 'icon', 'Color', 0, now(), now(), 11)


SELECT * FROM "CreditCards";
SELECT * FROM "CreditExpenses";
SELECT * FROM "CreditPurchases";
SELECT * FROM "Invoices";
SELECT * FROM "InvoicePayments";

SELECT * FROM "CategoriesDefault"

INSERT INTO "CategoriesDefault" ("Name", "Icon", "Color", "BillType")
VALUES 
('Casa', 'home', '#00bfff', 0),
('Educação', 'import_contacts', '#ba55d3', 0),
('Eletrônicos', 'laptop_chromebook', '#ffef00', 0),
('Lazer', 'beach_access', '#ff9f00', 0),
('Outros', 'more_horiz', '#808080', 0),
('Presentes', 'featured_seasonal_and_gifts', '#9dc209', 0),
('Restaurante', 'restaurant', '#ae0c00', 0),
('Saúde', 'syringe', '#98fb98', 0),
('Serviços', 'work', '#228b22', 0),
('Supermercado', 'shopping_cart', '#e32636', 0),
('Transporte', 'local_taxi', '#00ced1', 0),
('Vestuário', 'checkroom', '#008b8b', 0),
('Viagem', 'travel', '#da70d6', 0),
('Investimento', 'trending_up', '#00cccc', 1),
('Outros', 'more_horiz', '#808080', 1),
('Presente', 'featured_seasonal_and_gifts', '#9dc209', 1),
('Prêmio', 'trophy', '#ffe135', 1),
('Salário', 'payments', '#8b0000', 1);

SELECT * FROM "Categories" WHERE "UserId" = 11;

SELECT "Icon" FROM "CategoriesDefault";