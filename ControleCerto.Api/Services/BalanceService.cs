using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Services
{
    public interface IBalanceService
    {
        Result<bool> UpdateAccountBalance(Account account, double amount, TransactionTypeEnum type, bool isReversal = false);
        Result<bool> ProcessTransferBalance(Account originAccount, Account destinationAccount, double amount);
    }

    public class BalanceService : IBalanceService
    {
        private readonly AppDbContext _context;

        public BalanceService(AppDbContext context)
        {
            _context = context;
        }

        public Result<bool> UpdateAccountBalance(Account account, double amount, TransactionTypeEnum type, bool isReversal = false)
        {
            var multiplier = isReversal ? -1 : 1;
            
            switch (type)
            {
                case TransactionTypeEnum.INCOME:
                    account.Balance += (amount * multiplier);
                    break;
                case TransactionTypeEnum.EXPENSE:
                    account.Balance -= (amount * multiplier);
                    break;
                default:
                    return new AppError("Tipo de transação não suportado para atualização de saldo.", ErrorTypeEnum.BusinessRule);
            }

            _context.Update(account);
            
            return true;
        }

        public Result<bool> ProcessTransferBalance(Account originAccount, Account destinationAccount, double amount)
        {
            var roundedAmount = Math.Round(amount, 2);
            
            originAccount.Balance = Math.Round(originAccount.Balance, 2) - roundedAmount;
            destinationAccount.Balance = Math.Round(destinationAccount.Balance, 2) + roundedAmount;

            _context.Update(originAccount);
            _context.Update(destinationAccount);
            
            return true;
        }
    }
}
