using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurcchase;
using Finantech.DTOs.Invoice;

namespace Finantech.Services.Interfaces
{
    public interface ICreditCardService
    {
        public Task<InfoCreditCardResponse> CreateCreditCardAsync(CreateCreditCardRequest request, int userId);
        public Task<InfoCreditCardResponse> UpdateCreditCardAsync(UpdateCreditCardRequest request, int userId);
        public Task<InfoCreditPurchaseResponse> CreateCreditPurchaseAsync(CreateCreditPurchaseRequest request, int userId);
        public Task<InfoCreditPurchaseResponse> UpdateCreditPurchaseAsync(UpdateCreditPurchaseResponse request, int userId);
        public Task DeleteCreditPurchaseAsync(int purchaseId, int userId);
        public Task<IEnumerable<InfoInvoiceResponse>> GetInvoicesWithPaginationAsync(int pageNumber, int pageSize, int userId, DateTime startDate, DateTime endDate, int? accountId);

    }
}
