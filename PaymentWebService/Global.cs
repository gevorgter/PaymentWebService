using AutoMapper;
using CommonDTO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PaymentDTO.Payment;
using System;
using System.Reflection;

namespace PaymentWebService
{
    public static class Global
    {
        static string _sqlConnection;

        public static IMapper _mapper;
        public static Random _rnd = new Random();


        public static void Init(string sqlConnection)
        {
            _sqlConnection = sqlConnection;
        }

        public static void Init(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            Init(configuration.GetConnectionString("connectionString"));

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<VaultDTO.CreditCardDBDTO, PaymentDTO.Vault.CreditCard>();
                cfg.CreateMap<VaultDTO.AchDBDTO, PaymentDTO.Vault.Ach>();
                cfg.CreateMap<VaultDTO.CustomerDBDTO, PaymentDTO.Vault.Vault>();

                cfg.CreateMap<PaymentDTO.Vault.CreditCard, VaultDTO.CreditCardDBDTO>();
                cfg.CreateMap<PaymentDTO.Vault.Ach, VaultDTO.AchDBDTO>();
                cfg.CreateMap<PaymentDTO.Vault.Vault, VaultDTO.CustomerDBDTO>();

                cfg.CreateMap<SaleRequestDTO, SaleTransaction>();
                cfg.CreateMap<AuthRequestDTO, AuthTransaction>();
                cfg.CreateMap<SaleTransaction, SaleRequestDTO>();
                cfg.CreateMap<AuthTransaction, AuthRequestDTO>();
            });

            _mapper = config.CreateMapper();

            //register all Transaction objects
            Type transactionType = typeof(Transaction);
            Type paymentInfoType = typeof(PaymentInfo);
            Type eventDataType = typeof(EventData);
            Type hookDataType = typeof(HookData);
            foreach (var t in transactionType.Assembly.GetTypes())
            {
                if (!t.IsClass || t.IsAbstract)
                    continue;

                if (transactionType.IsAssignableFrom(t))
                {
                    Helpers.JsonHelper.RegisterType(t);
                    continue;
                }
                if (paymentInfoType.IsAssignableFrom(t))
                {
                    Helpers.JsonHelper.RegisterType(t);
                    continue;
                }
                if (eventDataType.IsAssignableFrom(t))
                {
                    Helpers.JsonHelper.RegisterType(t);
                    continue;
                }
                if (hookDataType.IsAssignableFrom(t))
                {
                    Helpers.JsonHelper.RegisterType(t);
                    continue;
                }
            }


        }

        public static SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_sqlConnection);
            }
        }

    }
}
