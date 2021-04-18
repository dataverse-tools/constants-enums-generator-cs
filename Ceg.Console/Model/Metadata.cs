using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Metadata;
using XrmAttributeMetadata = Microsoft.Xrm.Sdk.Metadata.AttributeMetadata;
using XrmEntityMetadata = Microsoft.Xrm.Sdk.Metadata.EntityMetadata;
using XrmOptionMetadata = Microsoft.Xrm.Sdk.Metadata.OptionMetadata;


namespace Ceg.Model
{
    public abstract class Metadata
    {
        public string LogicalName { get; set; }
        public string DisplayName { get; set; }
        public string VariableName { get; set; }
        public string Description { get; set; }
    }

    public sealed class EntityMetadata : Metadata
    {
        public List<AttributeMetadata> Attributes { get; set; }


        public EntityMetadata(XrmEntityMetadata entity)
        {
            LogicalName = entity.LogicalName;
            DisplayName = entity.DisplayName.UserLocalizedLabel?.Label;
            Description = entity.Description.UserLocalizedLabel?.Label;

            PopulateAttributes(entity);
        }


        private void PopulateAttributes(XrmEntityMetadata entity)
        {
            Attributes = new List<AttributeMetadata>(
                3 +
                entity.Attributes.Length +
                entity.OneToManyRelationships.Length +
                entity.ManyToManyRelationships.Length);

            var attrs = new Dictionary<string, string>
            {
                ["LogicalName"] = entity.LogicalName,
                ["Id"] = entity.PrimaryIdAttribute
            };

            var entityAttrs = attrs.Select(attr => new AttributeMetadata(attr.Value, attr.Key));
            Attributes.AddRange(entityAttrs);

            if (!string.IsNullOrEmpty(entity.PrimaryNameAttribute))
            {
                Attributes.Add(new AttributeMetadata(entity.PrimaryNameAttribute, "PrimaryAttribute"));
            }

            var meta = entity.Attributes
                .OrderBy(a => a.LogicalName)
                .Select(a => new AttributeMetadata(a));
            Attributes.AddRange(meta);

            PopulateRelationships(entity.OneToManyRelationships);
            PopulateRelationships(entity.ManyToManyRelationships);
        }

        private void PopulateRelationships(IEnumerable<RelationshipMetadataBase> relationshipsMetadata)
        {
            var meta = relationshipsMetadata
                .OrderBy(r => r.SchemaName)
                .Select(r => new AttributeMetadata(r.SchemaName));
            Attributes.AddRange(meta);
        }
    }

    public sealed class AttributeMetadata : Metadata
    {
        public AttributeTypeCode? Type { get; set; }
        public List<OptionMetadata> Options { get; set; }


        public AttributeMetadata(XrmAttributeMetadata attribute)
            : this(attribute.LogicalName, attribute.DisplayName.UserLocalizedLabel?.Label, attribute.LogicalName)
        {
            Type = attribute.AttributeType;
            Description = attribute.Description.UserLocalizedLabel?.Label;

            AddOptions(attribute);
        }

        public AttributeMetadata(string relationshipName) : this(relationshipName, null, relationshipName) { }

        public AttributeMetadata(string logicalName, string displayName) : this(logicalName, displayName, displayName) { }

        public AttributeMetadata(string logicalName, string displayName, string variableName)
        {
            LogicalName = logicalName;
            DisplayName = displayName;
            VariableName = variableName;
        }


        private static OptionMetadataCollection GetOptionListFromOptionSet(XrmAttributeMetadata attribute)
        {
            switch (attribute.AttributeType)
            {
                case AttributeTypeCode.Picklist:
                    return ((PicklistAttributeMetadata)attribute).OptionSet.Options;

                case AttributeTypeCode.State:
                    return ((StateAttributeMetadata)attribute).OptionSet.Options;

                case AttributeTypeCode.Status:
                    return ((StatusAttributeMetadata)attribute).OptionSet.Options;

                case AttributeTypeCode.Boolean:
                    var optionSet = ((BooleanAttributeMetadata)attribute).OptionSet;
                    return new OptionMetadataCollection()
                    {
                        new XrmOptionMetadata(optionSet.FalseOption.Label, 0),
                        new XrmOptionMetadata(optionSet.TrueOption.Label, 1)
                    };

                default:
                    return new OptionMetadataCollection();
            }
        }

        private void AddOptions(XrmAttributeMetadata attribute)
        {
            Options = GetOptionListFromOptionSet(attribute).Select(o => new OptionMetadata(o)).ToList();
        }
    }

    public sealed class OptionMetadata
    {
        public string Label { get; set; }
        public int? Value { get; set; }

        public OptionMetadata(XrmOptionMetadata option)
        {
            Label = option.Label.UserLocalizedLabel?.Label;
            Value = option.Value;
        }
    }
}
