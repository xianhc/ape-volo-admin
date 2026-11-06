using Ape.Volo.Entity.Organization.Departments;
using Ape.Volo.Application.Contracts.Organization.Departments.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Identity;

[assembly: AutoMapping(typeof(Department), typeof(CreateUpdateDepartmentDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Department), typeof(DepartmentSmallVo))]
[assembly: AutoMapping(typeof(Department), typeof(DepartmentVo))]
