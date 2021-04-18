# Constants and Enums Generator for MS Dataverse (formerly CDS)

[![Build status](https://dev.azure.com/power-platform-tools/dataverse-tools/_apis/build/status/constants-enums-generator-cs-import?branchName=working)](https://dev.azure.com/power-platform-tools/dataverse-tools/_build/latest?definitionId=1&branchName=working)

This utility can be used to generate constants and enums from MS Dataverse tables (formerly CDS entities) metadata in C# language to simplify further development.
Primarily, it is intended to help when late-bound development approach is used.
Constants with names are generated for table columns (entity fields) and relationships.
Enums are generated for choice (picklist) and Yes/No (boolean) columns.

The usage is quite simple - please refer to the included sample _run.cmd_.
Also you can run `ceg.exe -h` for the full list of parameters.
