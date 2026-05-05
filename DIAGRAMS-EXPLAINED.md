# All Diagrams & Graphs Explained / Explication des diagrammes / Объяснение всех диаграмм

This document explains **every diagram and graph** in your diploma project so you can confidently present and defend them.

---

## TABLE OF CONTENTS

1. [Architecture Diagrams (6)](#architecture-diagrams)
2. [UML Diagrams (10)](#uml-diagrams)
3. [DFD Diagrams (2)](#dfd-diagrams)
4. [Statistical Graphs (8+)](#statistical-graphs)

---

---

# ARCHITECTURE DIAGRAMS

## 1. High-Level System Diagram (Диаграмма верхнего уровня)

### 🇬🇧 English
This diagram shows the **entire system as a bird's eye view**. It has layers:
- **Client Layer** — the web browser
- **Presentation Layer** — Razor Pages (server-side rendering), JavaScript (tracking user clicks), SignalR (real-time notifications)
- **Application Layer** — the services: AdaptiveRecommendation, AIRecommendation, ProductEmbedding, ABTest, Metrics, UserInteraction
- **Domain Layer (Core)** — entities (Product, UserInteraction, etc.), interfaces, specifications
- **Data Layer (Infrastructure)** — Entity Framework Core, seeders
- **External Services** — Azure OpenAI, Cloudinary, SendGrid, Paystack
- **Data Stores** — SQL Server, Redis

**What it shows:** How the user's browser request travels through layers to reach the database and external APIs.

**Why it matters for the thesis:** Proves your system follows Clean Architecture with proper separation of concerns.

### 🇫🇷 Français
Ce diagramme montre **le système entier vu d'en haut**. Il a des couches :
- **Couche Client** — le navigateur web
- **Couche Présentation** — Pages Razor (rendu côté serveur), JavaScript (suivi des clics), SignalR (notifications en temps réel)
- **Couche Application** — les services : Recommandation Adaptative, Recommandation IA, Embeddings Produit, Test A/B, Métriques, Interaction Utilisateur
- **Couche Domaine (Core)** — entités, interfaces, spécifications
- **Couche Données (Infrastructure)** — Entity Framework Core
- **Services Externes** — Azure OpenAI, Cloudinary, SendGrid, Paystack
- **Stockages** — SQL Server, Redis

**Ce qu'il montre :** Comment la requête du navigateur traverse les couches pour atteindre la base de données.

**Pourquoi c'est important :** Prouve que le système suit l'Architecture Propre avec séparation des responsabilités.

### 🇷🇺 Русский
Эта диаграмма показывает **всю систему с высоты птичьего полёта**. Слои:
- **Клиентский слой** — веб-браузер
- **Слой представления** — Razor Pages (серверный рендеринг), JavaScript (трекинг кликов), SignalR (уведомления)
- **Слой приложения** — сервисы: адаптивные рекомендации, ИИ-рекомендации, эмбеддинги, A/B тесты, метрики, взаимодействия
- **Доменный слой (Core)** — сущности, интерфейсы, спецификации
- **Слой данных (Infrastructure)** — Entity Framework Core, сидеры
- **Внешние сервисы** — Azure OpenAI, Cloudinary, SendGrid, Paystack
- **Хранилища** — SQL Server, Redis

**Что показывает:** Как запрос пользователя проходит через все слои до базы данных и внешних API.

**Зачем нужна в дипломе:** Доказывает, что система построена по Clean Architecture с правильным разделением ответственности.

---

## 2. Recommendation System Workflow (Поток рекомендательной системы)

### 🇬🇧 English
This shows **step by step how recommendations are generated**:
1. User visits a page → system checks if there's an A/B test active
2. Based on assigned strategy (collaborative, content-based, hybrid, AI, popular, random)
3. The selected strategy fetches user's interaction history from DB
4. Generates a ranked list of products
5. Returns Top-N recommendations to the page
6. Every impression (show) and click is recorded for metrics

**Key insight:** The system uses a **hybrid approach** — it blends multiple strategies and picks the best one based on A/B test results.

### 🇫🇷 Français
Cela montre **étape par étape comment les recommandations sont générées** :
1. L'utilisateur visite une page → le système vérifie s'il y a un test A/B actif
2. Selon la stratégie assignée (collaborative, contenu, hybride, IA, populaire, aléatoire)
3. La stratégie sélectionnée récupère l'historique des interactions de l'utilisateur
4. Génère une liste classée de produits
5. Retourne les Top-N recommandations à la page
6. Chaque impression et clic est enregistré pour les métriques

**Point clé :** Le système utilise une **approche hybride** — il mélange plusieurs stratégies et choisit la meilleure grâce aux tests A/B.

### 🇷🇺 Русский
Показывает **пошагово, как генерируются рекомендации**:
1. Пользователь заходит на страницу → система проверяет активный A/B тест
2. По назначенной стратегии (коллаборативная, контентная, гибридная, ИИ, популярное, случайное)
3. Выбранная стратегия загружает историю взаимодействий пользователя из БД
4. Генерирует ранжированный список товаров
5. Возвращает Top-N рекомендаций на страницу
6. Каждый показ и клик записывается для метрик

**Ключевой момент:** Система использует **гибридный подход** — комбинирует несколько стратегий и выбирает лучшую через A/B тесты.

---

## 3. A/B Testing Flow (Поток A/B тестирования)

### 🇬🇧 English
This shows **how users are split into experiment groups**:
1. User makes a request → system checks active experiments
2. Hash of (userId + experimentId) → deterministic 50/50 split
3. Group A gets "Control" strategy (e.g., random recommendations)
4. Group B gets "Treatment" strategy (e.g., AI recommendations)
5. Both groups' clicks and purchases are tracked separately
6. Metrics are compared to measure the "lift" (improvement)

**Formula:** `group = SHA256(userId + experimentId) % 100 < trafficPercentage`

This ensures the same user ALWAYS gets the same group (no randomness between visits).

### 🇫🇷 Français
Cela montre **comment les utilisateurs sont divisés en groupes d'expérience** :
1. L'utilisateur fait une requête → le système vérifie les expériences actives
2. Hash de (userId + experimentId) → division déterministe 50/50
3. Groupe A reçoit la stratégie "Contrôle" (ex. : recommandations aléatoires)
4. Groupe B reçoit la stratégie "Traitement" (ex. : recommandations IA)
5. Les clics et achats des deux groupes sont suivis séparément
6. Les métriques sont comparées pour mesurer le "lift" (amélioration)

**Formule :** `groupe = SHA256(userId + experimentId) % 100 < pourcentageTrafic`

Cela garantit que le même utilisateur obtient TOUJOURS le même groupe.

### 🇷🇺 Русский
Показывает **как пользователи распределяются по группам эксперимента**:
1. Пользователь делает запрос → система проверяет активные эксперименты
2. Хэш от (userId + experimentId) → детерминированное деление 50/50
3. Группа A получает стратегию «Контроль» (напр., случайные рекомендации)
4. Группа B получает стратегию «Лечение/Эксперимент» (напр., ИИ-рекомендации)
5. Клики и покупки обеих групп отслеживаются раздельно
6. Метрики сравниваются для вычисления «лифта» (улучшения)

**Формула:** `группа = SHA256(userId + experimentId) % 100 < процентТрафика`

Один пользователь ВСЕГДА попадает в одну и ту же группу (нет случайности между визитами).

---

## 4. Offline Evaluation Flow (Поток Offline-оценки)

### 🇬🇧 English
This shows how we **evaluate recommendation quality without live users**:
1. Take historical data (past interactions)
2. Split into train set (80%) and test set (20%)
3. Train each strategy on the training data
4. Predict recommendations for test users
5. Compare predictions vs actual user behavior
6. Calculate Precision@K, Recall@K, NDCG

**Why offline evaluation?** You can test new strategies safely before deploying them to real users.

### 🇫🇷 Français
Cela montre comment on **évalue la qualité des recommandations sans utilisateurs en direct** :
1. Prendre les données historiques (interactions passées)
2. Diviser en ensemble d'entraînement (80%) et ensemble de test (20%)
3. Entraîner chaque stratégie sur les données d'entraînement
4. Prédire les recommandations pour les utilisateurs test
5. Comparer les prédictions vs le comportement réel
6. Calculer Precision@K, Recall@K, NDCG

**Pourquoi l'évaluation offline ?** On peut tester de nouvelles stratégies en sécurité avant de les déployer.

### 🇷🇺 Русский
Показывает, как **оценивать качество рекомендаций без живых пользователей**:
1. Берём исторические данные (прошлые взаимодействия)
2. Делим на обучающую (80%) и тестовую (20%) выборки
3. Обучаем каждую стратегию на тренировочных данных
4. Предсказываем рекомендации для тестовых пользователей
5. Сравниваем предсказания с реальным поведением
6. Вычисляем Precision@K, Recall@K, NDCG

**Зачем offline-оценка?** Можно безопасно тестировать новые стратегии до развёртывания на реальных пользователях.

---

## 5. Clean Architecture Layers (Слои Clean Architecture)

### 🇬🇧 English
Shows the **dependency rule**: outer layers depend on inner layers, never the reverse.
- **Core (innermost)** — entities + interfaces. Has ZERO dependencies.
- **Infrastructure** — implements interfaces (database, external APIs). Depends on Core.
- **StorefrontRazor (outermost)** — web UI. Depends on Core + Infrastructure.

**Key principle:** If you change the database (e.g., switch from SQL Server to PostgreSQL), you only change Infrastructure — Core and UI remain untouched.

### 🇫🇷 Français
Montre la **règle de dépendance** : les couches externes dépendent des internes, jamais l'inverse.
- **Core (le plus interne)** — entités + interfaces. ZÉRO dépendance.
- **Infrastructure** — implémente les interfaces (base de données, APIs). Dépend de Core.
- **StorefrontRazor (le plus externe)** — interface web. Dépend de Core + Infrastructure.

**Principe clé :** Si vous changez la base de données, vous ne modifiez que Infrastructure — Core et UI restent intacts.

### 🇷🇺 Русский
Показывает **правило зависимостей**: внешние слои зависят от внутренних, никогда наоборот.
- **Core (самый внутренний)** — сущности + интерфейсы. НОЛЬ зависимостей.
- **Infrastructure** — реализует интерфейсы (БД, внешние API). Зависит от Core.
- **StorefrontRazor (самый внешний)** — веб-интерфейс. Зависит от Core + Infrastructure.

**Ключевой принцип:** При смене БД (напр., SQL Server → PostgreSQL) меняется только Infrastructure — Core и UI остаются нетронутыми.

---

## 6. Docker Deployment Architecture (Архитектура развёртывания Docker)

### 🇬🇧 English
Shows the **physical deployment**: which containers run where.
- Developer machine has .NET 10 SDK + VS Code
- Docker Desktop runs 2 containers: SQL Server 2022 (port 1433) and Redis (port 6379)
- The .NET app connects to SQL via TCP/1433, Redis via TCP/6379
- External calls go to Azure OpenAI and Cloudinary via HTTPS

### 🇫🇷 Français
Montre le **déploiement physique** : quels conteneurs s'exécutent où.
- La machine du développeur a .NET 10 SDK + VS Code
- Docker Desktop exécute 2 conteneurs : SQL Server 2022 (port 1433) et Redis (port 6379)
- L'application .NET se connecte à SQL via TCP/1433, Redis via TCP/6379
- Les appels externes vont vers Azure OpenAI et Cloudinary via HTTPS

### 🇷🇺 Русский
Показывает **физическое развёртывание**: какие контейнеры где запускаются.
- Машина разработчика: .NET 10 SDK + VS Code
- Docker Desktop запускает 2 контейнера: SQL Server 2022 (порт 1433) и Redis (порт 6379)
- Приложение .NET подключается к SQL по TCP/1433, к Redis по TCP/6379
- Внешние вызовы идут в Azure OpenAI и Cloudinary по HTTPS

---

---

# UML DIAGRAMS

## 1. Class Diagram — Domain Layer (Core)

### 🇬🇧 English
Shows all the **data entities** (classes) in your system and their relationships:
- **Product** — has Id, Name, Price, Embedding (AI vector), Images, Reviews
- **UserInteraction** — records every click/view/purchase (links User ↔ Product)
- **RecommendationEvent** — records when a recommendation was shown/clicked
- **ABTestExperiment** — defines an experiment (control vs treatment strategy)
- **ABTestAssignment** — which user is in which group
- **Order** — customer order with items and delivery info

**Relationships:** Product has many UserInteractions, many RecommendationEvents. Experiment has many Assignments.

### 🇫🇷 Français
Montre toutes les **entités de données** (classes) et leurs relations :
- **Product** — Id, Nom, Prix, Embedding (vecteur IA), Images, Avis
- **UserInteraction** — enregistre chaque clic/vue/achat (lie Utilisateur ↔ Produit)
- **RecommendationEvent** — enregistre quand une recommandation a été montrée/cliquée
- **ABTestExperiment** — définit une expérience (stratégie contrôle vs traitement)
- **ABTestAssignment** — quel utilisateur est dans quel groupe
- **Order** — commande client avec articles et livraison

### 🇷🇺 Русский
Показывает все **сущности данных** (классы) системы и их связи:
- **Product** — Id, Имя, Цена, Embedding (ИИ-вектор), Изображения, Отзывы
- **UserInteraction** — записывает каждый клик/просмотр/покупку (связь Пользователь ↔ Товар)
- **RecommendationEvent** — записывает показ/клик по рекомендации
- **ABTestExperiment** — определяет эксперимент (контрольная vs экспериментальная стратегия)
- **ABTestAssignment** — какой пользователь в какой группе
- **Order** — заказ покупателя с товарами и доставкой

---

## 2. Class Diagram — Service Layer

### 🇬🇧 English
Shows the **business logic services** and what methods they provide:
- **AdaptiveRecommendationService** — main entry point, decides which strategy to use
- **AIRecommendationService** — calls Azure OpenAI for AI-powered recommendations
- **ProductEmbeddingService** — converts product text into 1536-dimensional vectors
- **ABTestService** — assigns users to groups, retrieves their strategy
- **RecommendationMetricsService** — calculates CTR, conversion rate per strategy
- **UserInteractionService** — saves and queries user behavior data

### 🇫🇷 Français
Montre les **services de logique métier** et leurs méthodes :
- **AdaptiveRecommendationService** — point d'entrée principal, décide quelle stratégie utiliser
- **AIRecommendationService** — appelle Azure OpenAI pour les recommandations IA
- **ProductEmbeddingService** — convertit le texte produit en vecteurs de dimension 1536
- **ABTestService** — assigne les utilisateurs aux groupes
- **RecommendationMetricsService** — calcule CTR, taux de conversion par stratégie
- **UserInteractionService** — sauvegarde le comportement utilisateur

### 🇷🇺 Русский
Показывает **сервисы бизнес-логики** и их методы:
- **AdaptiveRecommendationService** — главная точка входа, решает какую стратегию использовать
- **AIRecommendationService** — вызывает Azure OpenAI для ИИ-рекомендаций
- **ProductEmbeddingService** — конвертирует текст товара в 1536-мерный вектор
- **ABTestService** — назначает пользователей в группы, возвращает их стратегию
- **RecommendationMetricsService** — вычисляет CTR, конверсию по стратегии
- **UserInteractionService** — сохраняет и запрашивает данные о поведении пользователей

---

## 3. Component Diagram

### 🇬🇧 English
Shows **how the 3 .NET projects connect**:
- StorefrontRazor → depends on → Core (via interfaces)
- StorefrontRazor → depends on → Infrastructure (DI registration)
- Infrastructure → depends on → Core (implements interfaces)
- Core → depends on → nothing (independent)

This is the **Dependency Inversion Principle** in action.

### 🇫🇷 Français
Montre **comment les 3 projets .NET se connectent** :
- StorefrontRazor → dépend de → Core (via interfaces)
- StorefrontRazor → dépend de → Infrastructure (enregistrement DI)
- Infrastructure → dépend de → Core (implémente les interfaces)
- Core → ne dépend de → rien (indépendant)

C'est le **Principe d'Inversion de Dépendance** en action.

### 🇷🇺 Русский
Показывает **как 3 .NET-проекта связаны между собой**:
- StorefrontRazor → зависит от → Core (через интерфейсы)
- StorefrontRazor → зависит от → Infrastructure (регистрация DI)
- Infrastructure → зависит от → Core (реализует интерфейсы)
- Core → не зависит от → ничего (независимый)

Это **Принцип Инверсии Зависимостей** в действии.

---

## 4. Use Case Diagram

### 🇬🇧 English
Shows **what each actor can do** in the system:
- **Customer (Покупатель):** browse products, view recommendations, add to cart, checkout, write reviews, view order history
- **Administrator:** manage products, manage users, configure A/B tests, view metrics dashboard, set API keys
- **System (automatic):** track interactions, generate recommendations, assign A/B groups, calculate metrics

### 🇫🇷 Français
Montre **ce que chaque acteur peut faire** dans le système :
- **Client :** parcourir les produits, voir les recommandations, ajouter au panier, passer commande, écrire des avis
- **Administrateur :** gérer les produits, configurer les tests A/B, voir le tableau de bord des métriques
- **Système (automatique) :** suivre les interactions, générer les recommandations, assigner les groupes A/B

### 🇷🇺 Русский
Показывает **что каждый актор может делать** в системе:
- **Покупатель:** просматривать товары, видеть рекомендации, добавлять в корзину, оформлять заказ, писать отзывы
- **Администратор:** управлять товарами, настраивать A/B тесты, просматривать дашборд метрик, устанавливать API-ключи
- **Система (автоматически):** отслеживать взаимодействия, генерировать рекомендации, назначать группы A/B

---

## 5. Activity Diagram — Recommendation Generation

### 🇬🇧 English
Shows the **algorithm flow** step by step:
1. START → Get user ID
2. Check if user has interaction history
3. IF no history → Cold Start: use popularity-based
4. IF has history → Check active A/B experiment
5. Get assigned strategy from experiment
6. Execute strategy (collaborative filtering / content-based / AI / hybrid)
7. Filter out already-purchased products
8. Return Top-8 products
9. Record "Impression" event
10. END

### 🇫🇷 Français
Montre le **flux de l'algorithme** étape par étape :
1. DÉBUT → Obtenir l'ID utilisateur
2. Vérifier si l'utilisateur a un historique d'interactions
3. SI pas d'historique → Démarrage à froid : utiliser la popularité
4. SI historique → Vérifier l'expérience A/B active
5. Obtenir la stratégie assignée
6. Exécuter la stratégie (filtrage collaboratif / contenu / IA / hybride)
7. Filtrer les produits déjà achetés
8. Retourner Top-8 produits
9. Enregistrer l'événement "Impression"
10. FIN

### 🇷🇺 Русский
Показывает **поток алгоритма** пошагово:
1. НАЧАЛО → Получить ID пользователя
2. Проверить, есть ли история взаимодействий
3. ЕСЛИ нет → Холодный старт: использовать популярное
4. ЕСЛИ есть → Проверить активный A/B эксперимент
5. Получить назначенную стратегию
6. Выполнить стратегию (коллаборативная / контентная / ИИ / гибридная)
7. Отфильтровать уже купленные товары
8. Вернуть Top-8 товаров
9. Записать событие «Показ» (Impression)
10. КОНЕЦ

---

## 6. Activity Diagram — A/B Assignment Process

### 🇬🇧 English
Shows **how a user gets assigned to a test group**:
1. User arrives → Check if already assigned to this experiment
2. IF already assigned → return existing group
3. IF new → compute hash(userId + experimentId)
4. IF hash % 100 < trafficPercentage → Treatment group
5. ELSE → Control group
6. Save assignment to database
7. Return strategy name

**Key:** Deterministic! Same user always gets same group.

### 🇫🇷 Français
Montre **comment un utilisateur est assigné à un groupe de test** :
1. L'utilisateur arrive → Vérifier s'il est déjà assigné
2. SI déjà assigné → retourner le groupe existant
3. SI nouveau → calculer hash(userId + experimentId)
4. SI hash % 100 < pourcentageTrafic → Groupe Traitement
5. SINON → Groupe Contrôle
6. Sauvegarder l'assignation
7. Retourner le nom de la stratégie

**Clé :** Déterministe ! Le même utilisateur obtient toujours le même groupe.

### 🇷🇺 Русский
Показывает **как пользователь попадает в группу теста**:
1. Пользователь приходит → Проверить, назначен ли уже
2. ЕСЛИ уже назначен → вернуть существующую группу
3. ЕСЛИ новый → вычислить hash(userId + experimentId)
4. ЕСЛИ hash % 100 < процентТрафика → Группа «Эксперимент»
5. ИНАЧЕ → Группа «Контроль»
6. Сохранить назначение в БД
7. Вернуть название стратегии

**Ключ:** Детерминировано! Один пользователь всегда в одной группе.

---

## 7. State Diagram — Order Lifecycle

### 🇬🇧 English
Shows **all possible states of an order**:
- Pending → PaymentReceived → Processing → Shipped → Delivered
- Pending → PaymentFailed (terminal state)
- Any state → Cancelled (by admin)

For Cash on Delivery (COD): Pending → Processing → Shipped → Delivered → PaymentReceived

### 🇫🇷 Français
Montre **tous les états possibles d'une commande** :
- En attente → Paiement reçu → En traitement → Expédié → Livré
- En attente → Échec de paiement (état terminal)
- Tout état → Annulé (par admin)

### 🇷🇺 Русский
Показывает **все возможные состояния заказа**:
- Ожидание → Оплата получена → Обработка → Отправлен → Доставлен
- Ожидание → Ошибка оплаты (терминальное состояние)
- Любое → Отменён (администратором)

Для наложенного платежа (COD): Ожидание → Обработка → Отправлен → Доставлен → Оплата получена

---

## 8. State Diagram — Experiment Lifecycle

### 🇬🇧 English
Shows **states of an A/B experiment**:
- Draft → Active (started by admin)
- Active → Completed (manually stopped or reached end date)
- Active → Paused (temporarily stopped)
- Paused → Active (resumed)
- Completed → Archived

An experiment needs minimum data (usually 100+ events per group) to be statistically significant.

### 🇫🇷 Français
Montre les **états d'une expérience A/B** :
- Brouillon → Actif (démarré par admin)
- Actif → Terminé (arrêté manuellement ou date fin atteinte)
- Actif → En pause
- En pause → Actif (repris)
- Terminé → Archivé

### 🇷🇺 Русский
Показывает **состояния A/B эксперимента**:
- Черновик → Активный (запущен админом)
- Активный → Завершён (остановлен вручную или достигнута дата окончания)
- Активный → Приостановлен
- Приостановлен → Активный (возобновлён)
- Завершён → Архивирован

---

## 9. Sequence Diagram — Interaction Tracking

### 🇬🇧 English
Shows the **time sequence** of messages between components when a user clicks a product:
1. Browser → JavaScript: user clicks product
2. JavaScript → API: POST /api/interactions {productId, type: "Click"}
3. API → UserInteractionService: RecordInteraction()
4. Service → Database: INSERT INTO UserInteractions
5. Service → RecommendationMetricsService: InvalidateCache()
6. API → Browser: 200 OK

**Happens invisibly in background** — user doesn't see any delay.

### 🇫🇷 Français
Montre la **séquence temporelle** des messages quand un utilisateur clique sur un produit :
1. Navigateur → JavaScript : l'utilisateur clique
2. JavaScript → API : POST /api/interactions {productId, type: "Click"}
3. API → Service : RecordInteraction()
4. Service → Base de données : INSERT INTO UserInteractions
5. Service → Métriques : InvalidateCache()
6. API → Navigateur : 200 OK

**Se passe invisiblement en arrière-plan** — l'utilisateur ne voit aucun délai.

### 🇷🇺 Русский
Показывает **временную последовательность** сообщений при клике пользователя:
1. Браузер → JavaScript: пользователь кликает
2. JavaScript → API: POST /api/interactions {productId, type: "Click"}
3. API → UserInteractionService: RecordInteraction()
4. Сервис → БД: INSERT INTO UserInteractions
5. Сервис → RecommendationMetricsService: InvalidateCache()
6. API → Браузер: 200 OK

**Происходит незаметно в фоне** — пользователь не видит задержки.

---

## 10. Deployment Diagram

### 🇬🇧 English
Shows the **physical container topology**:
- Your machine runs .NET 10 app on port 5249
- Docker Desktop runs SQL Server (port 1433) and Redis (port 6379)
- HTTPS connections go to Azure OpenAI (embeddings) and Cloudinary (images)

### 🇫🇷 Français
Montre la **topologie physique des conteneurs** :
- Votre machine exécute l'app .NET 10 sur le port 5249
- Docker Desktop exécute SQL Server (port 1433) et Redis (port 6379)
- Connexions HTTPS vers Azure OpenAI et Cloudinary

### 🇷🇺 Русский
Показывает **физическую топологию контейнеров**:
- Ваша машина запускает .NET 10 приложение на порту 5249
- Docker Desktop запускает SQL Server (порт 1433) и Redis (порт 6379)
- HTTPS-соединения идут к Azure OpenAI и Cloudinary

---

---

# DFD DIAGRAMS

## 11. DFD Context Diagram (Level 0)

### 🇬🇧 English
A **Data Flow Diagram (DFD)** shows how DATA moves through the system. Level 0 is the highest level — the entire system is ONE circle.

External entities (boxes):
- **User** — sends: product views, clicks, purchases. Receives: personalized recommendations, product pages
- **Administrator** — sends: product management, A/B test settings. Receives: statistics, metrics, dashboard
- **Azure OpenAI** — receives: product text. Sends back: 1536-dimensional embedding vectors
- **Cloudinary** — receives: image uploads. Sends back: image URLs

The circle in the center = your ENTIRE system treated as a black box.

### 🇫🇷 Français
Un **Diagramme de Flux de Données (DFD)** montre comment les DONNÉES circulent. Le niveau 0 est le plus haut — tout le système est UN cercle.

Entités externes (rectangles) :
- **Utilisateur** — envoie : vues, clics, achats. Reçoit : recommandations personnalisées
- **Administrateur** — envoie : gestion produits, config A/B. Reçoit : statistiques, métriques
- **Azure OpenAI** — reçoit : texte produit. Renvoie : vecteurs d'embedding (1536-d)
- **Cloudinary** — reçoit : images. Renvoie : URLs

Le cercle au centre = votre système ENTIER traité comme une boîte noire.

### 🇷🇺 Русский
**DFD (Диаграмма Потоков Данных)** показывает, как ДАННЫЕ перемещаются через систему. Уровень 0 — самый высокий: вся система — ОДИН круг.

Внешние сущности (прямоугольники):
- **Пользователь** — отправляет: просмотры, клики, покупки. Получает: персонализированные рекомендации
- **Администратор** — отправляет: управление товарами, настройки A/B. Получает: статистику, метрики, дашборд
- **Azure OpenAI** — получает: текст товара. Возвращает: векторные эмбеддинги (1536-d)
- **Cloudinary** — получает: загрузку изображений. Возвращает: URL изображений

Круг в центре = ваша ВСЯ система как «чёрный ящик».

---

## 12. DFD Level 1 (Detailed Data Flows)

### 🇬🇧 English
Now we "open" the black box and show **6 internal processes**:

| # | Process | What it does |
|---|---------|-------------|
| 1 | Catalog Management | CRUD operations on products |
| 2 | Interaction Tracking | Records every user action (view, click, add-to-cart, purchase) |
| 3 | Recommendation Generation | The core algorithm — takes history + strategy → produces Top-N list |
| 4 | A/B Testing | Assigns users to groups, stores experiments |
| 5 | Metrics Calculation | Computes CTR, conversion rate, lift from raw events |
| 6 | Order Processing | Checkout, payment, order lifecycle |

**Data Stores** (cylinders):
- D1: Products (catalog)
- D2: UserInteractions (click/view history)
- D3: RecommendationEvents (impressions + clicks on recommendations)
- D4: ABTestExperiments (experiment definitions + assignments)
- D5: Orders (purchases)
- D6: Redis Cache (fast cached recommendations)

**The arrows** show WHAT data flows WHERE. Example: Process 2 writes to D2, and Process 3 reads FROM D2 to generate recommendations.

### 🇫🇷 Français
On "ouvre" la boîte noire et montre **6 processus internes** :

| # | Processus | Ce qu'il fait |
|---|-----------|--------------|
| 1 | Gestion du catalogue | Opérations CRUD sur les produits |
| 2 | Suivi des interactions | Enregistre chaque action utilisateur |
| 3 | Génération des recommandations | L'algorithme principal — historique + stratégie → liste Top-N |
| 4 | Tests A/B | Assigne les utilisateurs, stocke les expériences |
| 5 | Calcul des métriques | CTR, conversion, lift à partir des événements bruts |
| 6 | Traitement des commandes | Paiement, cycle de vie de la commande |

**Stockages de données** (cylindres) :
- D1 : Products — D2 : UserInteractions — D3 : RecommendationEvents
- D4 : ABTestExperiments — D5 : Orders — D6 : Redis Cache

**Les flèches** montrent QUELLES données vont OÙ.

### 🇷🇺 Русский
Теперь «открываем» чёрный ящик и показываем **6 внутренних процессов**:

| # | Процесс | Что делает |
|---|---------|-----------|
| 1 | Управление каталогом | CRUD-операции с товарами |
| 2 | Трекинг взаимодействий | Записывает каждое действие пользователя (просмотр, клик, корзина, покупка) |
| 3 | Генерация рекомендаций | Основной алгоритм — история + стратегия → список Top-N |
| 4 | A/B тестирование | Назначает группы, хранит эксперименты |
| 5 | Расчёт метрик | Вычисляет CTR, конверсию, лифт из сырых событий |
| 6 | Оформление заказа | Оплата, жизненный цикл заказа |

**Хранилища данных** (цилиндры):
- D1: Products (каталог)
- D2: UserInteractions (история кликов/просмотров)
- D3: RecommendationEvents (показы + клики по рекомендациям)
- D4: ABTestExperiments (определения экспериментов + назначения)
- D5: Orders (покупки)
- D6: Redis Cache (быстрый кэш рекомендаций)

**Стрелки** показывают КАКИЕ данные КУДА перетекают. Пример: Процесс 2 записывает в D2, а Процесс 3 читает ИЗ D2 для генерации рекомендаций.

---

---

# STATISTICAL GRAPHS

## 1. CTR Comparison by Strategy (bar chart)

### 🇬🇧 English
**CTR = Click-Through Rate = Clicks ÷ Impressions × 100%**

This bar chart compares CTR across all recommendation strategies:
- Popular: ~6%
- Random: ~4%
- Collaborative: ~10%
- Content-based: ~8%
- Hybrid: ~12%
- AI: ~15.84%

**What it proves:** AI and Hybrid strategies make users click MORE than random.

### 🇫🇷 Français
**CTR = Taux de Clics = Clics ÷ Impressions × 100%**

Compare le CTR entre toutes les stratégies. L'IA (15.84%) surpasse le Aléatoire (4%).

**Ce que ça prouve :** Les stratégies IA/Hybride font cliquer les utilisateurs PLUS que l'aléatoire.

### 🇷🇺 Русский
**CTR = Click-Through Rate = Клики ÷ Показы × 100%**

Сравнивает CTR по всем стратегиям. ИИ (15.84%) превосходит Случайное (4%).

**Что доказывает:** Стратегии ИИ/Гибридная заставляют пользователей кликать ЧАЩЕ, чем случайные рекомендации.

---

## 2. Conversion Comparison (bar chart)

### 🇬🇧 English
**Conversion Rate = Purchases ÷ Clicks × 100%**

Shows how many clicks actually turned into purchases for each strategy.

**Key insight:** High CTR doesn't always mean high conversion. Content-based may get fewer clicks but higher conversion (users click what they actually want to buy).

### 🇫🇷 Français
**Taux de Conversion = Achats ÷ Clics × 100%**

Montre combien de clics sont devenus des achats. Un CTR élevé ne signifie pas toujours une conversion élevée.

### 🇷🇺 Русский
**Конверсия = Покупки ÷ Клики × 100%**

Показывает, сколько кликов превратились в покупки. Высокий CTR не всегда означает высокую конверсию.

---

## 3. A/B Test: Control vs Experiment (grouped bar chart)

### 🇬🇧 English
Shows **side-by-side comparison** of the Control group (random/popular) vs Treatment group (AI/hybrid):
- CTR: Control 8.12% vs Treatment 15.84%
- Conversion: Control vs Treatment
- Revenue per user: Control vs Treatment

**This is the core proof** that the recommendation system works better than random.

### 🇫🇷 Français
Montre la **comparaison côte à côte** Contrôle (aléatoire) vs Traitement (IA) :
- CTR, Conversion, Revenu par utilisateur

**C'est la preuve principale** que le système de recommandation fonctionne mieux que l'aléatoire.

### 🇷🇺 Русский
Показывает **сравнение бок о бок** Контроля (случайное) vs Эксперимента (ИИ):
- CTR: Контроль 8.12% vs Эксперимент 15.84%
- Конверсия, Выручка на пользователя

**Это главное доказательство**, что рекомендательная система работает лучше случайных рекомендаций.

---

## 4. A/B Experiment Lifts (horizontal bar chart)

### 🇬🇧 English
**Lift = (Treatment - Control) / Control × 100%**

Shows the percentage IMPROVEMENT of the experimental group over the control:
- CTR Lift: +95% (almost double!)
- Conversion Lift: +XX%
- Revenue Lift: +XX%

**Positive lift = the experiment wins.** The larger the bar, the bigger the improvement.

### 🇫🇷 Français
**Lift = (Traitement - Contrôle) / Contrôle × 100%**

Montre le pourcentage d'AMÉLIORATION. Un lift positif = l'expérience gagne.

### 🇷🇺 Русский
**Лифт = (Эксперимент - Контроль) / Контроль × 100%**

Показывает процент УЛУЧШЕНИЯ экспериментальной группы. Положительный лифт = эксперимент выигрывает.

---

## 5. Conversion Funnel (control vs experiment)

### 🇬🇧 English
A **funnel chart** shows how users drop off at each stage:
1. Page views (100%) → 2. Recommendation impressions → 3. Clicks → 4. Add to cart → 5. Purchase

The funnel narrows at each step. The AI group has a **wider funnel** (fewer drop-offs) = better retention.

### 🇫🇷 Français
Un **graphique en entonnoir** montre comment les utilisateurs abandonnent à chaque étape. Le groupe IA a un entonnoir plus large = meilleure rétention.

### 🇷🇺 Русский
**Воронка** показывает, как пользователи «отваливаются» на каждом этапе:
1. Просмотры страниц (100%) → 2. Показы рекомендаций → 3. Клики → 4. Корзина → 5. Покупка

Группа ИИ имеет **более широкую воронку** (меньше потерь) = лучшее удержание.

---

## 6. Hybrid Model Weight Distribution (pie chart)

### 🇬🇧 English
Shows how the **hybrid strategy blends** multiple approaches:
- Collaborative filtering: 40%
- Content-based: 35%
- Popularity-based: 15%
- Random exploration: 10%

**Why blend?** No single strategy is best for ALL users. New users → popularity works. Active users → collaborative works. Blending covers all cases.

### 🇫🇷 Français
Montre comment la **stratégie hybride mélange** plusieurs approches :
- Filtrage collaboratif : 40% — Contenu : 35% — Popularité : 15% — Exploration : 10%

**Pourquoi mélanger ?** Aucune stratégie unique n'est la meilleure pour TOUS les utilisateurs.

### 🇷🇺 Русский
Показывает, как **гибридная стратегия комбинирует** несколько подходов:
- Коллаборативная фильтрация: 40%
- Контентная: 35%
- По популярности: 15%
- Случайное исследование: 10%

**Зачем смешивать?** Ни одна стратегия не лучшая для ВСЕХ пользователей. Новые → популярность. Активные → коллаборативная.

---

## 7. Interaction Type Weights (bar chart)

### 🇬🇧 English
Shows **how much each user action is "worth"** for the recommendation algorithm:
- View: 1.0 (base weight)
- Click: 2.0
- Add to cart: 3.0
- Purchase: 5.0
- Review: 4.0

**Meaning:** A purchase signals 5× stronger interest than a view. The algorithm weighs recent purchases heavily when deciding what to recommend next.

### 🇫🇷 Français
Montre **combien chaque action vaut** pour l'algorithme :
- Vue : 1.0 — Clic : 2.0 — Panier : 3.0 — Avis : 4.0 — Achat : 5.0

Un achat signale un intérêt 5× plus fort qu'une vue.

### 🇷🇺 Русский
Показывает **сколько каждое действие «весит»** для алгоритма:
- Просмотр: 1.0 — Клик: 2.0 — Корзина: 3.0 — Отзыв: 4.0 — Покупка: 5.0

Покупка сигнализирует в 5 раз более сильный интерес, чем просмотр.

---

## 8. Cold Start Handling (area chart)

### 🇬🇧 English
Shows how the system handles **new users with no history**:
- Day 1-3: 100% popularity-based (no personal data yet)
- Day 4-7: 70% popularity + 30% collaborative (some data)
- Day 8+: 20% popularity + 50% collaborative + 30% content-based (enough data)

The chart shows the **transition from generic to personalized** recommendations over time.

### 🇫🇷 Français
Montre comment le système gère les **nouveaux utilisateurs sans historique** :
- Jours 1-3 : 100% popularité — Jours 4-7 : 70% popularité + 30% collaboratif — Jour 8+ : personnalisé

Le graphique montre la **transition du générique au personnalisé**.

### 🇷🇺 Русский
Показывает, как система обрабатывает **новых пользователей без истории**:
- Дни 1-3: 100% по популярности (нет персональных данных)
- Дни 4-7: 70% популярность + 30% коллаборативная (немного данных)
- День 8+: 20% популярность + 50% коллаборативная + 30% контентная (достаточно данных)

График показывает **переход от общих к персонализированным** рекомендациям.

---

---

# QUICK REFERENCE — How to defend each diagram

| Diagram | Key phrase for defense |
|---------|----------------------|
| High-level architecture | "Shows separation of concerns across 5 architectural layers" |
| Recommendation flow | "Demonstrates the adaptive algorithm pipeline from request to response" |
| A/B testing flow | "Deterministic user assignment using hash function ensures statistical validity" |
| Offline evaluation | "Train/test split methodology allows safe strategy comparison" |
| Clean Architecture | "Dependency inversion principle — inner layers have zero external dependencies" |
| Docker deployment | "Containerized infrastructure enables reproducible development environment" |
| Class diagrams | "Domain entities and their relationships form the data model" |
| Component diagram | "Three-project structure with unidirectional dependencies" |
| Use Case | "Functional requirements from the perspective of each actor" |
| Activity (recommendations) | "Step-by-step algorithm with cold start handling" |
| Activity (A/B) | "Deterministic group assignment prevents selection bias" |
| State (order) | "Complete order lifecycle with all valid transitions" |
| State (experiment) | "Experiment governance — controlled start, stop, and archival" |
| Sequence (tracking) | "Asynchronous interaction capture without UX impact" |
| Deployment | "Infrastructure topology with ports and protocols" |
| DFD Level 0 | "System boundary — external entities and their data exchanges" |
| DFD Level 1 | "Internal process decomposition with 6 processes and 6 data stores" |
| CTR comparison | "AI strategy achieves 95% higher CTR vs random baseline" |
| Conversion comparison | "Revenue impact — clicks that convert to actual purchases" |
| A/B grouped bars | "Statistically significant difference between control and treatment" |
| Lift chart | "Quantified improvement: +95% CTR lift proves hypothesis" |
| Conversion funnel | "Drop-off analysis shows AI reduces user abandonment" |
| Hybrid weights | "Multi-strategy blending covers diverse user segments" |
| Interaction weights | "Implicit feedback signals weighted by purchase intent strength" |
| Cold start | "Graceful degradation for new users transitions to personalization" |
