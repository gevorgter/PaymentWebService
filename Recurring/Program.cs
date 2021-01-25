using CommonDTO;
using Dapper;
using Newtonsoft.Json;
using PaymentDB;
using PaymentDTO.Payment;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Recurring
{
    class Program
    {
        static DateTime _today = DateTime.Today;
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
           //.Enrich.FromLogContext()
           .Enrich.WithProperty("App", "Recurring")
           .MinimumLevel.Information()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
           //.WriteTo.File(new RenderedCompactJsonFormatter(), "log.txt", LogEventLevel.Information)//, outputTemplate: "[{Timestamp:HH:mm:ss}{Level:u3}] {Message:lj}{NewLine}{Exception}")
           .WriteTo.File("log.txt", LogEventLevel.Information, outputTemplate: "[{Timestamp:HH:mm:ss}{Level:u3}] {Message:lj}{Properties:j}{NewLine}{Exception}")
           .CreateLogger();

            Global.Init();
            var transactions = GetTransactionsToRun();
            foreach (var tr in transactions)
            {
                try
                {
                    RunTransaction(tr);
                }
                catch (Exception e)
                {
                    Log.Error(e, "recurring id:{id} threw an error", tr.id);
                }
            }

        }

        static IEnumerable<ScheduledTransactionDBDTO> GetTransactionsToRun()
        {
            using var con = Global.Connection;
            return con.Query<ScheduledTransactionDBDTO>(@"SELECT * FROM tblScheduledTransactions 
                        WHERE disabled=0 AND startOn<=GETDATE() AND endOn>=GETDATE() AND nextRunOn<=GETDATE()");
        }

        static void RunTransaction(ScheduledTransactionDBDTO tr)
        {
            Transaction transaction = Helpers.JsonHelper.GetObject(tr.trJson) as Transaction;
            if (transaction == null)
                throw new Exception($"scheduled id:{tr.id} is not a Transasction object");

            SaleTransaction saleTransaction = transaction as SaleTransaction;
            if (saleTransaction == null)
                return;

            SaleRequestDTO rq = Global._mapper.Map<SaleRequestDTO>(saleTransaction);
            rq.accountId = tr.accountId;
            rq.source = $"Recurring - trId:{tr.id}";
            var rp = PaymentWebServiceClient.Client.CallApi<SaleResponseDTO>("/payment/v1/Sale", rq).GetAwaiter().GetResult();
            if (rp.errorCode < 0 )
            {
                //Something really bad happened
                Log.Error("Could not call payment service id:{id} with reason :{errors}", tr.id, String.Join("\r\n", rp.errors));
                Environment.Exit(-1);
            }
            int disabled = 0;
            int lastTrId = -1;
            DateTime nextRun = _today;
            if (rp.errorCode != 0)
            {
                tr.errorCount++;
                Log.Error("Payment failed for recurring id:{id} with reason :{errors}", tr.id, String.Join("\r\n", rp.errors));
            }
            else
            {
                lastTrId = (int)rp.trId;
                nextRun = Helpers.Scheduler.CalculateNextRun(_today, tr.startOn, tr.endOn, (ScheduleType)tr.interval);
                if (nextRun == DateTime.MaxValue)
                {
                    disabled = 1;
                    nextRun = new DateTime(2079, 1, 1); //set to max smalldatetime in ms sql
                }
            }

            if (tr.errorCount > tr.maxErrorCount)
                disabled = 1;

            using var con = Global.Connection;
            con.Execute("UPDATE tblScheduledTransactions SET lastTrId=@lastTrId, nextRunOn=@nextRunOn,lastRunOn=@lastRunOn,disabled=@disabled, errorCount=@errorCount WHERE id=@id", new
            {
                tr.id,
                nextRunOn = nextRun,
                lastRunOn = _today,
                lastTrId,
                disabled,
                tr.errorCount
            });
        }
    }
}
