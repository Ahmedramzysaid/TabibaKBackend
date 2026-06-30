using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public DateTime PaymentDate { get; set; }
        public float AmountPaid { get; set; }
        public string? AdditionalNotes { get; set; }
        public Appointment Appointment { get; set; }
    }
}
