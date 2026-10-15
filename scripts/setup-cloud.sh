#!/usr/bin/env bash
# Debian cloud-only build/test dependencies, extracted without root or system changes.
set -euo pipefail
tool_dir=/workspace/toolchain
mkdir -p "$tool_dir/apt/lists/partial" "$tool_dir/apt/archives/partial" "$tool_dir/apt/etc" "$tool_dir/root"
cat > "$tool_dir/apt/apt.conf" <<'CONF'
Dir::Etc::parts "/workspace/toolchain/apt/etc";
Dir::Etc::main "-";
Dir::Etc::sourcelist "/workspace/toolchain/apt/etc/sources.list";
Dir::Etc::sourceparts "-";
Dir::State::lists "/workspace/toolchain/apt/lists";
Dir::Cache::archives "/workspace/toolchain/apt/archives";
APT::Sandbox::User "agent";
Debug::NoLocking "true";
CONF
printf 'deb [signed-by=/usr/share/keyrings/debian-archive-keyring.gpg] https://deb.debian.org/debian trixie main\n' > "$tool_dir/apt/etc/sources.list"
APT_CONFIG="$tool_dir/apt/apt.conf" apt-get update
APT_CONFIG="$tool_dir/apt/apt.conf" apt-get --download-only -y install mono-mcs libmono-system-windows-forms4.0-cil libmono-system-web-extensions4.0-cil xvfb xauth libgdiplus fonts-dejavu-core
for archive in "$tool_dir"/apt/archives/*.deb; do dpkg-deb -x "$archive" "$tool_dir/root"; done
cat > "$tool_dir/activate.sh" <<'ACTIVATE'
export MONO_ROOT=/workspace/toolchain/root
export MONO_CFG_DIR="$MONO_ROOT/etc"
export MONO_PATH="$MONO_ROOT/usr/lib/mono/4.5"
export LD_LIBRARY_PATH="$MONO_ROOT/usr/lib:$MONO_ROOT/usr/lib/x86_64-linux-gnu${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
export PATH="$MONO_ROOT/usr/bin:$PATH"
export XDG_CACHE_HOME=/workspace/toolchain/cache
mkdir -p "$XDG_CACHE_HOME"
mono() { "$MONO_ROOT/usr/bin/mono-sgen" "$@"; }
ACTIVATE
source "$tool_dir/activate.sh"
mono --version
