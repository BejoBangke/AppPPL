using Win10BloatRemover.Operations;
using Win10BloatRemover.Utils;

namespace Win10BloatRemover.UI;

abstract class MenuEntry
{
    public abstract string FullName { get; }
    public virtual bool ShouldQuit => false;
    public abstract string GetExplanation();
    public abstract IOperation CreateNewOperation(IUserInterface ui);
}

class UWPAppRemovalEntry(AppConfiguration configuration) : MenuEntry
{
    public override string FullName => "Hapus UWP app";
    public override string GetExplanation()
    {
        string impactedUsers = configuration.UWPAppsRemovalMode == UwpAppRemovalMode.CurrentUser
            ? "Pengguna sekarang"
            : "Pengguna sekarang dan mendatang";
        string explanation = $"Berikut yang akan dihapus{impactedUsers}:";
        foreach (UwpAppGroup app in configuration.UWPAppsToRemove)
            explanation += $"\n  {app}";

        if (configuration.UWPAppsRemovalMode == UwpAppRemovalMode.AllUsers)
            explanation += "\n\nPelayanan, Komponen dan tugas yang terjadwal yang digunakan oleh aplikasi juga" +
                           "dihapus atau dimatikan,\nbersama dengan data yang tersimpan.";

        return explanation;
    }

    public override IOperation CreateNewOperation(IUserInterface ui)
        => new UwpAppGroupRemover(configuration.UWPAppsToRemove, configuration.UWPAppsRemovalMode,
                                  ui, new AppxRemover(ui), new ServiceRemover(ui));
}

class DefenderDisablingEntry : MenuEntry
{
    public override string FullName => "Mematikan Antivirus Windows";
    public override string GetExplanation() => """
        Penting: Sebelum memulai, nonaktifkan Tamper Protection di aplikasi Windows Security pada pengaturan Virus & threat protection.

        Mesin antimalware Windows Defender dan fitur SmartScreen akan dinonaktifkan melalui Group Policies, dan layanan terkait fitur-fitur tersebut akan dihapus. Selain itu, aplikasi Windows Security akan dicegah untuk berjalan otomatis saat sistem mulai.

        Firewall Windows Defender akan tetap berfungsi seperti biasa.

        Perlu diingat bahwa SmartScreen untuk Microsoft Edge dan aplikasi Store hanya akan dinonaktifkan untuk pengguna yang sedang masuk dan pengguna baru yang dibuat setelah menjalankan prosedur ini.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new DefenderDisabler(ui, new ServiceRemover(ui));
}

class EdgeRemovalEntry : MenuEntry
{
    public override string FullName => "Hapus Edge Browser";
    public override string GetExplanation() => """
        Kedua browser, Edge Chromium dan Edge versi lama (yang muncul di menu Start setelah Anda menghapus Edge Chromium), akan dihapus dari sistem. 
        Pastikan bahwa Edge Chromium tidak memperbarui dirinya sendiri sebelum melanjutkan.

        Perlu dicatat bahwa runtime Edge WebView2 TIDAK akan dihapus jika sudah terpasang, karena mungkin diperlukan oleh program lain yang terpasang di PC ini.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new EdgeRemover(ui, new AppxRemover(ui));
}

class OneDriveRemovalEntry : MenuEntry
{
    public override string FullName => "Hapus OneDrive";
    public override string GetExplanation() => """
        OneDrive akan dinonaktifkan menggunakan Group Policies dan kemudian dihapus untuk pengguna saat ini. Selain itu, 
        
        OneDrive akan dicegah untuk dipasang saat pengguna baru masuk untuk pertama kali.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new OneDriveRemover(ui);
}

class ServicesRemovalEntry(AppConfiguration configuration) : MenuEntry
{
    public override string FullName => "Hapus Layanan lai-lain";
    public override string GetExplanation()
    {
        string explanation = "Semua layanan yang namany berawal di list akan dihapus:\n";
        foreach (string service in configuration.ServicesToRemove)
            explanation += $"  {service}\n";
        return explanation + "\nLayanan akan di Back up di dalam folder yang sama.";
    }

    public override IOperation CreateNewOperation(IUserInterface ui)
        => new ServiceRemovalOperation(configuration.ServicesToRemove, ui, new ServiceRemover(ui));
}

class WindowsFeaturesRemovalEntry(AppConfiguration configuration) : MenuEntry
{
    public override string FullName => "Hapus Fitur Windows";
    public override string GetExplanation()
    {
        string explanation = "Fitur sesuai permintaan berikut akan dihapus:";
        foreach (string feature in configuration.WindowsFeaturesToRemove)
            explanation += $"\n  {feature}";
        return explanation;
    }

    public override IOperation CreateNewOperation(IUserInterface ui)
        => new FeaturesRemover(configuration.WindowsFeaturesToRemove, ui);
}

class PrivacySettingsTweakEntry : MenuEntry
{
    public override string FullName => "Sesuaikan pengaturan untuk privasi";
    public override string GetExplanation() => """
        Beberapa pengaturan dan kebijakan default akan diubah untuk membuat Windows lebih menghargai privasi pengguna. Perubahan ini terutama meliputi:

          - Menyesuaikan berbagai opsi di bawah bagian Privasi pada aplikasi Pengaturan (menonaktifkan ID iklan, pelacakan peluncuran aplikasi, dll.)
          - Mencegah data input (informasi pengetikan/tulisan tangan, ucapan) dikirim ke Microsoft untuk meningkatkan layanan mereka
          - Mencegah Edge mengirim riwayat penelusuran, favorit, dan data lainnya ke Microsoft untuk mempersonalisasi iklan, berita, dan layanan lainnya untuk akun Microsoft Anda
          - Menolak akses ke data sensitif (lokasi, dokumen, aktivitas, detail akun, info diagnostik) ke semua aplikasi UWP secara default
          - Menolak akses lokasi untuk pencarian Windows
          - Menonaktifkan aktivasi suara untuk asisten suara (sehingga mereka tidak selalu mendengarkan)
          - Menonaktifkan sinkronisasi cloud untuk data sensitif (aktivitas pengguna, papan klip, pesan teks, kata sandi, dan data aplikasi)

        Hampir semua pengaturan ini diterapkan untuk semua pengguna, namun beberapa hanya akan diubah untuk pengguna saat ini dan untuk pengguna baru yang dibuat setelah menjalankan prosedur ini.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new PrivacySettingsTweaker(ui);
}

class TelemetryDisablingEntry : MenuEntry
{
    public override string FullName => "Mematikan Telemetry";
    public override string GetExplanation() => """
        Prosedur ini akan menonaktifkan tugas terjadwal, layanan, dan fitur yang bertanggung jawab untuk mengumpulkan dan melaporkan data ke Microsoft, 
        termasuk Compatibility Telemetry, Device Census, Customer Experience Improvement Program, dan Compatibility Assistant.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new TelemetryDisabler(ui, new ServiceRemover(ui));
}

class AutoUpdatesDisablingEntry : MenuEntry
{
    public override string FullName => "Menonaktifkan Update otomatis";
    public override string GetExplanation() => """
        Pembaruan otomatis untuk Windows, aplikasi Store, dan model suara akan dinonaktifkan menggunakan Group Policies.
        
        Setidaknya diperlukan edisi Windows 10 Pro untuk menonaktifkan pembaruan otomatis Windows.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new AutoUpdatesDisabler(ui);
}

class ScheduledTasksDisablingEntry(AppConfiguration configuration) : MenuEntry
{
    public override string FullName => "Menonaktifkan berbagai tugas terjadwal";
    public override string GetExplanation()
    {
        string explanation = "\r\nTugas terjadwal berikut akan dinonaktifkan:";
        foreach (string task in configuration.ScheduledTasksToDisable)
            explanation += $"\n  {task}";
        return explanation;
    }

    public override IOperation CreateNewOperation(IUserInterface ui)
        => new ScheduledTasksDisabler(configuration.ScheduledTasksToDisable, ui);
}

class ErrorReportingDisablingEntry : MenuEntry
{
    public override string FullName => "Menonaktifkan Pelaporan Kesalahan Windows";
    public override string GetExplanation() => """
        Pelaporan Kesalahan Windows akan dinonaktifkan dengan mengedit Kebijakan Grup, serta dengan menghapus layanannya (setelah
        mendukung mereka).
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new ErrorReportingDisabler(ui, new ServiceRemover(ui));
}

class ConsumerFeaturesDisablingEntry : MenuEntry
{
    public override string FullName => "Nonaktifkan fitur konsumen";
    public override string GetExplanation() => """
        Prosedur ini akan menonaktifkan fitur-fitur berbasis cloud berikut yang ditujukan untuk pasar konsumen:
          - Windows Spotlight (latar belakang layar kunci dinamis)
          - Pengalaman Spotlight dan rekomendasi di Microsoft Edge
          - Berita dan Minat
          - Sorotan pencarian
          - Pencarian Bing di bilah pencarian Windows
          - Ikon Meet Now Skype di taskbar
          - instalasi otomatis aplikasi yang disarankan
          - konten yang dioptimalkan cloud di taskbar

        Perlu diingat bahwa beberapa fitur ini hanya akan dinonaktifkan untuk pengguna yang sedang masuk dan pengguna baru yang dibuat setelah menjalankan prosedur ini.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new ConsumerFeaturesDisabler(ui);
}

class SuggestionsDisablingEntry : MenuEntry
{
    public override string FullName => "Menonaktifkan permintaan saran dan umpan balik";
    public override string GetExplanation() => """
        Notifikasi dan permintaan umpan balik, saran aplikasi, dan tips Windows akan dimatikan dengan mengubah Group Policies dan pengaturan sistem sesuai, 
        serta menonaktifkan beberapa tugas terjadwal terkait.

        Jika Anda tidak menggunakan edisi Enterprise atau Education dari Windows, 
        saran hanya akan dinonaktifkan untuk pengguna yang sedang masuk dan pengguna baru yang dibuat setelah menjalankan prosedur ini.
        """;

    public override IOperation CreateNewOperation(IUserInterface ui) => new SuggestionsDisabler(ui);
}

//class NewGitHubIssueEntry : MenuEntry
//{
//    public override string FullName => "Laporkan isu atau saran";
//    public override string GetExplanation() => """
//        Dibawa ke github buat kirim saran atau isu :D
//        """;

//    public override IOperation CreateNewOperation(IUserInterface ui)
//        => new BrowserOpener("https://github.com/");
//}

class AboutEntry : MenuEntry
{
    public override string FullName => "Tentang Program";
    public override string GetExplanation()
    {
        Version programVersion = GetType().Assembly.GetName().Version!;
        return $"""
            Toolkit {programVersion.Major}.{programVersion.Minor}
            Dikembangkan lebih lanjut oleh:
            Darmawanysah Dawi 202012015 
            Prasetyo Adji Purnomo 202012035

            Kelompok kami mengembangkan sebuah proyek perangkat lunak berbasis CLI (Command Line Interface) 
            Proyek ini bertujuan untuk menyediakan alat yang efisien 
            dan mudah digunakan untuk menyelesaikan berbagai tugas yang relevan 
            dengan kebutuhan pengguna kami.
            Didasarkan oleh Pengguna Github Fs00
            Official GitHub repository: https://github.com/Fs00/Win10BloatRemover

            Awalnya berdasarkan panduan Windows 10 de-botnet oleh Federico Dossena: https://fdossena.com

            Kredit kepada semua proyek sumber terbuka yang telah membantu meningkatkan perangkat lunak ini:
              - Situs web privacy.sexy: https://privacy.sexy
              - Source Debloat Windows 10: https://github.com/W4RH4WK/Debloat-Windows-10
              - Source penghapusan Edge oleh AveYo: https://github.com/AveYo/fox

            Kami percaya bahwa proyek ini akan memberikan kontribusi yang signifikan dalam mempermudah pengguna dalam mengelola file dan direktori 
            mereka melalui antarmuka CLI yang intuitif dan fungsional.
            Perangkat lunak ini dirilis di bawah lisensi BSD 3-Clause Clear.

            """;
    }

    public override IOperation CreateNewOperation(IUserInterface ui) => new LicensePrinter(ui);
}

class QuitEntry(RebootRecommendedFlag rebootFlag) : MenuEntry
{
    public override string FullName => "Keluar";
    public override bool ShouldQuit => true;
    public override string GetExplanation() => "Meluncur";
    public override IOperation CreateNewOperation(IUserInterface ui) => new AskForRebootOperation(ui, rebootFlag);
}
