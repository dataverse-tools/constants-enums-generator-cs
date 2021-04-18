using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Ceg.ConsoleApp.Extensions;
using Ceg.Model;
using Microsoft.CSharp;
using NLog;
using XrmMetadata = Microsoft.Xrm.Sdk.Metadata;


namespace Ceg.Services
{
    public sealed class CodeGenerationService : IDisposable
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private static readonly CodeGeneratorOptions _options = new CodeGeneratorOptions
        {
            BlankLinesBetweenMembers = false,
            BracingStyle = "C"
        };
        private readonly CodeDomProvider _provider = new CSharpCodeProvider();


        public void Dispose()
        {
            _provider.Dispose();
        }


        public void GenerateCode(List<EntityMetadata> metadata, string namespaceName, string outputDir)
        {
            GenerateCodeForConstants(metadata, namespaceName, outputDir);
            GenerateCodeForEnums(metadata, namespaceName, outputDir);
        }


        #region Enums
        private void GenerateCodeForEnums(List<EntityMetadata> metadata, string baseNamespaceName, string outputDir)
        {
            _logger.Info("Generating code files...");

            var namespaceName = baseNamespaceName + ".Enums";
            foreach (var meta in metadata)
            {
                GenerateEnumsCodeFile(meta, namespaceName, outputDir);
            }
        }

        private void GenerateEnumsCodeFile(EntityMetadata entityMetadata, string namespaceName, string outputDir)
        {
            if (entityMetadata == null)
            {
                throw new ArgumentNullException("entityMetadata");
            }

            if (entityMetadata.Attributes.Count == 0)
            {
                _logger.Warn("There is nothing to generate for '{0}'", entityMetadata.LogicalName);
                return;
            }

            _logger.Info("Start processing {0} ({1})", entityMetadata.DisplayName, entityMetadata.LogicalName);

            var targetNamespace = GenerateEnumsNamespaceFromEntityMetadata(entityMetadata, namespaceName);
            if (targetNamespace != null)
            {
                var targetUnit = new CodeCompileUnit();
                targetUnit.Namespaces.Add(targetNamespace);
                using (var sourceWriter = new StreamWriter(GetSourceFileName(outputDir, "Enums", FormatIdentifier(entityMetadata))))
                {
                    _provider.GenerateCodeFromCompileUnit(targetUnit, sourceWriter, _options);
                }
            }

            _logger.Info("Done");
        }

        private CodeNamespace GenerateEnumsNamespaceFromEntityMetadata(EntityMetadata entity, string namespaceName)
        {
            var targetNamespace = new CodeNamespace(namespaceName);
            AddComment(targetNamespace.Comments, entity);

            var entityName = FormatIdentifier(entity);
            foreach (var attribute in entity.Attributes)
            {
                var targetType = GenerateTypeFromAttributeMetadata(attribute, entityName, entity.LogicalName);
                if (targetType != null)
                {
                    targetNamespace.Types.Add(targetType);
                }
            }

            if (targetNamespace.Types.Count > 0)
            {
                return targetNamespace;
            }

            _logger.Warn("There is nothing to generate for '{0}'", entity.LogicalName);
            return null;
        }

        private CodeTypeDeclaration GenerateTypeFromAttributeMetadata(AttributeMetadata attribute, string entityName, string entityLogicalName)
        {
            var className = FormatIdentifier(attribute);
            var prefix = className.StartsWith(entityName) ? string.Empty : entityName;
            var displayName = attribute.DisplayName ?? string.Empty;

            switch (attribute.Type)
            {
                case XrmMetadata.AttributeTypeCode.Picklist:
                    return GenerateTypeFromOptionSetMetadata(attribute, prefix + className, entityLogicalName);

                case XrmMetadata.AttributeTypeCode.State:
                    return GenerateTypeFromOptionSetMetadata(attribute, entityName + "StateCode", entityLogicalName);

                case XrmMetadata.AttributeTypeCode.Status:
                    return GenerateTypeFromOptionSetMetadata(attribute, entityName + "StatusCode", entityLogicalName);

                case XrmMetadata.AttributeTypeCode.Boolean:
                    return
                        className.StartsWith("_") ||
                        displayName.StartsWith("Is ") ||
                        displayName.StartsWith("Are ") ||
                        displayName.StartsWith("Has ") ||
                        displayName.StartsWith("Have ") ||
                        displayName.StartsWith("Do ") ||
                        displayName.StartsWith("Does ")
                        ?
                        GenerateTypeFromBooleanMetadata(attribute, prefix + className, entityLogicalName) :
                        GenerateTypeFromBooleanMetadata(attribute, prefix + "Is" + className, entityLogicalName);

                default:
                    return null;
            }
        }

        private CodeTypeDeclaration GenerateTypeFromOptionSetMetadata(AttributeMetadata attribute, string enumName, string entityLogicalName)
        {
            var typeDeclaration = new CodeTypeDeclaration(enumName)
            {
                IsEnum = true,
                TypeAttributes = TypeAttributes.Public
            };
            AddComment(typeDeclaration.Comments, attribute);

            foreach (var option in attribute.Options)
            {
                var targetMember = GenerateMemberFromOptionMetadata(attribute, option, entityLogicalName);
                if (targetMember != null)
                {
                    typeDeclaration.Members.Add(targetMember);
                }
            }

            if (typeDeclaration.Members.Count > 0)
            {
                return typeDeclaration;
            }

            _logger.Warn("There is nothing to generate for '{0}' attribute of '{1}'", attribute.LogicalName, entityLogicalName);
            return null;
        }

        private CodeMemberField GenerateMemberFromOptionMetadata(AttributeMetadata attribute, OptionMetadata option, string entityLogicalName)
        {
            if (option.Value == null)
            {
                _logger.Warn("There is nothing to generate for '{0}' option of '{1}' attribute of '{2}'", option.Label, attribute.LogicalName, entityLogicalName);
                return null;
            }

            var memberDeclaration = new CodeMemberField
            {
                Name = FormatIdentifier(option),
                InitExpression = new CodePrimitiveExpression(option.Value)
            };
            AddComment(memberDeclaration.Comments, option);
            return memberDeclaration;
        }

        private CodeTypeDeclaration GenerateTypeFromBooleanMetadata(AttributeMetadata attribute, string className, string entityLogicalName)
        {
            var typeDeclaration = new CodeTypeDeclaration(className)
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };
            AddComment(typeDeclaration.Comments, attribute);

            foreach (var option in attribute.Options)
            {
                var targetMember = GenerateMemberFromBooleanMetadata(option);
                if (targetMember != null)
                {
                    typeDeclaration.Members.Add(targetMember);
                }
            }

            if (typeDeclaration.Members.Count > 0)
            {
                return typeDeclaration;
            }

            _logger.Warn("There is nothing to generate for '{0}' attribute of '{1}'", attribute.LogicalName, entityLogicalName);
            return null;
        }

        private CodeMemberField GenerateMemberFromBooleanMetadata(OptionMetadata option)
        {
            var constant = GenerateConstant(FormatIdentifier(option), Convert.ToBoolean(option.Value.GetValueOrDefault()));
            AddComment(constant.Comments, option);
            return constant;
        }
        #endregion


        #region Constants
        private void GenerateCodeForConstants(List<EntityMetadata> metadata, string baseNamespaceName, string outputDir)
        {
            _logger.Info("Generating code files...");

            var namespaceName = baseNamespaceName + ".Constants";
            foreach (var meta in metadata)
            {
                GenerateConstantsCodeFile(meta, namespaceName, outputDir);
            }
        }

        private void GenerateConstantsCodeFile(EntityMetadata entityMetadata, string namespaceName, string outputDir)
        {
            if (entityMetadata == null)
            {
                throw new ArgumentNullException("entityMetadata");
            }

            if (entityMetadata.Attributes.Count == 0)
            {
                _logger.Warn("There is nothing to generate for '{0}'", entityMetadata.LogicalName);
                return;
            }

            _logger.Info("Start processing {0} ({1})...", entityMetadata.DisplayName, entityMetadata.LogicalName);

            var targetNamespace = GenerateConstantsNamespaceFromEntityMetadata(entityMetadata, namespaceName);
            if (targetNamespace != null)
            {
                var targetUnit = new CodeCompileUnit();
                targetUnit.Namespaces.Add(targetNamespace);
                using (var sourceWriter = new StreamWriter(GetSourceFileName(outputDir, "Constants", FormatIdentifier(entityMetadata))))
                {
                    _provider.GenerateCodeFromCompileUnit(targetUnit, sourceWriter, _options);
                }
            }

            _logger.Info("Done");
        }

        private CodeNamespace GenerateConstantsNamespaceFromEntityMetadata(EntityMetadata entity, string namespaceName)
        {
            var targetClass = GenerateClassFromAttributesMetadata(entity);
            if (targetClass != null)
            {
                var targetNamespace = new CodeNamespace(namespaceName);
                targetNamespace.Types.Add(targetClass);
                return targetNamespace;
            }

            _logger.Warn("There is nothing to generate for '{0}'", entity.LogicalName);
            return null;
        }

        private CodeTypeDeclaration GenerateClassFromAttributesMetadata(EntityMetadata entity)
        {
            var className = FormatIdentifier(entity);
            var typeDeclaration = new CodeTypeDeclaration(className)
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed
            };
            AddComment(typeDeclaration.Comments, entity);

            foreach (var attribute in entity.Attributes)
            {
                var constant = GenerateConstant(attribute.VariableName, attribute.LogicalName);
                AddComment(constant.Comments, attribute);
                typeDeclaration.Members.Add(constant);
            }

            if (typeDeclaration.Members.Count > 0)
            {
                return typeDeclaration;
            }

            _logger.Warn("There is nothing to generate for '{0}'", entity.LogicalName);
            return null;
        }
        #endregion


        #region Helper Methods
        public string NormalizeLabel(string label, string fallback)
        {
            if (string.IsNullOrEmpty(label))
            {
                return ValidateAndUseFallback(fallback);
            }

            var result = label
                .ReplaceEx(@"\[(.+?)\]", @"_$1_")                  // Replace square brackets with underscores (for [DEPRECATED], [OBSOLETE], etc.)
                .ReplaceEx(@"^\W+", string.Empty)                  // Remove all non-word symbols in the beginning of the string
                .ReplaceEx(@"[^\w-+]+$", string.Empty)             // Remove all non-word symbols in the end of the string (except '+' and '-')
                .ReplaceEx(@"['""]", string.Empty)                 // Remove quotes
                .ReplaceEx(@"(-+?)(?!.*\w+)", "Minus")             // Replace trailing '-' with word representation
                .ReplaceEx(@"(\++?)(?!.*\w+)", "Plus")             // Replace trailing '+' with word representation
                .ReplaceEx(@"[^\w\s]", " ")                        // Replace any non-word symbols with space
                .ReplaceEx(@"\b(\w)", m => m.Value.ToUpper())      // Uppercase words
                .ReplaceEx(@"\b(?:\s+)(\d+)", @"_$1")              // Replace any remaining spaces before a number with a single underscore, so 'Foo5 10 20 Bar' will become 'Foo5_10_20_Bar'
                .ReplaceEx(@"(\d+)(?:\s+)\b", @"$1_")              // Replace any remaining spaces after a number with a single underscore, so 'Foo 5,10,20 Bar' will become 'Foo 5_10_20_Bar'
                .ReplaceEx(@"\s", string.Empty);                   // Remove spaces

            if (result.Length == 0)
            {
                return ValidateAndUseFallback(fallback);
            }

            if (char.IsDigit(result, 0))
            {
                return "_" + result;
            }

            return result;
        }

        private string ValidateAndUseFallback(string fallback)
        {
            if (string.IsNullOrEmpty(fallback))
            {
                throw new ArgumentNullException(nameof(fallback));
            }

            if (!_provider.IsValidIdentifier(fallback))
            {
                throw new ArgumentException("Parameter fallback does not fit a valid C# identifier requirements.", nameof(fallback));
            }

            return fallback;
        }

        private string GetSourceFileName(string outputDir, string subFolder, string fileName)
        {
            var sourceFilePath =
                _provider.FileExtension[0] == '.' ?
                Path.Combine(outputDir, subFolder, fileName + _provider.FileExtension) :
                Path.Combine(outputDir, subFolder, fileName + "." + _provider.FileExtension);

            sourceFilePath.EnsureDirectoryExists();

            return sourceFilePath;
        }


        private string FormatIdentifier(Metadata metadata)
        {
            return NormalizeLabel(metadata.DisplayName, metadata.LogicalName);
        }

        private string FormatIdentifier(OptionMetadata option)
        {
            var optionFallback = "_" + (option.Value.HasValue ? option.Value.Value.ToString() : "");
            return NormalizeLabel(option.Label, optionFallback);
        }


        private static void AddComment(CodeCommentStatementCollection comments, Metadata metadata)
        {
            var comment = GetComment(metadata);

            if (string.IsNullOrEmpty(comment))
            {
                return;
            }

            var commentsToAdd = new CodeCommentStatementCollection
            {
                new CodeCommentStatement("<summary>", true),
                new CodeCommentStatement(comment, true)
            };

            if (!string.IsNullOrEmpty(metadata.Description))
            {
                commentsToAdd.Add(new CodeCommentStatement(metadata.Description, true));
            }

            commentsToAdd.Add(new CodeCommentStatement("</summary>", true));

            comments.AddRange(commentsToAdd);
        }

        private static void AddComment(CodeCommentStatementCollection comments, OptionMetadata option)
        {
            var commentsToAdd = new CodeCommentStatementCollection
            {
                new CodeCommentStatement("<summary>", true),
                new CodeCommentStatement(option.Label, true),
                new CodeCommentStatement("</summary>", true)
            };
            comments.AddRange(commentsToAdd);
        }

        private static string GetComment(Metadata metadata)
        {
            if (string.IsNullOrEmpty(metadata.DisplayName))
            {
                if (metadata.VariableName == metadata.LogicalName)
                {
                    return null;
                }
                else
                {
                    return metadata.LogicalName;
                }
            }
            else
            {
                return $"{metadata.DisplayName} ({metadata.LogicalName})";
            }
        }

        [SuppressMessage(
            "Critical Code Smell",
            "S3265:Non-flags enums should not be used in bitwise operations",
            Justification = "MemberAttributes enum is mistakenly missing the Flag attribute")]
        private static CodeMemberField GenerateConstant<T>(string name, T value)
        {
            return new CodeMemberField
            {
                Attributes = MemberAttributes.Const | MemberAttributes.Public,
                Type = new CodeTypeReference(typeof(T)),
                Name = name,
                InitExpression = new CodePrimitiveExpression(value)
            };
        }
        #endregion
    }
}
