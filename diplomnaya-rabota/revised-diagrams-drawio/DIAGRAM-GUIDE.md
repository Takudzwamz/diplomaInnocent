# Полное руководство по диаграммам дипломной работы
# Complete Guide to Thesis Diagrams
# Guide complet des diagrammes du mémoire

> **Для кого этот документ / Who is this for / À qui s'adresse ce document:**
> Этот документ помогает студенту (1) понять каждую диаграмму, (2) знать, что писать в дипломной работе, и (3) что говорить на презентации.
>
> This document helps the student (1) understand each diagram, (2) know what to write in the thesis, and (3) what to say in the presentation.
>
> Ce document aide l'étudiant à (1) comprendre chaque diagramme, (2) savoir quoi écrire dans le mémoire, et (3) quoi dire lors de la présentation.

---

## Содержание / Table of Contents / Table des matières

| № | Файл | Название |
|---|------|----------|
| 01 | `01_архитектура_системы.png` | Архитектура системы |
| 02 | `02_база_данных_ER.png` | ER-диаграмма базы данных |
| 03 | `03_алгоритм_рекомендаций.png` | Алгоритм генерации рекомендаций |
| 04 | `04_формула_гибрид.png` | Формула гибридного алгоритма |
| 05 | `05_AB_тестирование.png` | Процесс A/B тестирования |
| 06 | `06_результаты_CTR.png` | Результаты CTR |
| 07 | `07_воронка_конверсии.png` | Воронка конверсии |
| 08 | `08_варианты_использования.png` | Диаграмма вариантов использования (Use Case) |
| 09а | `09а_классы_сущности.png` | Диаграмма классов: Сущности |
| 09б | `09б_классы_перечисления.png` | Диаграмма классов: Перечисления (Enum) |
| 09в | `09в_классы_интерфейсы.png` | Диаграмма классов: Интерфейсы (часть 1) |
| 09г | `09г_классы_AB_метрики.png` | Диаграмма классов: Интерфейсы (часть 2) |

---

# ═══════════════════════════════════════════════
# 01 — Архитектура системы
# `01_архитектура_системы.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This diagram shows the **high-level architecture** of the e-commerce platform. It has 3 layers:

1. **User layer (top):** The browser — the user interacts with the site via HTML/CSS/JS pages.
2. **Web server (middle):** ASP.NET Razor Pages application that also contains the recommendation engine. This is the central piece — it handles HTTP requests, renders pages, and runs all recommendation logic.
3. **Data layer (bottom):** Three storage systems:
   - **SQL Server** — the main database storing products, orders, user interactions, and AI embeddings.
   - **Redis** — an in-memory cache used for shopping cart data and session caching.
   - **Azure OpenAI** — an external AI service called via API to generate vector embeddings (1536-dimensional float arrays) for each product. These embeddings power the content-based recommendation algorithm.

The arrows show data flow: the browser sends HTTP requests to the server, the server queries SQL/Redis for data, and calls Azure OpenAI when it needs to generate product embeddings.

**Key point to understand:** This is a monolithic architecture (single application), not microservices. The recommendation system is built directly into the web application, not as a separate service.

### Русский
Эта диаграмма показывает **общую архитектуру** интернет-магазина. Три уровня:

1. **Пользователь (сверху):** Браузер — пользователь взаимодействует с сайтом через HTML/CSS/JS.
2. **Веб-сервер (середина):** ASP.NET Razor Pages приложение, которое также содержит рекомендательную систему. Это центральная часть — обрабатывает HTTP-запросы, рендерит страницы и выполняет всю логику рекомендаций.
3. **Хранилища данных (снизу):** Три системы хранения:
   - **SQL Server** — основная БД: товары, заказы, взаимодействия пользователей, эмбеддинги ИИ.
   - **Redis** — кэш в оперативной памяти для корзины и сессий.
   - **Azure OpenAI** — внешний ИИ-сервис для генерации векторов-эмбеддингов (массивы из 1536 чисел) для каждого товара.

**Ключевой момент:** Это монолитная архитектура (одно приложение), а не микросервисы. Рекомендательная система встроена прямо в веб-приложение.

### Français
Ce diagramme montre l'**architecture globale** de la plateforme e-commerce en 3 couches :

1. **Utilisateur (haut) :** Le navigateur — l'utilisateur interagit avec le site via HTML/CSS/JS.
2. **Serveur web (milieu) :** Application ASP.NET Razor Pages qui contient aussi le moteur de recommandation. C'est la pièce centrale.
3. **Couche de données (bas) :** Trois systèmes de stockage :
   - **SQL Server** — base de données principale (produits, commandes, interactions, embeddings IA).
   - **Redis** — cache en mémoire pour le panier et les sessions.
   - **Azure OpenAI** — service IA externe pour générer des vecteurs d'embedding (tableaux de 1536 nombres).

**Point clé :** C'est une architecture monolithique (une seule application), pas des microservices.

## Что писать в дипломе / What to write in the thesis

> **Раздел диплома: «Проектирование системы» или «Архитектура»**

На рисунке X представлена общая архитектура разработанной системы электронной коммерции с интегрированной рекомендательной системой.

Система построена по монолитной архитектуре на базе фреймворка ASP.NET Core с использованием технологии Razor Pages для серверного рендеринга интерфейса. Все компоненты, включая рекомендательный движок, развёрнуты в рамках единого приложения, что упрощает развёртывание и снижает задержки при обращении к модулю рекомендаций.

В качестве основного хранилища данных используется SQL Server, который содержит информацию о товарах, заказах, взаимодействиях пользователей и векторных представлениях (эмбеддингах) товаров. Для кэширования данных корзины и пользовательских сессий применяется Redis — высокопроизводительное хранилище «ключ-значение» в оперативной памяти.

Для генерации векторных представлений товаров используется внешний сервис Azure OpenAI, к которому приложение обращается по HTTP API. Каждый товар представляется вектором из 1536 чисел с плавающей точкой, что позволяет измерять семантическую близость товаров методом косинусного сходства.

## Что говорить на презентации / What to say in the presentation

> **На слайде с этой диаграммой:**

«Перед вами архитектура разработанной системы. Как видно, это монолитное приложение на ASP.NET Core Razor Pages. Рекомендательная система встроена непосредственно в веб-приложение, что обеспечивает минимальную задержку при генерации рекомендаций. Данные хранятся в SQL Server, кэш — в Redis, а для генерации ИИ-эмбеддингов товаров мы обращаемся к Azure OpenAI по API.»

---

# ═══════════════════════════════════════════════
# 02 — ER-диаграмма базы данных
# `02_база_данных_ER.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This is an **Entity-Relationship (ER) diagram** showing the 6 database tables that power the recommendation system. Each table is shown in a grid with columns: data type | field name | PK/FK marker | Russian description.

**Tables and their purpose:**
- **Products** — stores products with their names, prices, and AI embedding vectors (1536 floats).
- **AspNetUsers** — the ASP.NET Identity user table (email, first name, etc.).
- **ABTestExperiments** — stores A/B test configurations: which two strategies are being compared, traffic split percentage, and whether the test is active.
- **UserInteractions** — every user action (view, click, add-to-cart, purchase) is logged here with timestamps. This is the raw data the recommendation engine learns from.
- **RecommendationEvents** — each time the system shows a recommendation, it logs which product was recommended, by which algorithm, and at what position (1-8). Also logs clicks on recommendations.
- **ABTestAssignments** — links each user to an experiment and records which group (control or treatment) they were assigned to.

**Relationships (all are 1:many):**
- One Product → many UserInteractions and RecommendationEvents
- One User → many UserInteractions, RecommendationEvents, and ABTestAssignments
- One ABTestExperiment → many ABTestAssignments

**Key insight:** The UserInteractions table is the "fuel" for the recommendation engine — it stores every action. The RecommendationEvents table is how we measure effectiveness — it tracks what was shown and whether it was clicked.

### Русский
Это **ER-диаграмма** (диаграмма «сущность—связь»), показывающая 6 таблиц базы данных, которые обеспечивают работу рекомендательной системы. Каждая таблица показана в виде таблицы с колонками: тип данных | имя поля | PK/FK | описание.

**Таблицы:**
- **Products** — товары с названиями, ценами и векторами-эмбеддингами ИИ.
- **AspNetUsers** — таблица пользователей ASP.NET Identity.
- **ABTestExperiments** — конфигурации A/B тестов: две сравниваемые стратегии, процент трафика, активность.
- **UserInteractions** — каждое действие пользователя (просмотр, клик, корзина, покупка) с временной меткой. Это «топливо» для рекомендательного движка.
- **RecommendationEvents** — каждый показ рекомендации: какой товар, каким алгоритмом, на какой позиции (1-8). Также фиксирует клики.
- **ABTestAssignments** — привязка пользователя к эксперименту и группе (контрольная/экспериментальная).

**Ключевой момент:** UserInteractions — это данные, на которых учится система. RecommendationEvents — это данные, по которым мы измеряем эффективность.

### Français
C'est un **diagramme Entité-Relation (ER)** montrant les 6 tables de base de données du système de recommandation.

**Tables :**
- **Products** — produits avec noms, prix et vecteurs d'embedding IA (1536 nombres).
- **AspNetUsers** — table des utilisateurs ASP.NET Identity.
- **ABTestExperiments** — configurations des tests A/B : deux stratégies comparées, pourcentage de trafic, statut actif.
- **UserInteractions** — chaque action utilisateur (vue, clic, panier, achat) avec horodatage. C'est le « carburant » du moteur de recommandation.
- **RecommendationEvents** — chaque affichage de recommandation : quel produit, par quel algorithme, à quelle position.
- **ABTestAssignments** — liaison utilisateur → expérience et groupe (contrôle/traitement).

**Point clé :** UserInteractions = données d'apprentissage. RecommendationEvents = données de mesure.

## Что писать в дипломе

> **Раздел: «Проектирование базы данных»**

На рисунке X представлена схема базы данных, включающая таблицы, относящиеся к рекомендательной системе и модулю A/B тестирования.

Таблица UserInteractions является ключевой для работы рекомендательного алгоритма — в ней фиксируются все действия пользователей: просмотры товаров (View), клики (Click), добавления в корзину (AddToCart), покупки (Purchase), действия с избранным (Wishlist), поиск (Search) и клики по рекомендациям (RecommendationClick). Каждая запись содержит идентификатор пользователя (FK к AspNetUsers), идентификатор товара (FK к Products), тип действия, временную метку и опционально — идентификатор сессии и продолжительность в секундах.

Таблица RecommendationEvents предназначена для отслеживания качества работы рекомендательной системы. При каждом показе блока рекомендаций создаются записи типа Impression для каждого из 8 позиций. При клике пользователя по рекомендованному товару создаётся запись типа Click. Поле Strategy фиксирует, каким алгоритмом (Popular, Adaptive, CollaborativeFiltering, ContentBased) был выбран данный товар.

Таблицы ABTestExperiments и ABTestAssignments реализуют механизм A/B тестирования. ABTestExperiments хранит параметры эксперимента: контрольную и экспериментальную стратегии, процент распределения трафика. ABTestAssignments привязывает каждого пользователя к эксперименту и определяет, в какую группу (контрольную или экспериментальную) он попал.

Все связи между таблицами имеют тип «один ко многим» (1:N), что обеспечивает нормализацию данных и целостность ссылок через внешние ключи.

## Что говорить на презентации

«Здесь показана схема базы данных рекомендательной системы. Ключевая таблица — UserInteractions — в ней записывается каждое действие пользователя: что он посмотрел, на что кликнул, что добавил в корзину. Это «топливо» для наших алгоритмов. Вторая важная таблица — RecommendationEvents — она фиксирует, что мы порекомендовали и кликнул ли пользователь. По этим данным мы вычисляем CTR. Таблицы ABTestExperiments и ABTestAssignments реализуют механизм A/B тестирования для сравнения стратегий.»

---

# ═══════════════════════════════════════════════
# 03 — Алгоритм генерации рекомендаций
# `03_алгоритм_рекомендаций.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This is a **flowchart** showing the step-by-step decision process when the system generates recommendations for a user.

**The flow:**
1. **User opens a page** — this triggers the recommendation engine.
2. **Check: does the user have interaction history?** (Diamond = decision point)
   - **No** → **Cold start:** show the most popular products from the last 30 days. This handles brand new users who have no history.
   - **Yes** → **Run the hybrid algorithm**, which consists of 4 parallel sub-algorithms:
     - **Collaborative Filtering (weight 0.40)** — finds users with similar behavior and recommends what they liked. "Users who bought X also bought Y."
     - **Content-Based AI Analysis (weight 0.35)** — uses AI embeddings to find products semantically similar to what the user previously viewed/purchased. Uses cosine similarity on 1536-dimensional vectors.
     - **Trends (weight 0.15)** — products trending in the last 7 days across all users.
     - **User Categories (weight 0.10)** — products from categories the user has shown interest in.
3. **Sum scores with weights** — each sub-algorithm gives a score per product; they're combined using the weights above.
4. **Filter out already-viewed products** — remove products the user has already seen to keep recommendations fresh.
5. **Output TOP-8 recommendations** — the final sorted list.

**Key insight:** The "cold start problem" is a classic challenge in recommendation systems. This diagram shows how the system handles it gracefully by falling back to popularity-based recommendations.

### Русский
Это **блок-схема**, показывающая пошаговый процесс генерации рекомендаций.

**Поток:**
1. **Пользователь открывает страницу** → запуск движка рекомендаций.
2. **Проверка: есть ли история взаимодействий?** (Ромб = точка принятия решения)
   - **Нет** → **Холодный старт:** показать популярные товары за 30 дней. Решает проблему новых пользователей.
   - **Да** → **Гибридный алгоритм** из 4 параллельных подалгоритмов:
     - **Коллаборативная фильтрация (0.40)** — находит похожих пользователей, рекомендует их предпочтения.
     - **Контентный анализ ИИ (0.35)** — косинусное сходство эмбеддингов товаров.
     - **Тренды за 7 дней (0.15)** — что сейчас популярно у всех.
     - **Категории пользователя (0.10)** — товары из предпочитаемых категорий.
3. **Суммирование баллов с весами.**
4. **Фильтрация уже просмотренных.**
5. **Выдача ТОП-8.**

**Ключевой момент:** «Проблема холодного старта» — классическая проблема рекомендательных систем. Алгоритм решает её, откатываясь к популярным товарам.

### Français
C'est un **organigramme** montrant le processus pas à pas de génération de recommandations.

**Flux :**
1. **L'utilisateur ouvre une page** → déclenchement du moteur.
2. **Vérification : a-t-il un historique ?** (Losange = point de décision)
   - **Non** → **Démarrage à froid :** afficher les produits populaires des 30 derniers jours.
   - **Oui** → **Algorithme hybride** avec 4 sous-algorithmes parallèles :
     - **Filtrage collaboratif (0.40)** — utilisateurs similaires.
     - **Analyse de contenu IA (0.35)** — similarité cosinus des embeddings.
     - **Tendances 7 jours (0.15)** — tendances globales.
     - **Catégories utilisateur (0.10)** — catégories préférées.
3. **Somme pondérée des scores.**
4. **Filtrage des produits déjà vus.**
5. **TOP-8 recommandations.**

## Что писать в дипломе

> **Раздел: «Алгоритм рекомендательной системы»**

На рисунке X представлена блок-схема алгоритма генерации персонализированных рекомендаций.

При загрузке страницы товара или главной страницы система проверяет наличие истории взаимодействий текущего пользователя. Если пользователь новый и данных о его предпочтениях нет (проблема «холодного старта»), система выдаёт список наиболее популярных товаров за последние 30 дней, ранжированных по количеству просмотров, покупок и добавлений в корзину.

Если история взаимодействий имеется, запускается гибридный алгоритм, объединяющий четыре подхода:
- Коллаборативная фильтрация (вес 0.40) — анализирует поведение похожих пользователей;
- Контентный анализ на основе ИИ-эмбеддингов (вес 0.35) — вычисляет косинусное сходство между векторными представлениями товаров;
- Анализ трендов за последние 7 дней (вес 0.15) — учитывает текущую популярность товаров;
- Анализ категорий пользователя (вес 0.10) — предлагает товары из предпочитаемых категорий.

Каждый подалгоритм вычисляет балл релевантности для каждого товара. Баллы взвешиваются и суммируются. После этого из результатов исключаются товары, которые пользователь уже просматривал, и формируется итоговый список из 8 рекомендаций.

## Что говорить на презентации

«На этой блок-схеме показан алгоритм генерации рекомендаций. Сначала система проверяет — есть ли у пользователя история действий. Если нет — это так называемая проблема "холодного старта", и мы просто показываем популярные товары. Если история есть — запускаются параллельно четыре подалгоритма: коллаборативная фильтрация с весом 40%, контентный анализ на основе ИИ-эмбеддингов — 35%, тренды — 15% и категории — 10%. Баллы суммируются, фильтруются уже просмотренные товары, и выдаётся ТОП-8 рекомендаций.»

---

# ═══════════════════════════════════════════════
# 04 — Формула гибридного алгоритма
# `04_формула_гибрид.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This is a **visual representation of the scoring formula** used by the hybrid recommendation algorithm. It's the same information as diagram 03, but presented as a mathematical formula with visual weights.

**The formula:** `Score = 0.40·CF + 0.35·CB + 0.15·Trending + 0.10·Recency`

Each colored box on the left represents one sub-algorithm:
- **Blue (Collaborative Filtering)** × 0.40 — the biggest contributor because "what similar users liked" is usually the most accurate signal.
- **Green (Content-Based AI)** × 0.35 — second biggest because AI embeddings capture semantic similarity (e.g., all running shoes are similar even if names differ).
- **Yellow (Trends, 7 days)** × 0.15 — smaller weight, but captures seasonal/viral trends.
- **Orange (User Categories)** × 0.10 — smallest weight, provides category diversity.

All four scores flow into the **Σ (summation)** node, producing a single **final score** for each product. The top 8 products by score become the recommendations.

**Why these specific weights?** They were tuned during development. Collaborative filtering gets the most weight because it leverages collective intelligence. Content-based gets the second-most because AI embeddings are very accurate for "similar items." Trends and categories are supplementary signals.

### Русский
Это **визуальное представление формулы** гибридного алгоритма рекомендаций.

**Формула:** `Score = 0.40·CF + 0.35·CB + 0.15·Trending + 0.10·Recency`

Четыре блока слева — подалгоритмы:
- **Коллаборативная фильтрация** × 0.40 — наибольший вес, т.к. коллективный опыт обычно самый точный.
- **Контентный анализ (ИИ)** × 0.35 — второй по весу, ИИ-эмбеддинги точно определяют семантическое сходство.
- **Тренды (7 дней)** × 0.15 — улавливают сезонные/вирусные тренды.
- **Категории пользователя** × 0.10 — обеспечивают категорийное разнообразие.

Все баллы суммируются (Σ), образуя **итоговый балл** для каждого товара → ТОП-8.

### Français
C'est la **représentation visuelle de la formule de scoring** de l'algorithme hybride.

**Formule :** `Score = 0.40·CF + 0.35·CB + 0.15·Trending + 0.10·Recency`

Quatre blocs = quatre sous-algorithmes avec leurs poids. Tous convergent vers la somme (Σ) pour produire un score final → TOP-8.

## Что писать в дипломе

> **Раздел: «Математическая модель» или продолжение раздела «Алгоритм»**

Итоговый балл релевантности каждого товара вычисляется по следующей формуле (см. рис. X):

Score(p) = 0.40 · CF(p) + 0.35 · CB(p) + 0.15 · T(p) + 0.10 · R(p)

где:
- CF(p) — балл коллаборативной фильтрации для товара p, вычисленный на основе поведения похожих пользователей;
- CB(p) — балл контентного анализа, основанный на косинусном сходстве ИИ-эмбеддингов товаров;
- T(p) — балл трендовости, отражающий популярность товара за последние 7 дней;
- R(p) — балл категорийной релевантности, учитывающий предпочтительные категории пользователя.

Весовые коэффициенты (0.40, 0.35, 0.15, 0.10) были определены экспериментально. Наибольший вес присвоен коллаборативной фильтрации, так как анализ поведения схожих пользователей обеспечивает наиболее точные рекомендации. Контентный анализ на основе ИИ-эмбеддингов получил второй по значимости вес благодаря способности определять семантическое сходство между товарами.

## Что говорить на презентации

«Вот формула нашего гибридного алгоритма. Итоговый балл каждого товара — это взвешенная сумма четырёх компонентов. Коллаборативная фильтрация имеет наибольший вес — 40%, потому что коллективный интеллект пользователей — самый точный сигнал. Контентный анализ на основе ИИ-эмбеддингов — 35%. Тренды и категории — это дополнительные сигналы. Восемь товаров с наивысшим баллом показываются пользователю.»

---

# ═══════════════════════════════════════════════
# 05 — Процесс A/B тестирования
# `05_AB_тестирование.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This flowchart shows how **A/B testing** works in the system to scientifically compare recommendation strategies.

**The process:**
1. A **new user** visits the site.
2. The system **randomly assigns** them to one of two groups (50/50 split):
   - **Group A (Control)** — sees recommendations generated by the **Popular** strategy (just top-selling products, no personalization).
   - **Group B (Treatment/Experiment)** — sees recommendations generated by the **Adaptive** hybrid algorithm (the sophisticated 4-component formula).
3. **Metrics are recorded** for both groups: impressions (recommendations shown), clicks, add-to-carts, and purchases.
4. **CTR and conversion rates are compared** between the two groups.
5. **Result:** The Adaptive algorithm outperformed Popular with CTR 15% vs 8% — an **87.5% improvement**.

**Why this matters:** A/B testing is the gold standard for evaluating algorithm quality. Without it, you can't prove that your complex algorithm is actually better than a simple baseline. The 50/50 split ensures both groups receive the same traffic conditions, making the comparison fair.

### Русский
Эта блок-схема показывает, как работает **A/B тестирование** для научного сравнения стратегий рекомендаций.

**Процесс:**
1. Новый пользователь заходит на сайт.
2. Система случайно распределяет его в одну из двух групп (50/50):
   - **Группа А (контроль)** — стратегия Popular (просто популярные товары, без персонализации).
   - **Группа Б (эксперимент)** — стратегия Adaptive (гибридный алгоритм).
3. Записываются метрики: показы, клики, корзина, покупки.
4. Сравнение CTR и конверсии.
5. **Результат:** Adaptive лучше — CTR 15% vs 8% (+87.5%).

**Почему это важно:** A/B тестирование — золотой стандарт оценки алгоритмов. Без него нельзя доказать, что сложный алгоритм действительно лучше простого.

### Français
Cet organigramme montre comment fonctionne le **test A/B** pour comparer scientifiquement les stratégies.

**Processus :** Utilisateur nouveau → répartition aléatoire 50/50 → Groupe A (Popular) vs Groupe B (Adaptive) → enregistrement des métriques → comparaison CTR → résultat : Adaptive gagne (15% vs 8%, +87.5%).

## Что писать в дипломе

> **Раздел: «Экспериментальная оценка» или «A/B тестирование»**

Для объективной оценки эффективности разработанного рекомендательного алгоритма был реализован механизм A/B тестирования (см. рис. X).

При первом посещении сайта каждый пользователь случайным образом назначается в одну из двух групп с равным распределением трафика (50/50):
- Группа А (контрольная) — рекомендации формируются стратегией Popular, которая ранжирует товары по общей популярности без персонализации;
- Группа Б (экспериментальная) — рекомендации формируются гибридным адаптивным алгоритмом Adaptive.

Назначение пользователя в группу фиксируется в таблице ABTestAssignments и остаётся неизменным на протяжении всего эксперимента, что исключает смещение результатов. Для обеих групп фиксируются одинаковые метрики: количество показов рекомендаций (Impression), кликов (Click), добавлений в корзину (AddToCart) и покупок (Purchase).

Результаты эксперимента показали, что адаптивный алгоритм значительно превосходит базовую стратегию: CTR составил 15% против 8% у контрольной группы, что представляет улучшение на 87.5%.

## Что говорить на презентации

«Для объективной оценки мы реализовали A/B тестирование. Каждый новый пользователь случайно попадает в группу А или Б — 50 на 50. Группа А видит просто популярные товары, группа Б — рекомендации нашего адаптивного алгоритма. Мы записываем все метрики одинаково для обеих групп. Результат: наш алгоритм показал CTR 15% против 8% у контрольной группы — улучшение почти на 88%.»

---

# ═══════════════════════════════════════════════
# 06 — Результаты CTR
# `06_результаты_CTR.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This is a **bar chart** comparing the Click-Through Rate (CTR) of the two recommendation strategies from the A/B test.

- **Red bar (Popular / Control):** 8.0% CTR — out of every 100 recommendations shown, 8 were clicked.
- **Green bar (Adaptive / Experiment):** 15.0% CTR — out of every 100 recommendations shown, 15 were clicked.
- The green annotation shows **+87.5% improvement**.

**What is CTR?** CTR = (Number of Clicks ÷ Number of Impressions) × 100%. It's the primary metric for recommendation quality — how often users find the recommendations relevant enough to click.

**Why is this significant?** An 87.5% improvement in CTR is a very strong result. It proves that the hybrid algorithm (collaborative filtering + AI embeddings + trends + categories) generates much more relevant recommendations than simply showing popular products.

### Русский
Это **столбчатая диаграмма**, сравнивающая CTR двух стратегий рекомендаций.

- **Красный (Popular):** 8.0% CTR — из 100 показов 8 кликов.
- **Зелёный (Adaptive):** 15.0% CTR — из 100 показов 15 кликов.
- Улучшение: **+87.5%**.

**Что такое CTR?** CTR = (Клики ÷ Показы) × 100%. Главная метрика качества рекомендаций.

### Français
C'est un **diagramme en barres** comparant le CTR des deux stratégies. Popular : 8.0%, Adaptive : 15.0%, amélioration : +87.5%.

## Что писать в дипломе

> **Раздел: «Результаты экспериментов»**

На рисунке X представлены результаты сравнения показателя CTR (Click-Through Rate — коэффициент кликабельности) для двух стратегий рекомендаций.

CTR рассчитывается как отношение количества кликов к количеству показов рекомендаций, выраженное в процентах. Данный показатель является основной метрикой оценки релевантности рекомендаций, поскольку отражает, насколько часто пользователи находят предложенные товары достаточно интересными для перехода.

Контрольная стратегия Popular, основанная на ранжировании товаров по общей популярности, показала CTR 8.0%. Экспериментальная стратегия Adaptive, использующая гибридный алгоритм с коллаборативной фильтрацией и контентным анализом на основе ИИ-эмбеддингов, достигла CTR 15.0%.

Абсолютное улучшение составило 7 процентных пунктов, относительное — 87.5%. Данный результат подтверждает, что персонализированные рекомендации на основе гибридного подхода значительно более релевантны для пользователей по сравнению с неперсонализированным подходом.

## Что говорить на презентации

«На этом графике видно главный результат нашей работы. CTR — это доля кликов от показов рекомендаций. Простые популярные товары дали 8%, а наш адаптивный алгоритм — 15%. Это улучшение на 87.5%. Это доказывает, что персонализированный гибридный подход работает значительно лучше, чем показ одинаковых популярных товаров всем пользователям.»

---

# ═══════════════════════════════════════════════
# 07 — Воронка конверсии
# `07_воронка_конверсии.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This diagram shows two **conversion funnels** side by side — one for each A/B test group. A conversion funnel shows how users progress through stages from seeing a recommendation to actually purchasing.

**Left funnel — Group A (Popular / Control):**
- 100% Impressions (baseline)
- 8.0% Clicks
- 1.2% Add to Cart
- 0.3% Purchases

**Right funnel — Group B (Adaptive / Experiment):**
- 100% Impressions (baseline)
- 15.0% Clicks
- 3.75% Add to Cart
- 1.5% Purchases

**Key comparisons:**
- Click rate: 15% vs 8% (1.9× better)
- Add to cart: 3.75% vs 1.2% (3.1× better)
- Purchase: 1.5% vs 0.3% (5× better!)

**Key insight:** The improvement gets BIGGER at each stage. The adaptive algorithm doesn't just drive more clicks — it drives more *relevant* clicks that lead to actual purchases. This is because personalized recommendations match user preferences better, so when users click, they're more likely to buy.

### Русский
Две **воронки конверсии** — для каждой группы A/B теста.

**Группа А (Popular):** 100% → 8% кликов → 1.2% в корзину → 0.3% покупок.
**Группа Б (Adaptive):** 100% → 15% кликов → 3.75% в корзину → 1.5% покупок.

**Ключевое наблюдение:** Улучшение **нарастает** на каждом этапе: клики в 1.9 раза, корзина в 3.1 раза, покупки в **5 раз**! Адаптивный алгоритм не просто генерирует больше кликов — он генерирует более *релевантные* клики, которые ведут к покупкам.

### Français
Deux **entonnoirs de conversion** côte à côte.

**Groupe A (Popular) :** 100% → 8% clics → 1.2% panier → 0.3% achats.
**Groupe B (Adaptive) :** 100% → 15% clics → 3.75% panier → 1.5% achats.

**Observation clé :** L'amélioration CROÎT à chaque étape : clics ×1.9, panier ×3.1, achats **×5**.

## Что писать в дипломе

> **Раздел: «Анализ воронки конверсии»**

На рисунке X представлены воронки конверсии для контрольной и экспериментальной групп. Воронка конверсии отражает последовательное уменьшение количества пользователей на каждом этапе: от показа рекомендации до совершения покупки.

Для контрольной группы (стратегия Popular) показатели составили: 8.0% кликов от показов, 1.2% добавлений в корзину и 0.3% покупок. Для экспериментальной группы (стратегия Adaptive): 15.0% кликов, 3.75% добавлений в корзину и 1.5% покупок.

Примечательно, что относительное улучшение нарастает на каждом этапе воронки: кликабельность выше в 1.9 раза, добавления в корзину — в 3.1 раза, а конверсия в покупки — в 5 раз. Это свидетельствует о том, что адаптивный алгоритм не просто привлекает больше внимания, а предлагает товары, более соответствующие реальным потребностям пользователей, что приводит к более глубокому вовлечению и более высокой конверсии.

## Что говорить на презентации

«Особенно показательна воронка конверсии. Обратите внимание: на каждом этапе разрыв между стратегиями увеличивается. Кликов больше почти в 2 раза, добавлений в корзину — в 3 раза, а покупок — в 5 раз! Это значит, что наш алгоритм не просто заставляет кликать — он рекомендует товары, которые люди действительно хотят купить.»

---

# ═══════════════════════════════════════════════
# 08 — Диаграмма вариантов использования
# `08_варианты_использования.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This is a **UML Use Case diagram** showing the two main actors (user roles) and their available actions in the system.

**Actor 1 — Buyer (Покупатель):**
- Browse catalog (Просмотр каталога)
- Receive recommendations (Получение рекомендаций)
- Add to cart (Добавление в корзину)
- Place order (Оформление заказа)
- Write review (Написание отзыва)

**Actor 2 — Administrator (Администратор):**
- Manage products (Управление товарами)
- View statistics (Просмотр статистики)
- Manage orders (Управление заказами)

**Key point:** The "Receive recommendations" use case is what makes this system special — it's the entry point to the entire recommendation engine. Every other use case (browse, add to cart, purchase) generates data that feeds back into the recommendation algorithm via UserInteractions.

### Русский
**Диаграмма вариантов использования UML** с двумя актёрами:

**Покупатель:** Просмотр каталога, Получение рекомендаций, Добавление в корзину, Оформление заказа, Написание отзыва.

**Администратор:** Управление товарами, Просмотр статистики, Управление заказами.

**Ключевой момент:** «Получение рекомендаций» — это точка входа в рекомендательную систему. Все остальные действия покупателя генерируют данные (UserInteractions), которые питают алгоритм рекомендаций.

### Français
**Diagramme de cas d'utilisation UML** avec deux acteurs :

**Acheteur :** Parcourir le catalogue, Recevoir des recommandations, Ajouter au panier, Passer commande, Écrire un avis.

**Administrateur :** Gérer les produits, Voir les statistiques, Gérer les commandes.

## Что писать в дипломе

> **Раздел: «Функциональные требования» или «Проектирование»**

На рисунке X представлена диаграмма вариантов использования, отражающая основные функциональные возможности системы с точки зрения двух категорий пользователей.

Покупатель может выполнять следующие действия: просматривать каталог товаров, получать персонализированные рекомендации, добавлять товары в корзину, оформлять заказы и оставлять отзывы. Вариант использования «Получение рекомендаций» является центральным для данной работы — именно он инициирует работу рекомендательного алгоритма. При этом все остальные действия покупателя (просмотр, добавление в корзину, покупка) автоматически фиксируются в таблице UserInteractions и используются рекомендательной системой для персонализации.

Администратор имеет доступ к управлению каталогом товаров, просмотру статистики работы рекомендательной системы (включая результаты A/B тестов) и управлению заказами.

## Что говорить на презентации

«На диаграмме вариантов использования видны два актёра. Покупатель может просматривать каталог, получать рекомендации, делать покупки и писать отзывы. Администратор управляет товарами, заказами и просматривает статистику рекомендаций. Важно, что все действия покупателя автоматически записываются и становятся обучающими данными для алгоритма рекомендаций.»

---

# ═══════════════════════════════════════════════
# 09а — Диаграмма классов: Сущности
# `09а_классы_сущности.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This is **Part 1 of the UML class diagram** showing the **entity classes** — the C# objects that map directly to database tables via Entity Framework.

**Classes shown:**
- **BaseEntity** — abstract base class with just `Id : int`. All other entities inherit from it (shown by hollow-triangle arrows labeled "наследует").
- **UserInteraction** — records user actions. Fields: UserId, ProductId, Type (enum InteractionType), Timestamp, optional SessionId and DurationSeconds.
- **RecommendationEvent** — records what the system recommended. Fields: UserId, RecommendedProductId, SourceProductId, EventType, Strategy, Position (1-8), optional ExperimentId, Timestamp.
- **ABTestExperiment** — an A/B test configuration. Fields: Name, Description, ControlStrategy, TreatmentStrategy, TreatmentPercentage, StartDate, EndDate, IsActive.
- **ABTestAssignment** — links a user to an experiment. Fields: ExperimentId, UserId, IsTreatment, AssignedAt.

**Relationships:**
- All 4 entities inherit from BaseEntity (generalization).
- ABTestExperiment → ABTestAssignment: 1:* (one experiment has many assignments).
- ABTestExperiment → RecommendationEvent: 0..1:* (an event may optionally be linked to an experiment).

### Русский
**Часть 1 UML-диаграммы классов** — классы-сущности (Entity), которые отображаются на таблицы БД через Entity Framework.

**Классы:**
- **BaseEntity** — абстрактный базовый класс с полем Id : int. Все сущности наследуют от него.
- **UserInteraction** — действия пользователя (UserId, ProductId, Type, Timestamp и др.).
- **RecommendationEvent** — события рекомендаций (что показали, каким алгоритмом, позиция).
- **ABTestExperiment** — конфигурация A/B теста.
- **ABTestAssignment** — привязка пользователя к эксперименту.

**Связи:** Наследование от BaseEntity. ABTestExperiment → ABTestAssignment (1:*). ABTestExperiment → RecommendationEvent (0..1:*).

### Français
**Partie 1 du diagramme de classes UML** — les classes entités (C# → tables DB via Entity Framework).

**Classes :** BaseEntity (base abstraite), UserInteraction, RecommendationEvent, ABTestExperiment, ABTestAssignment. Toutes héritent de BaseEntity. Relations : ABTestExperiment → ABTestAssignment (1:*).

## Что писать в дипломе

> **Раздел: «Проектирование классов» (первая часть)**

На рисунках 09а–09г представлена диаграмма классов рекомендательной системы, разделённая на четыре части для удобства восприятия.

На рисунке 09а показаны классы-сущности (entities), которые отображаются на таблицы базы данных через механизм ORM (Entity Framework Core). Все сущности наследуют от абстрактного базового класса BaseEntity, содержащего поле Id типа int, что обеспечивает единообразие идентификации объектов.

Класс UserInteraction хранит информацию о действиях пользователя: идентификатор пользователя (UserId), идентификатор товара (ProductId), тип действия (Type — перечисление InteractionType), временную метку (Timestamp), а также опциональные поля для идентификатора сессии и продолжительности взаимодействия в секундах.

Класс RecommendationEvent фиксирует каждый факт выдачи или клика по рекомендации, включая идентификаторы пользователя и товара, стратегию алгоритма, позицию в списке рекомендаций (1–8) и опциональную привязку к эксперименту A/B.

Классы ABTestExperiment и ABTestAssignment реализуют модель данных для A/B тестирования: эксперимент определяет контрольную и экспериментальную стратегии, а назначение связывает каждого пользователя с конкретным экспериментом и группой.

## Что говорить на презентации

«На этой диаграмме показаны основные классы-сущности рекомендательной системы. Все наследуют от BaseEntity. UserInteraction — это запись каждого действия пользователя. RecommendationEvent — запись каждого показа и клика рекомендации. ABTestExperiment и ABTestAssignment — модель данных для A/B тестов. Эти классы через Entity Framework отображаются непосредственно на таблицы базы данных, которые мы видели на ER-диаграмме.»

---

# ═══════════════════════════════════════════════
# 09б — Диаграмма классов: Перечисления
# `09б_классы_перечисления.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This is **Part 2 of the class diagram** showing the three **enum types** used by the recommendation system.

**Enums:**
1. **InteractionType** — categorizes user actions:
   - View = 0 (viewed a product page)
   - Click = 1 (clicked on a product)
   - AddToCart = 2 (added to shopping cart)
   - Purchase = 3 (bought the product)
   - Wishlist = 4 (added to wishlist/favorites)
   - Search = 5 (searched for a product)
   - RecommendationClick = 6 (clicked a recommended product — important for CTR!)

2. **RecommendationEventType** — categorizes recommendation tracking events:
   - Impression = 0 (recommendation was shown to user)
   - Click = 1 (user clicked the recommendation)
   - AddToCart = 2 (user added recommended product to cart)
   - Purchase = 3 (user bought the recommended product)

3. **RecommendationStrategy** — the available recommendation algorithms:
   - None = 0 (no strategy)
   - Popular = 1 (popularity-based, the control strategy)
   - CollaborativeFiltering = 2 (based on similar users)
   - ContentBased = 3 (based on AI embeddings)
   - Adaptive = 4 (the hybrid algorithm combining all of the above)

**Key insight:** The `RecommendationStrategy` enum is what the A/B test switches between. Group A gets `Popular`, Group B gets `Adaptive`.

### Русский
**Часть 2 диаграммы классов** — три перечисления (enum).

1. **InteractionType** — типы действий: View, Click, AddToCart, Purchase, Wishlist, Search, RecommendationClick.
2. **RecommendationEventType** — типы событий рекомендаций: Impression, Click, AddToCart, Purchase.
3. **RecommendationStrategy** — стратегии алгоритмов: None, Popular, CollaborativeFiltering, ContentBased, Adaptive.

**Ключевой момент:** В A/B тесте переключается именно RecommendationStrategy: группа А — Popular, группа Б — Adaptive.

### Français
**Partie 2** — trois types enum : InteractionType (7 valeurs), RecommendationEventType (4 valeurs), RecommendationStrategy (5 valeurs dont Adaptive = l'algorithme hybride).

## Что писать в дипломе

> **Раздел: «Проектирование классов» (продолжение)**

На рисунке 09б представлены перечисления (enum), используемые в рекомендательной системе.

Перечисление InteractionType определяет 7 типов взаимодействий пользователя с платформой: просмотр (View), клик (Click), добавление в корзину (AddToCart), покупка (Purchase), добавление в избранное (Wishlist), поиск (Search) и клик по рекомендации (RecommendationClick). Последний тип особенно важен для вычисления CTR рекомендательной системы.

Перечисление RecommendationEventType описывает 4 стадии воронки конверсии рекомендаций: показ (Impression), клик (Click), добавление в корзину (AddToCart) и покупка (Purchase).

Перечисление RecommendationStrategy определяет доступные алгоритмы рекомендаций: без стратегии (None), популярные товары (Popular), коллаборативная фильтрация (CollaborativeFiltering), контентный анализ (ContentBased) и адаптивный гибридный алгоритм (Adaptive). В рамках A/B тестирования контрольная группа использует стратегию Popular, экспериментальная — Adaptive.

## Что говорить на презентации

«На этой диаграмме показаны три перечисления. InteractionType — 7 типов действий пользователя, от просмотра до клика по рекомендации. RecommendationEventType — 4 стадии воронки. И RecommendationStrategy — 5 стратегий рекомендаций. В A/B тесте мы сравниваем Popular и Adaptive.»

---

# ═══════════════════════════════════════════════
# 09в — Диаграмма классов: Интерфейсы (часть 1)
# `09в_классы_интерфейсы.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This is **Part 3 of the class diagram** showing 3 **service interfaces** related to recommendations and user tracking.

**Interfaces:**
1. **IAdaptiveRecommendationService** — the main recommendation interface:
   - `GetAdaptiveRecommendationsAsync(userId, count)` — runs the full hybrid algorithm for a user
   - `GetPopularProductsAsync(count)` — gets popular products (cold start fallback)
   - `GetCollaborativeRecommendationsAsync(userId, count)` — runs collaborative filtering only
   - `GetContentBasedRecommendationsAsync(productId, count)` — runs content-based analysis only

2. **IUserInteractionService** — tracks what users do:
   - `TrackInteractionAsync(userId, productId, type, ...)` — records a user action
   - `GetUserInteractionsAsync(userId, limit)` — retrieves a user's action history
   - `GetUserTopProductsAsync(userId, count)` — gets the user's most-interacted products

3. **IProductEmbeddingService** — manages AI vector embeddings:
   - `GenerateMissingEmbeddingsAsync()` — batch-generates embeddings for all products that don't have one
   - `GetProductEmbeddingAsync(productId)` — gets the 1536-float vector for one product
   - `RegenerateProductEmbeddingAsync(productId)` — regenerates a single product's embedding

**Dashed arrows show dependencies:**
- IAdaptiveRecommendationService *reads* UserInteraction data and *returns* Product objects
- IUserInteractionService *creates* UserInteraction records
- IProductEmbeddingService *enriches* Product with embedding data

**Why interfaces?** Using interfaces (not concrete classes) follows the Dependency Injection pattern in ASP.NET Core. The actual implementations are registered in DI container and can be swapped without changing consuming code.

### Русский
**Часть 3 диаграммы классов** — 3 сервисных интерфейса:

1. **IAdaptiveRecommendationService** — главный интерфейс рекомендаций: адаптивные, популярные, коллаборативные и контентные рекомендации.
2. **IUserInteractionService** — отслеживание действий: запись, получение истории, топ товаров.
3. **IProductEmbeddingService** — управление ИИ-эмбеддингами: генерация, получение, регенерация.

**Пунктирные стрелки:** зависимости между сервисами и сущностями (создаёт, читает, возвращает, обогащает).

**Почему интерфейсы?** Паттерн Dependency Injection в ASP.NET Core — реализации можно менять без изменения вызывающего кода.

### Français
**Partie 3** — 3 interfaces de service : IAdaptiveRecommendationService (recommandations), IUserInteractionService (suivi des actions), IProductEmbeddingService (embeddings IA). Les flèches en pointillé montrent les dépendances vers les entités.

## Что писать в дипломе

> **Раздел: «Проектирование классов» (продолжение)**

На рисунке 09в представлены интерфейсы сервисов, отвечающих за генерацию рекомендаций и сбор данных о поведении пользователей.

Интерфейс IAdaptiveRecommendationService является центральным компонентом рекомендательной системы. Метод GetAdaptiveRecommendationsAsync реализует полный гибридный алгоритм, описанный в разделе X. Методы GetPopularProductsAsync, GetCollaborativeRecommendationsAsync и GetContentBasedRecommendationsAsync предоставляют доступ к отдельным подалгоритмам, что позволяет использовать их как независимо, так и в составе гибридной модели.

Интерфейс IUserInteractionService отвечает за запись и извлечение данных о взаимодействиях пользователей. Метод TrackInteractionAsync вызывается при каждом значимом действии пользователя и создаёт запись в таблице UserInteractions.

Интерфейс IProductEmbeddingService инкапсулирует работу с векторными представлениями товаров. Метод GenerateMissingEmbeddingsAsync выполняет пакетную генерацию эмбеддингов для всех товаров, у которых они отсутствуют, обращаясь к сервису Azure OpenAI.

Использование интерфейсов вместо конкретных классов обеспечивает слабую связанность компонентов и позволяет применять паттерн внедрения зависимостей (Dependency Injection), поддерживаемый фреймворком ASP.NET Core.

## Что говорить на презентации

«Здесь три ключевых интерфейса. IAdaptiveRecommendationService — это "мозг" системы, он запускает гибридный алгоритм и возвращает список рекомендованных товаров. IUserInteractionService записывает каждое действие пользователя. IProductEmbeddingService отвечает за генерацию ИИ-эмбеддингов через Azure OpenAI. Мы используем интерфейсы, а не конкретные классы, следуя принципу Dependency Injection.»

---

# ═══════════════════════════════════════════════
# 09г — Диаграмма классов: Интерфейсы (часть 2)
# `09г_классы_AB_метрики.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
This is **Part 4 of the class diagram** showing 2 more **service interfaces** — for A/B testing and metrics.

**Interfaces:**
1. **IABTestService** — manages A/B test experiments:
   - `GetActiveExperimentAsync()` — gets the currently running experiment (if any)
   - `GetOrAssignUserAsync(userId, experimentId)` — assigns a user to a group (or returns existing assignment). This is where the 50/50 random split happens.
   - `GetUserStrategyAsync(userId)` — returns which recommendation strategy a user should see (based on their group assignment)
   - `CreateExperimentAsync(...)` — creates a new A/B test
   - `EndExperimentAsync(experimentId)` — ends a running experiment

2. **IRecommendationMetricsService** — records and retrieves metrics:
   - `RecordImpressionAsync(...)` — logs when a recommendation is shown
   - `RecordClickAsync(...)` — logs when a recommendation is clicked
   - `RecordPurchaseAsync(...)` — logs when a recommended product is purchased
   - `GetExperimentMetricsAsync(experimentId)` — calculates CTR, conversion for a specific experiment
   - `GetSystemMetricsAsync(from, to)` — calculates overall system metrics for a date range

**Dashed arrows show dependencies:**
- IABTestService *manages* ABTestExperiment and *assigns* ABTestAssignment
- IRecommendationMetricsService *writes* RecommendationEvent records

**How these interfaces work together:** When a user visits the site, IABTestService determines their group. Then IAdaptiveRecommendationService (from Part 3) generates recommendations using the appropriate strategy. IRecommendationMetricsService records what was shown. Later, administrators use GetExperimentMetricsAsync to see the results.

### Русский
**Часть 4 диаграммы классов** — ещё 2 интерфейса: A/B тестирование и метрики.

1. **IABTestService** — управление A/B тестами: получение активного эксперимента, назначение пользователя в группу (50/50), определение стратегии пользователя, создание и завершение экспериментов.
2. **IRecommendationMetricsService** — метрики: запись показов, кликов, покупок; расчёт метрик эксперимента (CTR, конверсия); системные метрики за период.

**Как все 5 интерфейсов работают вместе:** IABTestService определяет группу → IAdaptiveRecommendationService генерирует рекомендации нужной стратегией → IRecommendationMetricsService записывает результаты → IUserInteractionService фиксирует действия → IProductEmbeddingService обновляет эмбеддинги.

### Français
**Partie 4** — 2 interfaces : IABTestService (gestion des tests A/B, assignation 50/50) et IRecommendationMetricsService (enregistrement et calcul des métriques CTR/conversion).

## Что писать в дипломе

> **Раздел: «Проектирование классов» (завершение)**

На рисунке 09г представлены интерфейсы, обеспечивающие функционирование модуля A/B тестирования и сбора метрик.

Интерфейс IABTestService реализует полный жизненный цикл A/B экспериментов. Метод GetOrAssignUserAsync при первом обращении пользователя к рекомендациям выполняет случайное распределение в контрольную или экспериментальную группу с заданным процентом разделения трафика. Последующие обращения возвращают ранее сохранённое назначение, что гарантирует консистентность эксперимента. Метод GetUserStrategyAsync возвращает стратегию рекомендаций для конкретного пользователя на основе его назначения в группу.

Интерфейс IRecommendationMetricsService отвечает за сбор и агрегацию метрик. Методы RecordImpressionAsync, RecordClickAsync и RecordPurchaseAsync создают записи в таблице RecommendationEvents. Метод GetExperimentMetricsAsync вычисляет ключевые показатели (CTR, конверсия) для конкретного эксперимента, а GetSystemMetricsAsync — общесистемные метрики за произвольный временной период.

В совокупности пять интерфейсов (09в и 09г) образуют замкнутый цикл: IABTestService определяет стратегию → IAdaptiveRecommendationService генерирует рекомендации → IRecommendationMetricsService фиксирует показы и клики → IUserInteractionService записывает дальнейшие действия → IProductEmbeddingService обеспечивает актуальность векторных представлений.

## Что говорить на презентации

«И последняя часть диаграммы классов — два интерфейса для A/B тестирования и метрик. IABTestService назначает пользователя в группу и определяет, какую стратегию рекомендаций он увидит. IRecommendationMetricsService записывает все показы и клики и вычисляет CTR. Вместе все пять интерфейсов из частей 3 и 4 образуют замкнутый цикл: определение стратегии, генерация рекомендаций, сбор метрик, обучение на действиях пользователя.»

---

# ═══════════════════════════════════════════════
# Советы по использованию диаграмм
# Tips for Using the Diagrams
# Conseils pour l'utilisation des diagrammes
# ═══════════════════════════════════════════════

## В дипломной работе / In the Thesis

1. **Нумерация рисунков:** Замените «рисунок X» на фактический номер в вашем документе.
2. **Порядок размещения:** Рекомендуемый порядок в дипломе:
   - Глава «Проектирование»: 01 (архитектура) → 08 (use case) → 02 (ER) → 09а–09г (классы)
   - Глава «Алгоритмы»: 03 (алгоритм) → 04 (формула)
   - Глава «Экспериментальная оценка»: 05 (A/B тест) → 06 (CTR) → 07 (воронка)
3. **Размер:** Все диаграммы в высоком разрешении (150 DPI). В Word вставляйте на ширину страницы.
4. **Подписи:** Каждый рисунок должен иметь подпись снизу: «Рисунок X — [название]».

## На презентации / In the Presentation

1. **Один слайд = одна диаграмма.** Не пытайтесь вместить несколько.
2. **Минимум текста на слайде** — диаграмма говорит сама за себя, вы объясняете устно.
3. **Ключевые цифры:** 87.5% улучшение CTR и 5× рост покупок — это ваши главные аргументы.
4. **Порядок для презентации:** 01 → 03 → 04 → 05 → 06 → 07 (результаты в конце = сильный финал).
5. **Для диаграмм 09а–09г:** Можно показать одну общую и одну детальную, не обязательно все четыре.

## General Notes / Общие замечания / Notes générales

- All diagrams are generated by `generate_drawio.py` — to regenerate, run:
  ```bash
  cd revised-diagrams-drawio
  /home/sputniktech/.local/diagenv/bin/python generate_drawio.py
  ```
- The diagrams use Russian text throughout, suitable for a Russian-language thesis at a Moscow university.
- Font: DejaVu Sans (available on all systems).
- Resolution: 150 DPI — high enough for print, reasonable file size.
