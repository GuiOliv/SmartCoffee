using SmartCoffee.Data;
using SmartCoffee.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCoffee.Business
{
    public class DateCalculatorModel
    {
        public List<ProductDateToOrder> Products { get; set; } = new List<ProductDateToOrder>();

        public bool CalculateDates(DateTime date)
        {
            int differenceBetweenDates = GetBusinessDaysDifference(DateTime.Today.Date, date);

            using (var context = new Context())
            {
                var longestDelivery = context.Products.Max(w => w.DeliveryDays);
                if (longestDelivery > differenceBetweenDates) return false;

                foreach (var product in context.Products.Where(w => w.DeliveryDays != 0))
                {
                    var orderDate = AddBusinessDays(DateTime.Today, differenceBetweenDates - product.DeliveryDays);
                    Products.Add(new ProductDateToOrder
                    {
                        Date = orderDate,
                        Name = product.Name,
                        URL = product.URL
                    });
                }
            }

            return true;
        }

        public static DateTime AddBusinessDays(DateTime startDate, int businessDays)
        {
            if (businessDays < 0) throw new ArgumentException("Business days cannot be negative", nameof(businessDays));

            var currentDate = startDate;
            while (businessDays > 0)
            {
                currentDate = currentDate.AddDays(1);

                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDays--;
                }
            }

            return currentDate;
        }

        public static int GetBusinessDaysDifference(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate) throw new ArgumentException("Start date cannot be after end date");

            int businessDays = 0;
            var currentDate = startDate;

            while (currentDate < endDate)
            {
                currentDate = currentDate.AddDays(1);

                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDays++;
                }
            }

            return businessDays;
        }
    }

    public struct ProductDateToOrder
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string URL { get; set; }
    }
}
