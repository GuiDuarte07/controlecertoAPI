using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Validations
{
    public static class BusinessValidations
    {
        public static Result<bool> ValidateAccountBalance(Account account, double amount, TransactionTypeEnum type)
        {
            if (type == TransactionTypeEnum.EXPENSE && account.Balance < amount)
            {
                return new AppError("Saldo insuficiente para esta operação.", ErrorTypeEnum.BusinessRule);
            }
            
            return true;
        }

        public static async Task<Result<bool>> ValidateCategoryTypeAsync(long categoryId, TransactionTypeEnum transactionType, AppDbContext context)
        {
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            
            if (category is null)
            {
                return new AppError("Categoria não encontrada.", ErrorTypeEnum.NotFound);
            }

            if ((category.BillType == BillTypeEnum.EXPENSE && transactionType != TransactionTypeEnum.EXPENSE) ||
                (category.BillType == BillTypeEnum.INCOME && transactionType != TransactionTypeEnum.INCOME))
            {
                return new AppError("Categoria não é compatível com o tipo de transação.", ErrorTypeEnum.BusinessRule);
            }

            return true;
        }

        public static Result<bool> ValidateTransferAmount(Account originAccount, double amount)
        {
            if (originAccount.Balance < Math.Round(amount, 2))
            {
                return new AppError("Conta origem não possui saldo suficiente para essa transferência.", ErrorTypeEnum.BusinessRule);
            }

            return true;
        }

        public static Result<bool> ValidateTransactionOwnership(Transaction transaction, int userId)
        {
            if (transaction.Account?.UserId != userId)
            {
                return new AppError("Transação não encontrada ou não pertence ao usuário.", ErrorTypeEnum.NotFound);
            }

            return true;
        }
    }
}
