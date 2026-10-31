using Ape.Volo.Application.Contracts.Organization.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Organization;

[assembly: AutoMapping(typeof(Position), typeof(CreateUpdatePositionDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Position), typeof(PositionSmallVo))]
[assembly: AutoMapping(typeof(Position), typeof(PositionVo))]
