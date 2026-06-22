using CommunityToolkit.Mvvm.ComponentModel;
using SpecMind.Models;
using SpecMind.Services;

namespace SpecMind.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // Создаем свойство для хранения данных о железе.
    // Атрибут [ObservableProperty] автоматически создаст уведомление об изменении для UI.
    [ObservableProperty]
    private HardwareInfo _hardwareInfo = new();

    public MainWindowViewModel()
    {
        // Запускаем загрузку данных при создании ViewModel
        LoadHardwareData();
    }

    private async void LoadHardwareData()
    {
        // Создаем экземпляр нашего сервиса (позже мы настроим Dependency Injection, но пока так)
        IHardwareScannerService scanner = new HardwareScannerService();

        // Получаем тестовые данные и присваиваем их свойству
        HardwareInfo = await scanner.GetHardwareInfoAsync();
    }
}