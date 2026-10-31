using Ape.Volo.Application.Contracts.Permission.Menus.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Permission;

[assembly: AutoMapping(typeof(Menu), typeof(CreateUpdateMenuDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Menu), typeof(MenuVo))]
