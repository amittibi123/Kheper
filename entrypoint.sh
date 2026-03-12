#!/bin/bash

echo "🚀 מפעיל LibreTranslate..."
/opt/libretranslate-venv/bin/libretranslate \
  --load-only en,he \
  --host 0.0.0.0 \
  --port 5000 2>&1 &

LIBRE_PID=$!
echo "✅ LibreTranslate רץ עם PID: $LIBRE_PID"

echo "🚀 מפעיל Kheper Web..."
exec dotnet Kheper.Web.dll
