# 09 — UML-диаграммы

> Все диаграммы выполнены в формате Mermaid и могут быть отрисованы в любом Markdown-редакторе, VS Code, или экспортированы в PNG/SVG через [mermaid.live](https://mermaid.live).

---

## 1. Диаграмма классов — Доменный слой (Core)

```mermaid
classDiagram
    class Product {
        +int Id
        +string Name
        +string Description
        +decimal Price
        +string PictureUrl
        +string Type
        +string Brand
        +int QuantityInStock
        +float[] Embedding
        +DateTime DateAdded
        +List~ProductImage~ Images
        +List~Review~ Reviews
    }

    class UserInteraction {
        +int Id
        +string UserId
        +int ProductId
        +InteractionType InteractionType
        +DateTime InteractionDate
        +string Metadata
    }

    class RecommendationEvent {
        +int Id
        +string UserId
        +int ProductId
        +RecommendationEventType EventType
        +string Strategy
        +int Position
        +DateTime EventDate
        +string SessionId
    }

    class ABTestExperiment {
        +int Id
        +string Name
        +string ControlStrategy
        +string TreatmentStrategy
        +int TrafficPercentage
        +bool IsActive
        +DateTime StartDate
        +DateTime? EndDate
        +List~ABTestAssignment~ Assignments
    }

    class ABTestAssignment {
        +int Id
        +string UserId
        +int ExperimentId
        +bool IsInTreatmentGroup
        +DateTime AssignedDate
    }

    class AppUser {
        +string Id
        +string DisplayName
        +string Email
        +Address Address
    }

    class Order {
        +int Id
        +string BuyerEmail
        +DateTime OrderDate
        +Address ShipToAddress
        +DeliveryMethod DeliveryMethod
        +List~OrderItem~ OrderItems
        +decimal Subtotal
        +OrderStatus Status
    }

    class OrderItem {
        +int Id
        +ProductItemOrdered ItemOrdered
        +decimal Price
        +int Quantity
    }

    %% Перечисления
    class InteractionType {
        <<enumeration>>
        View = 0
        Click = 1
        AddToCart = 2
        Purchase = 3
        Wishlist = 4
        RecommendationClick = 5
    }

    class RecommendationEventType {
        <<enumeration>>
        Impression = 0
        Click = 1
        AddToCart = 2
        Purchase = 3
    }

    class RecommendationStrategy {
        <<enumeration>>
        Popular
        CollaborativeFiltering
        ContentBased
        Adaptive
    }

    %% Связи
    Product "1" --> "*" UserInteraction : productId
    Product "1" --> "*" RecommendationEvent : productId
    AppUser "1" --> "*" UserInteraction : userId
    AppUser "1" --> "*" RecommendationEvent : userId
    AppUser "1" --> "*" ABTestAssignment : userId
    AppUser "1" --> "*" Order : buyerEmail
    ABTestExperiment "1" --> "*" ABTestAssignment : experimentId
    Order "1" --> "*" OrderItem : orderId
    UserInteraction --> InteractionType
    RecommendationEvent --> RecommendationEventType
```

---

## 2. Диаграмма классов — Сервисный слой

```mermaid
classDiagram
    class IAdaptiveRecommendationService {
        <<interface>>
        +GetRecommendations(userId, strategy, productId, count) Task~List~Product~~
        +GetUserStrategy(userId) Task~RecommendationStrategy~
    }

    class IAIRecommendationService {
        <<interface>>
        +GetSimilarProducts(productId, count) Task~List~ScoredProduct~~
        +ComputeCosineSimilarity(vectorA, vectorB) double
    }

    class IProductEmbeddingService {
        <<interface>>
        +GenerateEmbeddings() Task
        +GetEmbedding(product) Task~float[]~
    }

    class IABTestService {
        <<interface>>
        +GetUserStrategy(userId) Task~RecommendationStrategy~
        +CreateExperiment(name, control, treatment, traffic) Task~ABTestExperiment~
        +EndExperiment(experimentId) Task
    }

    class IRecommendationMetricsService {
        <<interface>>
        +RecordImpression(userId, productId, strategy, position) Task
        +RecordClick(userId, productId, strategy, position) Task
        +GetExperimentMetrics(experimentId) Task~ExperimentMetrics~
    }

    class IOfflineMetricsService {
        <<interface>>
        +EvaluateStrategy(strategy, k) Task~StrategyOfflineMetrics~
        +EvaluateAllStrategies(k) Task~List~StrategyOfflineMetrics~~
    }

    class IUserInteractionService {
        <<interface>>
        +TrackInteraction(userId, productId, type) Task
        +GetUserTopProducts(userId, count) Task~List~int~~
        +GetInteractionCount(userId) Task~int~
    }

    class AdaptiveRecommendationService {
        -StoreContext _context
        -IAIRecommendationService _aiService
        -IABTestService _abTestService
        -IUserInteractionService _interactionService
        +GetRecommendations(...) Task~List~Product~~
        -GetPopularRecommendations(count) Task~List~Product~~
        -GetCFRecommendations(userId, count) Task~List~Product~~
        -GetHybridRecommendations(userId, productId, count) Task~List~Product~~
        -NormalizeScores(scores) Dictionary~int, double~
    }

    class AIRecommendationService {
        -StoreContext _context
        +GetSimilarProducts(productId, count) Task~List~ScoredProduct~~
        +ComputeCosineSimilarity(vectorA, vectorB) double
    }

    class ABTestService {
        -StoreContext _context
        +GetUserStrategy(userId) Task~RecommendationStrategy~
        +CreateExperiment(...) Task~ABTestExperiment~
        -DeterministicHash(userId, experimentId) int
    }

    IAdaptiveRecommendationService <|.. AdaptiveRecommendationService
    IAIRecommendationService <|.. AIRecommendationService
    IABTestService <|.. ABTestService
    AdaptiveRecommendationService --> IAIRecommendationService
    AdaptiveRecommendationService --> IABTestService
    AdaptiveRecommendationService --> IUserInteractionService
```

---

## 3. Диаграмма компонентов

```mermaid
graph TB
    subgraph "StorefrontRazor (Представление)"
        Pages["Razor Pages<br/>(Index, Product, Cart, Admin)"]
        JS["JavaScript<br/>(tracking.js, recommendations.js)"]
        Hub["SignalR Hub<br/>(NotificationHub)"]
    end

    subgraph "Infrastructure (Сервисы)"
        ARS["AdaptiveRecommendation<br/>Service"]
        AIRS["AIRecommendation<br/>Service"]
        PES["ProductEmbedding<br/>Service"]
        ABT["ABTest<br/>Service"]
        MET["RecommendationMetrics<br/>Service"]
        OFF["OfflineMetrics<br/>Service"]
        UIS["UserInteraction<br/>Service"]
        SEED["RecommendationData<br/>Seeder"]
    end

    subgraph "Core (Домен)"
        Entities["Сущности"]
        Interfaces["Интерфейсы"]
        Specs["Спецификации"]
    end

    subgraph "Внешние сервисы"
        AzureOAI["Azure OpenAI API"]
        Cloudinary["Cloudinary CDN"]
    end

    subgraph "Хранилища"
        SQLDB[("SQL Server 2022")]
        RedisDB[("Redis")]
    end

    Pages --> ARS
    Pages --> UIS
    JS --> UIS
    ARS --> AIRS
    ARS --> ABT
    ARS --> UIS
    AIRS --> PES
    PES --> AzureOAI
    ARS --> MET
    
    ARS --> SQLDB
    AIRS --> SQLDB
    UIS --> SQLDB
    ABT --> SQLDB
    MET --> SQLDB
    OFF --> SQLDB
    SEED --> SQLDB
    
    Pages --> RedisDB
    Pages --> Cloudinary
```

---

## 4. Диаграмма вариантов использования (Use Case)

```mermaid
graph LR
    subgraph Actors["Актёры"]
        User((Покупатель))
        Admin((Администратор))
        System((Система))
    end

    subgraph UC_Shopping["Покупки"]
        UC1[Просмотр каталога]
        UC2[Просмотр товара]
        UC3[Добавление в корзину]
        UC4[Оформление заказа]
        UC5[Просмотр рекомендаций]
        UC6[Клик по рекомендации]
    end

    subgraph UC_Recommendations["Рекомендации"]
        UC7[Генерация персональных рекомендаций]
        UC8[Выбор стратегии через A/B тест]
        UC9[Расчёт косинусного сходства]
        UC10[Обработка холодного старта]
    end

    subgraph UC_Admin["Администрирование"]
        UC11[Создание A/B эксперимента]
        UC12[Просмотр метрик эксперимента]
        UC13[Завершение эксперимента]
        UC14[Запуск Offline-оценки]
        UC15[Управление товарами]
    end

    subgraph UC_System["Системные"]
        UC16[Генерация эмбеддингов при старте]
        UC17[Сидирование данных]
        UC18[Трекинг взаимодействий]
        UC19[Запись событий рекомендаций]
    end

    User --> UC1
    User --> UC2
    User --> UC3
    User --> UC4
    User --> UC5
    User --> UC6

    UC5 --> UC7
    UC7 --> UC8
    UC7 --> UC9
    UC7 --> UC10

    Admin --> UC11
    Admin --> UC12
    Admin --> UC13
    Admin --> UC14
    Admin --> UC15

    System --> UC16
    System --> UC17
    System --> UC18
    System --> UC19

    UC2 --> UC18
    UC6 --> UC19
```

---

## 5. Диаграмма активности — Генерация рекомендаций

```mermaid
flowchart TD
    Start([Запрос рекомендаций]) --> A{Получить стратегию<br/>через A/B тест}
    
    A -->|Popular| B[Подсчитать взвешенную<br/>популярность за 30 дней]
    B --> B1[Сортировка по убыванию]
    B1 --> Return

    A -->|CollaborativeFiltering| C[Найти соседей пользователя]
    C --> C1[Получить товары соседей<br/>не виденные текущим пользователем]
    C1 --> C2[Подсчитать CFScore<br/>для каждого кандидата]
    C2 --> Return

    A -->|ContentBased| D[Загрузить эмбеддинг<br/>текущего товара]
    D --> D1[Загрузить все эмбеддинги<br/>параллельно]
    D1 --> D2[Вычислить косинусное<br/>сходство для каждого]
    D2 --> D3{similarity >= 0.1?}
    D3 -->|Да| D4[Добавить в кандидаты]
    D3 -->|Нет| D5[Отбросить]
    D4 --> Return
    D5 --> Return

    A -->|Adaptive| E{Количество<br/>взаимодействий?}
    E -->|0| F[100% Popular]
    E -->|1-2| G[70% Popular +<br/>30% Content-Based]
    E -->|≥3| H[Полная гибридная формула]
    
    H --> H1[Получить CF оценки]
    H --> H2[Получить CB оценки]
    H --> H3[Рассчитать Trending]
    H --> H4[Рассчитать Recency]
    H1 --> H5[Нормализация 0..1]
    H2 --> H5
    H3 --> H5
    H4 --> H5
    H5 --> H6["Score = 0.40·CF + 0.35·CB<br/>+ 0.15·Trend + 0.10·Recency"]
    H6 --> Return

    F --> Return
    G --> Return

    Return([Вернуть Top-K товаров])
```

---

## 6. Диаграмма активности — Процесс A/B назначения

```mermaid
flowchart TD
    Start([Пользователь запрашивает<br/>рекомендации]) --> A{Есть активный<br/>эксперимент?}
    
    A -->|Нет| B[Вернуть стратегию<br/>по умолчанию: Popular]
    
    A -->|Да| C{Существует назначение<br/>для этого пользователя?}
    
    C -->|Да| D[Вернуть сохранённую<br/>стратегию]
    
    C -->|Нет| E["Вычислить hash =<br/>DeterministicHash(userId, expId)"]
    E --> F["bucket = hash mod 100"]
    F --> G{bucket < trafficPct?}
    
    G -->|Да| H[Назначить в Treatment<br/>IsInTreatmentGroup = true]
    G -->|Нет| I[Назначить в Control<br/>IsInTreatmentGroup = false]
    
    H --> J[Сохранить ABTestAssignment<br/>в базу данных]
    I --> J
    J --> D
    
    B --> End([Конец])
    D --> End
```

---

## 7. Диаграмма состояний — Жизненный цикл заказа

```mermaid
stateDiagram-v2
    [*] --> Pending: Заказ создан
    Pending --> PaymentReceived: Оплата подтверждена
    Pending --> PaymentFailed: Ошибка оплаты
    PaymentReceived --> ReadyForShipping: Обработан складом
    ReadyForShipping --> Shipped: Передан курьеру
    Shipped --> Delivered: Доставлен
    PaymentFailed --> [*]: Отменён
    Delivered --> [*]: Завершён

    note right of Pending
        Создаётся при оформлении.
        Ожидает подтверждения платежа.
    end note

    note right of PaymentReceived
        Взаимодействие отслеживается
        как Purchase (вес = 5)
    end note
```

---

## 8. Диаграмма состояний — Жизненный цикл эксперимента

```mermaid
stateDiagram-v2
    [*] --> Created: Админ создаёт
    Created --> Active: Активация (IsActive=true)
    Active --> Active: Новые назначения пользователей
    Active --> Ended: Админ завершает / дата окончания
    Ended --> Analyzed: Расчёт метрик завершён
    Analyzed --> DecisionMade: Выбрана лучшая стратегия
    DecisionMade --> [*]

    note right of Active
        Каждый новый пользователь
        получает назначение
        через DeterministicHash
    end note

    note right of Analyzed
        z-тест (пропорции)
        t-тест Уэлча (средние)
        α = 0.05
    end note
```

---

## 9. Диаграмма последовательности — Трекинг взаимодействия

```mermaid
sequenceDiagram
    participant B as Браузер (JS)
    participant P as Razor Page
    participant UIS as UserInteractionService
    participant MET as MetricsService
    participant DB as SQL Server

    B->>P: POST /track-interaction<br/>{productId, type: "Click"}
    P->>UIS: TrackInteraction(userId, productId, Click)
    UIS->>DB: INSERT INTO UserInteractions<br/>(UserId, ProductId, InteractionType, Date)
    DB-->>UIS: OK
    UIS-->>P: Success

    Note over B,P: Если клик по рекомендации:
    B->>P: POST /track-recommendation-click<br/>{productId, strategy, position}
    P->>MET: RecordClick(userId, productId, strategy, position)
    MET->>DB: INSERT INTO RecommendationEvents<br/>(Type=Click, Strategy, Position)
    DB-->>MET: OK
    MET-->>P: Success
```

---

## 10. Диаграмма развёртывания (Deployment)

```mermaid
graph TB
    subgraph DevMachine["Машина разработчика"]
        VS[VS Code / Rider]
        DotNet[".NET 10 SDK"]
        Browser["Браузер"]
    end

    subgraph DockerHost["Docker Desktop"]
        subgraph SQLContainer["Контейнер SQL Server"]
            SQL[SQL Server 2022<br/>mcr.microsoft.com/mssql/server:2022-latest<br/>Порт: 1433]
        end
        subgraph RedisContainer["Контейнер Redis"]
            Redis[Redis 7<br/>redis:latest<br/>Порт: 6379]
        end
    end

    subgraph AzureCloud["Облако Azure"]
        AOAI[Azure OpenAI<br/>text-embedding-ada-002<br/>HTTPS/443]
    end

    subgraph CDN["Cloudinary CDN"]
        Images[Хранение изображений<br/>HTTPS/443]
    end

    DotNet -->|"TCP/1433<br/>ADO.NET"| SQL
    DotNet -->|"TCP/6379<br/>StackExchange.Redis"| Redis
    DotNet -->|"HTTPS<br/>Azure.AI.OpenAI SDK"| AOAI
    DotNet -->|"HTTPS<br/>CloudinaryDotNet SDK"| Images
    Browser -->|"HTTPS/5001<br/>HTTP/5000"| DotNet
```

---

## 11. DFD — Контекстная диаграмма (Уровень 0)

```mermaid
graph LR
    %% External Entities (rectangles)
    User["👤 Пользователь<br/>(Покупатель)"]
    Admin["👔 Администратор"]
    AzureAI["☁️ Azure OpenAI<br/>(Внешний сервис)"]
    Cloudinary["☁️ Cloudinary<br/>(CDN изображений)"]

    %% Central Process (rounded)
    System(("0<br/>Система интернет-магазина<br/>с рекомендациями"))

    %% Data Flows
    User -->|"Просмотр товаров,<br/>клики, покупки"| System
    System -->|"Персонализированные<br/>рекомендации,<br/>страницы товаров"| User

    Admin -->|"Управление товарами,<br/>настройка A/B тестов,<br/>API-ключи"| System
    System -->|"Статистика, метрики,<br/>дашборд, отчёты"| Admin

    System -->|"Текст товара<br/>для эмбеддинга"| AzureAI
    AzureAI -->|"Векторные<br/>эмбеддинги (1536-d)"| System

    System -->|"Загрузка<br/>изображений"| Cloudinary
    Cloudinary -->|"URL<br/>изображений"| System
```

---

## 12. DFD — Диаграмма потоков данных (Уровень 1)

```mermaid
graph TB
    %% External Entities
    User["👤 Пользователь"]
    Admin["👔 Администратор"]
    AzureAI["☁️ Azure OpenAI"]

    %% Processes
    P1(("1<br/>Управление<br/>каталогом"))
    P2(("2<br/>Трекинг<br/>взаимодействий"))
    P3(("3<br/>Генерация<br/>рекомендаций"))
    P4(("4<br/>A/B<br/>тестирование"))
    P5(("5<br/>Расчёт<br/>метрик"))
    P6(("6<br/>Оформление<br/>заказа"))

    %% Data Stores
    DS1[("D1 Products<br/>Каталог товаров")]
    DS2[("D2 UserInteractions<br/>Взаимодействия")]
    DS3[("D3 RecommendationEvents<br/>События рекомендаций")]
    DS4[("D4 ABTestExperiments<br/>Эксперименты")]
    DS5[("D5 Orders<br/>Заказы")]
    DS6[("D6 Redis Cache<br/>Кэш")]

    %% === Flows: Пользователь ===
    User -->|"Запрос страницы<br/>товара"| P1
    P1 -->|"Данные товара,<br/>изображения, цена"| User
    User -->|"Клик, просмотр,<br/>добавление в корзину"| P2
    User -->|"Оформление заказа"| P6
    P3 -->|"Список рекомендованных<br/>товаров (Top-N)"| User
    P6 -->|"Подтверждение<br/>заказа"| User

    %% === Flows: Администратор ===
    Admin -->|"CRUD товаров,<br/>настройки"| P1
    Admin -->|"Создание/остановка<br/>экспериментов"| P4
    P5 -->|"CTR, конверсия,<br/>лифт A/B"| Admin

    %% === Flows: Внешние сервисы ===
    P3 -->|"Текст для<br/>эмбеддинга"| AzureAI
    AzureAI -->|"Вектор float[1536]"| P3

    %% === Flows: Процессы ↔ Хранилища ===
    P1 -->|"Сохранение товара"| DS1
    DS1 -->|"Список товаров"| P1
    DS1 -->|"Каталог для<br/>ранжирования"| P3

    P2 -->|"Запись события"| DS2
    DS2 -->|"История действий<br/>пользователя"| P3

    P3 -->|"Запись показа,<br/>клика"| DS3
    DS3 -->|"Данные для<br/>расчёта метрик"| P5

    P4 -->|"Назначение группы"| DS4
    DS4 -->|"Стратегия<br/>пользователя"| P3

    P6 -->|"Сохранение заказа"| DS5
    DS5 -->|"Покупки для<br/>конверсии"| P5

    P3 -->|"Кэшированные<br/>рекомендации"| DS6
    DS6 -->|"Быстрый ответ<br/>из кэша"| P3
```

---

## Как использовать в дипломной работе

### Экспорт в изображения:

1. **VS Code**: установить расширение «Markdown Preview Mermaid Support»
2. **Онлайн**: вставить код в [mermaid.live](https://mermaid.live) → Export PNG/SVG
3. **CLI**: `npx @mermaid-js/mermaid-cli mmdc -i diagram.mmd -o diagram.png -w 1200`

### Рекомендации по размещению в тексте:

| Диаграмма | Глава |
|-----------|-------|
| Диаграмма классов (домен) | Глава 2 — Проектирование системы |
| Диаграмма классов (сервисы) | Глава 2 — Проектирование системы |
| Диаграмма компонентов | Глава 2 — Архитектура |
| Варианты использования | Глава 2 — Функциональные требования |
| Активности (рекомендации) | Глава 3 — Алгоритмы |
| Активности (A/B назначение) | Глава 3 — A/B тестирование |
| Состояния (заказ) | Глава 2 — Бизнес-логика |
| Состояния (эксперимент) | Глава 3 — Методология оценки |
| Последовательность (трекинг) | Глава 3 — Сбор данных |
| Развёртывание | Глава 2 — Инфраструктура |
| DFD контекстная (уровень 0) | Глава 2 — Общее описание системы |
| DFD потоков данных (уровень 1) | Глава 2 — Архитектура / Глава 3 — Данные |
