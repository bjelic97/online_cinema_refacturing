using System;

namespace Logic.Dtos
{
    public class CustomerInListDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime? StatusExpirationDate { get; set; }
        public decimal MoneySpent { get; set; }
    }
}
