using Crypto;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using System.Threading.Tasks;

namespace Vault
{
    public class Vault2
    {
        public static async Task UpdateVaultData(int accountId, string vaultId, int sequence, int dataType, bool encrypted, string data)
        {
            using SqlConnection con = Global.Connection;
            if (encrypted)
                data = data.Encrypt();

            var param = new
            {
                accountId,
                vaultId,
                sequence,
                dataType,
                encrypted,
                data
            };
            int recordAffected = await con.ExecuteAsync("UPDATE tblVault SET [dataType]=@dataType,[encrypted]=@encrypted, [data]=@data WHERE accountId=@accountId AND vaultId=@vaultId AND sequence = @sequence", param).ConfigureAwait(false);
            if (recordAffected == 0)
                await con.ExecuteScalarAsync<int>("INSERT INTO tblVault SET [accountId],[vaultId],[sequence],[dataType],[data] VALUES(@accountID, @vaultId, @sequence, @dataType, @data)", param).ConfigureAwait(false);
        }
        public static async Task<string> GetVaultData(int accountId, string vaultId, int sequence)
        {
            using SqlConnection con = Global.Connection;
            var vault = await con.QuerySingleAsync<VaultDBDTO>("SELECT data FROM tblVault WHERE accountId=@accountId AND vaultId=@vaultId AND sequence = @sequence", new
            {
                accountId,
                vaultId,
                sequence,
            }).ConfigureAwait(false);
            if (vault.encrypted)
                vault.data = vault.data.Decrypt();
            return vault.data;
        }

        public static async Task<IEnumerable<VaultDBDTO>> GetVaultDataRecords(int accountId, string vaultId, int sequence = -1, int dataType = -1)
        {
            using SqlConnection con = Global.Connection;
            string sql = "SELECT id, accountId, sequence, dataType,data FROM tblVault WHERE accoountId=@accountId AND vaultId=@vaultId";
            if (sequence >= 0)
                sql += " AND sequence=@sequence";
            if (dataType >= 0)
                sql += " AND dataType=@dataType";

            var lst = await con.QueryAsync<VaultDBDTO>(sql, new
            {
                accountId,
                vaultId,
                sequence,
                dataType,
            }).ConfigureAwait(false);
            foreach (var vault in lst)
            {
                if (vault.encrypted)
                    vault.data = vault.data.Decrypt();
            }
            return lst;
        }

        public static async Task DeleteVault(int accountId, string vaultId, int sequence = -1)
        {
            using SqlConnection con = Global.Connection;
            string sql = "DELETE tblVault WHERE accoountId=@accountId AND vaultId=@vaultId";
            if (sequence >= 0)
                sql += " AND sequence=@sequence";
            await con.ExecuteAsync(sql, new
            {
                accountId,
                vaultId,
                sequence,
            }).ConfigureAwait(false);
        }
    }

    public class VaultDBDTO
    {
        public int id { get; set; }
        public int accountId { get; set; }
        public int sequence { get; set; }
        public int dataType { get; set; }
        public bool encrypted { get; set; }
        public string data { get; set; }
    }

}
