using System;
using System.Collections.Generic;
using System.Linq;
using Ceg.Model;
using Ceg.Services;
using CommandLine;
using NLog;


namespace Ceg.ConsoleApp
{
    public static class Program
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


        public static void Main(string[] args)
        {
            var parsedArgs = Parser.Default.ParseArguments<Options>(args);
            try
            {
                var genSvc = new GenerationService();
                parsedArgs
                    .WithParsed(genSvc.Generate)
                    .WithNotParsed(ProcessParserErrors);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
            finally
            {
                parsedArgs.WithParsed(options =>
                {
                    if (options.Quiet)
                    {
                        return;
                    }

                    Console.WriteLine("Press ENTER to exit...");
                    Console.ReadLine();
                });
            }
        }


        private static void ProcessParserErrors(IEnumerable<Error> errors)
        {
            var msg = errors.Aggregate("Failed to parse tool arguments:\n", (current, err) => current + $"\t- {err}\n");
            _logger.Error(msg);
        }
    }
}
