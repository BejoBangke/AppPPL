﻿using System;
using System.Diagnostics;
using System.Security.Principal;
using Win10BloatRemover.Utils;

namespace Win10BloatRemover
{
    static class Program
    {
        public const string SUPPORTED_WINDOWS_RELEASE_ID = "2004";
        private const string SUPPORTED_WINDOWS_RELEASE_NAME = "May 2020 Update";

        private static void Main()
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            Console.Title = "Windows 10 Bloat Remover and Tweaker";
            EnsurePreliminaryChecksAreSuccessful();

            var configuration = LoadConfigurationFromFileOrDefault();

            RegisterExitEventHandlers();

            var menu = new ConsoleMenu(CreateMenuEntries(configuration));
            menu.RunLoopUntilExitRequested();
        }

        private static MenuEntry[] CreateMenuEntries(Configuration configuration)
        {
            return new MenuEntry[] {
                new SystemAppsRemovalEnablingEntry(),
                new UWPAppRemovalEntry(configuration),
                new EdgeRemovalEntry(),
                new OneDriveRemovalEntry(),
                new ServicesRemovalEntry(configuration),
                new WindowsFeaturesRemovalEntry(configuration),
                new PrivacySettingsTweakEntry(),
                new TelemetryDisablingEntry(),
                new DefenderDisablingEntry(),
                new AutoUpdatesDisablingEntry(),
                new ScheduledTasksDisablingEntry(configuration),
                new ErrorReportingDisablingEntry(),
                new TipsAndFeedbackDisablingEntry(),
                new NewGitHubIssueEntry(),
                new AboutEntry(),
                new QuitEntry()
            };
        }

        private static void EnsurePreliminaryChecksAreSuccessful()
        {
            if (!Program.HasAdministratorRights())
            {
                ConsoleHelpers.WriteLine("This application needs to be run with administrator rights!", ConsoleColor.Red);
                Console.ReadKey();
                Environment.Exit(-1);
            }

            #if !DEBUG
            if (!SystemUtils.IsWindowsReleaseId(SUPPORTED_WINDOWS_RELEASE_ID))
            {
                ConsoleHelpers.WriteLine($"This application is compatible only with Windows 10 {SUPPORTED_WINDOWS_RELEASE_NAME}!", ConsoleColor.Red);
                Console.ReadKey();
                Environment.Exit(-1);
            }
            #endif
        }

        private static bool HasAdministratorRights()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        
        private static Configuration LoadConfigurationFromFileOrDefault()
        {
            try
            {
                return Configuration.LoadFromFileOrDefault();
            }
            catch (ConfigurationException exc)
            {
                ConsoleHelpers.WriteLine(exc.Message, ConsoleColor.DarkYellow);
                Console.WriteLine("Press a key to continue to the main menu.");
                Console.ReadKey();
                return Configuration.Default;
            }
        }

        private static void RegisterExitEventHandlers()
        {
            #if !DEBUG
            bool cancelKeyPressedOnce = false;
            Console.CancelKeyPress += (sender, args) => {
                if (!cancelKeyPressedOnce)
                {
                    ConsoleHelpers.WriteLine("Press Ctrl+C again to terminate the program.", ConsoleColor.Red);
                    cancelKeyPressedOnce = true;
                    args.Cancel = true;
                }
                else
                    Process.GetCurrentProcess().KillChildProcesses();
            };
            #endif

            // Executed when the user closes the window. This handler is not fired when process is terminated with Ctrl+C
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => Process.GetCurrentProcess().KillChildProcesses();
        }
    }
}
