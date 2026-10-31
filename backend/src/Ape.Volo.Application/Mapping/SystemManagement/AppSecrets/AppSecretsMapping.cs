using Ape.Volo.Application.Contracts.SystemManagement.AppSecrets.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.SystemManagement;

[assembly: AutoMapping(typeof(AppSecret), typeof(AppSecretVo))]
[assembly: AutoMapping(typeof(AppSecret), typeof(CreateUpdateAppSecretDto), ToEntity = true)]
