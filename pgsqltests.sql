SELECT * FROM "Users"

SELECT * FROM "Accounts" WHERE "Accounts"."UserId" = (
	SELECT "Id" FROM "Users" WHERE "Users"."Email" = 'test@test.com'
)