using Ape.Volo.Application.Contracts.Organization.Positions.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Organization.Positions;

[assembly: AutoMapping(typeof(Position), typeof(CreateUpdatePositionDto), ToEntity = true)]
[assembly: AutoMapping(typeof(Position), typeof(PositionSmallVo))]
[assembly: AutoMapping(typeof(Position), typeof(PositionVo))]
