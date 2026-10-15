#!/usr/bin/env python3
"""Pack the checked-in, trimmed 60 fps video into seekable JPEG frames for offline WinForms playback.
Only asset regeneration needs ffmpeg; the game has no video codec dependency.
"""
import hashlib
import json
from pathlib import Path
import struct
import subprocess
import tempfile
root = Path(__file__).resolve().parents[1]
assets = root / 'F1-Manager-Handoff/Source/assets/transition'
video = assets / 'session.mp4'
with tempfile.TemporaryDirectory(prefix='f1-transition-') as directory:
    subprocess.run(['ffmpeg', '-v', 'error', '-i', str(video), '-vf', 'fps=60', '-q:v', '5', str(Path(directory)/'%04d.jpg')], check=True)
    frames = sorted(Path(directory).glob('*.jpg'))
    if len(frames) != 210:
        raise ValueError('Expected 210 frames / 3.5 seconds at 60 FPS')
    with (assets/'session.frames').open('wb') as output:
        output.write(struct.pack('<6i', 0x52543146, 1, 60, len(frames), 1280, 720))
        for path in frames:
            data = path.read_bytes()
            output.write(struct.pack('<i', len(data)))
            output.write(data)
metadata = {'source': 'User supplied gemini_generated_video_95978ab4.mp4', 'source_sha256': '736b1cf30829c224101afce856376da9a2a307331707cde38b026e115cfb42c9', 'trim_seconds': [0, 3.5], 'fps': 60, 'frames': 210, 'audio': False, 'processing': 'ffmpeg minterpolate=mci:aobmc:vsbmc, H.264 CRF 20; JPEG frame pack q=5', 'video_sha256': hashlib.sha256(video.read_bytes()).hexdigest(), 'frames_sha256': hashlib.sha256((assets/'session.frames').read_bytes()).hexdigest()}
# Keep the supplied source checksum when regenerating the playback pack.
old = assets/'credits.json'
if old.exists():
    metadata['source_sha256'] = json.loads(old.read_text())['source_sha256']
old.write_text(json.dumps(metadata, ensure_ascii=False, indent=2)+'\n')
