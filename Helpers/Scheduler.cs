using CommonDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helpers
{
    public class Scheduler
    {
        public static DateTime CalculateNextRun(DateTime today, DateTime startDate, DateTime endDate, ScheduleType interval)
        {

            DateTime nextRunDate;
            int amountOfPaymentsMade = 0;
            switch (interval)
            {
                case ScheduleType.ONETIME:
                    return DateTime.MaxValue;
                case ScheduleType.DAILY:
                    amountOfPaymentsMade = today.Subtract(startDate).Days;
                    nextRunDate = startDate.AddDays(amountOfPaymentsMade + 1);
                    break;
                case ScheduleType.WEEKLY:
                    amountOfPaymentsMade = today.Subtract(startDate).Days / 7;
                    nextRunDate = startDate.AddDays(amountOfPaymentsMade + 7);
                    break;
                case ScheduleType.BIWEEKLY:
                    amountOfPaymentsMade = today.Subtract(startDate).Days / 14;
                    nextRunDate = startDate.AddDays(amountOfPaymentsMade + 14);
                    break;
                case ScheduleType.TRIWEEKLY:
                    amountOfPaymentsMade = today.Subtract(startDate).Days / 21;
                    nextRunDate = startDate.AddDays(amountOfPaymentsMade + 21);
                    break;
                case ScheduleType.MONTLY:
                    amountOfPaymentsMade = ((today.Year - startDate.Year) * 12) + (today.Month - startDate.Month);
                    nextRunDate = startDate.AddMonths(amountOfPaymentsMade + 1);
                    break;
                case ScheduleType.BIMONTHLY:
                    amountOfPaymentsMade = (((today.Year - startDate.Year) * 12) + (today.Month - startDate.Month)) / 2;
                    nextRunDate = startDate.AddMonths(amountOfPaymentsMade + 2);
                    break;
                case ScheduleType.QUATERLY:
                    amountOfPaymentsMade = (((today.Year - startDate.Year) * 12) + (today.Month - startDate.Month)) / 3;
                    nextRunDate = startDate.AddMonths(amountOfPaymentsMade + 3);
                    break;
                case ScheduleType.SEMIANNUAL:
                    amountOfPaymentsMade = (((today.Year - startDate.Year) * 12) + (today.Month - startDate.Month)) / 6;
                    nextRunDate = startDate.AddMonths(amountOfPaymentsMade + 6);
                    break;
                case ScheduleType.ANNUAL:
                    amountOfPaymentsMade = today.Year - startDate.Year;
                    nextRunDate = startDate.AddMonths(amountOfPaymentsMade + 12);
                    break;
                default:
                    throw new Exception($"Unknown recurring interval  {interval}");
            }
            if (nextRunDate > endDate)
                return DateTime.MaxValue;

            return nextRunDate;
        }
    }
}
