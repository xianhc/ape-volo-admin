using Ape.Volo.Application.Contracts.System.AppSecrets.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.System;

[assembly: AutoMapping(typeof(AppSecret), typeof(AppSecretVo))]
[assembly: AutoMapping(typeof(AppSecret), typeof(CreateUpdateAppSecretDto), ToEntity = true)]
