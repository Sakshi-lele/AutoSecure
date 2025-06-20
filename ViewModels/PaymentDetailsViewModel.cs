// ViewModels/PaymentDetailsViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Auto_Insurance_Management_System.ViewModels
{
    public class PaymentDetailsViewModel
    {
        public int PaymentId { get; set; }
        
        [Required]
        [Display(Name = "Policy ID")]
        public int PolicyId { get; set; }
        public DateTime DueDate { get; set; }
        
        [Required]
        [Display(Name = "Amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal PaymentAmount { get; set; }
        
        [Required]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        
        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }
        
        // Card details
        [CreditCard(ErrorMessage = "Invalid card number.")]
        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }
        
        [Display(Name = "Card Holder Name")]
        public string? CardHolderName { get; set; }
        
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Invalid expiry date. Format: MM/YY")]
        [Display(Name = "Expiry Date")]
        public string? ExpiryDate { get; set; }
        
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Invalid CVV.")]
        [Display(Name = "CVV")]
        public string? CVV { get; set; }
        
        // UPI
        [EmailAddress(ErrorMessage = "Invalid UPI ID.")]
        [Display(Name = "UPI ID")]
        public string? UpiId { get; set; }
        
        // For display only
        public string? CustomerName { get; set; }
        public string? PolicyNumber { get; set; }
    }
}