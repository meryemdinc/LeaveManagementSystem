using System;

//"Cumartesi ve Pazar günlerini sayma" mantığını yapan yardımcı sınıf.
namespace LeaveManagement.Application.Common.Helpers
{
    public static class DateHelper
    {
        public static int CalculateBusinessDays(DateTime startDate, DateTime endDate)
        {
            int businessDays = 0;
            DateTime current = startDate;

            while (current <= endDate)
            {
                // Eğer gün Cumartesi veya Pazar DEĞİLSE sayacı artır
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDays++;
                }
                current = current.AddDays(1);
            }
            return businessDays;
        }
    }
}