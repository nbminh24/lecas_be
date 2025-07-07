using System;
using System.Collections.Generic;

namespace be_lecas.Models
{
    public class Promotion
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "percent"; // "percent" hoặc "amount"
        public decimal DiscountValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public List<string> ProductIds { get; set; } = new List<string>(); // Danh sách sản phẩm áp dụng
    }
}
