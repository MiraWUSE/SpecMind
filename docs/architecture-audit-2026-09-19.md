# SpecMind: исходный аудит и план исправления

Дата: 19 сентября 2026. Исходный HEAD: `403a0b5` (`main`).
Репозиторий: https://github.com/MiraWUSE/SpecMind

## Область проверки

Корень репозитория: `C:\Users\Danilka\Desktop\SpecMindFiles\SpecMind`.
Корень приложения: `C:\Users\Danilka\Desktop\SpecMindFiles\SpecMind\SpecMind\SpecMind`.
Все пути ниже, кроме явно оговорённых, относительно корня приложения.

Проверены MainWindow, App, Program, ViewLocator, все шесть страниц с code-behind и ViewModel, HardwareInfo, HardwareScannerService и весь Modules/AI. Проверены зависимости, Git и локальное наличие модели. Инструкций AGENTS.md в рабочем дереве не найдено.

Аудит относится к существующему рабочему дереву: до начала работы были 13 изменённых/удалённых отслеживаемых файлов и новый `ViewModels/Pages/PagesViewModelBase.cs`. Эти изменения не выполнены в рамках аудита и не включаются в его коммит. Код приложения на этом этапе не изменяется.

## 1. Что реализовано

- .NET 9, Avalonia 11.2.1, CommunityToolkit.Mvvm 8.4.1, LibreHardwareMonitorLib 0.9.6, System.Management 10.0.9, LLamaSharp и CPU backend 0.27.0.
- `Views/MainWindow.axaml` уже содержит единую навигацию и один ContentControl с CurrentPage.
- Все шесть страниц находятся в `Views/Pages`, у каждой есть code-behind.
- `Services/HardwareScannerService.cs` собирает оборудование, мониторы и датчики через WMI и LibreHardwareMonitor.
- `ViewModels/MainWindowViewModel.cs` хранит HardwareInfo, четыре коллекции графиков и секундный таймер.
- `ViewModels/Pages/ExportViewModel.cs` содержит команды четырёх форматов и диалог сохранения; экспорт вынесен в ReportExporterService.
- `ViewModels/Pages/SettingsViewModel.cs` содержит темы, выбор категории и команды смены темы.
- В Modules/AI есть конфигурация, окружение, менеджер модели, сервис, snapshot/prompt builder, provider и runtime.

## 2. Что работает и что ещё не подтверждено

Подтверждены наличие SDK 9.0.308, доступность локальных зависимостей для сборки, совпадение origin с заданным репозиторием, наличие локального GGUF и правило его исключения из Git.

По коду присутствуют сканирование, экспорт, темы и графики. Их полноценная работа в текущем GUI **не подтверждена**: сборка приложения не проходит. Реальная генерация Qwen не запускалась. Наличие GGUF не доказывает его целостность или успешную загрузку.

## 3. Частично реализованное

- Разделение страниц выполнено физически, но XAML сохранил старые привязки и неверные пространства имён.
- AIViewModel физически лежит в `ViewModels/Pages/AIViewModel.cs`, namespace — `SpecMind.Modules.AI.ViewModels`. Наследование уже идёт через PagesViewModelBase к ViewModelBase.
- AIViewModel принимает MainWindowViewModel и возвращает задержанную заглушку, не вызывает AIService.
- AIView содержит список сообщений и ввод, но не связан с моделью и не отображает полноценно IsBusy/IsThinking.
- ModelDownloader фактически только регистрирует локальный файл: сетевой загрузки нет, ModelsDirectory не присваивается, IsInstalled при регистрации не выставляется.
- Конфигурация сохраняется, но параметры генерации и путь модели практически не подключены к runtime.

## 4. Ошибки компиляции

Команда из корня репозитория:

```powershell
dotnet build SpecMind\SpecMind.sln --no-restore --nologo -v quiet
```

Результат: **245 ошибок Avalonia**, код возврата 1. Первая сборка также показала 87 предупреждений (в том числе nullable-аннотации при отключённом nullable). Повторная инкрементальная сборка показала те же 245 ошибок и 0 предупреждений; это не означает устранение предупреждений. Локальный полный журнал: `baseline-build.log` в корне репозитория, исключён правилом `*.log`.

Основные причины каскада AVLN2000/AVLN2100:

- В корневых элементах страниц не указаны x:DataType при включённых compiled bindings; типизация нужна и шаблонам данных.
- `Views/Pages/DashboardView.axaml` и `MonitoringView.axaml` ищут контролы в SpecMind.Controls, но реальные CircularGauge/SimpleLineChart объявлены в SpecMind.Views.
- `Views/Pages/AIView.axaml` обращается к неразрешимому BooleanToTextConverter.Instance; для подписи уже есть ChatMessage.Sender.
- `Views/Pages/SettingsView.axaml` использует необъявленный префикс vm.
- `Views/Pages/DetailedView.axaml` указывает SpecMind.Converters, тогда как существующие конвертеры находятся в SpecMind.ViewModels.

Дополнительные ошибки контрактов привязок, которые нужно устранить вместе с типизацией:

- DetailedView, ExportView, SettingsView ищут ShowDashboardCommand в своих ViewModel, где её нет.
- SettingsView ищет отсутствующее IsSettingsVisible.
- Шаблон темы ищет ApplyThemeCommand у Window.DataContext, хотя команда уже перенесена в SettingsViewModel.

## 5. Архитектурные противоречия

- Getter HardwareInfo в Dashboard/Detailed/Monitoring не пересылает PropertyChanged при замене HardwareInfo в MainWindowViewModel. Первоначально пустые/старые данные могут оставаться на странице.
- Каждый переход создаёт новую PageViewModel. Для AI это потеря диалога; для Monitoring — новый таймер, который не останавливается.
- MainWindowViewModel не освобождает scanner и свой таймер при закрытии.
- HardwareScannerService при каждом сканировании создаёт и открывает новый Computer, перезаписывая предыдущий без Close. Нужна минимальная коррекция жизненного цикла, а не переписывание сканирования.
- После Task.Delay сканирование выполняет синхронные WMI-вызовы; вызов с UI может блокировать интерфейс. Повторные async Tick не защищены от наложения.
- ChatMessage — обычный класс без уведомлений. Присваивание Message/IsThinking после добавления в коллекцию не уведомляет интерфейс.
- В сканере часть полей рассчитана эвристически (например, MaxClock и CacheL1); AI нельзя представлять их как независимо измеренные достоверные факты. Неизвестные/нулевые показания также требуют осторожной формулировки.

## 6. Дублирование ответственности

- MainWindowViewModel и MonitoringViewModel независимо содержат четыре истории и таймеры. Страница должна отображать общие данные мониторинга.
- AIModule, AIConfigurationService и ModelDownloader создают отдельные AIEnvironment и повторно инициализируют одинаковые папки.
- AIConfiguration задаёт ContextSize=8192/MaxTokens=2048/UseGpu=true, runtime жёстко использует 4096/CPU, executor — 512 токенов. Настройки не являются единым источником параметров.
- ChatMemory хранит UI-историю, PromptBuilder повторно формирует полный диалог, а один InteractiveExecutor переиспользуется. При подключении необходимо согласовать историю и состояние контекста, чтобы не накапливать повторённый prompt.

## 7. Конструкторы

Описанной в исходном промпте ошибки передачи MainWindowViewModel в конструктор AIViewModel(AIService) в текущем дереве уже нет: конструктор изменён на AIViewModel(MainWindowViewModel). Но это достигнуто удалением реальной интеграции и подстановкой заглушки.

Существующий AIModule собирает AIService(PromptBuilder, SnapshotBuilder, IChatProvider), однако App/MainWindowViewModel его не создают. Нужно восстановить зависимость AIViewModel от сервиса через один AIModule, а не закреплять заглушку.

## 8. ViewLocator

`ViewLocator.cs` содержит явные соответствия пяти обычных страниц и MainWindow, но не AIViewModel → Views.Pages.AIView. Match уже принимает AIViewModel через ViewModelBase. Следовательно, причина отсутствия AI-страницы сейчас — отсутствующая запись в ViewMap, а не наследование или автоматическая замена namespace.

## 9. Путь к GGUF

`Modules/AI/AIModule.cs` и `Runtime/LLamaTest.cs` используют:

```text
AppContext.BaseDirectory/Models/qwen2.5-3b-instruct-q4_k_m.gguf
```

Путь жёстко задан, Configuration.ModelsDirectory не используется. AIModule немедленно бросает FileNotFoundException при отсутствии файла — нельзя подключать такой конструктор к запуску всего GUI без обработки этого сценария.

Найден локальный файл `Models/qwen2.5-3b-instruct-q4_k_m.gguf`, размер **2 104 932 768 байт**. SpecMind.csproj содержит CopyToOutputDirectory=PreserveNewest для Models/*.gguf. Успешное копирование после сборки пока не подтверждено.

В корневом `.gitignore` уже есть `*.gguf`; git check-ignore подтверждает исключение, git ls-files '*.gguf' пуст. Модель не должна включаться в последующие коммиты.

## 10. Получает ли AI реальные характеристики

Нет. Текущая UI-заглушка вообще не вызывает модель. Если подключить AIService без исправлений, оба пути генерации вызовут SnapshotBuilder.BuildAsync(), который создаёт пустой new HardwareInfo(). Метод Build(HardwareInfo) существует, но сервис его не использует.

Дополнительно PromptBuilder вызывает HardwareInfo.ToString(), а HardwareInfo не переопределяет ToString(): получится имя типа вместо CPU/GPU/RAM/датчиков. Нужно явно формировать представление фактических полей актуального снимка.

PromptBuilder не ограничивает историю и не фильтрует thinking/служебные сообщения. Добавление текущего вопроса в Memory до передачи всей истории приведёт к его дублированию в history и userMessage. В действующем UI это пока не проявляется, потому что сервис не вызывается.

## 11. Повторная загрузка и lifecycle модели

В текущем пользовательском сценарии модель вообще не загружается. QwenProvider проверяет IsLoaded, LLamaRuntime повторяет проверку — последовательные вызовы одного runtime не должны заново грузить веса. Но защиты от конкурентных загрузок нет, состояние Error при исключении не выставляется, частично созданные ресурсы не очищаются надёжно. Время жизни AIModule/runtime не связано с закрытием приложения.

Смена страницы не должна создавать новый модуль или новый диалог. Нужны один runtime, сериализация операций, освобождение ресурсов и понятная ошибка внутри AI-страницы при отсутствующей/повреждённой модели.

## 12. Временный код

- `Program.cs` уже запускает только Avalonia, вызова LLamaTest.RunAsync там нет.
- `Modules/AI/Runtime/LLamaTest.cs` остался отдельным неиспользуемым тестовым классом.
- `ViewModels/Pages/AIViewModel.cs`: заглушки SendAsync/TestAI, Task.Delay и комментарии о временной интеграции.
- `SnapshotBuilder.BuildAsync()`: временное создание пустого HardwareInfo.
- `ViewLocator.cs`: комментарий об отложенном добавлении AIViewModel.
- ChatMemory уже имеет все четыре метода: AddUser, AddAssistantThinking, AddAssistant, Clear; ошибки отсутствующего AddAssistant нет.

## План по этапам с критериями приёмки

### Этап 1. Восстановить сборку и навигацию

Файлы: `Views/Pages/{Dashboard,Detailed,Monitoring,Export,Settings,AI}View.axaml`, `ViewLocator.cs`; при необходимости небольшие делегирующие команды в соответствующих `ViewModels/Pages/*ViewModel.cs`.

Исправить namespace, типы compiled bindings и DataTemplate, конвертеры, старые команды и источник ApplyThemeCommand. Добавить явное сопоставление AIViewModel. Исправить кодировку текста AIView. Сохранить существующий дизайн и единую оболочку.

Приёмка: dotnet build успешен; все шесть страниц открываются; нет ошибок привязок; возврат и темы работают. На этом этапе AI ещё явно не считается подключённым к модели.

### Этап 2. Общее состояние и жизненный цикл страниц

Файлы: `ViewModels/MainWindowViewModel.cs`, `ViewModels/Pages/PagesViewModelBase.cs`, `DashboardViewModel.cs`, `DetailedViewModel.cs`, `MonitoringViewModel.cs`, `App.axaml.cs`; минимальные необходимые изменения в `Services/HardwareScannerService.cs`.

Переиспользовать страницы, пересылать уведомления об актуальном HardwareInfo, использовать общие коллекции мониторинга вместо второго таймера. Не допускать перекрывающихся опросов, обеспечить остановку таймера и освобождение scanner. Закрыть подтверждённую утечку Computer без переработки алгоритмов определения оборудования.

Приёмка: успешная сборка; данные обновляются без повторного открытия страниц; переходы не размножают таймеры; закрытие освобождает ресурсы.

### Этап 3. Подключить базовый локальный чат

Файлы: `Modules/AI/AIModule.cs`, `Modules/AI/Services/{AIService,SnapshotBuilder,PromptBuilder}.cs`, `Modules/AI/Models/ChatMessage.cs`, `Modules/AI/Runtime/{LLamaRuntime,LLamaExecutor,ChatMemory}.cs`, `Modules/AI/Providers/QwenProvider.cs`, `ViewModels/Pages/AIViewModel.cs`, `Views/Pages/AIView.axaml`, точки композиции в `App.axaml.cs`/`MainWindowViewModel.cs`. Удалить ненужный `Modules/AI/Runtime/LLamaTest.cs` после подтверждения реального сценария.

Использовать один существующий AIModule; передавать AIService в AIViewModel и актуальные данные сканера в snapshot. Явно сериализовать характеристики; передавать историю без текущего вопроса и служебных сообщений; согласовать формат Qwen и состояние executor. Подключить уведомляемые сообщения, IsBusy, ошибки и безопасное повторное использование/освобождение runtime. Отсутствие модели не должно препятствовать запуску GUI. Базовая генерация имеет приоритет над streaming.

Приёмка: успешная сборка; проверки prompt на реальные поля и единственное включение вопроса; реальный ответ локального GGUF в чате; второй вопрос сохраняет смысл истории без повторной загрузки весов; переход со страницы и обратно сохраняет диалог; отсутствие модели даёт понятную ошибку и не закрывает приложение.

### После базового сценария

Потоковая генерация, автопрокрутка, очистка диалога, анализ ПК одной кнопкой и управление загрузкой модели — отдельные согласованные улучшения. Не включать крупные дополнительные функции в текущую стабилизацию.

## Порядок фиксации изменений

Аудит фиксируется отдельным документационным коммитом. Исходная сборка уже сломана; это зафиксированный baseline, а не регрессия от аудита. Каждый этап изменения кода должен завершаться успешной сборкой, проверкой diff, отдельным коммитом и обычным push в заданный origin без force. Полные изменённые файлы будут доступны в рабочем дереве и коммите, а не как фрагменты для ручной вставки.

Переход к этапу 1 ожидает подтверждения пользователя согласно прямому требованию исходного промпта: «не переходи к следующему этапу без подтверждения».
