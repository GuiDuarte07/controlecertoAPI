using Finantech.DTOs.Account;
using Finantech.Models.Entities;

namespace Finantech.DTOs.TransferenceDTO
{
    public class InfoTransferenceResponse
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public string PurchaseDate { get; set; }
        public string AccountDestinyBank { get; set; }
        public string AccountOriginBank { get; set; }

        public InfoTransferenceResponse(Transference transference)
        {
            Id = transference.Id;
            Description = transference.Description;
            Amount = transference.Amount;
            PurchaseDate = transference.PurchaseDate;
            AccountDestinyBank = transference.AccountDestiny.Bank;
            AccountOriginBank = transference.AccountOrigin.Bank;
        }
    }
}
