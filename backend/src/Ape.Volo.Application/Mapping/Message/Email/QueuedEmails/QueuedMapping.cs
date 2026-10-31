using Ape.Volo.Application.Contracts.Message.Email.QueuedEmails.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Message.Email.QueuedEmails;

[assembly: AutoMapping(typeof(QueuedEmail), typeof(CreateUpdateQueuedEmailDto), ToEntity = true)]
[assembly: AutoMapping(typeof(QueuedEmail), typeof(QueuedEmailVo))]
