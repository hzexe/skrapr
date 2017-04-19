﻿namespace Skrapr
{
    using BaristaLabs.Skrapr;
    using BaristaLabs.Skrapr.Extensions;
    using BaristaLabs.Skrapr.Tasks;
    using EntryPoint;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Events;
    using Serilog.Formatting.Json;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    class Program
    {
        //Launch chrome with
        //"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9223
        //Consider using PM2 to launch both chrome and SkraprCLI (is there a .net equiv?)

        static void Main(string[] args)
        {
            var cliArguments = Cli.Parse<CliArguments>(args);

            //Do an initial check to ensure that the Skrapr Definition exists.
            if (!File.Exists(cliArguments.SkraprDefinitionPath))
                throw new FileNotFoundException($"The specified skrapr definition ({cliArguments.SkraprDefinitionPath}) could not be found. Please check that the skrapr definition exists.");

            //Setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            //Remove previous log file
            File.Delete("skraprlog.json");

            //Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(new JsonFormatter(), "skraprlog.json")
                .WriteTo.ColoredConsole(restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();

            //Configure the logger.
            var logger = serviceProvider
                .GetService<ILoggerFactory>()
                .AddSerilog()
                .CreateLogger<Program>();

            logger.LogDebug($"Connecting to a Chrome session on {cliArguments.RemoteDebuggingHost}:{cliArguments.RemoteDebuggingPort}...");

            var sessions = ChromeBrowser.GetChromeSessions(cliArguments.RemoteDebuggingHost, cliArguments.RemoteDebuggingPort).GetAwaiter().GetResult();
            var session = sessions.FirstOrDefault(s => s.Type == "page" && !String.IsNullOrWhiteSpace(s.WebSocketDebuggerUrl));

            //TODO: Create a new session if one doesn't exist.
            if (session == null)
                throw new InvalidOperationException("Unable to locate a suitable session. Ensure that the Developer Tools window is closed on an existing session or create a new chrome instance, specifying the debugger port as a command line argument.");

            var devTools = SkraprDevTools.Connect(serviceProvider, session).GetAwaiter().GetResult();
            logger.LogDebug($"Using session {session.Id}: {session.Title} - {session.WebSocketDebuggerUrl}");

            var worker = SkraprWorker.Create(serviceProvider, cliArguments.SkraprDefinitionPath, devTools.Session, devTools, debugMode: cliArguments.Debug);

            if (cliArguments.Debug)
            {
                logger.LogDebug($"Operating in debug mode. Tasks may perform additional behavior or may skip themselves.");
            }
            
            if (cliArguments.Attach == true)
            {
                var targetInfo = devTools.Session.Target.GetTargetInfo(session.Id).GetAwaiter().GetResult();
                var matchingRuleCount = worker.GetMatchingRules().GetAwaiter().GetResult().Count();
                if (matchingRuleCount > 0)
                {
                    logger.LogDebug($"Attach specified and {matchingRuleCount} rules match the current session's state; Continuing.");
                    worker.AddTask(new NavigateTask
                    {
                        Url = targetInfo.Url
                    });
                }
                else
                {
                    logger.LogDebug($"Attach specified but no rules matched the current session's state; Adding start urls.");
                    worker.AddStartUrls();
                }
            }
            else
            {
                logger.LogDebug($"Adding start urls.");
                worker.AddStartUrls();
            }

            //Setup Hangfire
            //GlobalConfiguration.Configuration.UseStorage(new MemoryStorage());

            //using (new BackgroundJobServer())
            //{
            //    Console.WriteLine("Skrapr started. Press ENTER to exit...");

            //    Console.WriteLine("Executing initial Skrape...");
            //    var definitionJobId = BackgroundJob.Enqueue(() => SkraprDefinitionProcessor.Start(cliArguments.SkraprDefinitionPath, devTools));
            //    BackgroundJob.Enqueue(() => Console.WriteLine("Fire-and-forget"));
            //    Console.ReadKey();
            //}

            logger.LogDebug("Skrapr is currently processing. Press ENTER to exit...");
            Task.WaitAny(worker.Completion, Task.Run(() =>
            {
                Console.ReadKey();
                logger.LogDebug("Stop requested. Shutting down.");
                //TODO: dispose of session, worker.
                worker.Dispose();
            }));           
        }
    }
}