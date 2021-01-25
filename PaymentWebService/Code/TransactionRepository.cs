using CommonDTO;
using Dapper;
using Microsoft.Data.SqlClient;
using PaymentDB;
using PaymentDTO;
using System;
using System.Threading.Tasks;

namespace PaymentWebService.Code
{
    public class TransactionRepository
    {
        public static async Task<int> SaveTransaction(int accountId, TransactionType trType, int parentId, decimal amount, CommonDTO.Transaction transaction)
        {
            string trJson = Helpers.JsonHelper.GetJson(transaction);
            using SqlConnection con = Global.Connection;
            int id = await con.ExecuteScalarAsync<int>("INSERT INTO tblTransaction(trType, source, lockId, parentId, accountId, createdDate, status, statusMessage, originalAmount, amountLeft, trJson, authorizerResponseJson) VALUES(@trType, @source, 0, @parentId, @accountId, @createDate, @status, @statusMessage, @originalAmount, @amountLeft, @trJson, ''); SELECT @@IDENTITY", new
            {
                trType = (int)trType,
                source = transaction.source??"",
                parentId,
                accountId,                
                createDate = DateTime.Now,
                status = (int)CommonDTO.TransactionStatus.PENDING,
                statusMessage = "",
                originalAmount = amount,
                amountLeft = amount,
                trJson
            }).ConfigureAwait(false);
            transaction.id = id;
            return id;
        }

        public static async Task<int> SaveAuthorizerResponse(int trId, int status, string statusMessage, decimal originalAmount, decimal amountLeft, CommonDTO.AuthorizerResponse response)
        {
            string responseJson = "";
            if( response!= null )
                responseJson = Helpers.JsonHelper.GetJson(response);
            using SqlConnection con = Global.Connection;
            int recordsAffected = await con.ExecuteAsync("UPDATE tblTransaction SET status=@status, statusMessage=@statusMessage, originalAmount=@originalAmount, amountLeft=@amountLeft, authorizerResponseJson=@responseJson WHERE id=@trId", new
            {
                trId,
                status,
                statusMessage,
                originalAmount,
                amountLeft,
                responseJson
            }).ConfigureAwait(false);
            return recordsAffected;
        }

        public static async Task<int> SaveResultAndUnlock(int trId, int status, string statusMessage, decimal originalAmount, decimal amountLeft)
        {
            using SqlConnection con = Global.Connection;
            return await con.ExecuteAsync("UPDATE tblTransaction SET lockId=0, status=@status, statusMessage=@statusMessage, originalAmount=@originalAmount, amountLeft=@amountLeft WHERE id=@trId", new
            {
                trId,
                status,
                statusMessage, 
                originalAmount,
                amountLeft,
            }).ConfigureAwait(false);
        }

        public static async Task<TransactionDBDTO> GetTransactionDBDTO(int Id, int lockId)
        {
            using SqlConnection con = Global.Connection;
            if (lockId != 0)
            {
                int recordsAffected = await con.ExecuteAsync("UPDATE tblTransaction SET lockId=@lockId WHERE Id=@Id AND lockId=0", new
                {
                    Id,
                    lockId,
                }).ConfigureAwait(false);
                if (recordsAffected != 1)
                    return null;
            }

            return await con.QuerySingleOrDefaultAsync<TransactionDBDTO>("SELECT [Id],[lockId],[accountId],[createdDate],[status],[statusMessage],[originalAmount],[amountLeft],[trJson],[authorizerResponseJson] FROM tblTransaction WHERE Id=@Id", new
            {
                Id,
                lockId,
            }).ConfigureAwait(false);
        }

        public static Transaction GetTransaction(TransactionDBDTO trDBO)
        {
            Transaction obj = Helpers.JsonHelper.GetObject(trDBO.trJson) as Transaction;
            obj.id = trDBO.id;
            obj.status = (TransactionStatus)trDBO.status;
            return obj;
        }

        public static async Task<bool> LockTransaction(int Id, int lockId)
        {
            using SqlConnection con = Global.Connection;
            int recordAffected = await con.ExecuteAsync("UPDATE tblTransaction SET lockId=@lockId WHERE Id=@Id AND lockid=0", new
            {
                Id,
                lockId
            }).ConfigureAwait(false);

            return recordAffected == 1;
        }
        public static async Task<bool> UnlockTransaction(int Id, int lockId)
        {
            using SqlConnection con = Global.Connection;
            int recordAffected = await con.ExecuteAsync("UPDATE tblTransaction SET lockId=0 WHERE Id=@Id AND lockid=@lockId", new
            {
                Id,
                lockId
            }).ConfigureAwait(false);

            return recordAffected == 1;
        }
    }
}
