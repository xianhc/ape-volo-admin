using Ape.Volo.Application.Contracts.Message.Email.Accounts.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Message.Email;

[assembly: AutoMapping(typeof(EmailAccount), typeof(CreateUpdateEmailAccountDto), ToEntity = true)]
[assembly: AutoMapping(typeof(EmailAccount), typeof(EmailAccountVo))]
