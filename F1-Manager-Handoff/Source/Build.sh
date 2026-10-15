#!/usr/bin/env bash
set -euo pipefail
source_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
output_dir="${1:-$source_dir/../Build}"
mkdir -p "$output_dir"
output_dir="$(cd "$output_dir" && pwd)"
compiler=(mcs)
if [[ -n "${MONO_ROOT:-}" ]]; then
  export MONO_CFG_DIR="$MONO_ROOT/etc"
  export MONO_PATH="$MONO_ROOT/usr/lib/mono/4.5"
  export LD_LIBRARY_PATH="$MONO_ROOT/usr/lib:$MONO_ROOT/usr/lib/x86_64-linux-gnu${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
  compiler=("$MONO_ROOT/usr/bin/mono-sgen" "$MONO_PATH/mcs.exe")
fi
args=(-nologo -optimize+ -target:winexe -platform:x64 "-out:$output_dir/F1Manager.exe" "-win32icon:$source_dir/app.ico" "-win32manifest:$source_dir/app.manifest" -r:System.Web.Extensions -r:System.Windows.Forms -r:System.Drawing)
for file in "$source_dir"/assets/*.jpg; do
  index="$(basename "$file" .jpg)"
  args+=("-resource:$file,track$((10#$index))")
done
for file in "$source_dir"/assets/drivers/*.png; do
  index="$(basename "$file" .png)"
  args+=("-resource:$file,driver$((10#$index))")
done
args+=("-resource:$source_dir/assets/credits.json,credits" "-resource:$source_dir/assets/drivers/credits.json,drivercredits")
for file in Models Data Engine UI Workflow UITests Program AssemblyInfo; do args+=("$source_dir/$file.cs"); done
"${compiler[@]}" "${args[@]}"
printf 'Built: %s/F1Manager.exe\n' "$output_dir"
