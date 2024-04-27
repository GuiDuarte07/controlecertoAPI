SELECT * FROM "Users"

-- Id = 11 test@test.com

SELECT * FROM "Accounts" WHERE "Accounts"."UserId" = (
	SELECT "Id" FROM "Users" WHERE "Users"."Email" = 'test@test.com'
)

-- Id = 4 nubank


SELECT * FROM "Expenses" WHERE "Expenses"."AccountId" = 4

SELECT * FROM "Incomes" WHERE "Incomes"."AccountId" = 4

INSERT INTO "Categories"
VALUES (0, 'Teste', 'icon', 'Color', 0, now(), now(), 11)