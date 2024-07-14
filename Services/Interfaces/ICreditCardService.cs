using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurchase;
using Finantech.DTOs.Invoice;
using Finantech.Models.DTOs;

namespace Finantech.Services.Interfaces
{
    public interface ICreditCardService
    {
        public Task<InfoCreditCardResponse> CreateCreditCardAsync(CreateCreditCardRequest request, int userId);

        public Task<InfoCreditCardResponse[]> GetCreditCardInfo(int userId);
        public Task<InfoCreditCardResponse> UpdateCreditCardAsync(UpdateCreditCardRequest request, int userId);
        public Task<InfoCreditPurchaseResponse> CreateCreditPurchaseAsync(CreateCreditPurchaseRequest request, int userId);
        public Task<InfoCreditPurchaseResponse> UpdateCreditPurchaseAsync(UpdateCreditPurchaseResponse request, int userId);
        public Task DeleteCreditPurchaseAsync(long purchaseId, int userId);
        public Task<IEnumerable<InfoInvoiceResponse>> GetInvoicesByDateAsync(int userId, DateTime startDate, DateTime endDate, long? creditCardId);
        public Task<InvoicePageResponse> GetInvoicesByIdAsync(long invoiceId,  int userId);
        public Task<InfoInvoicePaymentResponse> PayInvoiceAsync(CreteInvoicePaymentRequest invoicePaymentRequest, int userId);
        public Task<InfoTransactionResponse[]> GetCreditExpensesFromInvoice(int invoiceId, int userId);

    }
}
