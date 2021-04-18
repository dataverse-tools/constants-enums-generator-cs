# Constants and Enums Generator for MS Dataverse (formerly CDS)

[![Build](https://github.com/dataverse-tools/constants-enums-generator-cs/actions/workflows/build.yml/badge.svg?branch=working)](https://github.com/dataverse-tools/constants-enums-generator-cs/actions/workflows/build.yml)

This utility can be used to generate constants and enums from MS Dataverse tables (formerly CDS entities) metadata in C# language to simplify further development.
Primarily, it is intended to help when late-bound development approach is used.
Constants with names are generated for table columns (entity fields) and relationships.
Enums are generated for choice (picklist) and Yes/No (boolean) columns.

The usage is quite simple - please refer to the included sample _run.cmd_.
Also you can run `ceg.exe -h` for the full list of parameters.
