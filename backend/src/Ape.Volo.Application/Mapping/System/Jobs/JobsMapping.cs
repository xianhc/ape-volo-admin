using Ape.Volo.Application.Contracts.System.Jobs.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.System;

[assembly: AutoMapping(typeof(Job), typeof(CreateUpdateJobDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Job), typeof(JobVo))]
