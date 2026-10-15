# F1 Manager Desktop 2.2

Офлайн-игра для Windows 10/11 x64, C# / Windows Forms / .NET Framework 4.x.

Актуальная игра: [`F1-Manager-Handoff/Game/F1Manager.exe`](F1-Manager-Handoff/Game/F1Manager.exe). Исходники открыты в [`F1-Manager-Handoff/Source`](F1-Manager-Handoff/Source), инструкция игрока — в [`Game/README.md`](F1-Manager-Handoff/Game/README.md).

В версии 2.2 запуск начинается с главного меню: «Играть», «Настройки» и «Выйти из игры». В настройках доступны полноэкранный режим и уменьшение анимаций; F11 переключает экран, Esc возвращает в меню. При существующей карьере «Играть» продолжает её. Все сессии рассчитываются целиком за короткой анимацией. Перед Гран-при и спринтом игрок подтверждает стратегию каждого пилота на общей панели внутри игры с плавным появлением и затемнением фона. Портреты 22 пилотов стартового грида 2026 и Цуноды встроены в EXE. Экономика, контракты, разработка болида, полный календарь, чемпионат, архив и многолетняя карьера сохранены.

## Сборка на Windows

```powershell
cd F1-Manager-Handoff/Source
./Build.ps1
```

Результат: `F1-Manager-Handoff/Build/F1Manager.exe`. Сборка использует системный компилятор .NET Framework и не требует NuGet. Облачная сборка явно использует reference assemblies .NET Framework 4.5 (`-sdk:4.5`), а предупреждения компилятора считаются ошибками. Это предотвращает случайное использование API Mono, отсутствующего на Windows, включая несовместимый вариант `String.Split`.

## Облачная проверка в Debian

```bash
bash scripts/setup-cloud.sh
source /workspace/toolchain/activate.sh
F1-Manager-Handoff/Source/Build.sh
mkdir -p F1-Manager-Handoff/Checks
mono F1-Manager-Handoff/Build/F1Manager.exe --self-test /workspace/gamef1/F1-Manager-Handoff/Checks/self-test.txt
xvfb-run -a -s '-screen 0 1600x1100x24' mono F1-Manager-Handoff/Build/F1Manager.exe --ui-test /workspace/gamef1/F1-Manager-Handoff/Checks/UI
```

`setup-cloud.sh` получает подписанные пакеты Debian и извлекает Mono/Xvfb и совместимые reference assemblies в `/workspace/toolchain`, без прав root и изменения системных пакетов. В обычной системе с установленным Mono достаточно `Source/Build.sh`. Проверки интерфейса создают только тестовые карьеры и снимки в указанной папке. Код возврата ненулевой при провале проверок.

Отчёты и снимки создаются заново в `F1-Manager-Handoff/Checks/` и не хранятся в Git. Проверки под Mono не заменяют проверку запуска на Windows.

Источники портретов и сведения о правах: [`Game/driver-photo-credits.json`](F1-Manager-Handoff/Game/driver-photo-credits.json). Снимки могут относиться к предыдущему сезону; портрет закреплён за именем, а не за командой, и сохраняется при переходах.

## Структура репозитория

- `F1-Manager-Handoff/Game/` — актуальный EXE, инструкция и сведения об источниках фотографий.
- `F1-Manager-Handoff/Source/` — исходники игры, проверки, сборочные скрипты и все встраиваемые ресурсы.
- `F1-Manager-Handoff/F1-Manager-SHA256.txt` — контрольная сумма готового EXE.
- `scripts/` — подготовка облачной среды для разработки.

Сборочные результаты, тестовые отчёты, снимки интерфейса и пользовательские сохранения исключены из Git.

Оригинальные логотипы 11 реальных команд встроены в EXE и показаны при выборе команды, в шапке игры и командном зачёте. Источники и права: [`Game/team-logo-credits.json`](F1-Manager-Handoff/Game/team-logo-credits.json). Внешнее подключение для загрузки логотипов не требуется. Настройки хранятся в `Game/Saves/settings.json` отдельно от карьеры.

В выборе стратегии используются четыре игровые карточки вместо системного выпадающего списка. Для каждого отрезка шины выбираются по изображениям и полным названиям; «+» и «−» меняют число пит-стопов, круги можно отредактировать отдельно. Изображения шин встроены в EXE и также показаны в результатах. Источники: [`Game/tyre-photo-credits.json`](F1-Manager-Handoff/Game/tyre-photo-credits.json).

Новые элементы интерфейса находятся в `Source/Shell.cs` (меню, настройки, переходы и встроенная симуляция) и `Source/StrategyUI.cs` (карточки планов и визуальный выбор шин).
