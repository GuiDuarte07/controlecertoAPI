using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurcchase;

namespace Finantech.Services.Interfaces
{
    public interface ICreditCardService
    {
        public Task<InfoCreditCardResponse> CreateCreditCardAsync(CreateCreditCardRequest request, int userId);
        public Task<InfoCreditCardResponse> UpdateCreditCardAsync(UpdateCreditCardRequest request, int userId);
        public Task<InfoCreditPurchaseResponse> CreateCreditPurchaseAsync(CreateCreditPurchaseRequest request, int userId);
        public Task<InfoCreditPurchaseResponse> UpdateCreditPurchaseAsync(UpdateCreditPurchaseResponse request, int userId);
        public Task DeleteCreditPurchaseAsync(int purchaseId, int userId);

    }
}
