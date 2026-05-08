#!/usr/bin/env python3
"""
Генератор профессиональных диаграмм для дипломной работы.
ВСЕ тексты на русском языке, крупные шрифты, высокая читаемость.

Использует: matplotlib (графики), graphviz (блок-схемы, архитектура, ER).
Результат: PNG файлы 300 DPI, готовые для вставки в диплом.

Запуск: /home/sputniktech/.local/diagenv/bin/python generate_drawio.py
"""

import os
import subprocess
import textwrap
from pathlib import Path

# Use the venv matplotlib
import matplotlib
matplotlib.use('Agg')  # Non-interactive backend
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch
import numpy as np

OUTPUT_DIR = Path(__file__).parent
FONT = 'DejaVu Sans'
DPI = 150
GRAPHVIZ_DPI = 150  # Lower DPI = larger text relative to image

plt.rcParams.update({
    'font.family': FONT,
    'font.size': 16,
    'axes.titlesize': 22,
    'axes.labelsize': 18,
    'xtick.labelsize': 14,
    'ytick.labelsize': 14,
    'figure.dpi': DPI,
    'savefig.dpi': DPI,
    'savefig.bbox': 'tight',
    'savefig.pad_inches': 0.4,
})


def diagram_01_architecture():
    """Диаграмма 1: Общая архитектура системы."""
    fig, ax = plt.subplots(1, 1, figsize=(14, 10))
    ax.set_xlim(0, 14)
    ax.set_ylim(0, 10)
    ax.axis('off')
    ax.set_title('Архитектура системы', fontsize=28, fontweight='bold', pad=30)

    # Define boxes: (x, y, width, height, color, label, sublabel)
    boxes = [
        (5.0, 8.0, 4, 1.4, '#E3F2FD', 'Пользователь', 'Браузер (HTML/CSS/JS)'),
        (5.0, 5.2, 4, 1.8, '#FFF3E0', 'Веб-сервер', 'ASP.NET Razor Pages\n+ Рекомендательная система'),
        (0.8, 1.5, 3.5, 1.5, '#E8F5E9', 'SQL Server', 'Товары, заказы,\nвзаимодействия, эмбеддинги'),
        (5.5, 1.5, 3.0, 1.5, '#E8F5E9', 'Redis', 'Корзина, кэш\nсессий'),
        (9.8, 1.5, 3.5, 1.5, '#F3E5F5', 'Azure OpenAI', 'Генерация\nвекторов-эмбеддингов'),
    ]

    for (x, y, w, h, color, label, sublabel) in boxes:
        rect = FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.1",
                              facecolor=color, edgecolor='#333', linewidth=2)
        ax.add_patch(rect)
        ax.text(x + w/2, y + h*0.65, label, ha='center', va='center',
                fontsize=18, fontweight='bold')
        ax.text(x + w/2, y + h*0.25, sublabel, ha='center', va='center',
                fontsize=13, color='#555')

    # Arrows
    arrow_props = dict(arrowstyle='->', linewidth=2.5, color='#333')
    # User -> Server
    ax.annotate('', xy=(7, 7.0), xytext=(7, 8.0),
                arrowprops=arrow_props)
    ax.text(7.4, 7.5, 'HTTP', fontsize=14, color='#666')
    
    # Server -> SQL
    ax.annotate('', xy=(2.55, 3.0), xytext=(5.5, 5.2),
                arrowprops=arrow_props)
    
    # Server -> Redis
    ax.annotate('', xy=(7.0, 3.0), xytext=(7.0, 5.2),
                arrowprops=arrow_props)
    
    # Server -> Azure
    ax.annotate('', xy=(11.55, 3.0), xytext=(8.5, 5.2),
                arrowprops=arrow_props)
    ax.text(10.5, 4.3, 'API', fontsize=14, color='#666')

    plt.savefig(OUTPUT_DIR / '01_архитектура_системы.png')
    plt.close()
    print("  ✓ 01_архитектура_системы.png")


def diagram_02_er():
    """Диаграмма 2: ER-диаграмма таблиц рекомендательной системы."""
    dot_code = '''
    digraph ER {
        rankdir=TB;
        nodesep=1.0;
        ranksep=1.2;
        pad="0.8,0.6";
        node [shape=record, style=filled, fontname="DejaVu Sans", fontsize=18, margin="0.4,0.3"];
        edge [fontname="DejaVu Sans", fontsize=15, penwidth=1.5];
        
        graph [label="Схема базы данных\\n(таблицы рекомендательной системы)", 
               labelloc=t, fontsize=28, fontname="DejaVu Sans Bold"];
        
        Products [fillcolor="#E3F2FD" label="{Products (Товары)|Id : int (PK)\\lName : строка\\lPrice : число\\lEmbedding : вектор ИИ [1536]\\l}"];
        
        UserInteractions [fillcolor="#C8E6C9" label="{UserInteractions\\n(Взаимодействия)|Id : int (PK)\\lUserId : FK → Пользователь\\lProductId : FK → Товар\\lType : Просмотр/Клик/Корзина/Покупка\\lTimestamp : дата и время\\l}"];
        
        RecommendationEvents [fillcolor="#FFF9C4" label="{RecommendationEvents\\n(События рекомендаций)|Id : int (PK)\\lUserId : FK → Пользователь\\lProductId : FK → Товар\\lStrategy : Popular/CF/CB/Adaptive\\lPosition : 1-8\\lEventType : Показ/Клик/Покупка\\l}"];
        
        ABTestExperiments [fillcolor="#FFCCBC" label="{ABTestExperiments\\n(Эксперименты A/B)|Id : int (PK)\\lName : название теста\\lControlStrategy : контроль\\lTreatmentStrategy : эксперимент\\lTrafficPercent : 50%\\lIsActive : да/нет\\l}"];
        
        ABTestAssignments [fillcolor="#E1BEE7" label="{ABTestAssignments\\n(Назначения в группы)|Id : int (PK)\\lUserId : FK → Пользователь\\lExperimentId : FK → Эксперимент\\lIsTreatment : контроль или эксперимент\\l}"];
        
        Users [fillcolor="#F5F5F5" label="{AspNetUsers\\n(Пользователи)|Id : строка (PK)\\lEmail : электронная почта\\lFirstName : имя\\l}"];
        
        Users -> UserInteractions [label="1 : *\\nсоздаёт"];
        Users -> RecommendationEvents [label="1 : *\\nполучает"];
        Users -> ABTestAssignments [label="1 : *\\nназначен"];
        Products -> UserInteractions [label="1 : *\\nучаствует"];
        Products -> RecommendationEvents [label="1 : *\\nрекомендован"];
        ABTestExperiments -> ABTestAssignments [label="1 : *\\nсодержит"];
    }
    '''
    
    dot_path = OUTPUT_DIR / '_temp_er.dot'
    out_path = OUTPUT_DIR / '02_база_данных_ER.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 02_база_данных_ER.png")


def diagram_03_algorithm_flowchart():
    """Диаграмма 3: Блок-схема алгоритма генерации рекомендаций."""
    dot_code = '''
    digraph Algorithm {
        rankdir=TB;
        nodesep=1.0;
        ranksep=1.2;
        pad="1.0,0.8";
        node [shape=box, style="filled,rounded", fontname="DejaVu Sans", fontsize=22, margin="0.5,0.4"];
        edge [fontname="DejaVu Sans", fontsize=18, penwidth=2.0];
        
        graph [label="Алгоритм генерации рекомендаций", 
               labelloc=t, fontsize=30, fontname="DejaVu Sans Bold"];
        
        start [label="Пользователь\\nоткрывает страницу", fillcolor="#E3F2FD"];
        check [label="Есть ли история\\nвзаимодействий?", shape=diamond, fillcolor="#FFF9C4"];
        cold [label="Холодный старт:\\nпоказать популярные\\nтовары за 30 дней", fillcolor="#FFCCBC"];
        hybrid [label="Запустить гибридный\\nалгоритм", fillcolor="#C8E6C9"];
        
        cf [label="Коллаборативная\\nфильтрация\\n(вес 0.40)", fillcolor="#BBDEFB"];
        cb [label="Контентный\\nанализ ИИ\\n(вес 0.35)", fillcolor="#C8E6C9"];
        trend [label="Тренды\\n7 дней\\n(вес 0.15)", fillcolor="#FFF9C4"];
        cat [label="Категории\\nпользователя\\n(вес 0.10)", fillcolor="#FFCCBC"];
        
        sum [label="Суммировать баллы\\nс учётом весов", fillcolor="#E1BEE7"];
        filter [label="Убрать товары,\\nкоторые уже смотрел", fillcolor="#F5F5F5"];
        result [label="Выдать ТОП-8\\nрекомендаций", fillcolor="#A5D6A7", style="filled,rounded,bold"];
        
        start -> check;
        check -> cold [label="  Нет"];
        check -> hybrid [label="  Да"];
        hybrid -> cf;
        hybrid -> cb;
        hybrid -> trend;
        hybrid -> cat;
        cf -> sum;
        cb -> sum;
        trend -> sum;
        cat -> sum;
        sum -> filter;
        filter -> result;
        cold -> result;
    }
    '''
    
    dot_path = OUTPUT_DIR / '_temp_algo.dot'
    out_path = OUTPUT_DIR / '03_алгоритм_рекомендаций.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 03_алгоритм_рекомендаций.png")


def diagram_04_hybrid_formula():
    """Диаграмма 4: Визуализация формулы гибридного алгоритма."""
    fig, ax = plt.subplots(1, 1, figsize=(14, 8))
    ax.set_xlim(0, 14)
    ax.set_ylim(0, 8)
    ax.axis('off')
    ax.set_title('Формула гибридного адаптивного алгоритма', fontsize=26, fontweight='bold', pad=25)

    # Components on the left
    components = [
        (0.5, 6.2, '#BBDEFB', 'Коллаборативная\nфильтрация', '×0.40'),
        (0.5, 4.5, '#C8E6C9', 'Контентный\nанализ (ИИ)', '×0.35'),
        (0.5, 2.8, '#FFF9C4', 'Тренды\n(7 дней)', '×0.15'),
        (0.5, 1.1, '#FFCCBC', 'Категории\nпользователя', '×0.10'),
    ]

    for (x, y, color, label, weight) in components:
        rect = FancyBboxPatch((x, y), 3.5, 1.1, boxstyle="round,pad=0.1",
                              facecolor=color, edgecolor='#333', linewidth=1.5)
        ax.add_patch(rect)
        ax.text(x + 1.75, y + 0.55, label, ha='center', va='center', fontsize=16)
        
        # Weight box
        rect2 = FancyBboxPatch((4.6, y + 0.2), 1.1, 0.7, boxstyle="round,pad=0.06",
                               facecolor='white', edgecolor='#666', linewidth=1)
        ax.add_patch(rect2)
        ax.text(5.15, y + 0.55, weight, ha='center', va='center', fontsize=18, fontweight='bold')
        
        # Arrow to sum
        ax.annotate('', xy=(7.0, y + 0.55), xytext=(5.7, y + 0.55),
                    arrowprops=dict(arrowstyle='->', linewidth=1.5, color='#666'))

    # Sum circle
    circle = plt.Circle((7.8, 4.0), 0.7, facecolor='#E1BEE7', edgecolor='#333', linewidth=2)
    ax.add_patch(circle)
    ax.text(7.8, 4.0, 'Σ', ha='center', va='center', fontsize=34, fontweight='bold')

    # Arrow from sum to result
    ax.annotate('', xy=(9.5, 4.0), xytext=(8.5, 4.0),
                arrowprops=dict(arrowstyle='->', linewidth=2.5, color='#333'))

    # Result box
    rect = FancyBboxPatch((9.5, 3.0), 4.0, 2.0, boxstyle="round,pad=0.12",
                          facecolor='#A5D6A7', edgecolor='#333', linewidth=2)
    ax.add_patch(rect)
    ax.text(11.5, 4.3, 'Итоговый балл', ha='center', va='center', fontsize=18, fontweight='bold')
    ax.text(11.5, 3.6, '→ ТОП-8 товаров', ha='center', va='center', fontsize=16, color='#333')

    # Formula text at bottom
    ax.text(7.0, -0.1, 'Score = 0.40·CF + 0.35·CB + 0.15·Trending + 0.10·Recency',
            ha='center', va='center', fontsize=16, style='italic',
            bbox=dict(boxstyle='round', facecolor='#F5F5F5', edgecolor='#CCC'))

    plt.savefig(OUTPUT_DIR / '04_формула_гибрид.png')
    plt.close()
    print("  ✓ 04_формула_гибрид.png")


def diagram_05_ab_test():
    """Диаграмма 5: Процесс A/B тестирования."""
    dot_code = '''
    digraph ABTest {
        rankdir=TB;
        nodesep=1.2;
        ranksep=1.4;
        pad="1.0,0.8";
        node [shape=box, style="filled,rounded", fontname="DejaVu Sans", fontsize=22, margin="0.5,0.4"];
        edge [fontname="DejaVu Sans", fontsize=18, penwidth=2.0];
        
        graph [label="Процесс A/B тестирования", 
               labelloc=t, fontsize=30, fontname="DejaVu Sans Bold"];
        
        user [label="Новый пользователь\\nзаходит на сайт", fillcolor="#E3F2FD"];
        split [label="Случайное распределение\\n50% / 50%", shape=diamond, fillcolor="#FFF9C4"];
        
        control [label="Группа А (контроль)\\n\\nАлгоритм: Popular\\nПросто популярные товары", fillcolor="#FFCDD2"];
        treatment [label="Группа Б (эксперимент)\\n\\nАлгоритм: Adaptive\\nГибридная модель", fillcolor="#C8E6C9"];
        
        metrics [label="Записываем метрики:\\n• Показы рекомендаций\\n• Клики\\n• Добавления в корзину\\n• Покупки", fillcolor="#F5F5F5"];
        
        compare [label="Сравниваем CTR и конверсию\\nдвух групп", fillcolor="#E1BEE7"];
        
        result [label="Вывод: Adaptive эффективнее\\nCTR: 15% vs 8% (+87.5%)", fillcolor="#A5D6A7", style="filled,rounded,bold"];
        
        user -> split;
        split -> control [label="  50%"];
        split -> treatment [label="  50%"];
        control -> metrics;
        treatment -> metrics;
        metrics -> compare;
        compare -> result;
    }
    '''
    
    dot_path = OUTPUT_DIR / '_temp_ab.dot'
    out_path = OUTPUT_DIR / '05_AB_тестирование.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 05_AB_тестирование.png")


def diagram_06_ctr_results():
    """Диаграмма 6: Сравнение CTR по стратегиям."""
    fig, ax = plt.subplots(figsize=(10, 7))
    
    strategies = ['Popular\n(Контроль)', 'Adaptive\n(Эксперимент)']
    ctrs = [8.0, 15.0]
    colors = ['#EF9A9A', '#81C784']
    
    bars = ax.bar(strategies, ctrs, color=colors, width=0.5, edgecolor='#333', linewidth=1.5)
    
    # Add value labels on bars
    for bar, val in zip(bars, ctrs):
        ax.text(bar.get_x() + bar.get_width()/2, bar.get_height() + 0.3,
                f'{val}%', ha='center', va='bottom', fontsize=22, fontweight='bold')
    
    ax.set_ylabel('CTR (кликабельность), %', fontsize=18)
    ax.set_title('Сравнение CTR рекомендаций:\nКонтроль vs Эксперимент', fontsize=22, fontweight='bold')
    ax.set_ylim(0, 20)
    ax.yaxis.grid(True, alpha=0.3)
    ax.set_axisbelow(True)
    
    # Add improvement annotation
    ax.annotate('Улучшение: +87.5%', xy=(1, 15), xytext=(1.3, 17.5),
                fontsize=16, fontweight='bold', color='#2E7D32',
                arrowprops=dict(arrowstyle='->', color='#2E7D32', linewidth=1.5))
    
    plt.tight_layout()
    plt.savefig(OUTPUT_DIR / '06_результаты_CTR.png')
    plt.close()
    print("  ✓ 06_результаты_CTR.png")


def diagram_07_funnel():
    """Диаграмма 7: Воронка конверсии."""
    fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(14, 8))
    
    stages = ['Показы', 'Клики', 'В корзину', 'Покупки']
    
    # Control group
    control_values = [100, 8.0, 1.2, 0.3]
    control_colors = ['#FFCDD2', '#EF9A9A', '#E57373', '#D32F2F']
    
    ax1.barh(stages[::-1], control_values[::-1], color=control_colors[::-1], 
             edgecolor='#333', linewidth=1, height=0.6)
    ax1.set_title('Группа А — Popular\n(Контроль)', fontsize=18, fontweight='bold')
    ax1.set_xlabel('% от показов', fontsize=16)
    for i, (stage, val) in enumerate(zip(stages[::-1], control_values[::-1])):
        ax1.text(val + 1, i, f'{val}%', va='center', fontsize=16, fontweight='bold')
    ax1.set_xlim(0, 115)
    
    # Treatment group
    treatment_values = [100, 15.0, 3.75, 1.5]
    treatment_colors = ['#C8E6C9', '#81C784', '#4CAF50', '#2E7D32']
    
    ax2.barh(stages[::-1], treatment_values[::-1], color=treatment_colors[::-1],
             edgecolor='#333', linewidth=1, height=0.6)
    ax2.set_title('Группа Б — Adaptive\n(Эксперимент)', fontsize=18, fontweight='bold')
    ax2.set_xlabel('% от показов', fontsize=16)
    for i, (stage, val) in enumerate(zip(stages[::-1], treatment_values[::-1])):
        ax2.text(val + 1, i, f'{val}%', va='center', fontsize=16, fontweight='bold')
    ax2.set_xlim(0, 115)
    
    fig.suptitle('Воронка конверсии: Контроль vs Эксперимент', fontsize=22, fontweight='bold', y=0.98)
    plt.tight_layout()
    plt.savefig(OUTPUT_DIR / '07_воронка_конверсии.png')
    plt.close()
    print("  ✓ 07_воронка_конверсии.png")


def diagram_08_use_case():
    """Диаграмма 8: Диаграмма вариантов использования."""
    dot_code = '''
    digraph UseCase {
        rankdir=LR;
        nodesep=0.8;
        ranksep=1.5;
        pad="1.0,0.8";
        node [fontname="DejaVu Sans", fontsize=20, margin="0.4,0.3"];
        edge [fontname="DejaVu Sans", fontsize=16, penwidth=1.5];
        
        graph [label="Диаграмма вариантов использования (Use Case)", 
               labelloc=t, fontsize=28, fontname="DejaVu Sans Bold"];
        
        // Actors
        buyer [label="Покупатель", shape=box, style=filled, fillcolor="#E3F2FD", 
               width=2.2, height=0.8, fontsize=22];
        admin [label="Администратор", shape=box, style=filled, fillcolor="#FFF3E0",
               width=2.5, height=0.8, fontsize=22];
        
        // Use cases - buyer
        subgraph cluster_buyer {
            label="Функции покупателя";
            style=filled;
            fillcolor="#F5F5F5";
            fontsize=20;
            
            uc1 [label="Просмотр каталога\\nтоваров", shape=ellipse, style=filled, fillcolor="white"];
            uc2 [label="Получение персональных\\nрекомендаций", shape=ellipse, style=filled, fillcolor="white"];
            uc3 [label="Добавление\\nв корзину", shape=ellipse, style=filled, fillcolor="white"];
            uc4 [label="Оформление\\nзаказа", shape=ellipse, style=filled, fillcolor="white"];
            uc5 [label="Написание\\nотзыва", shape=ellipse, style=filled, fillcolor="white"];
        }
        
        // Use cases - admin
        subgraph cluster_admin {
            label="Функции администратора";
            style=filled;
            fillcolor="#FFF8E1";
            fontsize=20;
            
            uc6 [label="Управление\\nтоварами", shape=ellipse, style=filled, fillcolor="white"];
            uc7 [label="Запуск A/B\\nтестов", shape=ellipse, style=filled, fillcolor="white"];
            uc8 [label="Просмотр метрик\\nрекомендаций", shape=ellipse, style=filled, fillcolor="white"];
            uc9 [label="Управление\\nзаказами", shape=ellipse, style=filled, fillcolor="white"];
        }
        
        buyer -> uc1;
        buyer -> uc2;
        buyer -> uc3;
        buyer -> uc4;
        buyer -> uc5;
        admin -> uc6;
        admin -> uc7;
        admin -> uc8;
        admin -> uc9;
    }
    '''
    
    dot_path = OUTPUT_DIR / '_temp_uc.dot'
    out_path = OUTPUT_DIR / '08_варианты_использования.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 08_варианты_использования.png")


def main():
    print("=" * 55)
    print("  Генерация диаграмм для дипломной работы")
    print("  Язык: РУССКИЙ | Формат: PNG 200 DPI")
    print("=" * 55)
    print()
    
    diagram_01_architecture()
    diagram_02_er()
    diagram_03_algorithm_flowchart()
    diagram_04_hybrid_formula()
    diagram_05_ab_test()
    diagram_06_ctr_results()
    diagram_07_funnel()
    diagram_08_use_case()
    
    print()
    print("=" * 55)
    print(f"  Готово! 8 диаграмм сохранены в:")
    print(f"  {OUTPUT_DIR}")
    print("=" * 55)


if __name__ == "__main__":
    main()
