using CommonDTO;
using Dapper;
using Microsoft.Data.SqlClient;
using PaymentDB;
using PaymentDTO;
using System;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PaymentWebService.Code
{
    
    public class EventRepository
    {
        public static async Task<int> SaveEvent(int accountId, int eventType, int eventSubtype, string eventId, EventData data)
        {
            string eventJson = Helpers.JsonHelper.GetJson(data);
            using SqlConnection con = Global.Connection;
            int id = await con.ExecuteScalarAsync<int>("INSERT INTO tblEvent(accountId,eventType,eventSubtype,eventId,eventJson) VALUES(@accountId,@eventType,@eventSubtype,@eventId,@eventJson); SELECT @@IDENTITY", new
            {
                accountId,
                eventType,
                eventSubtype,
                eventId,
                eventJson,
            }).ConfigureAwait(false);
            return id;
        }

        public static async Task<int> CreateHook(int accountId, int eventType, int eventSubtype, string eventId, int hookType, HookData data)
        {
            string hookJson = Helpers.JsonHelper.GetJson(data);
            using SqlConnection con = Global.Connection;
            var param = new
            {
                accountId,
                eventType,
                eventSubtype,
                eventId,
                hookType,
                hookJson,
            };
            //check if hook already exists, if it does then update it
            int id = await con.ExecuteScalarAsync<int>("UPDATE tblHook SET hookJson=@hookJson OUTPUT INSERTED.id WHERE [accountId]=@accountId AND [eventType]=@eventType AND [eventSubtype]=@eventSubtype AND [eventId]=@eventId AND [hookType]=@hookType", param);
            if(id == 0)
                id = await con.ExecuteScalarAsync<int>("INSERT INTO tblHook([accountId],[eventType],[eventSubtype],[eventId],[hookType],[hookJson]) VALUES(@accountId,@eventType,@eventSubtype,@eventId,@hookType,@hookJson); SELECT @@IDENTITY", param).ConfigureAwait(false);
            return id;
        }
    }
}
