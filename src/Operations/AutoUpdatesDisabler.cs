﻿using Microsoft.Win32;
using Win10BloatRemover.UI;
using Win10BloatRemover.Utils;

namespace Win10BloatRemover.Operations;

public class AutoUpdatesDisabler(IUserInterface ui) : IOperation
{
    public void Run()
    {
        ui.PrintMessage("Writing values into the Registry...");
        DisableAutomaticWindowsUpdates();
        DisableAutomaticStoreUpdates();
        DisableAutomaticSpeechModelUpdates();
    }

    private void DisableAutomaticWindowsUpdates()
    {
        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1);
    }

    private void DisableAutomaticStoreUpdates()
    {
        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsStore", "AutoDownload", 2);
        // The above policy does not work on Windows 10 Home, so we need to change the Store app setting
        // to disable automatic updates for all users
        using var key = RegistryUtils.LocalMachine64.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsStore\WindowsUpdate");
        key.SetValue("AutoDownload", 2);
    }

    private void DisableAutomaticSpeechModelUpdates()
    {
        Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Speech", "AllowSpeechModelUpdate", 0);
    }
}
