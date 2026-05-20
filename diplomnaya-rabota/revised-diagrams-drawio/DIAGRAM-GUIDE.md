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
| 09а | `09а_классы_товары.png` | Диаграмма классов: Домен товаров |
| 09б | `09б_классы_заказы.png` | Диаграмма классов: Домен заказов |
| 09в | `09в_классы_пользователь.png` | Диаграмма классов: Пользователь и избранное |
| 09г | `09г_классы_рекомендации.png` | Диаграмма классов: Рекомендации и A/B тестирование |
| 09д | `09д_классы_купоны_CMS.png` | Диаграмма классов: Купоны и CMS |

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
# 09а — Диаграмма классов: Домен товаров
# `09а_классы_товары.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
**Part 1 of the UML class diagram** showing the **Product domain** — all entities related to product catalog management. This diagram covers 10 classes and 1 enum with full PK/FK annotations and relationship multiplicities.

**Classes shown:**
- **Product** — central entity with Id [PK], Name, Description, Price, QuantityInStock, ProductKind enum, Embedding (AI vector), and three foreign keys: ProductTypeId, ProductBrandId, CategoryId.
- **ProductType** / **ProductBrand** / **Category** — reference tables (dictionaries) linked 1:* to Product.
- **ProductImage** — product photos. FK → Product (1:* with Cascade delete).
- **ProductReview** — customer reviews with Rating, Comment, ReviewDate. FK → Product and FK → AppUser.
- **ProductOption** — option names (e.g., "Color", "Size"). M:M relationship with Product.
- **ProductOptionValue** — specific values (e.g., "Red", "XL") with optional ColorHex. FK → ProductOption (1:*).
- **ProductVariant** — price/stock combinations per option selection. FK → Product (1:*, Restrict delete). M:M with ProductOptionValue. Optional FK → ProductImage (SetNull).
- **ProductKind** — enum: Simple | Variable.

**Key relationships:**
- ProductType/Brand/Category → Product: 1:* (reference dictionaries)
- Product → ProductImage: 1:* (Cascade — deleting product deletes images)
- Product → ProductVariant: 1:* (Restrict — can't delete product with variants)
- Product ↔ ProductOption: M:M (join table)
- ProductVariant ↔ ProductOptionValue: M:M (join table)
- ProductVariant → ProductImage: 0..1 (SetNull — image deletion nullifies reference)

### Русский
**Часть 1 UML-диаграммы классов** — домен товаров. 10 классов и 1 перечисление с полной разметкой PK/FK и кардинальностей связей.

**Ключевые связи:**
- Справочники (ProductType, ProductBrand, Category) → Product: 1:*
- Product → ProductImage: 1:* (Cascade), Product → ProductReview: 1:*, Product → ProductVariant: 1:* (Restrict)
- Product ↔ ProductOption: M:M (многие-ко-многим)
- ProductVariant ↔ ProductOptionValue: M:M
- ProductVariant → ProductImage: 0..1 (SetNull)

### Français
**Partie 1** — domaine des produits : Product, ProductType, ProductBrand, Category, ProductImage, ProductReview, ProductOption, ProductOptionValue, ProductVariant, enum ProductKind. Relations 1:*, M:M avec annotations Cascade/Restrict/SetNull.

## Что писать в дипломе

> **Раздел: «Проектирование классов» (часть 1 — товары)**

На рисунках 09а–09д представлена полная диаграмма классов платформы электронной коммерции, разделённая на пять частей по предметным областям для удобства восприятия. Каждый класс соответствует таблице базы данных через механизм ORM (Entity Framework Core). Все связи содержат аннотации первичных (PK) и внешних (FK) ключей, а также правила каскадного поведения при удалении.

На рисунке 09а показан домен товаров. Центральный класс Product содержит основные атрибуты товара (наименование, описание, цена, количество на складе), а также поле Embedding для хранения ИИ-вектора размерностью 1536, используемого в алгоритме контентных рекомендаций.

Три справочника — ProductType, ProductBrand и Category — связаны с Product отношением «один ко многим» и обеспечивают классификацию товаров.

Класс ProductImage хранит изображения товара со связью 1:* и каскадным удалением. Класс ProductReview — отзывы покупателей с рейтингом и привязкой к пользователю.

Для поддержки вариантов товаров (например, разные размеры и цвета) используется система опций: ProductOption (название опции) и ProductOptionValue (значение опции) связаны отношением 1:*, а Product и ProductOption — связью «многие ко многим». Класс ProductVariant хранит конкретные комбинации опций с индивидуальными ценой и складским остатком, связан с Product (1:*, Restrict) и ProductOptionValue (M:M).

## Что говорить на презентации

«На первой диаграмме классов показан домен товаров — 10 классов. Центральный класс Product связан с тремя справочниками: тип, бренд и категория. Для поддержки вариантов товаров — например, один товар в разных цветах и размерах — используется система ProductOption и ProductVariant со связями многие-ко-многим. Обратите внимание на поле Embedding в Product — это ИИ-вектор для контентных рекомендаций. Все связи аннотированы правилами удаления: Cascade, Restrict или SetNull.»

---

# ═══════════════════════════════════════════════
# 09б — Диаграмма классов: Домен заказов
# `09б_классы_заказы.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
**Part 2 of the class diagram** showing the **Order domain** — orders, order items, delivery, tracking, and owned value types.

**Classes shown:**
- **Order** — main order entity with OrderDate, BuyerEmail, Subtotal, Discount, CouponCode, Status, DeliveryStatus, payment fields, and FK → DeliveryMethod.
- **OrderItem** — line items with Price, Quantity, and FK → Order (1:* with Cascade delete).
- **DeliveryMethod** — shipping methods (ShortName, DeliveryTime, Price). 1:* to Order.
- **TrackingEvent** — delivery tracking events (EventDate, Status, Notes). FK → Order (1:*).

**Owned types (value objects, no separate tables):**
- **ShippingAddress** — owned by Order. Contains Name, LastName, Line1, City, State, PostalCode, Country, PhoneNumber, DeliveryNotes.
- **PaymentSummary** — owned by Order. Contains Last4, Brand, ExpMonth, ExpYear.
- **ProductItemOrdered** — owned by OrderItem. A snapshot of the product at order time (ProductId, ProductName, PictureUrl, SelectedOptions).

**Enums:**
- **OrderStatus**: Pending, PaymentReceived, PaymentFailed, PaymentMismatch, Refunded.
- **DeliveryStatus**: AwaitingProcessing, Processing, Shipped, OutForDelivery, Delivered.

### Русский
**Часть 2** — домен заказов: Order, OrderItem, DeliveryMethod, TrackingEvent, три owned type (ShippingAddress, PaymentSummary, ProductItemOrdered) и два enum (OrderStatus, DeliveryStatus).

**Owned types** — объекты-значения, хранимые в той же таблице, что и владелец (не имеют собственного PK).

### Français
**Partie 2** — domaine des commandes : Order, OrderItem, DeliveryMethod, TrackingEvent, 3 types possédés, 2 enums.

## Что писать в дипломе

> **Раздел: «Проектирование классов» (часть 2 — заказы)**

На рисунке 09б представлен домен заказов. Класс Order содержит дату заказа, электронную почту покупателя, сумму, скидку, статус оплаты и статус доставки. Внешний ключ DeliveryMethodId связывает заказ со способом доставки.

Класс OrderItem представляет позицию заказа (товар, цена, количество) и связан с Order отношением 1:* с каскадным удалением. Каждый OrderItem содержит embedded-объект ProductItemOrdered — снимок товара на момент заказа, что позволяет сохранить информацию даже при изменении или удалении исходного товара.

Класс TrackingEvent хранит историю отслеживания доставки с привязкой к заказу.

Embedded-объекты ShippingAddress и PaymentSummary реализуют паттерн Value Object — они не имеют собственного первичного ключа и хранятся непосредственно в таблице Order, но моделируются как отдельные классы в коде для обеспечения инкапсуляции.

## Что говорить на презентации

«Домен заказов включает Order с позициями OrderItem, способ доставки и отслеживание. Важная деталь — owned types: адрес доставки и данные оплаты не имеют отдельных таблиц, а хранятся прямо в таблице заказа как Value Object. ProductItemOrdered — это снимок товара на момент покупки, что гарантирует целостность данных.»

---

# ═══════════════════════════════════════════════
# 09в — Диаграмма классов: Пользователь и избранное
# `09в_классы_пользователь.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
**Part 3 of the class diagram** showing the **User domain** — identity, address, and wishlist, with cross-references to Product, ProductReview, and Order from other parts.

**Classes shown:**
- **AppUser** — inherits from ASP.NET Identity's IdentityUser. Adds: FirstName, LastName, DateRegistered. Id is string (GUID).
- **Address** — user's saved address. Linked 1:0..1 to AppUser.
- **Wishlist** — user's wishlist container. FK → AppUser (1:0..1).
- **WishlistItem** — items in wishlist. FK → Wishlist (1:*) and FK → Product.

**Cross-domain references (shown as mini-boxes):**
- AppUser → ProductReview: 1:* (a user writes many reviews — from Part 1)
- AppUser → Order: 1:* (by email — from Part 2)
- WishlistItem → Product: FK reference (from Part 1)

### Русский
**Часть 3** — домен пользователя: AppUser (наследует IdentityUser), Address (1:0..1), Wishlist/WishlistItem (избранное). Мини-ссылки на Product, ProductReview и Order из других частей.

### Français
**Partie 3** — domaine utilisateur : AppUser (hérite IdentityUser), Address, Wishlist/WishlistItem. Références croisées aux parties 1 et 2.

## Что писать в дипломе

> **Раздел: «Проектирование классов» (часть 3 — пользователь)**

На рисунке 09в представлен домен пользователя. Класс AppUser наследует от IdentityUser (ASP.NET Identity), добавляя поля FirstName, LastName и DateRegistered. Первичный ключ — строка формата GUID, унаследованная от IdentityUser.

Класс Address хранит адрес пользователя (связь 1:0..1 — один пользователь может иметь один сохранённый адрес).

Система избранного реализована через классы Wishlist и WishlistItem. Wishlist связан с AppUser (1:0..1), а WishlistItem содержит внешние ключи на Wishlist (1:*) и Product.

На диаграмме также показаны перекрёстные связи: AppUser → ProductReview (1:*, пользователь пишет отзывы), AppUser → Order (1:*, заказы по email).

## Что говорить на презентации

«Третья часть — пользователь. AppUser наследует от ASP.NET Identity, что даёт нам аутентификацию, роли и JWT-токены из коробки. У пользователя есть адрес и список избранного. Мини-боксы показывают связи с другими доменами: отзывы, заказы, рекомендации.»

---

# ═══════════════════════════════════════════════
# 09г — Диаграмма классов: Рекомендации и A/B тестирование
# `09г_классы_рекомендации.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
**Part 4 of the class diagram** — the **Recommendation and A/B testing domain**, the core of the thesis. Shows 4 entities, 3 enums, and cross-references to AppUser and Product.

**Entities:**
- **UserInteraction** — records every user action (view, click, add to cart, purchase, etc.). FKs: UserId → AppUser (Cascade), ProductId → Product (Cascade).
- **RecommendationEvent** — records what was recommended and what the user did with it. FKs: UserId → AppUser (Cascade), RecommendedProductId → Product (Restrict), ExperimentId → ABTestExperiment (SetNull — optional).
- **ABTestExperiment** — defines an A/B test with control and treatment strategies, traffic split percentage, start/end dates.
- **ABTestAssignment** — assigns a user to an experiment group. FKs: ExperimentId → ABTestExperiment (Cascade), UserId → AppUser (Cascade).

**Enums:**
- **InteractionType**: View, Click, AddToCart, Purchase, Wishlist, Search, RecommendationClick (7 values).
- **RecommendationEventType**: Impression, Click, AddToCart, Purchase (4 values).
- **RecommendationStrategy**: None, Popular, CollaborativeFiltering, ContentBased, Adaptive (5 values).

**Delete behaviors:**
- AppUser → UserInteraction/RecommendationEvent/ABTestAssignment: Cascade (delete user → delete all their data)
- Product → UserInteraction: Cascade
- Product → RecommendationEvent: Restrict (can't delete product with recommendation history)
- ABTestExperiment → ABTestAssignment: Cascade
- ABTestExperiment → RecommendationEvent: SetNull (ending experiment keeps events, just nulls the link)

### Русский
**Часть 4** — центральная для диплома: рекомендательная система и A/B тестирование. 4 сущности (UserInteraction, RecommendationEvent, ABTestExperiment, ABTestAssignment), 3 перечисления (InteractionType, RecommendationEventType, RecommendationStrategy).

**Правила удаления:** Cascade (AppUser → все дочерние), Restrict (нельзя удалить Product с историей рекомендаций), SetNull (ABTestExperiment → RecommendationEvent).

### Français
**Partie 4** — recommandations et tests A/B : 4 entités, 3 enums. Comportements de suppression : Cascade, Restrict, SetNull. C'est le cœur du mémoire.

## Что писать в дипломе

> **Раздел: «Проектирование классов» (часть 4 — рекомендации)**

На рисунке 09г представлен домен рекомендательной системы — ключевая часть архитектуры платформы.

Класс UserInteraction хранит каждое действие пользователя на платформе: просмотр, клик, добавление в корзину, покупку и т.д. Перечисление InteractionType определяет 7 типов взаимодействий. Поля UserId и ProductId являются внешними ключами к AppUser и Product соответственно, с каскадным удалением.

Класс RecommendationEvent фиксирует каждый факт выдачи рекомендации, включая идентификатор рекомендованного товара, стратегию алгоритма, позицию в списке и опциональную привязку к эксперименту A/B. Перечисление RecommendationStrategy определяет 5 стратегий: None, Popular, CollaborativeFiltering, ContentBased и Adaptive (гибридный алгоритм). В A/B тестировании контрольная группа использует Popular, экспериментальная — Adaptive.

Классы ABTestExperiment и ABTestAssignment реализуют модель данных для A/B тестирования. Эксперимент определяет контрольную и экспериментальную стратегии, процент распределения трафика, даты начала и окончания. Назначение связывает пользователя с экспериментом и группой.

Правила каскадного поведения обеспечивают целостность данных: удаление пользователя каскадно удаляет все его взаимодействия и назначения, но удаление товара с историей рекомендаций запрещено (Restrict), а завершение эксперимента сохраняет события с обнулением ссылки (SetNull).

## Что говорить на презентации

«Четвёртая часть — сердце диплома: рекомендательная система. UserInteraction записывает каждое действие пользователя — 7 типов от просмотра до клика по рекомендации. RecommendationEvent фиксирует каждый показ рекомендации с указанием стратегии алгоритма. ABTestExperiment и ABTestAssignment — модель данных A/B тестов. Обратите внимание на правила удаления: Cascade, Restrict и SetNull — они обеспечивают целостность данных при любых операциях.»

---

# ═══════════════════════════════════════════════
# 09д — Диаграмма классов: Купоны и CMS
# `09д_классы_купоны_CMS.png`
# ═══════════════════════════════════════════════

## Понимание диаграммы / Understanding the Diagram / Comprendre le diagramme

### English
**Part 5 of the class diagram** — **Coupons and CMS (Content Management System)** entities.

**Coupon subsystem:**
- **Coupon** — discount coupon with Code, AmountOff/PercentOff, validity dates, usage limits, and flags (FirstTimeCustomerOnly, LimitOnePerCustomer).
- **CouponProduct** — join table for M:M between Coupon and Product (composite PK: CouponId + ProductId).
- **CouponUsage** — tracks which user used which coupon and when. FKs: CouponId, AppUserId.

**CMS entities (independent, no FK relationships):**
- **ContentBlock** — editable content sections (Key, Title, Content, IsHtml).
- **HeroSlide** — homepage banner slides (ImageUrl, Title, Subtext, ButtonLink, DisplayOrder, IsActive).
- **SiteSetting** — key-value site configuration.
- **FaqItem** — FAQ entries (Question, Answer, DisplayOrder, IsPublished).
- **EmailTemplate** — email templates (Name, Subject, Body).

### Русский
**Часть 5** — купоны (Coupon, CouponProduct, CouponUsage) и CMS-сущности (ContentBlock, HeroSlide, SiteSetting, FaqItem, EmailTemplate). CMS-сущности независимы — не имеют внешних ключей к другим таблицам.

### Français
**Partie 5** — coupons (Coupon, CouponProduct, CouponUsage) et entités CMS (ContentBlock, HeroSlide, SiteSetting, FaqItem, EmailTemplate). Les entités CMS sont indépendantes.

## Что писать в дипломе

> **Раздел: «Проектирование классов» (часть 5 — купоны и CMS)**

На рисунке 09д представлены подсистемы купонов и управления контентом.

Класс Coupon реализует функциональность скидочных купонов с гибкой настройкой: фиксированная скидка (AmountOff) или процентная (PercentOff), ограничение по датам действия, лимит использований, флаги «только для новых покупателей» и «одно использование на клиента». Связующая таблица CouponProduct (составной PK: CouponId + ProductId) позволяет ограничить действие купона определёнными товарами. Класс CouponUsage ведёт учёт использования купонов.

Блок CMS (Content Management System) включает пять самостоятельных сущностей: ContentBlock для редактируемых текстовых блоков, HeroSlide для баннеров главной страницы, SiteSetting для пар «ключ-значение» настроек, FaqItem для раздела «Часто задаваемые вопросы» и EmailTemplate для шаблонов электронных писем. Эти сущности не имеют внешних ключей к другим таблицам и управляются через административную панель.

## Что говорить на презентации

«Последняя часть — купоны и CMS. Система купонов поддерживает фиксированные и процентные скидки с различными ограничениями. CMS-блок — пять независимых сущностей для управления контентом сайта через админ-панель без привлечения разработчика.»

---

# ═══════════════════════════════════════════════
# Советы по использованию диаграмм
# Tips for Using the Diagrams
# Conseils pour l'utilisation des diagrammes
# ═══════════════════════════════════════════════

## В дипломной работе / In the Thesis

1. **Нумерация рисунков:** Замените «рисунок X» на фактический номер в вашем документе.
2. **Порядок размещения:** Рекомендуемый порядок в дипломе:
   - Глава «Проектирование»: 01 (архитектура) → 08 (use case) → 02 (ER) → 09а–09д (классы)
   - Глава «Алгоритмы»: 03 (алгоритм) → 04 (формула)
   - Глава «Экспериментальная оценка»: 05 (A/B тест) → 06 (CTR) → 07 (воронка)
3. **Размер:** Все диаграммы в высоком разрешении (150 DPI). В Word вставляйте на ширину страницы.
4. **Подписи:** Каждый рисунок должен иметь подпись снизу: «Рисунок X — [название]».

## На презентации / In the Presentation

1. **Один слайд = одна диаграмма.** Не пытайтесь вместить несколько.
2. **Минимум текста на слайде** — диаграмма говорит сама за себя, вы объясняете устно.
3. **Ключевые цифры:** 87.5% улучшение CTR и 5× рост покупок — это ваши главные аргументы.
4. **Порядок для презентации:** 01 → 03 → 04 → 05 → 06 → 07 (результаты в конце = сильный финал).
5. **Для диаграмм 09а–09д:** Можно показать 2–3 наиболее важные (09а товары, 09г рекомендации), не обязательно все пять.

## General Notes / Общие замечания / Notes générales

- All diagrams are generated by `generate_drawio.py` — to regenerate, run:
  ```bash
  cd revised-diagrams-drawio
  /home/sputniktech/.local/diagenv/bin/python generate_drawio.py
  ```
- The diagrams use Russian text throughout, suitable for a Russian-language thesis at a Moscow university.
- Font: DejaVu Sans (available on all systems).
- Resolution: 150 DPI — high enough for print, reasonable file size.
