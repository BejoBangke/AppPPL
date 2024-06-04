using System.Data;
using System.Diagnostics;
using Win10BloatRemover.Operations;
using Win10BloatRemover.Utils;

namespace Win10BloatRemover.UI;

class ConsoleMenu(MenuEntry[] entries, RebootRecommendedFlag rebootFlag)
{
    private const int FirstMenuEntryNumber = 1;

    private bool exitRequested = false;

    private static readonly Version programVersion = typeof(ConsoleMenu).Assembly.GetName().Version!;

    public void RunLoopUntilExitRequested()
    {
        while (!exitRequested)
        {
            Console.Clear();
            PrintHeading();
            PrintMenuEntries();
            MenuEntry chosenEntry = RequestUserChoice();

            Console.Clear();
            PrintTitleAndExplanation(chosenEntry);
            if (UserWantsToProceed())
                TryPerformEntryOperation(chosenEntry);
        }
    }

    private void PrintHeading()
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("╔═══════════════════════════════════════════════════╗");
        Console.WriteLine("║               ╔═══╗ ╔═══╗ ╔═══╗ ╔════╗            ║");
        Console.WriteLine("║               ║   ║ ║   ╚═╝   ║ ║    ║            ║");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("║               ║   ║ ║         ║ ║    ║            ║");
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("║               ║   ║ ║   ╔═══╗ ║ ║    ║            ║");
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine("║               ║   ║ ║   ╚═══╝ ║ ║    ║            ║");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("║               ╚═══╝ ╚═════════╝ ╚════╝            ║");
        Console.WriteLine($"║       Toolkit Penghapus Bloatware - v{programVersion.Major}.{programVersion.Minor}          ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    private void PrintMenuEntries()
    {
        ConsoleHelpers.WriteLine("-- MENU --", ConsoleColor.Green);
        for (int i = 0; i < entries.Length; i++)
        {
            ConsoleHelpers.Write($"{FirstMenuEntryNumber + i}: ", ConsoleColor.Green);
            Console.WriteLine(entries[i].FullName);
        }
        Console.WriteLine();
    }

    private MenuEntry RequestUserChoice()
    {
        MenuEntry? chosenEntry = null;
        bool isUserInputCorrect = false;
        while (!isUserInputCorrect)
        {
            Console.Write("Silahkan dipilih: ");
            chosenEntry = GetEntryCorrespondingToUserInput(Console.ReadLine());
            if (chosenEntry == null)
                ConsoleHelpers.WriteLine("Harus ANGKA :D.", ConsoleColor.Red);
            else
                isUserInputCorrect = true;
        }
        return chosenEntry!;
    }

    private MenuEntry? GetEntryCorrespondingToUserInput(string? userInput)
    {
        bool inputIsNumeric = int.TryParse(userInput, out int entryNumber);
        if (inputIsNumeric)
        {
            int entryIndex = entryNumber - FirstMenuEntryNumber;
            return entries.ElementAtOrDefault(entryIndex);
        }

        return null;
    }

    private void PrintTitleAndExplanation(MenuEntry entry)
    {
        ConsoleHelpers.WriteLine($"-- {entry.FullName} --", ConsoleColor.Green);
        Console.WriteLine(entry.GetExplanation());
    }

    private bool UserWantsToProceed()
    {
        Console.WriteLine("\nTekan tombol *Enter* untuk lanjut atau tombo lain untuk kelaman utama");
        return Console.ReadKey().Key == ConsoleKey.Enter;
    }

    private void TryPerformEntryOperation(MenuEntry entry)
    {
        try
        {
            Console.WriteLine();
            IOperation operation = entry.CreateNewOperation(new ConsoleUserInterface());
            operation.Run();
            if (operation.IsRebootRecommended)
            {
                ConsoleHelpers.WriteLine("\nA Disarankan untuk restart.", ConsoleColor.Cyan);
                rebootFlag.SetRecommended();
            }

            if (entry.ShouldQuit)
            {
                exitRequested = true;
                return;
            }

            Console.Write("\nBerhasil :) ");
        }
        catch (Exception exc)
        {
            ConsoleHelpers.WriteLine($"Gagal Perhatikan: {exc.Message}", ConsoleColor.Red);
            Trace.WriteLine(exc.StackTrace);
            Console.WriteLine();
        }

        ConsoleHelpers.FlushStandardInput();
        Console.WriteLine("Tekan tombol untuk kembali ke laman Utama :) ");
        Console.ReadKey();
    }
}
