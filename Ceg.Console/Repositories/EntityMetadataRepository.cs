using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;


namespace Ceg.Repositories
{
    public sealed class EntityMetadataRepository
    {
        private static readonly string[] _entityProperties =
        {
            "Attributes",
            "DisplayName",
            "PrimaryIdAttribute",
            "PrimaryNameAttribute",
            "Description",
            "OneToManyRelationships",
            "ManyToManyRelationships"
        };

        private static readonly string[] _attributeProperties =
        {
            "OptionSet",
            "AttributeType",
            "DisplayName",
            "Description"
        };

        private static readonly string[] _relationshipProperties =
        {
            "SchemaName",
            "RelationShipType"
        };

        private static readonly AttributeTypeCode[] _excludedAttributeTypes =
        {
            AttributeTypeCode.BigInt,
            AttributeTypeCode.CalendarRules,
            AttributeTypeCode.EntityName,
            AttributeTypeCode.ManagedProperty,
            AttributeTypeCode.Virtual
        };


        private static EntityQueryExpression Query => new EntityQueryExpression
        {
            Properties = new MetadataPropertiesExpression(_entityProperties)
            {
                AllProperties = false
            },
            AttributeQuery = new AttributeQueryExpression
            {
                Properties = new MetadataPropertiesExpression(_attributeProperties)
                {
                    AllProperties = false
                },
                Criteria = new MetadataFilterExpression
                {
                    Conditions =
                    {
                        new MetadataConditionExpression("AttributeType", MetadataConditionOperator.NotIn, _excludedAttributeTypes),
                        new MetadataConditionExpression("AttributeOf", MetadataConditionOperator.Equals, null)
                    }
                }
            },
            RelationshipQuery = new RelationshipQueryExpression
            {
                Properties = new MetadataPropertiesExpression(_relationshipProperties)
                {
                    AllProperties = false
                }
            }
        };


        private readonly IOrganizationService _service;


        public ICollection<EntityMetadata> AllEntitiesMetadata => SendRequest(Query);


        public EntityMetadataRepository(IOrganizationService service)
        {
            _service = service;
        }

        public ICollection<EntityMetadata> GetEntityMetadata(params string[] entityNames)
        {
            var query = Query;
            query.Criteria.Conditions.Add(new MetadataConditionExpression("LogicalName", MetadataConditionOperator.In, entityNames));

            return SendRequest(query);
        }


        private ICollection<EntityMetadata> SendRequest(EntityQueryExpression query)
        {
            var request = new RetrieveMetadataChangesRequest
            {
                Query = query,
                ClientVersionStamp = null
            };
            var response = (RetrieveMetadataChangesResponse)_service.Execute(request);
            return response.EntityMetadata;
        }
    }
}
