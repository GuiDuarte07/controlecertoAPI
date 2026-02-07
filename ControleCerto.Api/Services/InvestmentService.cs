using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ControleCerto.DTOs.Investment;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using ControleCerto.Enums;
using ControleCerto.Validations;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Services
{
    public class InvestmentService : IInvestmentService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IBalanceService _balanceService;

        public InvestmentService(AppDbContext appDbContext, IMapper mapper, IBalanceService balanceService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _balanceService = balanceService;
        }

        public async Task<Result<InfoInvestmentResponse>> CreateInvestmentAsync(CreateInvestmentRequest request, int userId)
        {
            var entity = _mapper.Map<Investment>(request);
            entity.UserId = userId;
            entity.CurrentValue = Math.Round(request.InitialAmount ?? 0, 2);

            await _appDbContext.Investments.AddAsync(entity);
            if (request.InitialAmount.HasValue && request.InitialAmount.Value != 0)
            {
                var hist = new InvestmentHistory
                {
                    Investment = entity,
                    ChangeAmount = request.InitialAmount.Value,
                    TotalValue = entity.CurrentValue,
                    Type = InvestmentHistoryTypeEnum.INVEST,
                    OccurredAt = request.StartDate ?? entity.StartDate
                };
                await _appDbContext.AddAsync(hist);
            }

            await _appDbContext.SaveChangesAsync();

            var dto = _mapper.Map<InfoInvestmentResponse>(entity);
            return dto;
        }

        public async Task<Result<InfoInvestmentResponse>> UpdateInvestmentAsync(UpdateInvestmentRequest request, int userId)
        {
            var investment = await _appDbContext.Investments.FirstOrDefaultAsync(i => i.Id == request.Id);
            if (investment is null || investment.UserId != userId)
            {
                return new AppError("Investment not found.", ErrorTypeEnum.NotFound);
            }

            if (request.Name is not null) investment.Name = request.Name;
            if (request.StartDate.HasValue) investment.StartDate = request.StartDate.Value;
            if (request.Description is not null) investment.Description = request.Description;
            investment.UpdatedAt = DateTime.UtcNow;

            _appDbContext.Investments.Update(investment);
            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoInvestmentResponse>(investment);
        }

        public async Task<Result<InfoInvestmentResponse>> DepositAsync(DepositInvestmentRequest request, int userId)
        {
            var investment = await _appDbContext.Investments.FirstOrDefaultAsync(i => i.Id == request.InvestmentId);
            if (investment is null || investment.UserId != userId)
                return new AppError("Investment not found.", ErrorTypeEnum.NotFound);

            Account? account = null;
            if (request.AccountId.HasValue)
            {
                account = await _appDbContext.Accounts
                    .FirstOrDefaultAsync(a => a.Id == request.AccountId.Value && a.UserId == userId && a.Deleted == false);

                if (account is null)
                    return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);

                var balanceValidation = BusinessValidations.ValidateAccountBalance(account, request.Amount, TransactionTypeEnum.EXPENSE);
                if (balanceValidation.IsError)
                    return balanceValidation.Error;
            }

            using (var tx = _appDbContext.Database.BeginTransaction())
            {
                if (account is not null)
                {
                    var balanceResult = _balanceService.UpdateAccountBalance(account, request.Amount, TransactionTypeEnum.EXPENSE);
                    if (balanceResult.IsError)
                        return balanceResult.Error;
                }

                investment.CurrentValue = Math.Round(investment.CurrentValue + request.Amount, 2);
                investment.UpdatedAt = DateTime.UtcNow;

                var hist = new InvestmentHistory
                {
                    InvestmentId = investment.Id,
                    ChangeAmount = Math.Round(request.Amount, 2),
                    TotalValue = investment.CurrentValue,
                    SourceAccountId = request.AccountId,
                    Type = InvestmentHistoryTypeEnum.INVEST,
                    OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
                    Note = request.Note
                };

                _appDbContext.Investments.Update(investment);
                await _appDbContext.InvestmentHistories.AddAsync(hist);
                await _appDbContext.SaveChangesAsync();
                await tx.CommitAsync();
            }

            var investmentWithHistory = await GetInvestmentWithHistoryAsync(investment.Id);
            return _mapper.Map<InfoInvestmentResponse>(investmentWithHistory);
        }

        public async Task<Result<InfoInvestmentResponse>> WithdrawAsync(DepositInvestmentRequest request, int userId)
        {
            var investment = await _appDbContext.Investments.FirstOrDefaultAsync(i => i.Id == request.InvestmentId);
            if (investment is null || investment.UserId != userId)
                return new AppError("Investment not found.", ErrorTypeEnum.NotFound);

            if (investment.CurrentValue < request.Amount)
                return new AppError("Saldo do investimento insuficiente.", ErrorTypeEnum.BusinessRule);

            Account? account = null;
            if (request.AccountId.HasValue)
            {
                account = await _appDbContext.Accounts
                    .FirstOrDefaultAsync(a => a.Id == request.AccountId.Value && a.UserId == userId && a.Deleted == false);

                if (account is null)
                    return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
            }

            using (var tx = _appDbContext.Database.BeginTransaction())
            {
                if (account is not null)
                {
                    var balanceResult = _balanceService.UpdateAccountBalance(account, request.Amount, TransactionTypeEnum.INCOME);
                    if (balanceResult.IsError)
                        return balanceResult.Error;
                }

                investment.CurrentValue = Math.Round(investment.CurrentValue - request.Amount, 2);
                investment.UpdatedAt = DateTime.UtcNow;

                var hist = new InvestmentHistory
                {
                    InvestmentId = investment.Id,
                    ChangeAmount = Math.Round(-request.Amount, 2),
                    TotalValue = investment.CurrentValue,
                    SourceAccountId = request.AccountId,
                    Type = InvestmentHistoryTypeEnum.WITHDRAW,
                    OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
                    Note = request.Note
                };

                _appDbContext.Investments.Update(investment);
                await _appDbContext.InvestmentHistories.AddAsync(hist);
                await _appDbContext.SaveChangesAsync();
                await tx.CommitAsync();
            }

            var investmentWithHistory = await GetInvestmentWithHistoryAsync(investment.Id);
            return _mapper.Map<InfoInvestmentResponse>(investmentWithHistory);
        }

        public async Task<Result<InfoInvestmentResponse>> AdjustInvestmentAsync(AdjustInvestmentRequest request, int userId)
        {
            var investment = await _appDbContext.Investments.Include(i => i.Histories).FirstOrDefaultAsync(i => i.Id == request.InvestmentId);
            if (investment is null || investment.UserId != userId)
                return new AppError("Investimento não encontrado.", ErrorTypeEnum.NotFound);

            using (var tx = _appDbContext.Database.BeginTransaction())
            {
                var change = Math.Round(request.NewTotalValue - investment.CurrentValue, 2);
                investment.CurrentValue = Math.Round(request.NewTotalValue, 2);
                investment.UpdatedAt = DateTime.UtcNow;

                var hist = new InvestmentHistory
                {
                    InvestmentId = investment.Id,
                    ChangeAmount = change,
                    TotalValue = investment.CurrentValue,
                    Type = InvestmentHistoryTypeEnum.ADJUSTMENT,
                    OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
                    Note = request.Note
                };

                _appDbContext.Investments.Update(investment);
                await _appDbContext.InvestmentHistories.AddAsync(hist);
                await _appDbContext.SaveChangesAsync();
                await tx.CommitAsync();
            }

            var investmentWithHistory = await GetInvestmentWithHistoryAsync(investment.Id);
            return _mapper.Map<InfoInvestmentResponse>(investmentWithHistory);
        }

        private async Task<Investment?> GetInvestmentWithHistoryAsync(long investmentId)
        {
            var investment = await _appDbContext.Investments
                .Include(i => i.Histories)
                .FirstOrDefaultAsync(i => i.Id == investmentId);
            
            if (investment?.Histories != null)
            {
                investment.Histories = investment.Histories.OrderByDescending(h => h.OccurredAt).ToList();
            }
            
            return investment;
        }

        public async Task<Result<IEnumerable<InfoInvestmentResponse>>> GetInvestmentsAsync(int userId)
        {
            var list = await _appDbContext.Investments
                .Where(i => i.UserId == userId)
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<InfoInvestmentResponse>>(list);
            return new Result<IEnumerable<InfoInvestmentResponse>>(dto);
        }

        public async Task<Result<InfoInvestmentResponse>> GetInvestmentAsync(long investmentId, int userId)
        {
            var investment = await GetInvestmentWithHistoryAsync(investmentId);
            if (investment is null || investment.UserId != userId)
                return new AppError("Investimento não encontrado.", ErrorTypeEnum.NotFound);

            var dto = _mapper.Map<InfoInvestmentResponse>(investment);
            return dto;
        }

        public async Task<Result<IEnumerable<InvestmentHistoryResponse>>> GetInvestmentHistoryAsync(long investmentId, int userId)
        {
            var investment = await _appDbContext.Investments.FirstOrDefaultAsync(i => i.Id == investmentId && i.UserId == userId);
            if (investment is null)
                return new AppError("Investment not found.", ErrorTypeEnum.NotFound);

            var histories = await _appDbContext.InvestmentHistories
                .Include(h => h.SourceAccount)
                .Where(h => h.InvestmentId == investmentId)
                .OrderBy(h => h.OccurredAt)
                .ToListAsync();

            var dto = _mapper.Map<IEnumerable<InvestmentHistoryResponse>>(histories);
            return new Result<IEnumerable<InvestmentHistoryResponse>>(dto);
        }
    }
}
