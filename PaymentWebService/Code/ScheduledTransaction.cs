using CommonDTO;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentWebService.Code
{
    public class ScheduledTransaction
    {
        public static async Task<int> ScheduleTransaction(int accountId, SchedulingInfo schedulingInfo, CommonDTO.Transaction transaction)
        {
            string trJson = Helpers.JsonHelper.GetJson(transaction);

            using SqlConnection con = Global.Connection;
            int trId = await con.ExecuteScalarAsync<int>("INSERT INTO tblScheduledTransactions([accountId],[disabled],[startOn],[endOn],[nextRunOn],[lastRunOn],[skipDates],[interval],[trJson]) VALUES(@accountId,0,@startOn,@endOn,@nextRunOn,'1/1/1900','',@interval,@trJson); SELECT @@IDENTITY", new
            {
                accountId,
                startOn = schedulingInfo.startOn.ToString("MM/dd/yyyy"),
                endOn = schedulingInfo.endOn.ToString("MM/dd/yyyy"),
                nextRunOn = schedulingInfo.startOn.ToString("MM/dd/yyyy"),
                interval = (int)schedulingInfo.scheduleType,
                trJson,
            }).ConfigureAwait(false);
            return trId;
        }
    }
}
