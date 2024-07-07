using Finantech.DTOs.Account;
using System.ComponentModel.DataAnnotations;

namespace Finantech.DTOs.CreditCard
{
    public class InfoCreditCardResponse
    {
        public int Id { get; set; }
        public double TotalLimit { get; set; }
        public double UsedLimit { get; set; }
        public string Description { get; set; }
        public int DueDay { get; set; }
        public int CloseDay { get; set; }
        public InfoAccountResponse Account { get; set; }
    }
}
