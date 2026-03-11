using ControleCerto.Decorators;
using ControleCerto.DTOs.Transaction;
using ControleCerto.DTOs.TransferenceDTO;
using ControleCerto.Errors;
using ControleCerto.Extensions;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    [ExtractTokenInfo]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        public TransactionController(ITransactionService transactionService) 
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _transactionService.CreateTransactionAsync(request, userId);

            if(result.IsSuccess)
            {
                return Created($"/api/transactions/{result.Value.Id}", result.Value);
            }

            return result.HandleReturnResult();
        }

        [HttpDelete("{transactionId:int}")]
        public async Task<IActionResult> DeleteTransaction([FromRoute] int transactionId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _transactionService.DeleteTransactionAsync(transactionId, userId);
            return result.HandleReturnResult();
        }

        [HttpPatch("{transactionId:long}")]
        public async Task<IActionResult> UpdateTransaction([FromRoute] long transactionId, [FromBody] UpdateTransactionRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            request.Id = transactionId;
            ModelState.Clear();
            TryValidateModel(request);

            if (!ModelState.IsValid)
            {
                var errorResponse = ErrorResponse.FromModelState(ModelState);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            var result = await _transactionService.UpdateTransactionAsync(request, userId);
            return result.HandleReturnResult();
        }

        [HttpPost("transfers")]
        public async Task<IActionResult> CreateTransference([FromBody] CreateTransferenceRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _transactionService.CreateTransferenceAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created($"/api/transactions/{result.Value.Id}", result.Value);
            }

            return result.HandleReturnResult();
        }

        /// <summary>
        /// Obtém transações com filtros flexíveis, ordenação customizável e paginação.
        /// 
        /// Parâmetros de Query:
        /// 
        /// **Parâmetros Principais:**
        /// - mode (string, obrigatório): "invoice" (faturas mensais) ou "statement" (extrato detalhado com todas as transações)
        ///   Exemplo: ?mode=statement
        /// 
        /// **Filtros de Data:**
        /// - startDate (DateTime, obrigatório): Data inicial do filtro. Formato: ISO 8601 (ex: 2026-03-01)
        ///   No modo "invoice": apenas mês/ano importam (dia é ignorado)
        ///   No modo "statement": intervalo completo é usado
        /// - endDate (DateTime, obrigatório): Data final do filtro. Formato: ISO 8601 (ex: 2026-03-31)
        ///   No modo "statement": inclui transações até 23:59:59 da endDate
        /// 
        /// **Filtros de Recursos (apenas modo statement):**
        /// - accountId (int, opcional): Filtra por ID da conta bancária. Exemplo: ?accountId=1
        /// - cardId (long, opcional): Filtra por ID do cartão de crédito. Exemplo: ?cardId=5
        /// - categoryId (long, opcional): Filtra por ID da categoria. Exemplo: ?categoryId=3
        /// - searchText (string, opcional): Busca em descrição/destino. Case-insensitive. Exemplo: ?searchText=mercado
        /// 
        /// **Ordenação (apenas modo statement):**
        /// - sort (string, opcional): Critérios de ordenação separados por vírgula. Formato: "campo asc|desc"
        ///   Campos válidos: date (padrão), amount, account, category
        ///   Exemplos:
        ///     ?sort=date%20desc                    (mais recentes primeiro)
        ///     ?sort=amount%20desc,date%20asc       (maior valor primeiro, depois por data)
        ///     ?sort=category%20asc,amount%20desc   (por categoria crescente, depois por valor decrescente)
        /// 
        /// **Paginação (apenas modo statement):**
        /// - pageNumber (int, opcional, padrão: 1): Número da página. Deve ser >= 1
        /// - pageSize (int, opcional, padrão: 20): Quantidade de itens por página. Deve estar entre 1 e 100
        ///   Exemplos:
        ///     ?pageNumber=1&pageSize=20       (primeiros 20 itens)
        ///     ?pageNumber=2&pageSize=50       (itens 51-100)
        /// 
        /// **Response:**
        /// - modo invoice: { transactions: [], invoices: [] }
        /// - modo statement: { startDate, endDate, transactions: { data: [], pagination: { currentPage, pageSize, totalItems, totalPages, hasNextPage, hasPreviousPage } }, summary: { ... } }
        /// 
        /// **Exemplos de Requisições:**
        /// GET /api/transactions?mode=invoice&startDate=2026-03-01&endDate=2026-03-31
        /// GET /api/transactions?mode=statement&startDate=2026-03-01&endDate=2026-03-31&pageNumber=1&pageSize=20
        /// GET /api/transactions?mode=statement&startDate=2026-03-01&endDate=2026-03-31&accountId=1&sort=date%20desc&pageNumber=1&pageSize=30
        /// GET /api/transactions?mode=statement&startDate=2026-03-01&endDate=2026-03-31&searchText=mercado&categoryId=3&sort=amount%20desc&pageSize=50
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTransactions
        (
            [FromQuery] string mode,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? accountId,
            [FromQuery] long? cardId,
            [FromQuery] long? categoryId,
            [FromQuery] string? searchText,
            [FromQuery] string? sort,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20
        )
        {
            DateTime utcStartDate = startDate.ToUniversalTime();
            DateTime utcEndDate = endDate.ToUniversalTime();

            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _transactionService.GetTransactionsAsync(
                userId, 
                utcStartDate, 
                utcEndDate, 
                mode?.ToLower() ?? "statement",
                accountId,
                cardId,
                categoryId,
                searchText,
                sort,
                pageNumber,
                pageSize
            );

            return result.HandleReturnResult();
        }
    }
}
