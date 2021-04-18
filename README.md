# Constants and Enums Generator for MS Dataverse (formerly CDS)

[![Build](https://github.com/dataverse-tools/constants-enums-generator-cs/actions/workflows/build.yml/badge.svg?branch=working)](https://github.com/dataverse-tools/constants-enums-generator-cs/actions/workflows/build.yml)
[![deepcode](https://www.deepcode.ai/api/gh/badge?key=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwbGF0Zm9ybTEiOiJnaCIsIm93bmVyMSI6ImRhdGF2ZXJzZS10b29scyIsInJlcG8xIjoiY29uc3RhbnRzLWVudW1zLWdlbmVyYXRvci1jcyIsImluY2x1ZGVMaW50IjpmYWxzZSwiYXV0aG9ySWQiOjI5MjI5LCJpYXQiOjE2MTg3NjQ1ODh9.0sYQwrn9UwWt11ZXTebmwDPPVC6cr-SFEsAObtaVed4)](https://www.deepcode.ai/app/gh/dataverse-tools/constants-enums-generator-cs/_/dashboard?utm_content=gh%2Fdataverse-tools%2Fconstants-enums-generator-cs)

This utility can be used to generate constants and enums from MS Dataverse tables (formerly CDS entities) metadata in C# language to simplify further development.
Primarily, it is intended to help when late-bound development approach is used.
Constants with names are generated for table columns (entity fields) and relationships.
Enums are generated for choice (picklist) and Yes/No (boolean) columns.

The usage is quite simple - please refer to the included sample _run.cmd_.
Also you can run `ceg.exe -h` for the full list of parameters.
