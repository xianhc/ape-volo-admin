using Ape.Volo.Application.Contracts.System.Settings.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.System;

[assembly: AutoMapping(typeof(Setting), typeof(CreateUpdateSettingDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Setting), typeof(SettingVo))]
