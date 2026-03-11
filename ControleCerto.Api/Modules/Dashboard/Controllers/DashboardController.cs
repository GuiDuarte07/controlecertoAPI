using ControleCerto.Decorators;
using ControleCerto.Extensions;
using ControleCerto.Modules.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Modules.Dashboard.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    [ExtractTokenInfo]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Obtém dados consolidados do dashboard com indicadores financeiros, transações, contas, cartões, categorias e investimentos.
        /// 
        /// **Parâmetros de Query:**
        /// - startDate (DateTime, obrigatório): Data inicial do período. Formato: ISO 8601 (ex: 2026-03-01)
        /// - endDate (DateTime, obrigatório): Data final do período. Formato: ISO 8601 (ex: 2026-03-31)
        /// 
        /// **Response:**
        /// Retorna um objeto HomeDashboardResponse contendo:
        /// 
        /// - **FinancialSummary**: Resumo financeiro geral
        ///   - TotalIncome: Total de receitas no período
        ///   - TotalExpense: Total de despesas no período
        ///   - NetBalance: Saldo líquido (receitas - despesas)
        ///   - TotalAccountsBalance: Saldo total de todas as contas
        ///   - TotalCreditUsed: Total usado nos cartões de crédito
        ///   - TotalCreditAvailable: Total disponível nos cartões
        ///   - TotalInvestments: Valor total investido
        ///   - TotalTransactions: Quantidade de transações no período
        ///   - AverageDailyExpense: Média de gastos por dia
        ///   - ProjectedMonthlyExpense: Projeção de gastos mensais
        /// 
        /// - **Accounts**: Lista de contas com resumos individuais
        ///   - Balance, TotalIncome, TotalExpense, TransactionCount por conta
        /// 
        /// - **CreditCards**: Lista de cartões com uso e limites
        ///   - TotalLimit, UsedLimit, AvailableLimit, UsagePercentage
        ///   - CurrentInvoiceAmount: Valor da fatura no período
        /// 
        /// - **ExpensesByCategory**: Gastos agrupados por categoria (ordenado por valor)
        ///   - TotalAmount, TransactionCount, Percentage
        ///   - CategoryLimit e LimitUsagePercentage quando aplicável
        /// 
        /// - **InvestmentsSummary**: Resumo de investimentos
        ///   - TotalValue, TotalDividends, TotalDeposits, TotalWithdrawals, NetProfit
        ///   - Lista de investimentos individuais com detalhes
        /// 
        /// - **DailyBalances**: Balanço dia a dia do período
        ///   - Income, Expense, Balance e CumulativeBalance por dia
        /// 
        /// - **MonthlyTrends**: Tendências dos últimos 12 meses
        ///   - TotalIncome, TotalExpense, NetBalance por mês
        /// 
        /// - **RecentTransactions**: Últimas 10 transações do período
        /// 
        /// **Exemplos de Requisições:**
        /// GET /api/dashboard?startDate=2026-03-01&endDate=2026-03-31
        /// GET /api/dashboard?startDate=2026-01-01&endDate=2026-12-31
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetHomeDashboard(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            DateTime utcStartDate = startDate.ToUniversalTime();
            DateTime utcEndDate = endDate.ToUniversalTime();

            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _dashboardService.GetHomeDashboardAsync(userId, utcStartDate, utcEndDate);

            return result.HandleReturnResult();
        }
    }
}
