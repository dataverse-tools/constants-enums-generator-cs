using System.Collections.Generic;
using CommandLine;


namespace Ceg.Model
{
    public class Options
    {
        [Option('c', "connection", HelpText = "Dataverse connection string", Required = true)]
        public string ConnectionString { get; set; }

        [Option('n', "namespace", HelpText = "Namespace for generated enums/classes", Required = true)]
        public string Namespace { get; set; }

        [Option('e', "entities", Separator = ',', HelpText = "Comma-separated list of tables to generate code. Pass 'all' for all tables.", Required = true)]
        public IList<string> Entities { get; set; }

        [Option('j', "json", HelpText = "Generate enums/classes and save it to JSON files", Required = false, Default = false)]
        public bool GenerateJson { get; set; }

        [Option('o', "outDir", HelpText = "Folder for generated enums/classes", Required = false, Default = "GeneratedCode")]
        public string OutputDir { get; set; }

        [Option('q', "quiet", HelpText = "Do not ask for user input", Required = false, Default = false)]
        public bool Quiet { get; set; }
    }
}
