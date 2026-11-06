using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Operations;

[assembly: AutoMapping(typeof(ScheduledJob), typeof(CreateUpdateScheduledJobDto), ToEntity = true)]
[assembly: AutoMapping(typeof(ScheduledJob), typeof(ScheduledJobVo))]
