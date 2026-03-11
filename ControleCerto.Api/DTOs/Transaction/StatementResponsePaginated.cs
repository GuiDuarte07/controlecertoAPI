using ControleCerto.DTOs.Common;
using ControleCerto.Models.DTOs;

namespace ControleCerto.DTOs.Transaction
{
    /// <summary>
    /// Resposta do modo "statement" com suporte a paginação.
    /// Retorna transações com filtros avançados e metadados de paginação.
    /// </summary>
    public class StatementResponsePaginated
    {
        /// <summary>
        /// Data inicial do filtro aplicado
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Data final do filtro aplicado
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Lista paginada de transações
        /// </summary>
        public PaginatedResponse<InfoTransactionResponse> Transactions { get; set; } = new();

        /// <summary>
        /// Sumário com estatísticas do período (calculado sobre o filtro, não a página)
        /// </summary>
        public StatementSummary Summary { get; set; } = new();
    }
}
