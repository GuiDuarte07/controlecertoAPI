using System.ComponentModel.DataAnnotations;

namespace ControleCerto.Modules.Mcp.DTOs
{
    public class DeleteTransactionPayload
    {
        [Range(1, long.MaxValue, ErrorMessage = "Campo 'transactionId' deve ser maior que zero.")]
        public long TransactionId { get; set; }
    }
}
