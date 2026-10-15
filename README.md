# F1 Manager Desktop 2.1

Офлайн-игра для Windows 10/11 x64, C# / Windows Forms / .NET Framework 4.x.

Актуальная игра: [`F1-Manager-Handoff/Game/F1Manager.exe`](F1-Manager-Handoff/Game/F1Manager.exe). Исходники открыты в [`F1-Manager-Handoff/Source`](F1-Manager-Handoff/Source), инструкция игрока — в [`Game/README.md`](F1-Manager-Handoff/Game/README.md). Первоначальный `F1-Manager-Handoff.zip` сохранён без изменений как архив версии 2.0; он не содержит этот апдейт.

В версии 2.1 все сессии рассчитываются целиком за короткой анимацией. Перед Гран-при и спринтом игрок подтверждает стратегию каждого пилота в отдельном окне. Портреты 22 пилотов стартового грида 2026 и Цуноды встроены в EXE. Экономика, контракты, разработка болида, полный календарь, чемпионат, архив и многолетняя карьера сохранены.

## Сборка на Windows

```powershell
cd F1-Manager-Handoff/Source
./Build.ps1
```

Результат: `F1-Manager-Handoff/Build/F1Manager.exe`. Сборка использует системный компилятор .NET Framework и не требует NuGet.

## Облачная проверка в Debian

```bash
bash scripts/setup-cloud.sh
source /workspace/toolchain/activate.sh
F1-Manager-Handoff/Source/Build.sh
mkdir -p F1-Manager-Handoff/Checks
mono F1-Manager-Handoff/Build/F1Manager.exe --self-test /workspace/gamef1/F1-Manager-Handoff/Checks/self-test.txt
xvfb-run -a -s '-screen 0 1600x1100x24' mono F1-Manager-Handoff/Build/F1Manager.exe --ui-test /workspace/gamef1/F1-Manager-Handoff/Checks/UI
```

`setup-cloud.sh` получает подписанные пакеты Debian и извлекает Mono/Xvfb в `/workspace/toolchain`, без прав root и изменения системных пакетов. В обычной системе с установленным Mono достаточно `Source/Build.sh`. Проверки интерфейса создают только тестовые карьеры и снимки в указанной папке. Код возврата ненулевой при провале проверок.

Отчёт текущей сборки: [`Game/TEST-REPORT.txt`](F1-Manager-Handoff/Game/TEST-REPORT.txt). Проверки под Mono не заменяют проверку запуска на Windows.

Источники портретов и сведения о правах: [`Game/driver-photo-credits.json`](F1-Manager-Handoff/Game/driver-photo-credits.json). Снимки могут относиться к предыдущему сезону; портрет закреплён за именем, а не за командой, и сохраняется при переходах.
