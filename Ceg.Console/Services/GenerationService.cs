using System;
using System.Collections.Generic;
using System.IO;
using Ceg.ConsoleApp.Extensions;
using Ceg.Model;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using NLog;


namespace Ceg.Services
{
    public sealed class GenerationService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


        public void Generate(Options options)
        {
            var meta = RetrieveMetadata(options);

            if (options.GenerateJson)
            {
                var path = Path.Combine(options.OutputDir, "metadata.json");
                
                _logger.Info($"Writing serialized metadata to {path}...");

                var json = JsonConvert.SerializeObject(meta, Formatting.Indented);
                path.EnsureDirectoryExists();
                File.WriteAllText(path, json);

                _logger.Info("Done.");
            }
            else
            {
                using (var codeGenService = new CodeGenerationService())
                {
                    codeGenService.GenerateCode(meta, options.Namespace, options.OutputDir);
                }
            }
        }


        private static List<EntityMetadata> RetrieveMetadata(Options options)
        {
            using (var orgSvc = new CrmServiceClient(options.ConnectionString))
            {
                if (!orgSvc.IsReady)
                {
                    throw new Exception(orgSvc.LastCrmError, orgSvc.LastCrmException);
                }

                var metaSvc = new MetadataService(orgSvc);
                return metaSvc.GetMetadata(options.Entities);
            }
        }
    }
}
