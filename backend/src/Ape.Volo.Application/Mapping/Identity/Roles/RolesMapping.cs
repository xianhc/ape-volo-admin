using Ape.Volo.Application.Contracts.Identity.Roles.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Identity.Roles;

[assembly: AutoMapping(typeof(Role), typeof(CreateUpdateRoleDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Role), typeof(RoleSmallVo))]
[assembly: AutoMapping(typeof(Role), typeof(RoleVo))]
