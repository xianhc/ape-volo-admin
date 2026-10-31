using Ape.Volo.Application.Contracts.SystemManagement.FileRecords.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.SystemManagement;

[assembly: AutoMapping(typeof(FileRecord), typeof(CreateUpdateFileRecordDto), ToEntity = true)]
[assembly: AutoMapping(typeof(FileRecord), typeof(FileRecordVo))]
