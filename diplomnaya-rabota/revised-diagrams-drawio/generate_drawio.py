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
    """Диаграмма 2: ER-диаграмма таблиц рекомендательной системы.
    Табличный стиль: тип | имя | PK/FK | описание на русском."""
    dot_code = '''
    digraph ER {
        rankdir=TB;
        nodesep=0.8;
        ranksep=1.5;
        pad="0.8,0.6";
        node [shape=none, fontname="DejaVu Sans", fontsize=16, margin="0"];
        edge [fontname="DejaVu Sans", fontsize=14, penwidth=2.0, color="#5c3d7a"];

        graph [label="Схема базы данных\\n(таблицы рекомендательной системы)",
               labelloc=t, fontsize=28, fontname="DejaVu Sans Bold"];

        Products [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD COLSPAN="4" BGCOLOR="#d5e8f5" ALIGN="CENTER"><B><FONT POINT-SIZE="18">Products</FONT></B></TD></TR>
                <TR><TD>int</TD>     <TD><B>Id</B></TD>        <TD><B>PK</B></TD> <TD></TD></TR>
                <TR><TD>string</TD>  <TD><B>Name</B></TD>      <TD></TD>           <TD>Название товара</TD></TR>
                <TR><TD>decimal</TD> <TD><B>Price</B></TD>     <TD></TD>           <TD>Цена</TD></TR>
                <TR><TD>string</TD>  <TD><B>Embedding</B></TD> <TD></TD>           <TD>Вектор ИИ (1536 чисел)</TD></TR>
            </TABLE>
        >];

        Users [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD COLSPAN="4" BGCOLOR="#F5F5F5" ALIGN="CENTER"><B><FONT POINT-SIZE="18">AspNetUsers</FONT></B></TD></TR>
                <TR><TD>string</TD> <TD><B>Id</B></TD>        <TD><B>PK</B></TD> <TD></TD></TR>
                <TR><TD>string</TD> <TD><B>Email</B></TD>     <TD></TD>           <TD>Электронная почта</TD></TR>
                <TR><TD>string</TD> <TD><B>FirstName</B></TD> <TD></TD>           <TD>Имя</TD></TR>
            </TABLE>
        >];

        ABTestExperiments [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD COLSPAN="4" BGCOLOR="#FFCCBC" ALIGN="CENTER"><B><FONT POINT-SIZE="18">ABTestExperiments</FONT></B></TD></TR>
                <TR><TD>int</TD>    <TD><B>Id</B></TD>              <TD><B>PK</B></TD> <TD></TD></TR>
                <TR><TD>string</TD> <TD><B>Name</B></TD>            <TD></TD>           <TD>Название теста</TD></TR>
                <TR><TD>string</TD> <TD><B>Control</B></TD>         <TD></TD>           <TD>Контрольная стратегия</TD></TR>
                <TR><TD>string</TD> <TD><B>Treatment</B></TD>       <TD></TD>           <TD>Экспериментальная</TD></TR>
                <TR><TD>int</TD>    <TD><B>TrafficPercent</B></TD>   <TD></TD>           <TD>50/50</TD></TR>
                <TR><TD>bool</TD>   <TD><B>IsActive</B></TD>        <TD></TD>           <TD>Активен</TD></TR>
            </TABLE>
        >];

        UserInteractions [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD COLSPAN="4" BGCOLOR="#C8E6C9" ALIGN="CENTER"><B><FONT POINT-SIZE="18">UserInteractions</FONT></B></TD></TR>
                <TR><TD>int</TD>      <TD><B>Id</B></TD>        <TD><B>PK</B></TD> <TD></TD></TR>
                <TR><TD>string</TD>   <TD><B>UserId</B></TD>    <TD><B>FK</B></TD> <TD>Кто</TD></TR>
                <TR><TD>int</TD>      <TD><B>ProductId</B></TD> <TD><B>FK</B></TD> <TD>Что</TD></TR>
                <TR><TD>int</TD>      <TD><B>Type</B></TD>      <TD></TD>           <TD>Тип действия</TD></TR>
                <TR><TD>datetime</TD> <TD><B>Timestamp</B></TD> <TD></TD>           <TD>Когда</TD></TR>
            </TABLE>
        >];

        RecommendationEvents [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD COLSPAN="4" BGCOLOR="#FFF9C4" ALIGN="CENTER"><B><FONT POINT-SIZE="18">RecommendationEvents</FONT></B></TD></TR>
                <TR><TD>int</TD>    <TD><B>Id</B></TD>        <TD><B>PK</B></TD> <TD></TD></TR>
                <TR><TD>string</TD> <TD><B>UserId</B></TD>    <TD><B>FK</B></TD> <TD>Кому показали</TD></TR>
                <TR><TD>int</TD>    <TD><B>ProductId</B></TD> <TD><B>FK</B></TD> <TD>Что рекомендовали</TD></TR>
                <TR><TD>string</TD> <TD><B>Strategy</B></TD>  <TD></TD>           <TD>Какой алгоритм</TD></TR>
                <TR><TD>int</TD>    <TD><B>Position</B></TD>  <TD></TD>           <TD>Позиция 1-8</TD></TR>
                <TR><TD>string</TD> <TD><B>EventType</B></TD> <TD></TD>           <TD>Показ или Клик</TD></TR>
            </TABLE>
        >];

        ABTestAssignments [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD COLSPAN="4" BGCOLOR="#E1BEE7" ALIGN="CENTER"><B><FONT POINT-SIZE="18">ABTestAssignments</FONT></B></TD></TR>
                <TR><TD>int</TD>    <TD><B>Id</B></TD>           <TD><B>PK</B></TD> <TD></TD></TR>
                <TR><TD>string</TD> <TD><B>UserId</B></TD>       <TD><B>FK</B></TD> <TD>Пользователь</TD></TR>
                <TR><TD>int</TD>    <TD><B>ExperimentId</B></TD> <TD><B>FK</B></TD> <TD>Эксперимент</TD></TR>
                <TR><TD>bool</TD>   <TD><B>IsTreatment</B></TD>  <TD></TD>           <TD>В какой группе</TD></TR>
            </TABLE>
        >];

        /* Layout: top row */
        { rank=same; Products; Users; ABTestExperiments; }
        /* Layout: bottom row */
        { rank=same; RecommendationEvents; UserInteractions; ABTestAssignments; }

        /* Relationships */
        Products -> UserInteractions [label="  1 : *\\nтовар", dir=both, arrowhead=crow, arrowtail=tee];
        Products -> RecommendationEvents [label="  1 : *\\nрекомендован", dir=both, arrowhead=crow, arrowtail=tee];
        Users -> UserInteractions [label="  1 : *\\nсоздаёт", dir=both, arrowhead=crow, arrowtail=tee];
        Users -> RecommendationEvents [label="  1 : *\\nполучает", dir=both, arrowhead=crow, arrowtail=tee];
        Users -> ABTestAssignments [label="  1 : *\\nназначен", dir=both, arrowhead=crow, arrowtail=tee];
        ABTestExperiments -> ABTestAssignments [label="  1 : *\\nсодержит", dir=both, arrowhead=crow, arrowtail=tee];
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
        rankdir=TB;
        nodesep=2.0;
        ranksep=2.0;
        pad="1.5,1.0";
        node [fontname="DejaVu Sans", fontsize=28, margin="0.6,0.5"];
        edge [fontname="DejaVu Sans", fontsize=20, penwidth=2.0];
        
        graph [label="Диаграмма вариантов использования", 
               labelloc=t, fontsize=40, fontname="DejaVu Sans Bold"];
        
        // Actors on top row
        buyer [label="Покупатель", shape=box, style="filled,bold", fillcolor="#E3F2FD", 
               width=4.0, height=1.4, fontsize=36];
        admin [label="Администратор", shape=box, style="filled,bold", fillcolor="#FFF3E0",
               width=4.5, height=1.4, fontsize=36];
        
        // Force actors on same rank (side by side)
        {rank=same; buyer; admin;}
        
        // Buyer use cases — 2 rows for readability
        subgraph cluster_buyer {
            label="Функции покупателя";
            style=filled;
            fillcolor="#F5F5F5";
            fontsize=30;
            labelloc=t;
            margin="30";
            
            uc1 [label="Просмотр\\nкаталога", shape=ellipse, style=filled, fillcolor="white", fontsize=34, width=3.5, height=1.8];
            uc2 [label="Получение\\nрекомендаций", shape=ellipse, style=filled, fillcolor="white", fontsize=34, width=4.0, height=1.8];
            uc3 [label="Добавление\\nв корзину", shape=ellipse, style=filled, fillcolor="white", fontsize=34, width=3.5, height=1.8];
            uc4 [label="Оформление\\nзаказа", shape=ellipse, style=filled, fillcolor="white", fontsize=34, width=3.5, height=1.8];
            uc5 [label="Написание\\nотзыва", shape=ellipse, style=filled, fillcolor="white", fontsize=34, width=3.5, height=1.8];
            
            // Row 1: 3 items
            {rank=same; uc1; uc2; uc3;}
            // Row 2: 2 items
            {rank=same; uc4; uc5;}
            
            // invisible edges to force rows
            uc1 -> uc4 [style=invis];
            uc3 -> uc5 [style=invis];
        }
        
        // Admin use cases — 2 rows
        subgraph cluster_admin {
            label="Функции администратора";
            style=filled;
            fillcolor="#FFF8E1";
            fontsize=30;
            labelloc=t;
            margin="30";
            
            uc6 [label="Управление\\nтоварами", shape=ellipse, style=filled, fillcolor="white", fontsize=34, width=3.5, height=1.8];
            uc7 [label="Просмотр\\nстатистики", shape=ellipse, style=filled, fillcolor="white", fontsize=34, width=3.5, height=1.8];
            uc8 [label="Управление\\nзаказами", shape=ellipse, style=filled, fillcolor="white", fontsize=34, width=4.0, height=1.8];
            
            // Row 1: 2 items
            {rank=same; uc6; uc7;}
            // Row 2: 1 item
            
            // invisible edge to force layout
            uc6 -> uc8 [style=invis];
        }
        
        buyer -> uc1;
        buyer -> uc2;
        buyer -> uc3;
        buyer -> uc4;
        buyer -> uc5;
        admin -> uc6;
        admin -> uc7;
        admin -> uc8;
    }
    '''
    
    dot_path = OUTPUT_DIR / '_temp_uc.dot'
    out_path = OUTPUT_DIR / '08_варианты_использования.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 08_варианты_использования.png")


def diagram_09a_class_entities():
    """Диаграмма 9а: UML классы — сущности рекомендательной системы."""
    dot_code = '''
    digraph ClassEntities {
        rankdir=TB;
        nodesep=1.2;
        ranksep=1.5;
        pad="1.0,0.8";
        node [shape=none, fontname="DejaVu Sans", fontsize=18, margin="0"];
        edge [fontname="DejaVu Sans", fontsize=16, penwidth=2.0, color="#5c3d7a"];

        graph [label="Диаграмма классов — Часть 1: Сущности\\n(рекомендательная система)",
               labelloc=t, fontsize=28, fontname="DejaVu Sans Bold"];

        BaseEntity [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E3F2FD" ALIGN="CENTER"><B><FONT POINT-SIZE="22">BaseEntity</FONT></B><BR/><FONT POINT-SIZE="14">(Базовая сущность)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ Id : int</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14"> </FONT></TD></TR>
            </TABLE>
        >];

        UserInteraction [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#C8E6C9" ALIGN="CENTER"><B><FONT POINT-SIZE="22">UserInteraction</FONT></B><BR/><FONT POINT-SIZE="14">(Взаимодействие пользователя)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ UserId : string<BR/>+ ProductId : int<BR/>+ Type : InteractionType<BR/>+ Timestamp : DateTime<BR/>+ SessionId : string?<BR/>+ DurationSeconds : int?</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14"> </FONT></TD></TR>
            </TABLE>
        >];

        RecommendationEvent [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFF9C4" ALIGN="CENTER"><B><FONT POINT-SIZE="22">RecommendationEvent</FONT></B><BR/><FONT POINT-SIZE="14">(Событие рекомендации)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ UserId : string<BR/>+ RecommendedProductId : int<BR/>+ SourceProductId : int?<BR/>+ EventType : RecommendationEventType<BR/>+ Strategy : RecommendationStrategy<BR/>+ Position : int<BR/>+ ExperimentId : int?<BR/>+ Timestamp : DateTime</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14"> </FONT></TD></TR>
            </TABLE>
        >];

        ABTestExperiment [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFCCBC" ALIGN="CENTER"><B><FONT POINT-SIZE="22">ABTestExperiment</FONT></B><BR/><FONT POINT-SIZE="14">(Эксперимент A/B)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ Name : string<BR/>+ Description : string?<BR/>+ ControlStrategy : RecommendationStrategy<BR/>+ TreatmentStrategy : RecommendationStrategy<BR/>+ TreatmentPercentage : int<BR/>+ StartDate : DateTime<BR/>+ EndDate : DateTime?<BR/>+ IsActive : bool</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14"> </FONT></TD></TR>
            </TABLE>
        >];

        ABTestAssignment [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E1BEE7" ALIGN="CENTER"><B><FONT POINT-SIZE="22">ABTestAssignment</FONT></B><BR/><FONT POINT-SIZE="14">(Назначение в группу)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ ExperimentId : int<BR/>+ UserId : string<BR/>+ IsTreatment : bool<BR/>+ AssignedAt : DateTime</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14"> </FONT></TD></TR>
            </TABLE>
        >];

        /* Наследование */
        BaseEntity -> UserInteraction [arrowhead=onormal, style=solid, label="  наследует", fontsize=16, color="#333333"];
        BaseEntity -> RecommendationEvent [arrowhead=onormal, style=solid, label="  наследует", fontsize=16, color="#333333"];
        BaseEntity -> ABTestExperiment [arrowhead=onormal, style=solid, label="  наследует", fontsize=16, color="#333333"];
        BaseEntity -> ABTestAssignment [arrowhead=onormal, style=solid, label="  наследует", fontsize=16, color="#333333"];

        /* Ассоциации */
        ABTestExperiment -> ABTestAssignment [arrowhead=open, label="  1 : *  содержит", fontsize=16, color="#5c3d7a"];
        ABTestExperiment -> RecommendationEvent [arrowhead=open, style=dashed, label="  0..1 : *  связан", fontsize=14, color="#999999"];

        /* Компоновка */
        { rank=same; UserInteraction; RecommendationEvent; }
        { rank=same; ABTestExperiment; ABTestAssignment; }
    }
    '''
    dot_path = OUTPUT_DIR / '_temp_class_a.dot'
    out_path = OUTPUT_DIR / '09а_классы_сущности.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 09а_классы_сущности.png")


def diagram_09b_class_enums():
    """Диаграмма 9б: UML классы — перечисления рекомендательной системы."""
    dot_code = '''
    digraph ClassEnums {
        rankdir=LR;
        nodesep=1.5;
        ranksep=2.0;
        pad="1.0,0.8";
        node [shape=none, fontname="DejaVu Sans", fontsize=18, margin="0"];

        graph [label="Диаграмма классов — Часть 2: Перечисления\\n(рекомендательная система)",
               labelloc=t, fontsize=28, fontname="DejaVu Sans Bold"];

        InteractionType [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="10" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFFDE7" ALIGN="CENTER"><FONT POINT-SIZE="14">«перечисление / enum»</FONT><BR/><B><FONT POINT-SIZE="22">InteractionType</FONT></B><BR/><FONT POINT-SIZE="14">(Тип взаимодействия)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="18">View = 0        — Просмотр<BR/>Click = 1       — Клик<BR/>AddToCart = 2    — В корзину<BR/>Purchase = 3    — Покупка<BR/>Wishlist = 4    — Избранное<BR/>Search = 5      — Поиск<BR/>RecommendationClick = 6 — Клик по рекомендации</FONT></TD></TR>
            </TABLE>
        >];

        RecommendationEventType [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="10" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFFDE7" ALIGN="CENTER"><FONT POINT-SIZE="14">«перечисление / enum»</FONT><BR/><B><FONT POINT-SIZE="22">RecommendationEventType</FONT></B><BR/><FONT POINT-SIZE="14">(Тип события рекомендации)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="18">Impression = 0 — Показ<BR/>Click = 1      — Клик<BR/>AddToCart = 2   — В корзину<BR/>Purchase = 3   — Покупка</FONT></TD></TR>
            </TABLE>
        >];

        RecommendationStrategy [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="10" COLOR="#9673a6" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFFDE7" ALIGN="CENTER"><FONT POINT-SIZE="14">«перечисление / enum»</FONT><BR/><B><FONT POINT-SIZE="22">RecommendationStrategy</FONT></B><BR/><FONT POINT-SIZE="14">(Стратегия рекомендаций)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="18">None = 0                  — Нет<BR/>Popular = 1               — Популярные<BR/>CollaborativeFiltering = 2 — Коллаборативная<BR/>ContentBased = 3          — Контентная (ИИ)<BR/>Adaptive = 4              — Адаптивная</FONT></TD></TR>
            </TABLE>
        >];
    }
    '''
    dot_path = OUTPUT_DIR / '_temp_class_b.dot'
    out_path = OUTPUT_DIR / '09б_классы_перечисления.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 09б_классы_перечисления.png")


def diagram_09v_class_interfaces():
    """Диаграмма 9в: UML классы — интерфейсы сервисов (часть 1: рекомендации + действия)."""
    dot_code = '''
    digraph ClassInterfaces1 {
        rankdir=TB;
        nodesep=1.5;
        ranksep=1.5;
        pad="1.0,0.8";
        node [shape=none, fontname="DejaVu Sans", fontsize=18, margin="0"];
        edge [fontname="DejaVu Sans", fontsize=16, penwidth=2.0];

        graph [label="Диаграмма классов — Часть 3: Интерфейсы\\n(рекомендации и действия пользователей)",
               labelloc=t, fontsize=28, fontname="DejaVu Sans Bold"];

        IAdaptiveRecommendationService [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="10" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#BBDEFB" ALIGN="CENTER"><FONT POINT-SIZE="15">«интерфейс»</FONT><BR/><B><FONT POINT-SIZE="22">IAdaptiveRecommendationService</FONT></B><BR/><FONT POINT-SIZE="15">(Адаптивные рекомендации)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="15"> </FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ GetAdaptiveRecommendationsAsync(userId, count)<BR/>    → Task&lt;List&lt;Product&gt;&gt;<BR/>+ GetPopularProductsAsync(count)<BR/>    → Task&lt;List&lt;Product&gt;&gt;<BR/>+ GetCollaborativeRecommendationsAsync(userId, count)<BR/>    → Task&lt;List&lt;Product&gt;&gt;<BR/>+ GetContentBasedRecommendationsAsync(productId, count)<BR/>    → Task&lt;List&lt;Product&gt;&gt;</FONT></TD></TR>
            </TABLE>
        >];

        IUserInteractionService [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="10" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#BBDEFB" ALIGN="CENTER"><FONT POINT-SIZE="15">«интерфейс»</FONT><BR/><B><FONT POINT-SIZE="22">IUserInteractionService</FONT></B><BR/><FONT POINT-SIZE="15">(Отслеживание действий)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="15"> </FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ TrackInteractionAsync(userId, productId, type, ...)<BR/>    → Task<BR/>+ GetUserInteractionsAsync(userId, limit)<BR/>    → Task&lt;List&lt;UserInteraction&gt;&gt;<BR/>+ GetUserTopProductsAsync(userId, count)<BR/>    → Task&lt;List&lt;int&gt;&gt;</FONT></TD></TR>
            </TABLE>
        >];

        IProductEmbeddingService [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="10" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#BBDEFB" ALIGN="CENTER"><FONT POINT-SIZE="15">«интерфейс»</FONT><BR/><B><FONT POINT-SIZE="22">IProductEmbeddingService</FONT></B><BR/><FONT POINT-SIZE="15">(ИИ-эмбеддинги товаров)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="15"> </FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ GenerateMissingEmbeddingsAsync() → Task<BR/>+ GetProductEmbeddingAsync(productId)<BR/>    → Task&lt;float[]?&gt;<BR/>+ RegenerateProductEmbeddingAsync(productId)<BR/>    → Task</FONT></TD></TR>
            </TABLE>
        >];

        /* Мини-боксы сущностей */
        UserInteraction_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#C8E6C9">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="18">UserInteraction</FONT></B></TD></TR>
            </TABLE>
        >];
        Product_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#E3F2FD">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="18">Product</FONT></B></TD></TR>
            </TABLE>
        >];

        IUserInteractionService -> UserInteraction_ref [arrowhead=open, style=dashed, label="  создаёт", color="#5c3d7a"];
        IAdaptiveRecommendationService -> UserInteraction_ref [arrowhead=open, style=dashed, label="  читает", color="#5c3d7a"];
        IAdaptiveRecommendationService -> Product_ref [arrowhead=open, style=dashed, label="  возвращает", color="#5c3d7a"];
        IProductEmbeddingService -> Product_ref [arrowhead=open, style=dashed, label="  обогащает", color="#5c3d7a"];

        { rank=same; IAdaptiveRecommendationService; IUserInteractionService; }
        { rank=same; UserInteraction_ref; Product_ref; }
    }
    '''
    dot_path = OUTPUT_DIR / '_temp_class_v.dot'
    out_path = OUTPUT_DIR / '09в_классы_интерфейсы.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 09в_классы_интерфейсы.png")


def diagram_09g_class_interfaces2():
    """Диаграмма 9г: UML классы — интерфейсы сервисов (часть 2: A/B тесты + метрики)."""
    dot_code = '''
    digraph ClassInterfaces2 {
        rankdir=TB;
        nodesep=1.5;
        ranksep=1.5;
        pad="1.0,0.8";
        node [shape=none, fontname="DejaVu Sans", fontsize=18, margin="0"];
        edge [fontname="DejaVu Sans", fontsize=16, penwidth=2.0];

        graph [label="Диаграмма классов — Часть 4: Интерфейсы\\n(A/B тестирование и метрики)",
               labelloc=t, fontsize=28, fontname="DejaVu Sans Bold"];

        IABTestService [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="10" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#BBDEFB" ALIGN="CENTER"><FONT POINT-SIZE="15">«интерфейс»</FONT><BR/><B><FONT POINT-SIZE="22">IABTestService</FONT></B><BR/><FONT POINT-SIZE="15">(Управление A/B тестами)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="15"> </FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ GetActiveExperimentAsync()<BR/>    → Task&lt;ABTestExperiment?&gt;<BR/>+ GetOrAssignUserAsync(userId, experimentId)<BR/>    → Task&lt;ABTestAssignment&gt;<BR/>+ GetUserStrategyAsync(userId)<BR/>    → Task&lt;RecommendationStrategy&gt;<BR/>+ CreateExperimentAsync(...)<BR/>    → Task&lt;ABTestExperiment&gt;<BR/>+ EndExperimentAsync(experimentId) → Task</FONT></TD></TR>
            </TABLE>
        >];

        IRecommendationMetricsService [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="10" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#BBDEFB" ALIGN="CENTER"><FONT POINT-SIZE="15">«интерфейс»</FONT><BR/><B><FONT POINT-SIZE="22">IRecommendationMetricsService</FONT></B><BR/><FONT POINT-SIZE="15">(Метрики рекомендаций)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="15"> </FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="17">+ RecordImpressionAsync(...) → Task<BR/>+ RecordClickAsync(...) → Task<BR/>+ RecordPurchaseAsync(...) → Task<BR/>+ GetExperimentMetricsAsync(experimentId)<BR/>    → Task&lt;ExperimentMetrics&gt;<BR/>+ GetSystemMetricsAsync(from, to)<BR/>    → Task&lt;RecommendationSystemMetrics&gt;</FONT></TD></TR>
            </TABLE>
        >];

        /* Мини-боксы сущностей */
        ABTestExperiment_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#FFCCBC">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="18">ABTestExperiment</FONT></B></TD></TR>
            </TABLE>
        >];
        ABTestAssignment_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#E1BEE7">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="18">ABTestAssignment</FONT></B></TD></TR>
            </TABLE>
        >];
        RecommendationEvent_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="8" COLOR="#9673a6" BGCOLOR="#FFF9C4">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="18">RecommendationEvent</FONT></B></TD></TR>
            </TABLE>
        >];

        IABTestService -> ABTestExperiment_ref [arrowhead=open, style=dashed, label="  управляет", color="#5c3d7a"];
        IABTestService -> ABTestAssignment_ref [arrowhead=open, style=dashed, label="  назначает", color="#5c3d7a"];
        IRecommendationMetricsService -> RecommendationEvent_ref [arrowhead=open, style=dashed, label="  записывает", color="#5c3d7a"];

        { rank=same; IABTestService; IRecommendationMetricsService; }
        { rank=same; ABTestExperiment_ref; ABTestAssignment_ref; RecommendationEvent_ref; }
    }
    '''
    dot_path = OUTPUT_DIR / '_temp_class_g.dot'
    out_path = OUTPUT_DIR / '09г_классы_AB_метрики.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 09г_классы_AB_метрики.png")


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
    diagram_09a_class_entities()
    diagram_09b_class_enums()
    diagram_09v_class_interfaces()
    diagram_09g_class_interfaces2()
    
    print()
    print("=" * 55)
    print(f"  Готово! 12 диаграмм сохранены в:")
    print(f"  {OUTPUT_DIR}")
    print("=" * 55)


if __name__ == "__main__":
    main()
