using Microsoft.AspNetCore.Mvc;
using PaymentDTO;
using PaymentDTO.Vault;
using PaymentWebService;
using System;
using System.Collections.Generic;

namespace PaymentWebService.Controllers.V1
{
    [Route("v1/vault")]
    [ApiController]
    [ApiExceptionFilter]
    public class VaultController : ControllerBase
    {
        [HttpPost("DeleteVault")]
        public ResponseDTO DeleteVault(VaultRequest rq)
        {
            return Execute(rq, new ResponseDTO(), (accountId, rq, rp) =>
            {
                if (!Vault.Vault.DeleteVault(accountId, rq.vaultId))
                {
                    rp.errorCode = (int)PaymentDTO.Vault.ERRORCODE.NOTFOUND;
                    rp.errors = new List<string>() { $"Vault {rq.vaultId} not found" };
                }
            });
        }
        [HttpPost("DeleteCC")]
        public ResponseDTO DeleteCC(VaultSequenceRequest rq)
        {
            return Execute(rq, new ResponseDTO(), (accountId, rq, rp) =>
            {
                if (!Vault.Vault.DeleteCC(accountId, rq.vaultId, rq.sequence))
                {
                    rp.errorCode = (int)PaymentDTO.Vault.ERRORCODE.NOTFOUND;
                    rp.errors = new List<string>() { $"Vault {rq.vaultId}, sequence {rq.sequence} not found" };
                }
            });
        }
        [HttpPost("DeleteAch")]
        public ResponseDTO DeleteAch(VaultSequenceRequest rq)
        {
            return Execute(rq, new ResponseDTO(), (accountId, rq, rp) =>
            {
                if (!Vault.Vault.DeleteAch(accountId, rq.vaultId, rq.sequence))
                {
                    rp.errorCode = (int)PaymentDTO.Vault.ERRORCODE.NOTFOUND;
                    rp.errors = new List<string>() { $"Vault {rq.vaultId}, sequence {rq.sequence} not found" };
                }
            });
        }

        [HttpPost("GetAllCC")]
        public GetAllCCResponse GetAllCC(VaultRequest rq)
        {
            return Execute(rq, new GetAllCCResponse(), (accountId, rq, rp) =>
            {
                var lst = Vault.Vault.GetAllCC(accountId, rq.vaultId);
                List<PaymentDTO.Vault.CreditCard> l = new List<PaymentDTO.Vault.CreditCard>();
                foreach (var cc in lst)
                    l.Add(Global._mapper.Map<PaymentDTO.Vault.CreditCard>(cc));
                rp.data = l;
            });
        }

        [HttpPost("GetAllAch")]
        public GetAllAchResponse GetAllAch(VaultRequest rq)
        {
            return Execute(rq, new GetAllAchResponse(), (accountId, rq, rp) =>
            {
                var lst = Vault.Vault.GetAllAch(accountId, rq.vaultId);
                List<PaymentDTO.Vault.Ach> l = new List<PaymentDTO.Vault.Ach>();
                foreach (var o in lst)
                    l.Add(Global._mapper.Map<PaymentDTO.Vault.Ach>(o));
                rp.data = l;
            });
        }


        [HttpPost("GetVault")]
        public GetVaultResponse GetVault(VaultRequest rq)
        {
            return Execute(rq, new GetVaultResponse(), (accountId, rq, rp) =>
            {
                var v = Vault.Vault.GetVault(accountId, rq.vaultId);
                if (v == null)
                {
                    rp.errorCode = (int)PaymentDTO.Vault.ERRORCODE.NOTFOUND;
                    rp.errors = new List<string>() { $"Vault {rq.vaultId} not found" };
                }
                else
                    rp.data = Global._mapper.Map<PaymentDTO.Vault.Vault>(v);
            });
        }

        [HttpPost("GetCC")]
        public GetCCResponse GetCC(VaultSequenceRequest rq)
        {
            return Execute(rq, new GetCCResponse(), (accountId, rq, rp) =>
            {
                var v = Vault.Vault.GetCC(accountId, rq.vaultId, rq.sequence);
                if (v == null)
                {
                    rp.errorCode = (int)PaymentDTO.Vault.ERRORCODE.NOTFOUND;
                    rp.errors = new List<string>() { $"Vault {rq.vaultId}, sequence {rq.sequence} not found" };
                }
                else
                    rp.data = Global._mapper.Map<PaymentDTO.Vault.CreditCard>(v);
            });
        }

        [HttpPost("GetAch")]
        public GetAchResponse GetAch(VaultSequenceRequest rq)
        {
            return Execute(rq, new GetAchResponse(), (accountId, rq, rp) =>
            {
                var v = Vault.Vault.GetAch(accountId, rq.vaultId, rq.sequence);
                if (v == null)
                {
                    rp.errorCode = (int)PaymentDTO.Vault.ERRORCODE.NOTFOUND;
                    rp.errors = new List<string>() { $"Vault {rq.vaultId}, sequence {rq.sequence} not found" };
                }
                else
                    rp.data = Global._mapper.Map<PaymentDTO.Vault.Ach>(v);
            });
        }

        [HttpPost("UpdateVault")]
        public ResponseDTO UpdateVault(UpdateVaultRequest rq)
        {
            return Execute(rq, new ResponseDTO(), (accountId, rq, rp) =>
            {
                var c = Global._mapper.Map<VaultDTO.CustomerDBDTO>(rq.data);
                Vault.Vault.UpdateVault(accountId, c);
            });
        }

        [HttpPost("UpdateCC")]
        public ResponseDTO UpdateCC(UpdateCCRequest rq)
        {
            return Execute(rq, new ResponseDTO(), (accountId, rq, rp) =>
            {
                var c = Global._mapper.Map<VaultDTO.CreditCardDBDTO>(rq.data);
                Vault.Vault.UpdateCC(accountId, rq.vaultId, c);
            });
        }
        [HttpPost("UpdateAch")]
        public ResponseDTO UpdateAch(UpdateAchRequest rq)
        {
            return Execute(rq, new ResponseDTO(), (accountId, rq, rp) =>
            {
                var c = Global._mapper.Map<VaultDTO.AchDBDTO>(rq.data);
                Vault.Vault.UpdateAch(accountId, rq.vaultId, c);
            });
        }

        TOut Execute<TIn, TOut>(TIn rq, TOut rp, Action<int, TIn, TOut> action)
            where TIn : AuthroizedRequestDTO
            where TOut : ResponseDTO
        {
            /*
            authentication
            */
            int accountId = 1;
            action(accountId, rq, rp);
            return rp;
        }
    }
}
