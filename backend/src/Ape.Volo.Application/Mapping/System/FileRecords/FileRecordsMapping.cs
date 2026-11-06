using Ape.Volo.Application.Contracts.System.FileRecords.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.System;

[assembly: AutoMapping(typeof(FileRecord), typeof(CreateUpdateFileRecordDto), ToEntity = true)]
[assembly: AutoMapping(typeof(FileRecord), typeof(FileRecordVo))]
