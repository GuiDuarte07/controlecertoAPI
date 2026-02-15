using ControleCerto.DTOs.Investment;
using ControleCerto.Errors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControleCerto.Services.Interfaces
{
    public interface IInvestmentService
    {
        Task<Result<InfoInvestmentResponse>> CreateInvestmentAsync(CreateInvestmentRequest request, int userId);
        Task<Result<InfoInvestmentResponse>> UpdateInvestmentAsync(UpdateInvestmentRequest request, int userId);
        Task<Result<InfoInvestmentResponse>> DepositAsync(DepositInvestmentRequest request, int userId);
        Task<Result<InfoInvestmentResponse>> WithdrawAsync(DepositInvestmentRequest request, int userId);
        Task<Result<InfoInvestmentResponse>> AdjustInvestmentAsync(AdjustInvestmentRequest request, int userId);
        Task<Result<bool>> DeleteInvestmentAsync(long investmentId, int userId);
        Task<Result<IEnumerable<InfoInvestmentResponse>>> GetInvestmentsAsync(int userId);
        Task<Result<InfoInvestmentResponse>> GetInvestmentAsync(long investmentId, int userId);
        Task<Result<IEnumerable<InvestmentHistoryResponse>>> GetInvestmentHistoryAsync(long investmentId, int userId);
    }
}
