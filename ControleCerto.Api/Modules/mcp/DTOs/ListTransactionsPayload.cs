using System.ComponentModel.DataAnnotations;

namespace ControleCerto.Modules.Mcp.DTOs
{
    public class ListTransactionsPayload
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Mode { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Campo 'accountId' deve ser maior que zero.")]
        public int? AccountId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Campo 'cardId' deve ser maior que zero.")]
        public long? CardId { get; set; }

        [Range(1, long.MaxValue, ErrorMessage = "Campo 'categoryId' deve ser maior que zero.")]
        public long? CategoryId { get; set; }
        public string? SearchText { get; set; }
        public string? Sort { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Campo 'pageNumber' deve ser maior ou igual a 1.")]
        public int? PageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "Campo 'pageSize' deve estar entre 1 e 100.")]
        public int? PageSize { get; set; }
    }
}
