using Ape.Volo.Application.Contracts.SystemManagement.Settings.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.SystemManagement;

[assembly: AutoMapping(typeof(Setting), typeof(CreateUpdateSettingDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Setting), typeof(SettingVo))]
