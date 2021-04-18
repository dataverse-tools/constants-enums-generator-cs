# Constants and Enums Generator for MS Dataverse (formerly CDS)

[![Build status](https://dev.azure.com/zhaparoff/Xrm%20Tools/_apis/build/status/GitHub/constants-enums-generator-cs)](https://dev.azure.com/zhaparoff/Xrm%20Tools/_build/latest?definitionId=20)

This utility can be used to generate constants and enums from MS Dataverse tables (formerly CDS entities) metadata in C# language to simplify further development.
Primarily, it is intended to help when late-bound development approach is used.
Constants with names are generated for table columns (entity fields) and relationships.
Enums are generated for choice (picklist) and Yes/No (boolean) columns.

The usage is quite simple - please refer to the included sample _run.cmd_.
Also you can run `ceg.exe -h` for the full list of parameters.
