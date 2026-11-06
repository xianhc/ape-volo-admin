using Ape.Volo.Application.Contracts.Identity.Menus.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Identity;

[assembly: AutoMapping(typeof(Menu), typeof(CreateUpdateMenuDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Menu), typeof(MenuVo))]
