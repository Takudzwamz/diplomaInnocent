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
               labelloc=t, fontsize=30, fontname="DejaVu Sans Bold",
               ordering=out];
        
        start [label="Пользователь\\nоткрывает страницу", fillcolor="#E3F2FD"];
        check [label="Есть ли история\\nвзаимодействий?", shape=diamond, fillcolor="#FFF9C4", ordering=out];
        hybrid [label="Запустить гибридный\\nалгоритм", fillcolor="#C8E6C9"];
        cold [label="Холодный старт:\\nпоказать популярные\\nтовары за 30 дней", fillcolor="#FFCCBC"];
        
        cf [label="Коллаборативная\\nфильтрация\\n(вес 0.40)", fillcolor="#BBDEFB"];
        cb [label="Контентный\\nанализ ИИ\\n(вес 0.35)", fillcolor="#C8E6C9"];
        trend [label="Тренды\\n7 дней\\n(вес 0.15)", fillcolor="#FFF9C4"];
        cat [label="Категории\\nпользователя\\n(вес 0.10)", fillcolor="#FFCCBC"];
        
        sum [label="Суммировать баллы\\nс учётом весов", fillcolor="#E1BEE7"];
        filter [label="Убрать товары,\\nкоторые уже смотрел", fillcolor="#F5F5F5"];
        result [label="Выдать ТОП-8\\nрекомендаций", fillcolor="#A5D6A7", style="filled,rounded,bold"];
        
        /* Layout: Да (main flow) on LEFT, Нет (cold start) on RIGHT */
        { rank=same; hybrid; cold; }
        { rank=same; cf; cb; trend; cat; }
        
        start -> check;
        /* Да edge FIRST = placed LEFT */
        check -> hybrid [label="  Да"];
        check -> cold [label="  Нет"];
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


def diagram_09a_class_product_domain():
    """Диаграмма 9а: UML классы — Домен товаров (Product, Type, Brand, Category, Image, Review, Option, Variant)."""
    dot_code = '''
    digraph ClassProductDomain {
        rankdir=TB;
        nodesep=0.8;
        ranksep=1.2;
        pad="0.8,0.6";
        node [shape=none, fontname="DejaVu Sans", fontsize=14, margin="0"];
        edge [fontname="DejaVu Sans", fontsize=13, penwidth=1.8];

        graph [label="Диаграмма классов — Часть 1: Домен товаров",
               labelloc=t, fontsize=24, fontname="DejaVu Sans Bold"];

        Product [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#BBDEFB" ALIGN="CENTER"><B><FONT POINT-SIZE="18">Product</FONT></B><BR/><FONT POINT-SIZE="12">(Товар)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Name : string<BR/>+ Description : string<BR/>+ Price : decimal<BR/>+ QuantityInStock : int<BR/>+ ProductKind : ProductKind<BR/>+ Embedding : string?<BR/>+ ProductTypeId : int  [FK]<BR/>+ ProductBrandId : int  [FK]<BR/>+ CategoryId : int  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        ProductType [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E8F5E9" ALIGN="CENTER"><B><FONT POINT-SIZE="16">ProductType</FONT></B><BR/><FONT POINT-SIZE="12">(Тип товара)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Name : string</FONT></TD></TR>
            </TABLE>
        >];

        ProductBrand [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E8F5E9" ALIGN="CENTER"><B><FONT POINT-SIZE="16">ProductBrand</FONT></B><BR/><FONT POINT-SIZE="12">(Бренд)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Name : string</FONT></TD></TR>
            </TABLE>
        >];

        Category [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E8F5E9" ALIGN="CENTER"><B><FONT POINT-SIZE="16">Category</FONT></B><BR/><FONT POINT-SIZE="12">(Категория)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Name : string<BR/>+ Description : string</FONT></TD></TR>
            </TABLE>
        >];

        ProductImage [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFF9C4" ALIGN="CENTER"><B><FONT POINT-SIZE="16">ProductImage</FONT></B><BR/><FONT POINT-SIZE="12">(Изображение)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Url : string<BR/>+ IsMain : bool<BR/>+ ProductId : int  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        ProductReview [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFF9C4" ALIGN="CENTER"><B><FONT POINT-SIZE="16">ProductReview</FONT></B><BR/><FONT POINT-SIZE="12">(Отзыв)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Rating : int<BR/>+ Comment : string?<BR/>+ ReviewerName : string<BR/>+ ReviewDate : DateTime<BR/>+ ProductId : int  [FK]<BR/>+ AppUserId : string  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        ProductOption [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E1BEE7" ALIGN="CENTER"><B><FONT POINT-SIZE="16">ProductOption</FONT></B><BR/><FONT POINT-SIZE="12">(Опция: Цвет, Размер)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Name : string</FONT></TD></TR>
            </TABLE>
        >];

        ProductOptionValue [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E1BEE7" ALIGN="CENTER"><B><FONT POINT-SIZE="16">ProductOptionValue</FONT></B><BR/><FONT POINT-SIZE="12">(Значение опции)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Name : string<BR/>+ ColorHex : string?<BR/>+ ProductOptionId : int  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        ProductVariant [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFCCBC" ALIGN="CENTER"><B><FONT POINT-SIZE="16">ProductVariant</FONT></B><BR/><FONT POINT-SIZE="12">(Вариант товара)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Price : decimal<BR/>+ QuantityInStock : int<BR/>+ Sku : string?<BR/>+ ProductId : int  [FK]<BR/>+ ImageId : int?  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        ProductKindEnum [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#666666" BGCOLOR="#FFFDE7">
                <TR><TD ALIGN="CENTER"><FONT POINT-SIZE="11">«enum»</FONT><BR/><B><FONT POINT-SIZE="14">ProductKind</FONT></B></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="12">Simple, Variable</FONT></TD></TR>
            </TABLE>
        >];

        /* Связи: справочники → Product */
        ProductType -> Product [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#2E7D32"];
        ProductBrand -> Product [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#2E7D32"];
        Category -> Product [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#2E7D32"];

        /* Product → дочерние */
        Product -> ProductImage [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\nCascade", color="#1565C0"];
        Product -> ProductReview [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#1565C0"];
        Product -> ProductVariant [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\nRestrict", color="#1565C0"];

        /* M:M Product ↔ ProductOption */
        Product -> ProductOption [arrowhead=crow, arrowtail=crow, dir=both, label="M : M", style=bold, color="#7B1FA2"];

        /* ProductOption → ProductOptionValue */
        ProductOption -> ProductOptionValue [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#1565C0"];

        /* M:M ProductVariant ↔ ProductOptionValue */
        ProductVariant -> ProductOptionValue [arrowhead=crow, arrowtail=crow, dir=both, label="M : M", style=bold, color="#7B1FA2"];

        /* ProductVariant → ProductImage (optional) */
        ProductVariant -> ProductImage [arrowhead=open, style=dashed, label="0..1\\nSetNull", color="#999999"];

        /* Enum */
        Product -> ProductKindEnum [arrowhead=open, style=dotted, color="#999999"];

        /* Layout */
        { rank=same; ProductType; ProductBrand; Category; }
        { rank=same; ProductImage; ProductReview; }
        { rank=same; ProductOption; ProductOptionValue; ProductVariant; }
    }
    '''
    dot_path = OUTPUT_DIR / '_temp_class_09a.dot'
    out_path = OUTPUT_DIR / '09а_классы_товары.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 09а_классы_товары.png")


def diagram_09b_class_order_domain():
    """Диаграмма 9б: UML классы — Домен заказов (Order, OrderItem, DeliveryMethod, TrackingEvent, owned types)."""
    dot_code = '''
    digraph ClassOrderDomain {
        rankdir=TB;
        nodesep=1.0;
        ranksep=1.2;
        pad="0.8,0.6";
        node [shape=none, fontname="DejaVu Sans", fontsize=14, margin="0"];
        edge [fontname="DejaVu Sans", fontsize=13, penwidth=1.8];

        graph [label="Диаграмма классов — Часть 2: Домен заказов",
               labelloc=t, fontsize=24, fontname="DejaVu Sans Bold"];

        Order [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#BBDEFB" ALIGN="CENTER"><B><FONT POINT-SIZE="18">Order</FONT></B><BR/><FONT POINT-SIZE="12">(Заказ)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ OrderDate : DateTime<BR/>+ BuyerEmail : string<BR/>+ Subtotal : decimal<BR/>+ Discount : decimal<BR/>+ CouponCode : string?<BR/>+ Status : OrderStatus<BR/>+ DeliveryStatus : DeliveryStatus<BR/>+ PaymentReference : string<BR/>+ GatewayTransactionId : string?<BR/>+ PaymentGatewayName : string?<BR/>+ DeliveryMethodId : int  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        OrderItem [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#C8E6C9" ALIGN="CENTER"><B><FONT POINT-SIZE="16">OrderItem</FONT></B><BR/><FONT POINT-SIZE="12">(Позиция заказа)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Price : decimal<BR/>+ Quantity : int<BR/>+ OrderId : int  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        DeliveryMethod [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E8F5E9" ALIGN="CENTER"><B><FONT POINT-SIZE="16">DeliveryMethod</FONT></B><BR/><FONT POINT-SIZE="12">(Способ доставки)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ ShortName : string<BR/>+ DeliveryTime : string<BR/>+ Description : string<BR/>+ Price : decimal</FONT></TD></TR>
            </TABLE>
        >];

        TrackingEvent [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFF9C4" ALIGN="CENTER"><B><FONT POINT-SIZE="16">TrackingEvent</FONT></B><BR/><FONT POINT-SIZE="12">(Событие отслеживания)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ EventDate : DateTime<BR/>+ Status : string<BR/>+ Notes : string?<BR/>+ OrderId : int  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        ShippingAddress [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#999999" BGCOLOR="#F5F5F5">
                <TR><TD ALIGN="CENTER"><FONT POINT-SIZE="11">«owned type»</FONT><BR/><B><FONT POINT-SIZE="15">ShippingAddress</FONT></B><BR/><FONT POINT-SIZE="11">(Адрес доставки)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="13">Name, LastName, Line1,<BR/>Line2?, City, State,<BR/>PostalCode, Country,<BR/>PhoneNumber?, DeliveryNotes?</FONT></TD></TR>
            </TABLE>
        >];

        PaymentSummary [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#999999" BGCOLOR="#F5F5F5">
                <TR><TD ALIGN="CENTER"><FONT POINT-SIZE="11">«owned type»</FONT><BR/><B><FONT POINT-SIZE="15">PaymentSummary</FONT></B><BR/><FONT POINT-SIZE="11">(Данные оплаты)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="13">Last4, Brand,<BR/>ExpMonth, ExpYear</FONT></TD></TR>
            </TABLE>
        >];

        ProductItemOrdered [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#999999" BGCOLOR="#F5F5F5">
                <TR><TD ALIGN="CENTER"><FONT POINT-SIZE="11">«owned type»</FONT><BR/><B><FONT POINT-SIZE="15">ProductItemOrdered</FONT></B><BR/><FONT POINT-SIZE="11">(Снимок товара)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="13">ProductId, ProductVariantId?,<BR/>ProductName, PictureUrl,<BR/>SelectedOptions?</FONT></TD></TR>
            </TABLE>
        >];

        OrderStatusEnum [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#666666" BGCOLOR="#FFFDE7">
                <TR><TD ALIGN="CENTER"><FONT POINT-SIZE="11">«enum»</FONT><BR/><B><FONT POINT-SIZE="14">OrderStatus</FONT></B></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="12">Pending, PaymentReceived,<BR/>PaymentFailed, PaymentMismatch,<BR/>Refunded</FONT></TD></TR>
            </TABLE>
        >];

        DeliveryStatusEnum [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#666666" BGCOLOR="#FFFDE7">
                <TR><TD ALIGN="CENTER"><FONT POINT-SIZE="11">«enum»</FONT><BR/><B><FONT POINT-SIZE="14">DeliveryStatus</FONT></B></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="12">AwaitingProcessing, Processing,<BR/>Shipped, OutForDelivery,<BR/>Delivered</FONT></TD></TR>
            </TABLE>
        >];

        /* Связи */
        DeliveryMethod -> Order [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#2E7D32"];
        Order -> OrderItem [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\nCascade", color="#1565C0"];
        Order -> TrackingEvent [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#1565C0"];

        /* Owned types */
        Order -> ShippingAddress [arrowhead=diamond, dir=back, label="  owns", color="#666666"];
        Order -> PaymentSummary [arrowhead=diamond, dir=back, label="  owns", color="#666666"];
        OrderItem -> ProductItemOrdered [arrowhead=diamond, dir=back, label="  owns", color="#666666"];

        /* Enums */
        Order -> OrderStatusEnum [arrowhead=open, style=dotted, color="#999999"];
        Order -> DeliveryStatusEnum [arrowhead=open, style=dotted, color="#999999"];

        /* Layout */
        { rank=same; DeliveryMethod; Order; }
        { rank=same; OrderItem; TrackingEvent; }
        { rank=same; ShippingAddress; PaymentSummary; }
        { rank=same; OrderStatusEnum; DeliveryStatusEnum; }
    }
    '''
    dot_path = OUTPUT_DIR / '_temp_class_09b.dot'
    out_path = OUTPUT_DIR / '09б_классы_заказы.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 09б_классы_заказы.png")


def diagram_09v_class_user_domain():
    """Диаграмма 9в: UML классы — Пользователь, адрес, избранное + связи с товарами и заказами."""
    dot_code = '''
    digraph ClassUserDomain {
        rankdir=TB;
        nodesep=1.0;
        ranksep=1.2;
        pad="0.8,0.6";
        node [shape=none, fontname="DejaVu Sans", fontsize=14, margin="0"];
        edge [fontname="DejaVu Sans", fontsize=13, penwidth=1.8];

        graph [label="Диаграмма классов — Часть 3: Пользователь и избранное",
               labelloc=t, fontsize=24, fontname="DejaVu Sans Bold"];

        AppUser [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#BBDEFB" ALIGN="CENTER"><FONT POINT-SIZE="11">наследует IdentityUser</FONT><BR/><B><FONT POINT-SIZE="18">AppUser</FONT></B><BR/><FONT POINT-SIZE="12">(Пользователь)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : string  [PK]<BR/>+ Email : string<BR/>+ FirstName : string?<BR/>+ LastName : string?<BR/>+ DateRegistered : DateTime</FONT></TD></TR>
            </TABLE>
        >];

        Address [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E8F5E9" ALIGN="CENTER"><B><FONT POINT-SIZE="16">Address</FONT></B><BR/><FONT POINT-SIZE="12">(Адрес)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Line1 : string<BR/>+ Line2 : string?<BR/>+ City : string<BR/>+ State : string<BR/>+ PostalCode : string<BR/>+ Country : string<BR/>+ PhoneNumber : string?</FONT></TD></TR>
            </TABLE>
        >];

        Wishlist [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFF9C4" ALIGN="CENTER"><B><FONT POINT-SIZE="16">Wishlist</FONT></B><BR/><FONT POINT-SIZE="12">(Избранное)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ AppUserId : string  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        WishlistItem [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFF9C4" ALIGN="CENTER"><B><FONT POINT-SIZE="16">WishlistItem</FONT></B><BR/><FONT POINT-SIZE="12">(Элемент избранного)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ ProductId : int  [FK]<BR/>+ WishlistId : int  [FK]</FONT></TD></TR>
            </TABLE>
        >];

        /* Мини-ссылки на другие домены */
        Product_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#E3F2FD">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="14">Product</FONT></B><BR/><FONT POINT-SIZE="10">(см. часть 1)</FONT></TD></TR>
            </TABLE>
        >];
        ProductReview_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#E3F2FD">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="14">ProductReview</FONT></B><BR/><FONT POINT-SIZE="10">(см. часть 1)</FONT></TD></TR>
            </TABLE>
        >];
        Order_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#C8E6C9">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="14">Order</FONT></B><BR/><FONT POINT-SIZE="10">(см. часть 2)</FONT></TD></TR>
            </TABLE>
        >];

        /* Связи */
        AppUser -> Address [arrowhead=open, label="1 : 0..1", color="#1565C0"];
        AppUser -> Wishlist [arrowhead=open, label="1 : 0..1", color="#1565C0"];
        Wishlist -> WishlistItem [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#1565C0"];
        WishlistItem -> Product_ref [arrowhead=open, style=dashed, label="  → Product  [FK]", color="#5c3d7a"];

        AppUser -> ProductReview_ref [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#2E7D32"];
        AppUser -> Order_ref [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\n(по Email)", color="#2E7D32"];

        /* Layout */
        { rank=same; AppUser; Address; }
        { rank=same; Wishlist; WishlistItem; }
        { rank=same; Product_ref; ProductReview_ref; Order_ref; }
    }
    '''
    dot_path = OUTPUT_DIR / '_temp_class_09v.dot'
    out_path = OUTPUT_DIR / '09в_классы_пользователь.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 09в_классы_пользователь.png")


def diagram_09g_class_recommendation_domain():
    """Диаграмма 9г: UML классы — Рекомендательная система и A/B тестирование со связями."""
    dot_code = '''
    digraph ClassRecommendationDomain {
        rankdir=TB;
        nodesep=0.8;
        ranksep=1.2;
        pad="0.8,0.6";
        node [shape=none, fontname="DejaVu Sans", fontsize=14, margin="0"];
        edge [fontname="DejaVu Sans", fontsize=13, penwidth=1.8];

        graph [label="Диаграмма классов — Часть 4: Рекомендации и A/B тестирование",
               labelloc=t, fontsize=24, fontname="DejaVu Sans Bold"];

        UserInteraction [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#C8E6C9" ALIGN="CENTER"><B><FONT POINT-SIZE="18">UserInteraction</FONT></B><BR/><FONT POINT-SIZE="12">(Действие пользователя)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ UserId : string  [FK → AppUser]<BR/>+ ProductId : int  [FK → Product]<BR/>+ Type : InteractionType<BR/>+ Timestamp : DateTime<BR/>+ SessionId : string?<BR/>+ DurationSeconds : int?</FONT></TD></TR>
            </TABLE>
        >];

        RecommendationEvent [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFF9C4" ALIGN="CENTER"><B><FONT POINT-SIZE="18">RecommendationEvent</FONT></B><BR/><FONT POINT-SIZE="12">(Событие рекомендации)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ UserId : string  [FK → AppUser]<BR/>+ RecommendedProductId : int  [FK → Product]<BR/>+ SourceProductId : int?<BR/>+ EventType : RecommendationEventType<BR/>+ Strategy : RecommendationStrategy<BR/>+ Position : int<BR/>+ ExperimentId : int?  [FK]<BR/>+ Timestamp : DateTime</FONT></TD></TR>
            </TABLE>
        >];

        ABTestExperiment [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFCCBC" ALIGN="CENTER"><B><FONT POINT-SIZE="18">ABTestExperiment</FONT></B><BR/><FONT POINT-SIZE="12">(Эксперимент A/B)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Name : string<BR/>+ Description : string?<BR/>+ ControlStrategy : RecommendationStrategy<BR/>+ TreatmentStrategy : RecommendationStrategy<BR/>+ TreatmentPercentage : int<BR/>+ StartDate : DateTime<BR/>+ EndDate : DateTime?<BR/>+ IsActive : bool</FONT></TD></TR>
            </TABLE>
        >];

        ABTestAssignment [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E1BEE7" ALIGN="CENTER"><B><FONT POINT-SIZE="18">ABTestAssignment</FONT></B><BR/><FONT POINT-SIZE="12">(Назначение в группу)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ ExperimentId : int  [FK]<BR/>+ UserId : string  [FK → AppUser]<BR/>+ IsTreatment : bool<BR/>+ AssignedAt : DateTime</FONT></TD></TR>
            </TABLE>
        >];

        /* Enums */
        InteractionTypeEnum [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#666666" BGCOLOR="#FFFDE7">
                <TR><TD ALIGN="CENTER"><FONT POINT-SIZE="11">«enum»</FONT><BR/><B><FONT POINT-SIZE="14">InteractionType</FONT></B></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="12">View, Click, AddToCart,<BR/>Purchase, Wishlist, Search,<BR/>RecommendationClick</FONT></TD></TR>
            </TABLE>
        >];

        RecEventTypeEnum [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#666666" BGCOLOR="#FFFDE7">
                <TR><TD ALIGN="CENTER"><FONT POINT-SIZE="11">«enum»</FONT><BR/><B><FONT POINT-SIZE="14">RecommendationEventType</FONT></B></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="12">Impression, Click,<BR/>AddToCart, Purchase</FONT></TD></TR>
            </TABLE>
        >];

        RecStrategyEnum [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#666666" BGCOLOR="#FFFDE7">
                <TR><TD ALIGN="CENTER"><FONT POINT-SIZE="11">«enum»</FONT><BR/><B><FONT POINT-SIZE="14">RecommendationStrategy</FONT></B></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="12">None, Popular,<BR/>CollaborativeFiltering,<BR/>ContentBased, Adaptive</FONT></TD></TR>
            </TABLE>
        >];

        /* Мини-ссылки */
        AppUser_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#BBDEFB">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="14">AppUser</FONT></B><BR/><FONT POINT-SIZE="10">(см. часть 3)</FONT></TD></TR>
            </TABLE>
        >];
        Product_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#E3F2FD">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="14">Product</FONT></B><BR/><FONT POINT-SIZE="10">(см. часть 1)</FONT></TD></TR>
            </TABLE>
        >];

        /* Связи между таблицами */
        AppUser_ref -> UserInteraction [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\nCascade", color="#1565C0"];
        AppUser_ref -> RecommendationEvent [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\nCascade", color="#1565C0"];
        AppUser_ref -> ABTestAssignment [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\nCascade", color="#1565C0"];

        Product_ref -> UserInteraction [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\nCascade", color="#2E7D32"];
        Product_ref -> RecommendationEvent [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\nRestrict", color="#2E7D32"];

        ABTestExperiment -> ABTestAssignment [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *\\nCascade", color="#D32F2F"];
        ABTestExperiment -> RecommendationEvent [arrowhead=crow, arrowtail=tee, dir=both, label="0..1 : *\\nSetNull", style=dashed, color="#999999"];

        /* Enum connections */
        UserInteraction -> InteractionTypeEnum [arrowhead=open, style=dotted, color="#999999"];
        RecommendationEvent -> RecEventTypeEnum [arrowhead=open, style=dotted, color="#999999"];
        RecommendationEvent -> RecStrategyEnum [arrowhead=open, style=dotted, color="#999999"];
        ABTestExperiment -> RecStrategyEnum [arrowhead=open, style=dotted, color="#999999"];

        /* Layout */
        { rank=same; AppUser_ref; Product_ref; }
        { rank=same; UserInteraction; RecommendationEvent; }
        { rank=same; ABTestExperiment; ABTestAssignment; }
        { rank=same; InteractionTypeEnum; RecEventTypeEnum; RecStrategyEnum; }
    }
    '''
    dot_path = OUTPUT_DIR / '_temp_class_09g.dot'
    out_path = OUTPUT_DIR / '09г_классы_рекомендации.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 09г_классы_рекомендации.png")


def diagram_09d_class_coupon_cms():
    """Диаграмма 9д: UML классы — Купоны и CMS (Coupon, CouponProduct, CouponUsage, CMS entities)."""
    dot_code = '''
    digraph ClassCouponCMS {
        rankdir=TB;
        nodesep=1.0;
        ranksep=1.2;
        pad="0.8,0.6";
        node [shape=none, fontname="DejaVu Sans", fontsize=14, margin="0"];
        edge [fontname="DejaVu Sans", fontsize=13, penwidth=1.8];

        graph [label="Диаграмма классов — Часть 5: Купоны и управление контентом",
               labelloc=t, fontsize=24, fontname="DejaVu Sans Bold"];

        Coupon [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#FFCCBC" ALIGN="CENTER"><B><FONT POINT-SIZE="18">Coupon</FONT></B><BR/><FONT POINT-SIZE="12">(Купон)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Code : string<BR/>+ Description : string<BR/>+ AmountOff : decimal?<BR/>+ PercentOff : decimal?<BR/>+ IsActive : bool<BR/>+ ValidFrom : DateTime?<BR/>+ ValidUntil : DateTime?<BR/>+ UsageLimit : int?<BR/>+ UsageCount : int<BR/>+ FirstTimeCustomerOnly : bool<BR/>+ LimitOnePerCustomer : bool</FONT></TD></TR>
            </TABLE>
        >];

        CouponProduct [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E1BEE7" ALIGN="CENTER"><B><FONT POINT-SIZE="16">CouponProduct</FONT></B><BR/><FONT POINT-SIZE="12">(Связь купон-товар)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ CouponId : int  [PK, FK]<BR/>+ ProductId : int  [PK, FK]</FONT></TD></TR>
            </TABLE>
        >];

        CouponUsage [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#C8E6C9" ALIGN="CENTER"><B><FONT POINT-SIZE="16">CouponUsage</FONT></B><BR/><FONT POINT-SIZE="12">(Использование купона)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ CouponId : int  [FK]<BR/>+ AppUserId : string  [FK]<BR/>+ DateUsed : DateTime</FONT></TD></TR>
            </TABLE>
        >];

        /* CMS entities */
        ContentBlock [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E3F2FD" ALIGN="CENTER"><B><FONT POINT-SIZE="16">ContentBlock</FONT></B><BR/><FONT POINT-SIZE="12">(Блок контента)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Key : string<BR/>+ Title : string<BR/>+ Content : string<BR/>+ IsHtml : bool</FONT></TD></TR>
            </TABLE>
        >];

        HeroSlide [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E3F2FD" ALIGN="CENTER"><B><FONT POINT-SIZE="16">HeroSlide</FONT></B><BR/><FONT POINT-SIZE="12">(Слайд баннера)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ ImageUrl : string<BR/>+ Title : string<BR/>+ Subtext : string<BR/>+ ButtonLink : string<BR/>+ DisplayOrder : int<BR/>+ IsActive : bool</FONT></TD></TR>
            </TABLE>
        >];

        SiteSetting [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E3F2FD" ALIGN="CENTER"><B><FONT POINT-SIZE="16">SiteSetting</FONT></B><BR/><FONT POINT-SIZE="12">(Настройка сайта)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Key : string<BR/>+ Value : string</FONT></TD></TR>
            </TABLE>
        >];

        FaqItem [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E3F2FD" ALIGN="CENTER"><B><FONT POINT-SIZE="16">FaqItem</FONT></B><BR/><FONT POINT-SIZE="12">(Вопрос-ответ)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Question : string<BR/>+ Answer : string<BR/>+ DisplayOrder : int<BR/>+ IsPublished : bool</FONT></TD></TR>
            </TABLE>
        >];

        EmailTemplate [label=<
            <TABLE BORDER="2" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" COLOR="#336699" BGCOLOR="#ffffff">
                <TR><TD BGCOLOR="#E3F2FD" ALIGN="CENTER"><B><FONT POINT-SIZE="16">EmailTemplate</FONT></B><BR/><FONT POINT-SIZE="12">(Шаблон письма)</FONT></TD></TR>
                <TR><TD ALIGN="LEFT"><FONT POINT-SIZE="14">+ Id : int  [PK]<BR/>+ Name : string<BR/>+ Subject : string<BR/>+ Body : string</FONT></TD></TR>
            </TABLE>
        >];

        /* Мини-ссылки */
        Product_ref [label=<
            <TABLE BORDER="2" CELLBORDER="0" CELLSPACING="0" CELLPADDING="6" COLOR="#9673a6" BGCOLOR="#E3F2FD">
                <TR><TD ALIGN="CENTER"><B><FONT POINT-SIZE="14">Product</FONT></B><BR/><FONT POINT-SIZE="10">(см. часть 1)</FONT></TD></TR>
            </TABLE>
        >];

        /* Связи */
        Coupon -> CouponProduct [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#1565C0"];
        Product_ref -> CouponProduct [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#2E7D32"];
        Coupon -> CouponUsage [arrowhead=crow, arrowtail=tee, dir=both, label="1 : *", color="#1565C0"];

        /* Подпись CMS */
        subgraph cluster_cms {
            label="CMS — управление контентом (без связей)";
            style=dashed;
            color="#999999";
            fontsize=16;
            fontname="DejaVu Sans";
            ContentBlock; HeroSlide; SiteSetting; FaqItem; EmailTemplate;
        }

        /* Layout */
        { rank=same; Coupon; CouponProduct; Product_ref; }
    }
    '''
    dot_path = OUTPUT_DIR / '_temp_class_09d.dot'
    out_path = OUTPUT_DIR / '09д_классы_купоны_CMS.png'
    dot_path.write_text(dot_code, encoding='utf-8')
    subprocess.run(['dot', '-Tpng', f'-Gdpi={GRAPHVIZ_DPI}', str(dot_path), '-o', str(out_path)],
                   check=True, capture_output=True)
    dot_path.unlink()
    print("  ✓ 09д_классы_купоны_CMS.png")


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
    diagram_09a_class_product_domain()
    diagram_09b_class_order_domain()
    diagram_09v_class_user_domain()
    diagram_09g_class_recommendation_domain()
    diagram_09d_class_coupon_cms()
    
    print()
    print("=" * 55)
    print(f"  Готово! 13 диаграмм сохранены в:")
    print(f"  {OUTPUT_DIR}")
    print("=" * 55)


if __name__ == "__main__":
    main()
