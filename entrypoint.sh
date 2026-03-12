#!/bin/bash
set -e

echo "🚀 מפעיל LibreTranslate..."
/opt/libretranslate-venv/bin/libretranslate \
  --load-only en,he,ar,zh,es,fr,de,ru,pt,ja,ko,it,tr,pl,nl,vi,th,id,uk,fa,hi,sv \
  --host 0.0.0.0 \
  --port 5000 &

echo "✅ LibreTranslate רץ"

echo "🚀 מפעיל Kheper Web..."
exec dotnet Kheper.Web.dll
