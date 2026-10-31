using Ape.Volo.Application.Contracts.Permission.Users.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Organization;
using Ape.Volo.Entity.Permission;
using Ape.Volo.Entity.Permission.Roles;
using Ape.Volo.Entity.Permission.Users;

[assembly: AutoMapping(typeof(Department), typeof(UserDeptDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Position), typeof(UserPositionDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Role), typeof(UserRoleDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Role), typeof(UserRoleVo))]
[assembly: AutoMapping(typeof(User), typeof(CreateUpdateUserDto), ToEntity = true)]
[assembly: AutoMapping(typeof(User), typeof(UserVo))]
