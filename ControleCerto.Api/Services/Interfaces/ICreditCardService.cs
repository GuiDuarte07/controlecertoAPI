using ControleCerto.DTOs.CreditCard;
using ControleCerto.DTOs.CreditPurchase;
using ControleCerto.DTOs.Invoice;
using ControleCerto.Errors;
using ControleCerto.Models.DTOs;

namespace ControleCerto.Services.Interfaces
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
        public Task<Result<InfoTransactionResponse[]>> GetCreditExpensesFromInvoice(int invoiceId, int userId);
        public Task<Result<InfoInvoicePaymentResponse>> PayInvoiceAsync(CreteInvoicePaymentRequest invoicePaymentRequest, int userId);
        public Task<Result<bool>> DeleteInvoicePaymentAsync(
            long invoicePaymentId, int userId);

    }
}
