using Ape.Volo.Application.Contracts.Permission.Departments.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Permission;

[assembly: AutoMapping(typeof(Department), typeof(CreateUpdateDepartmentDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Department), typeof(DepartmentSmallVo))]
[assembly: AutoMapping(typeof(Department), typeof(DepartmentVo))]
