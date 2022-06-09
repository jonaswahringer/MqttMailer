using System;
namespace MQTTDemo.Model
{
    class InvoicePosition //n-Seite
    {
        public int Id { get; set; } // Surrogate Key / Stellvertreter Key

        public int ItemNr { get; set; }
        public int Qty { get; set; }
        public double Price { get; set; }
        public double priceOverall;
        public double PriceOverall
        {
            get { return Price * Qty; }
        }

        public int InvoiceId { get; set; }             // FK der Rechung
        public InvoicePosition Invoice { get; set; }     // Referenz auf die Rechnung

    }
}
