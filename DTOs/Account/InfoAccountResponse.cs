﻿namespace Finantech.DTOs.Account
{
    public class InfoAccountResponse
    {
        public int Id { get; set; }
        public double Balance { get; set; }
        public string Description { get; set; }
        public string Bank { get; set; }
        public string Type { get; set; }
        public string AccountType { get; set; }
        public string Color { get; set; }
    }
}
