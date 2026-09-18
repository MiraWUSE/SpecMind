# Этап 1: сборка и навигация

## Изменения

Пути относительно корня репозитория:

- `SpecMind/SpecMind/Views/Pages/DashboardView.axaml`: namespace существующего CircularGauge, тип страницы и шаблона StorageInfo.
- `SpecMind/SpecMind/Views/Pages/DetailedView.axaml`: namespace конвертеров, тип страницы и шаблонов StorageInfo/MonitorInfo.
- `SpecMind/SpecMind/Views/Pages/MonitoringView.axaml`: namespace существующего SimpleLineChart и тип страницы.
- `SpecMind/SpecMind/Views/Pages/ExportView.axaml`: тип страницы.
- `SpecMind/SpecMind/Views/Pages/SettingsView.axaml`: namespace конвертеров, типы страницы и AppTheme, удаление устаревшей привязки IsSettingsVisible, привязка ApplyThemeCommand к SettingsViewModel вместо Window.DataContext.
- `SpecMind/SpecMind/Views/Pages/AIView.axaml`: типизированные привязки, Sender вместо отсутствующего конвертера, восстановленный русский текст и явное обозначение демонстрационного режима.
- `SpecMind/SpecMind/ViewLocator.cs`: явное соответствие AIViewModel → AIView; все шесть страниц отображаются через ContentControl.
- `SpecMind/SpecMind/ViewModels/Pages/{Detailed,Export,Settings}ViewModel.cs`: делегирование ShowDashboardCommand существующей навигации MainWindowViewModel.

Коммит также сохраняет существовавшие до аудита изменения разделения страниц: MainWindow, ViewModel, PagesViewModelBase, удаление старой дублирующей AIView и пустых Controls. Это необходимо для воспроизводимого состояния проекта в Git: текущий AIViewModel зависит от ранее неотслеживаемого PagesViewModelBase. GGUF, bin/obj и журналы в коммит не включаются.

Полный код изменённых файлов доступен в рабочем дереве и коммите; ручная вставка фрагментов не требуется.

## Проверка

- `dotnet build SpecMind/SpecMind.sln --no-restore --nologo -v quiet`: 0 ошибок; при полной перекомпиляции остаются 87 существующих предупреждений nullable/Windows API.
- Приложение запущено локально; проверены переходы на все шесть страниц через реальное окно Windows с помощью computer-use.
- Кнопка «Назад» на странице подробной информации возвращает на главную.
- Подробная информация показывает считанные характеристики; графики мониторинга получают значения CPU/GPU.
- AIView открывается внутри оболочки, показывает историю, ввод и кнопку отправки. Реальная генерация ещё не подключена и не заявляется проверенной.
- Экспорт показывает четыре формата. Сохранение файлов на этом этапе не тестировалось, сервис экспорта не изменялся.
- Выбор Daylight меняет ресурсы оформления; затем возвращена Night Owl. Обнаружен существующий дефект контраста некоторых кнопок светлой темы; полноценная корректность всех тем не заявляется.
- `git diff --check`: без ошибок пробелов.

## Что остаётся

Этап 2: обновление HardwareInfo на уже открытых страницах, единый мониторинг, сохранение состояния страниц и освобождение ресурсов. При запуске первоначальная Dashboard действительно остаётся с пустыми данными до повторного открытия — дефект подтверждён в GUI.

Этап 3: реальная локальная генерация через AIModule/AIService, фактический hardware snapshot, история и уведомления ChatMessage. Текущий демонстрационный ответ и его обновление не являются завершённым AI-сценарием.
