using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MQTTDemo.Model
{
    class Invoice
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public double Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int Vat { get; set; }

        [JsonIgnore]
        public ICollection<InvoicePosition> Positions { get; set; } = new List<InvoicePosition>(); //Referenz auf die Rechnungsposition
    }
}
