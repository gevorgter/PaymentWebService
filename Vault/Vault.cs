using Dapper;
using System;
using System.Collections.Generic;
using VaultDTO;

namespace Vault
{
    public class Vault
    {
        public static void UpdateVault(int accountId, CustomerDBDTO customer)
        {
            using var con = Global.Connection;
            con.Open();
            var c = con.QueryFirstOrDefault<CustomerDBDTO>("SELECT [id],[accountId],[vaultId],[firstName],[lastName],[company],[address1],[address2],[city],[state],[postalCode],[country],[email],[phone] FROM tblVaultCustomer WHERE accountId=@accountId AND vaultId=@vaultId", new {
                accountId,
                customer.vaultId
            });
            if (c == null)
            {
                //we need to insert it
                customer.id = con.ExecuteScalar<int>(@"INSERT INTO tblVaultCustomer([accountId],[vaultId],[firstName],[lastName],[company],[address1],[address2],[city],[state],[postalCode],[country],[email],[phone]) 
                    VALUES(@accountId,@vaultId,@firstName,@lastName,@company,@address1,@address2,@city,@state,@postalCode,@country,@email,@phone); SELECT @@IDENTITY", new
                {
                    accountId,
                    customer.vaultId,
                    firstName = customer.firstName ?? "",
                    lastName = customer.lastName ?? "",
                    company = customer.company ?? "",
                    address1 = customer.address1 ?? "",
                    address2 = customer.address2 ?? "",
                    city = customer.city ?? "",
                    state = customer.state ?? "",
                    postalCode = customer.postalCode ?? "",
                    country = customer.country ?? "",
                    email = customer.email ?? "",
                    phone = customer.phone ?? "",
                });
            }
            else
            {
                //update
                con.Execute(@"UPDATE tblVaultCustomer  SET firstName=@firstName,lastName=@lastName,company=@company,address1=@address1,address2=@address2,city=@city,state=@state,
                                            postalCode=@postalCode,country=@country,email=@email,phone=@phone WHERE Id=@id", new
                {
                    id = c.id,
                    firstName = customer.firstName ?? c.firstName,
                    lastName = customer.lastName ?? c.lastName,
                    company = customer.company ?? c.company,
                    address1 = customer.address1 ?? c.address1,
                    address2 = customer.address2 ?? c.address2,
                    city = customer.city ?? c.city,
                    state = customer.state ?? c.state,
                    postalCode = customer.postalCode ?? c.postalCode,
                    country = customer.country ?? c.country,
                    email = customer.email ?? c.email,
                    phone = customer.phone ?? c.phone,
                });
            }
            con.Close();
        }

        //returns true if vault was updated or false if it does not exists
        public static bool UpdateCC(int accountId, string vaultId, CreditCardDBDTO ccInfo)
        {
            using var con = Global.Connection;
            con.Open();
            if (ccInfo.vaultCustomerId == -1)
            {
                //we need to find out if vault exists
                ccInfo.vaultCustomerId = con.ExecuteScalar<int>("SELECT Id FROM tblVaultCustomer WHERE accountId=@accountId AND vaultId=@vaultId", new
                {
                    accountId,
                    vaultId
                });
            }
            if (ccInfo.vaultCustomerId <= 0)
                return false;

            var c = con.QueryFirstOrDefault<CreditCardDBDTO>(@"SELECT [sequence],[ccNum],[ccExpMonth],[ccExpYear],[ccType],[firstName],[lastName],[company],[address1],[address2],[city],[state],[postalCode],[country] 
                                FROM tblVaultCC WHERE [vaultCustomerId]=@vaultCustomerId AND [sequence]=@sequence", new
            {
                ccInfo.vaultCustomerId,
                ccInfo.sequence
            });

            if (c == null)
            {
                //we need to insert it
                
                con.ExecuteScalar<int>(@"INSERT INTO tblVaultCC([vaultCustomerId],[sequence],[ccNum],[ccExpMonth],[ccExpYear],[ccType],[firstName],[lastName],[company],[address1],[address2],[city],[state],[postalCode],[country]) 
                    VALUES(@vaultCustomerId,@sequence,@ccNum,@ccExpMonth,@ccExpYear,@ccType,@firstName,@lastName,@company,@address1,@address2,@city,@state,@postalCode,@country)", new
                {
                    ccInfo.vaultCustomerId,
                    ccInfo.sequence,
                    ccInfo.ccNum,
                    ccInfo.ccExpMonth,
                    ccInfo.ccExpYear,
                    ccInfo.ccType,
                    firstName = ccInfo.firstName ?? "",
                    lastName = ccInfo.lastName ?? "",
                    company = ccInfo.company ?? "",
                    address1 = ccInfo.address1 ?? "",
                    address2 = ccInfo.address2 ?? "",
                    city = ccInfo.city ?? "",
                    state = ccInfo.state ?? "",
                    postalCode = ccInfo.postalCode ?? "",
                    country = ccInfo.country ?? "",
                });
            }
            else
            {
                //we need to update it
                con.Execute(@"UPDATE tblVaultCC SET [ccNum]=@ccNum,[ccExpMonth]=@ccExpMonth,[ccExpYear]=@ccExpYear,[ccType]=@ccType, firstName=@firstName,lastName=@lastName,
                            company=@company,address1=@address1,address2=@address2,city=@city,state=@state,postalCode=@postalCode,country=@country WHERE [vaultCustomerId]=@vaultCustomerId AND [sequence]=@sequence", new
                {
                    ccInfo.vaultCustomerId,
                    ccInfo.sequence,
                    ccInfo.ccNum,
                    ccInfo.ccExpMonth,
                    ccInfo.ccExpYear,
                    ccInfo.ccType,
                    firstName = ccInfo.firstName ?? c.firstName,
                    lastName = ccInfo.lastName ?? c.lastName,
                    company = ccInfo.company ?? c.company,
                    address1 = ccInfo.address1 ?? c.address1,
                    address2 = ccInfo.address2 ?? c.address2,
                    city = ccInfo.city ?? c.city,
                    state = ccInfo.state ?? c.state,
                    postalCode = ccInfo.postalCode ?? c.postalCode,
                    country = ccInfo.country ?? c.country,
                });
            }
            con.Close();
            return true;
        }

        //returns true if vault was updated or false if it does not exists
        public static bool UpdateAch(int accountId, string vaultId, AchDBDTO achInfo)
        {
            using var con = Global.Connection;
            con.Open();
            if (achInfo.vaultCustomerId == -1)
            {
                //we need to find out if vault exists
                achInfo.vaultCustomerId = con.ExecuteScalar<int>("SELECT Id FROM tblVaultCustomer WHERE accountId=@accountId AND vaultId=@vaultId", new
                {
                    accountId,
                    vaultId
                });
            }
            if (achInfo.vaultCustomerId <= 0)
                return false;

            var c = con.QueryFirstOrDefault<AchDBDTO>(@"SELECT [sequence],[aba],[dda],[accountType],[firstName],[lastName],[company],[address1],[address2],[city],[state],[postalCode],[country] 
                                FROM tblVaultAch WHERE [vaultCustomerId]=@vaultCustomerId AND [sequence]=@sequence", new
            {
                achInfo.vaultCustomerId,
                achInfo.sequence
            });

            if (c == null)
            {
                //we need to insert it

                con.ExecuteScalar<int>(@"INSERT INTO tblVaultAch([vaultCustomerId],[sequence],[aba],[dda],[accountType],[firstName],[lastName],[company],[address1],[address2],[city],[state],[postalCode],[country]) 
                    VALUES(@vaultCustomerId,@sequence,@aba,@dda,@accountType,@firstName,@lastName,@company,@address1,@address2,@city,@state,@postalCode,@country)", new
                {
                    achInfo.vaultCustomerId,
                    achInfo.sequence,
                    achInfo.aba,
                    achInfo.dda,
                    achInfo.accountType,
                    firstName = achInfo.firstName ?? "",
                    lastName = achInfo.lastName ?? "",
                    company = achInfo.company ?? "",
                    address1 = achInfo.address1 ?? "",
                    address2 = achInfo.address2 ?? "",
                    city = achInfo.city ?? "",
                    state = achInfo.state ?? "",
                    postalCode = achInfo.postalCode ?? "",
                    country = achInfo.country ?? "",
                });
            }
            else
            {
                //we need to update it
                con.Execute(@"UPDATE tblVaultAch SET [aba]=@aba,[dda]=@dda,[accountType]=@accountType,firstName=@firstName,lastName=@lastName,
                            company=@company,address1=@address1,address2=@address2,city=@city,state=@state,postalCode=@postalCode,country=@country WHERE [vaultCustomerId]=@vaultCustomerId AND [sequence]=@sequence", new
                {
                    achInfo.vaultCustomerId,
                    achInfo.sequence,
                    achInfo.aba,
                    achInfo.dda,
                    achInfo.accountType,
                    firstName = achInfo.firstName ?? c.firstName,
                    lastName = achInfo.lastName ?? c.lastName,
                    company = achInfo.company ?? c.company,
                    address1 = achInfo.address1 ?? c.address1,
                    address2 = achInfo.address2 ?? c.address2,
                    city = achInfo.city ?? c.city,
                    state = achInfo.state ?? c.state,
                    postalCode = achInfo.postalCode ?? c.postalCode,
                    country = achInfo.country ?? c.country,
                });
            }
            con.Close();
            return true;
        }

        public static bool DeleteCC(int accountId, string vaultId, int sequence)
        {
            using var con = Global.Connection;
            int recordsAffected = con.Execute("DELETE tblVaultCC FROM tblVaultCC a INNER join tblVaultCustomer b ON b.id=a.[vaultCustomerId] AND a.[sequence]=@sequence WHERE b.accountId=@accountId AND b.vaultId=@vaultId", new
            {
                accountId,
                vaultId,
                sequence
            });
            return recordsAffected == 1;
        }

        public static bool DeleteAch(int accountId, string vaultId, int sequence)
        {
            using var con = Global.Connection;
            int recordsAffected = con.Execute("DELETE tblVaultAch FROM tblVaultAch a INNER join tblVaultCustomer b ON b.id=a.[vaultCustomerId] AND a.[sequence]=@sequence WHERE b.accountId=@accountId AND b.vaultId=@vaultId", new {
                accountId,
                vaultId,
                sequence
            });

            return recordsAffected == 1;
        }
        public static bool DeleteVault(int accountId, string vaultId)
        {
            using var con = Global.Connection;
            int recordsAffected = con.Execute("DELETE tblVaultCC FROM tblVaultCC a INNER join tblVaultCustomer b ON b.id=a.[vaultCustomerId] WHERE b.accountId=@accountId AND b.vaultId=@vaultId; " +
                                                "DELETE tblVaultAch FROM tblVaultAch a INNER join tblVaultCustomer b ON b.id=a.[vaultCustomerId] WHERE b.accountId=@accountId AND b.vaultId=@vaultId; " +
                                                "DELETE [tblVaultCustomer] WHERE [accountId]=@accountId AND [vaultId]=@vaultId", new
            {
                accountId,
                vaultId,
            });

            return recordsAffected > 0;
        }

        public static IEnumerable<AchDBDTO> GetAllAch(int accountId, string vaultId)
        {
            using var con = Global.Connection;
            return con.Query<AchDBDTO>(@"SELECT a.[vaultCustomerId],a.[sequence],a.[aba],a.[dda],a.[accountType],a.[firstName],a.[lastName],a.[company],a.[address1],a.[address2],a.[city],a.[state],a.[postalCode],a.[country] FROM [tblVaultAch] a 
                INNER JOIN tblVaultCustomer b ON b.Id=a.vaultCustomerId WHERE b.vaultId=@vaultId AND b.accountId=@accountId", new { vaultId , accountId});
        }
        public static IEnumerable<CreditCardDBDTO> GetAllCC(int accountId, string vaultId)
        {
            using var con = Global.Connection;
            return con.Query<CreditCardDBDTO>(@"SELECT a.[vaultCustomerId],a.[sequence],a.[ccNum],a.[ccExpMonth],a.[ccExpYear],a.[ccType],a.[firstName],a.[lastName],a.[company],a.[address1],a.[address2],a.[city],a.[state],a.[postalCode],a.[country] FROM [tblVaultCC] a 
                INNER JOIN tblVaultCustomer b ON b.Id=a.vaultCustomerId WHERE b.vaultId=@vaultId AND b.accountId=@accountId", new { vaultId, accountId });
        }

        public static CustomerDBDTO GetVault(int accountId, string vaultId)
        {
            using var con = Global.Connection;
            return con.QueryFirstOrDefault<CustomerDBDTO>("SELECT [id],[accountId],[vaultId],[firstName],[lastName],[company],[address1],[address2],[city],[state],[postalCode],[country],[email],[phone] FROM tblVaultCustomer WHERE accountId=@accountId AND vaultId=@vaultId", new
            {
                accountId,
                vaultId
            });
        }
        public static CreditCardDBDTO GetCC(int accountId, string vaultId, int sequence)
        {
            using var con = Global.Connection;
            return con.QueryFirstOrDefault<CreditCardDBDTO>(@"SELECT a.[sequence],a.[ccNum],a.[ccExpMonth],a.[ccExpYear],a.[ccType],a.[firstName],a.[lastName],a.[company],a.[address1],a.[address2],a.[city],a.[state],a.[postalCode],a.[country]
                                    FROM tblVaultCC a INNER join tblVaultCustomer b ON b.id=a.[vaultCustomerId] AND a.[sequence]=@sequence WHERE b.accountId=@accountId AND b.vaultId=@vaultId", new
            {
                accountId,
                vaultId,
                sequence
            });
        }
        public static AchDBDTO GetAch(int accountId, string vaultId, int sequence)
        {
            using var con = Global.Connection;
            return con.QueryFirstOrDefault<AchDBDTO>(@"SELECT a.[sequence],a.[aba],a.[dda],a.[accountType],a.[firstName],a.[lastName],a.[company],a.[address1],a.[address2],a.[city],a.[state],a.[postalCode],a.[country]
                                    FROM tblVaultAch a INNER join tblVaultCustomer b ON b.id=a.[vaultCustomerId] AND a.[sequence]=@sequence WHERE b.accountId=@accountId AND b.vaultId=@vaultId", new
            {
                accountId,
                vaultId,
                sequence
            });
        }


       
    }
}
