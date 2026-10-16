param([string]$OutputDirectory = '')
$ErrorActionPreference = 'Stop'
$taskSource = $PSScriptRoot
$taskOutput = if ($OutputDirectory) { $OutputDirectory } else { Join-Path $taskSource '..\Build' }
$taskOutput = [System.IO.Path]::GetFullPath($taskOutput)
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
Add-Type -AssemblyName System.Drawing
$taskBitmap = New-Object System.Drawing.Bitmap(256,256)
$taskGraphics = [System.Drawing.Graphics]::FromImage($taskBitmap)
$taskGraphics.Clear([System.Drawing.Color]::FromArgb(12,14,18))
$taskGraphics.SmoothingMode = 'AntiAlias'
$taskGraphics.TextRenderingHint = 'AntiAliasGridFit'
$taskBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(235,35,43))
$taskGraphics.FillRectangle($taskBrush,0,0,256,32)
$taskFont = New-Object System.Drawing.Font('Segoe UI',100,[System.Drawing.FontStyle]::Bold,[System.Drawing.GraphicsUnit]::Pixel)
$taskGraphics.DrawString('F1',$taskFont,[System.Drawing.Brushes]::White,22,58)
$taskIcon = [System.Drawing.Icon]::FromHandle($taskBitmap.GetHicon())
$taskStream = [System.IO.File]::Create((Join-Path $taskSource 'app.ico'))
$taskIcon.Save($taskStream)
$taskStream.Dispose(); $taskIcon.Dispose(); $taskGraphics.Dispose(); $taskBitmap.Dispose(); $taskBrush.Dispose(); $taskFont.Dispose()
$taskArgs = @('/nologo','/warnaserror+','/optimize+','/target:winexe','/platform:x64',('/out:' + (Join-Path $taskOutput 'F1Manager.exe')),('/win32icon:' + (Join-Path $taskSource 'app.ico')),('/win32manifest:' + (Join-Path $taskSource 'app.manifest')),'/r:System.Web.Extensions.dll','/r:System.Windows.Forms.dll','/r:System.Drawing.dll')
Get-ChildItem -LiteralPath (Join-Path $taskSource 'assets') -Filter '*.jpg' | Sort-Object Name | ForEach-Object { $taskIndex = [int]$_.BaseName; $taskArgs += ('/resource:' + $_.FullName + ',track' + $taskIndex) }
$taskArgs += ('/resource:' + (Join-Path $taskSource 'assets\credits.json') + ',credits')
Get-ChildItem -LiteralPath (Join-Path $taskSource 'assets\drivers') -Filter '*.png' | Sort-Object Name | ForEach-Object { $taskArgs += ('/resource:' + $_.FullName + ',driver' + [int]$_.BaseName) }
$taskArgs += ('/resource:' + (Join-Path $taskSource 'assets\drivers\credits.json') + ',drivercredits')
Get-ChildItem -LiteralPath (Join-Path $taskSource 'assets\teams') -Filter '*.png' | Sort-Object Name | ForEach-Object { $taskArgs += ('/resource:' + $_.FullName + ',team' + [int]$_.BaseName) }
$taskArgs += ('/resource:' + (Join-Path $taskSource 'assets\teams\credits.json') + ',teamcredits')
Get-ChildItem -LiteralPath (Join-Path $taskSource 'assets\tyres') -Filter '*.png' | Sort-Object Name | ForEach-Object { $taskArgs += ('/resource:' + $_.FullName + ',tyre' + $_.BaseName) }
$taskArgs += ('/resource:' + (Join-Path $taskSource 'assets\tyres\credits.json') + ',tyrecredits')
$taskArgs += ('/resource:' + (Join-Path $taskSource 'assets\parts\atlas.png') + ',partatlas')
$taskArgs += ('/resource:' + (Join-Path $taskSource 'assets\parts\credits.json') + ',partcredits')
$taskArgs += ('/resource:' + (Join-Path $taskSource 'assets\maps\circuits.json') + ',circuitmaps')
$taskArgs += ('/resource:' + (Join-Path $taskSource 'assets\maps\credits.json') + ',mapcredits')
$taskArgs += ('/resource:' + (Join-Path $taskSource 'assets\maps\LICENSE.txt') + ',maplicense')
foreach ($taskFile in @('Models.cs','Data.cs','CarModel.cs','Research.cs','Powertrain.cs','RaceDirector.cs','RaceUI.cs','TrackMaps.cs','TyreArt.cs','StartupPresentation.cs','Patch252Tests.cs','Patch251Tests.cs','Patch25Tests.cs','WeatherModel.cs','EngineeringTests.cs','ResearchTests.cs','Patch242Tests.cs','Engine.cs','UI.cs','Workflow.cs','Shell.cs','FrameClock.cs','SliderInput.cs','DevelopmentUI.cs','ContractUI.cs','Presentations.cs','StrategyUI.cs','UITests.cs','Program.cs','AssemblyInfo.cs')) { $taskArgs += (Join-Path $taskSource $taskFile) }
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' @taskArgs
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed' }
Write-Output ('Built: ' + (Join-Path $taskOutput 'F1Manager.exe'))
