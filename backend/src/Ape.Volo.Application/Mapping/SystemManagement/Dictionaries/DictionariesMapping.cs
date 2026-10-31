using Ape.Volo.Application.Contracts.SystemManagement.Dictionaries.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.SystemManagement.Dictionaries;

[assembly: AutoMapping(typeof(Dict), typeof(CreateUpdateDictDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Dict), typeof(DictVo))]
[assembly: AutoMapping(typeof(DictDetail), typeof(CreateUpdateDictDetailDto), ToEntity = true)]
[assembly: AutoMapping(typeof(DictDetail), typeof(DictDetailVo))]
