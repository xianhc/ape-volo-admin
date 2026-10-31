using Ape.Volo.Application.Contracts.Permission.Roles.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Permission.Roles;

[assembly: AutoMapping(typeof(Role), typeof(CreateUpdateRoleDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Role), typeof(RoleSmallVo))]
[assembly: AutoMapping(typeof(Role), typeof(RoleVo))]
