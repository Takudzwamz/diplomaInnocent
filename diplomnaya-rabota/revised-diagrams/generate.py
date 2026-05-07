#!/usr/bin/env python3
"""
Генератор упрощённых диаграмм для диплома.
Использует бесплатный API mermaid.ink — не требует ключей.

Использование:
    python generate.py

Результат: PNG-файлы в текущей папке.
"""

import base64
import re
import subprocess
import time
from pathlib import Path

SOURCE_FILE = Path(__file__).parent / "diagrams-source.md"
OUTPUT_DIR = Path(__file__).parent
RETRY_COUNT = 3
RETRY_DELAY = 3
TIMEOUT = 30

# Имена файлов для каждой диаграммы (в порядке появления в source)
FILENAMES = [
    "01_архитектура_общая",
    "02_бд_рекомендации",
    "03_алгоритм_рекомендации",
    "04_формула_гибрид",
    "05_ab_тест",
    "06_результаты_ctr",
    "07_воронка_конверсии",
    "08_use_case",
]


def extract_mermaid_blocks(filepath: Path) -> list[str]:
    """Extract all mermaid code blocks from markdown file."""
    content = filepath.read_text(encoding="utf-8")
    pattern = re.compile(r"```mermaid\s*\n(.*?)```", re.DOTALL)
    return [m.group(1).strip() for m in pattern.finditer(content)]


def download_image(mermaid_code: str, output_path: Path) -> bool:
    """Download diagram PNG via mermaid.ink API (GET with base64)."""
    encoded = base64.urlsafe_b64encode(mermaid_code.encode("utf-8")).decode("ascii")
    url = f"https://mermaid.ink/img/{encoded}?type=png&bgColor=white"

    for attempt in range(1, RETRY_COUNT + 1):
        try:
            result = subprocess.run(
                [
                    "curl", "-s", "-f", "-L",
                    "-o", str(output_path),
                    "--max-time", str(TIMEOUT),
                    url,
                ],
                capture_output=True,
                timeout=TIMEOUT + 10,
            )
            if result.returncode == 0 and output_path.exists() and output_path.stat().st_size > 100:
                return True
            err = result.stderr.decode("utf-8", errors="replace")[:200]
            print(f"    ⚠ Ошибка (попытка {attempt}/{RETRY_COUNT}): код {result.returncode} {err}")
        except subprocess.TimeoutExpired:
            print(f"    ⚠ Таймаут (попытка {attempt}/{RETRY_COUNT})")
        except Exception as e:
            print(f"    ⚠ Ошибка: {e} (попытка {attempt}/{RETRY_COUNT})")

        if attempt < RETRY_COUNT:
            time.sleep(RETRY_DELAY)

    return False


def main():
    print("=" * 50)
    print("  Генерация упрощённых диаграмм для диплома")
    print("=" * 50)

    blocks = extract_mermaid_blocks(SOURCE_FILE)
    print(f"\n  Найдено {len(blocks)} диаграмм в {SOURCE_FILE.name}")

    if len(blocks) != len(FILENAMES):
        print(f"  ⚠ Ожидалось {len(FILENAMES)} диаграмм, найдено {len(blocks)}")

    success = 0
    failed = 0

    for i, (code, name) in enumerate(zip(blocks, FILENAMES), 1):
        output_path = OUTPUT_DIR / f"{name}.png"
        print(f"\n  [{i}/{len(FILENAMES)}] {name}.png ...")

        if download_image(code, output_path):
            size_kb = output_path.stat().st_size / 1024
            print(f"    ✓ Готово ({size_kb:.1f} КБ)")
            success += 1
        else:
            print(f"    ✗ ОШИБКА — не удалось сгенерировать")
            failed += 1

    print(f"\n{'=' * 50}")
    print(f"  Результат: {success} успешно, {failed} ошибок")
    print(f"  Файлы: {OUTPUT_DIR}")
    print(f"{'=' * 50}")


if __name__ == "__main__":
    main()
