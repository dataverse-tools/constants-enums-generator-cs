using System.Collections.Generic;
using System.Linq;
using Ceg.Repositories;
using Microsoft.Xrm.Sdk;
using NLog;


namespace Ceg.Services
{
    public sealed class MetadataService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly EntityMetadataRepository _metaRepo;


        public MetadataService(IOrganizationService orgSvc)
        {
            _metaRepo = new EntityMetadataRepository(orgSvc);
        }


        public List<Model.EntityMetadata> GetMetadata(IList<string> entities)
        {
            _logger.Info("Retrieving metadata...");

            var metadata = entities.Contains("all") ? _metaRepo.AllEntitiesMetadata : _metaRepo.GetEntityMetadata(entities.ToArray());

            _logger.Info("Generating metadata objects...");
            return metadata.Select(meta => new Model.EntityMetadata(meta)).ToList();
        }
    }
}
