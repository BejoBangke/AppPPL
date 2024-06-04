using System.Diagnostics;
using System.Security.Principal;
using Win10BloatRemover.UI;
using Win10BloatRemover.Utils;

namespace Win10BloatRemover;

static class Program
{
    private const int MINIMUM_SUPPORTED_WINDOWS_BUILD = 19044;  // 21H2 build

    private static void Main(string[] args)
    {
        if (IsTraceOutputEnabled(args))
            Trace.Listeners.Add(new ConsoleTraceListener());

        Console.Title = "Projek Toolkit Lemmesh";
        if (!HasAdministratorRights())
            Console.Title += " (unprivileged)";

        ShowWarningOnUnsupportedOS();
        RegisterTerminationHandler();

        var configuration = LoadConfigurationFromFileOrDefault();
        var rebootFlag = new RebootRecommendedFlag();
        var menu = new ConsoleMenu(CreateMenuEntries(configuration, rebootFlag), rebootFlag);
        menu.RunLoopUntilExitRequested();
    }

    private static bool IsTraceOutputEnabled(string[] args) => args.Contains("--show-trace-output");

    private static MenuEntry[] CreateMenuEntries(AppConfiguration configuration, RebootRecommendedFlag rebootFlag)
    {
        return [
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
            new ConsumerFeaturesDisablingEntry(),
            new SuggestionsDisablingEntry(),
            //new NewGitHubIssueEntry(),
            new AboutEntry(),
            new QuitEntry(rebootFlag)
        ];
    }

    private static bool HasAdministratorRights()
    {
        var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static void ShowWarningOnUnsupportedOS()
    {
        if (OS.IsWindows10() && OS.WindowsBuild >= MINIMUM_SUPPORTED_WINDOWS_BUILD)
            return;

        ConsoleHelpers.WriteLine("-- UNSUPPORTED WINDOWS VERSION --\n", ConsoleColor.DarkYellow);
        if (!OS.IsWindows10())
            Console.WriteLine("Cuman buat WINDOWS 10");
        else
        {
            Console.WriteLine(
                "Lah Windows 7 dkk? lawak?.\n" +
                "Cuman buat Windows 10 bro\n" +
                $"Windows 10 version ({OS.GetWindowsVersionName()}) at the following page:"
            );
            ConsoleHelpers.WriteLine("  https://github.com/Fs00/Win10BloatRemover/releases/", ConsoleColor.Cyan);
        }

        Console.WriteLine(
            "\nBisa pake cuman\n" +
            "Resiko ditanggung sendiri bro :D."
        );

        Console.WriteLine("\nEnter buat lanjut keyboard laen buat balek.");
        if (Console.ReadKey().Key != ConsoleKey.Enter)
            Environment.Exit(-1);
    }
    
    private static AppConfiguration LoadConfigurationFromFileOrDefault()
    {
        try
        {
            return AppConfiguration.LoadOrCreateFile();
        }
        catch (AppConfigurationException exc)
        {
            PrintConfigurationErrorMessage(exc);
            return AppConfiguration.Default;
        }
    }

    private static void PrintConfigurationErrorMessage(AppConfigurationException exc)
    {
        if (exc is AppConfigurationLoadException)
        {
            ConsoleHelpers.WriteLine($"An error occurred while loading settings file:\n{exc.Message}\n", ConsoleColor.Red);
            Console.WriteLine("Default settings have been loaded instead.\n");
        }
        else if (exc is AppConfigurationWriteException)
            ConsoleHelpers.WriteLine($"Default settings file could not be created: {exc.Message}\n", ConsoleColor.DarkYellow);

        Console.WriteLine("Kemaen menu tekan sembarang.....jan sembarang jua yang normal aje :)");
        Console.ReadKey();
    }

    private static void RegisterTerminationHandler()
    {
        bool cancelKeyPressedOnce = false;
        Console.CancelKeyPress += (sender, args) => {
            if (!cancelKeyPressedOnce)
            {
                ConsoleHelpers.WriteLine("CTRL + C buat batalin Proses bro :| ", ConsoleColor.Red);
                cancelKeyPressedOnce = true;
                args.Cancel = true;
            }
        };
    }
}
