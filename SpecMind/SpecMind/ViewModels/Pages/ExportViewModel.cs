using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using SpecMind.Models;
using SpecMind.Services;
using System;
using System.Threading.Tasks;

namespace SpecMind.ViewModels.Pages;

public partial class ExportViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;

    public System.Windows.Input.ICommand ShowDashboardCommand => _main.ShowDashboardCommand;

    private readonly Func<string, string, Task<string>> _selectPath;
    [ObservableProperty]
    private string statusText = "";

    public ExportViewModel(MainWindowViewModel main, Func<string, string, Task<string>> selectPath = null)
    {
        _main = main;
        _selectPath = selectPath ?? SelectPathAsync;
    }

    [RelayCommand]
    private async Task ExportTxt()
    {
        await SaveReportAsync(
            "txt",
            "Текстовый файл",
            ReportExporterService.ExportToTxtAsync);
    }

    [RelayCommand]
    private async Task ExportJson()
    {
        await SaveReportAsync(
            "json",
            "JSON файл",
            ReportExporterService.ExportToJsonAsync);
    }

    [RelayCommand]
    private async Task ExportCsv()
    {
        await SaveReportAsync(
            "csv",
            "CSV файл",
            ReportExporterService.ExportToCsvAsync);
    }

    [RelayCommand]
    private async Task ExportHtml()
    {
        await SaveReportAsync(
            "html",
            "HTML файл",
            ReportExporterService.ExportToHtmlAsync);
    }

    private async Task SaveReportAsync(
        string extension,
        string description,
        Func<HardwareInfo, string, Task<string>> exporter)
    {
        StatusText = "";
        try
        {
            var path = await _selectPath(extension, description);
            if (path == null) return;
            await exporter(_main.HardwareInfo, path);
            StatusText = "Отчёт сохранён: " + path;
        }
        catch (Exception ex)
        {
            StatusText = "Не удалось сохранить отчёт: " + ex.Message;
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
    private static async Task<string> SelectPathAsync(string extension, string description)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var window = desktop.MainWindow;

        if (window == null)
            return null;

        var file = await window.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Сохранить отчет",

                SuggestedFileName =
                    $"SpecMind_Report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",

                DefaultExtension = extension,

                FileTypeChoices =
                [
                    new FilePickerFileType(description)
                    {
                        Patterns =
                        [
                            $"*.{extension}"
                        ]
                    }
                ]
            });

        if (file == null)
            return null;

        return file.Path.LocalPath;
    }
}