#!/usr/bin/env python3
"""
Генератор изображений диаграмм из Mermaid-блоков в Markdown-файлах.

Использует бесплатный API kroki.io (POST, не требует ключей/регистрации).
Поддерживает Cyrillic, большие диаграммы, PNG/SVG.

Использование:
    python generate_diagrams.py          # Генерация PNG
    python generate_diagrams.py --svg    # Генерация SVG

Результат: папка graphs/ с пронумерованными файлами + INDEX.md.
"""

import os
import re
import subprocess
import sys
import time
from pathlib import Path

# ============ НАСТРОЙКИ ============

API_URL = "https://kroki.io/mermaid"
OUTPUT_DIR = Path(__file__).parent / "graphs"
DOCS_DIR = Path(__file__).parent
RETRY_COUNT = 3
RETRY_DELAY = 2  # секунд между попытками
TIMEOUT = 45  # секунд на запрос

# Файлы для обработки (в порядке)
FILES_TO_PROCESS = [
    "02-ARCHITECTURE.md",
    "03-ALGORITHMS.md",
    "04-DATABASE.md",
    "05-AB-TESTING.md",
    "06-METRICS.md",
    "09-UML-DIAGRAMS.md",
    "10-GRAPHS.md",
]

# ============ ФУНКЦИИ ============


def extract_mermaid_blocks(filepath: Path) -> list[dict]:
    """Извлекает все блоки ```mermaid ... ``` из файла."""
    content = filepath.read_text(encoding="utf-8")
    blocks = []

    # Находим все mermaid блоки и ближайший заголовок перед ними
    mermaid_pattern = re.compile(r"```mermaid\s*\n(.*?)```", re.DOTALL)
    heading_pattern = re.compile(r"^#{1,4}\s+(.+)$", re.MULTILINE)

    headings = [(m.start(), m.group(1)) for m in heading_pattern.finditer(content)]

    for match in mermaid_pattern.finditer(content):
        code = match.group(1).strip()
        pos = match.start()

        # Найти ближайший заголовок перед этим блоком
        title = "diagram"
        for h_pos, h_text in reversed(headings):
            if h_pos < pos:
                title = h_text
                break

        blocks.append({"title": title, "code": code})

    return blocks


def sanitize_filename(title: str) -> str:
    """Преобразует заголовок в безопасное имя файла."""
    clean = re.sub(r"[^\w\s\-]", "", title)
    clean = re.sub(r"\s+", "_", clean.strip())
    return clean[:60]


def download_image(mermaid_code: str, output_path: Path, fmt: str = "png") -> bool:
    """
    Скачивает изображение диаграммы через kroki.io API (POST via curl).
    fmt: 'png' или 'svg'
    """
    url = f"{API_URL}/{fmt}"

    for attempt in range(1, RETRY_COUNT + 1):
        try:
            result = subprocess.run(
                [
                    "curl", "-s", "-f",
                    "-X", "POST", url,
                    "-H", "Content-Type: text/plain; charset=utf-8",
                    "--data-binary", "@-",
                    "-o", str(output_path),
                    "--max-time", str(TIMEOUT),
                ],
                input=mermaid_code.encode("utf-8"),
                capture_output=True,
                timeout=TIMEOUT + 5,
            )
            if result.returncode == 0 and output_path.exists() and output_path.stat().st_size > 100:
                return True
            err = result.stderr.decode("utf-8", errors="replace")[:200]
            print(f"    ⚠ curl код {result.returncode}: {err} (попытка {attempt}/{RETRY_COUNT})")
        except subprocess.TimeoutExpired:
            print(f"    ⚠ Таймаут (попытка {attempt}/{RETRY_COUNT})")
        except Exception as e:
            print(f"    ⚠ Ошибка: {e} (попытка {attempt}/{RETRY_COUNT})")

        if attempt < RETRY_COUNT:
            time.sleep(RETRY_DELAY)

    return False


def main():
    # Определяем формат вывода
    fmt = "png"
    ext = "png"
    if "--svg" in sys.argv:
        fmt = "svg"
        ext = "svg"

    # Создаём директорию для вывода
    OUTPUT_DIR.mkdir(exist_ok=True)

    print("=" * 60)
    print("  Генератор диаграмм — mermaid.ink API")
    print(f"  Формат: {ext.upper()}")
    print(f"  Выходная папка: {OUTPUT_DIR}")
    print("=" * 60)
    print()

    total = 0
    success = 0
    failed = 0
    global_index = 0

    for filename in FILES_TO_PROCESS:
        filepath = DOCS_DIR / filename
        if not filepath.exists():
            print(f"⚠ Файл не найден: {filename}")
            continue

        blocks = extract_mermaid_blocks(filepath)
        if not blocks:
            continue

        file_stem = filepath.stem  # e.g., "02-ARCHITECTURE"
        print(f"📄 {filename} — {len(blocks)} диаграмм(а)")

        for i, block in enumerate(blocks, 1):
            global_index += 1
            total += 1

            title_slug = sanitize_filename(block["title"])
            output_name = f"{file_stem}_{i:02d}_{title_slug}.{ext}"
            output_path = OUTPUT_DIR / output_name

            print(f"  [{global_index:02d}] {block['title'][:50]}...", end=" ")

            if download_image(block["code"], output_path, fmt):
                size_kb = output_path.stat().st_size / 1024
                print(f"✅ ({size_kb:.1f} KB)")
                success += 1
            else:
                print("❌ ОШИБКА")
                failed += 1

            # Пауза между запросами чтобы не перегружать API
            time.sleep(0.5)

        print()

    # Итоги
    print("=" * 60)
    print(f"  Итого: {total} диаграмм")
    print(f"  ✅ Успешно: {success}")
    if failed:
        print(f"  ❌ Ошибок: {failed}")
    print(f"  📁 Файлы: {OUTPUT_DIR}/")
    print("=" * 60)

    # Создаём index.md для удобного просмотра
    if success > 0:
        create_index(ext)

    return 0 if failed == 0 else 1


def create_index(ext: str):
    """Создаёт index.md со всеми сгенерированными изображениями."""
    index_path = OUTPUT_DIR / "INDEX.md"
    images = sorted(OUTPUT_DIR.glob(f"*.{ext}"))

    lines = [
        "# Все сгенерированные диаграммы и графики\n",
        f"> Сгенерировано автоматически через mermaid.ink API\n",
        f"> Формат: {ext.upper()}, количество: {len(images)}\n\n",
    ]

    current_file = ""
    for img in images:
        # Извлекаем исходный файл из имени
        parts = img.stem.split("_", 2)
        file_prefix = f"{parts[0]}_{parts[1]}" if len(parts) > 1 else parts[0]

        if file_prefix != current_file:
            current_file = file_prefix
            lines.append(f"\n## {file_prefix}\n\n")

        title = parts[2].replace("_", " ") if len(parts) > 2 else img.stem
        if ext == "svg":
            lines.append(f"### {title}\n\n![{title}]({img.name})\n\n")
        else:
            lines.append(f"### {title}\n\n![{title}]({img.name})\n\n")

    index_path.write_text("".join(lines), encoding="utf-8")
    print(f"\n  📋 Индекс: {index_path}")


if __name__ == "__main__":
    sys.exit(main())
