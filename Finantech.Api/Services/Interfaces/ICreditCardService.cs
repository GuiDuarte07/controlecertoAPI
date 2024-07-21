using Finantech.DTOs.CreditCard;
using Finantech.DTOs.CreditPurchase;
using Finantech.DTOs.Invoice;
using Finantech.Errors;
using Finantech.Models.DTOs;

namespace Finantech.Services.Interfaces
{
    public interface ICreditCardService
    {
        public Task<Result<InfoCreditCardResponse>> CreateCreditCardAsync(CreateCreditCardRequest request, int userId);

        public Task<Result<InfoCreditCardResponse[]>> GetCreditCardInfo(int userId);
        public Task<Result<InfoCreditCardResponse>> UpdateCreditCardAsync(UpdateCreditCardRequest request, int userId);
        public Task<Result<InfoCreditPurchaseResponse>> CreateCreditPurchaseAsync(CreateCreditPurchaseRequest request, int userId);
        public Task<Result<InfoCreditPurchaseResponse>> UpdateCreditPurchaseAsync(UpdateCreditPurchaseRequest request, int userId);
        public Task<Result<bool>> DeleteCreditPurchaseAsync(long purchaseId, int userId);
        public Task<Result<IEnumerable<InfoInvoiceResponse>>> GetInvoicesByDateAsync(int userId, DateTime startDate, DateTime endDate, long? creditCardId);
        public Task<Result<InvoicePageResponse>> GetInvoicesByIdAsync(long invoiceId,  int userId);
        public Task<Result<InfoInvoicePaymentResponse>> PayInvoiceAsync(CreteInvoicePaymentRequest invoicePaymentRequest, int userId);
        public Task<Result<InfoTransactionResponse[]>> GetCreditExpensesFromInvoice(int invoiceId, int userId);

    }
}
