using Ape.Volo.Application.Contracts.Message.Email.Templates.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Message.Email;

[assembly: AutoMapping(typeof(EmailMessageTemplate), typeof(CreateUpdateEmailMessageTemplateDto), ToEntity = true)]
[assembly: AutoMapping(typeof(EmailMessageTemplate), typeof(EmailMessageTemplateVo))]
