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

    public ExportViewModel(MainWindowViewModel main)
    {
        _main = main;
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
        try
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            var window = desktop.MainWindow;

            if (window == null)
                return;

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
                return;

            await exporter(_main.HardwareInfo, file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}