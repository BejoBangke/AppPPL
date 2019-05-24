﻿using System;

namespace Win10BloatRemover.Utils
{
    /**
     *  Operations
     *  Contains functions that perform tasks which don't belong to a particular category
     */
    static class OperationUtils
    {
        /**
         *  Removes the specified component using install-wim-tweak synchronously
         *  It also prints output messages according to its exit status
         *  Messages from install-wim-tweak process are printed asynchronously (as soon as they are written to stdout/stderr)
         */
        public static void RemoveComponentUsingInstallWimTweak(string component)
        {
            Console.WriteLine($"Running install-wim-tweak to remove {component}...");
            using (var installWimTweakProcess = SystemUtils.RunProcess(Program.InstallWimTweakPath, $"/o /c {component} /r", true))
            {
                installWimTweakProcess.BeginOutputReadLine();
                installWimTweakProcess.BeginErrorReadLine();
                installWimTweakProcess.WaitForExit();
                if (installWimTweakProcess.ExitCode == 0)
                    Console.WriteLine("Install-wim-tweak executed successfully!");
                else
                    ConsoleUtils.WriteLine($"An error occurred during the removal of {component}: " +
                                            "install-wim-tweak exited with a non-zero status.", ConsoleColor.Red);
            }
        }
    }
}