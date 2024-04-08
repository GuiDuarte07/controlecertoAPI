﻿namespace Finantech.DTOs.CreditPurcchase
{
    public class InfoCreditPurchaseResponse
    {
        public int Id { get; set; }
        public double TotalAmount { get; set; }
        public int TotalInstalment { get; set; }
        public int InstalmentsPaid { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public bool Paid { get; set; }
        public string Destination { get; set; }
        public string? Description { get; set; }
        public int CreditCardId { get; set; }
    }
}
