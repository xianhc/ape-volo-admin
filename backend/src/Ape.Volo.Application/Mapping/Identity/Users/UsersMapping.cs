using Ape.Volo.Application.Contracts.Identity.Users.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Organization.Departments;
using Ape.Volo.Entity.Organization.Positions;
using Ape.Volo.Entity.Identity;
using Ape.Volo.Entity.Identity.Roles;
using Ape.Volo.Entity.Identity.Users;

[assembly: AutoMapping(typeof(Department), typeof(UserDeptDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Position), typeof(UserPositionDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Role), typeof(UserRoleDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Role), typeof(UserRoleVo))]
[assembly: AutoMapping(typeof(User), typeof(CreateUpdateUserDto), ToEntity = true)]
[assembly: AutoMapping(typeof(User), typeof(UserVo))]
