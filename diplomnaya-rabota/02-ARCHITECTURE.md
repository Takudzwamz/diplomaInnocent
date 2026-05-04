# 02 — Архитектура системы

## Диаграмма верхнего уровня

```mermaid
graph TB
    subgraph "Клиентский слой"
        Browser[Веб-браузер]
    end

    subgraph "Слой представления — StorefrontRazor"
        RP[Razor Pages<br/>SSR + Bootstrap 5]
        JS[JavaScript<br/>Трекинг + AJAX]
        SH[SignalR Hub<br/>Уведомления в реальном времени]
    end

    subgraph "Слой приложения — Сервисы"
        ARS[AdaptiveRecommendation<br/>Service]
        AIRS[AIRecommendation<br/>Service]
        PES[ProductEmbedding<br/>Service]
        ABT[ABTest<br/>Service]
        MET[RecommendationMetrics<br/>Service]
        OFF[OfflineMetrics<br/>Service]
        UIS[UserInteraction<br/>Service]
    end

    subgraph "Доменный слой — Core"
        ENT[Сущности<br/>Product, UserInteraction,<br/>ABTestExperiment и др.]
        INT[Интерфейсы<br/>Контракты сервисов]
        SPEC[Спецификации<br/>Паттерны запросов]
    end

    subgraph "Слой данных — Infrastructure"
        EFC[Entity Framework Core 10]
        SEED[Сидеры данных]
    end

    subgraph "Внешние сервисы"
        AZURE[Azure OpenAI<br/>Эмбеддинги + GPT]
        CLOUD[Cloudinary<br/>Изображения]
        EMAIL[SendGrid<br/>Email]
        PAY[Paystack / PayFast<br/>Платежи]
    end

    subgraph "Хранилища данных"
        SQL[(SQL Server 2022<br/>Docker)]
        REDIS[(Redis<br/>Docker)]
    end

    Browser --> RP
    Browser --> JS
    RP --> ARS
    RP --> AIRS
    RP --> UIS
    JS --> UIS
    ARS --> ABT
    ARS --> MET
    ARS --> UIS
    AIRS --> PES
    PES --> AZURE
    ARS --> EFC
    AIRS --> EFC
    UIS --> EFC
    ABT --> EFC
    MET --> EFC
    OFF --> EFC
    EFC --> SQL
    RP --> REDIS
    RP --> SH
    PES --> AZURE
    RP --> CLOUD
    RP --> EMAIL
    RP --> PAY
```

---

## Поток работы рекомендательной системы

```mermaid
sequenceDiagram
    participant U as Пользователь (Браузер)
    participant P as Страница товара
    participant ARS as AdaptiveRecommendation<br/>Service
    participant ABT as ABTestService
    participant AI as AIRecommendation<br/>Service
    participant UIS as UserInteraction<br/>Service
    participant DB as SQL Server
    participant MET as MetricsService

    U->>P: Посещает страницу товара
    P->>UIS: TrackInteraction(userId, productId, View)
    UIS->>DB: INSERT UserInteraction

    P->>ABT: GetUserStrategy(userId)
    ABT->>DB: Проверить ABTestAssignment
    ABT-->>P: Strategy = Adaptive

    P->>ARS: GetRecommendations(userId, Adaptive, productId)
    ARS->>UIS: GetUserTopProducts(userId)
    UIS->>DB: SELECT взвешенных взаимодействий
    UIS-->>ARS: [productId1, productId2, ...]

    ARS->>DB: Получить оценки CF (похожие пользователи)
    ARS->>AI: GetSimilarProducts(productId) [Контентная]
    AI->>DB: Загрузить эмбеддинги
    AI-->>ARS: Оценки косинусного сходства

    ARS->>ARS: Объединить: CF(0.4) + CB(0.35) + Trend(0.15) + Recency(0.1)
    ARS-->>P: Топ-8 рекомендованных товаров

    P->>MET: RecordImpression(userId, productId, strategy, position)
    MET->>DB: INSERT RecommendationEvent

    P-->>U: Отрисовка страницы с рекомендациями

    U->>P: Клик на рекомендацию
    P->>MET: RecordClick(userId, productId, strategy, position)
    MET->>DB: INSERT RecommendationEvent(Click)
```

---

## Поток A/B тестирования

```mermaid
sequenceDiagram
    participant Admin as Админ-панель
    participant ABT as ABTestService
    participant DB as SQL Server
    participant User as Новый пользователь
    participant ARS as AdaptiveRecommendation<br/>Service
    participant MET as MetricsService

    Admin->>ABT: CreateExperiment(Popular vs Adaptive, 50%)
    ABT->>DB: Деактивировать предыдущие эксперименты
    ABT->>DB: INSERT ABTestExperiment(IsActive=true)

    User->>ABT: GetUserStrategy(userId)
    ABT->>DB: Проверить существующее назначение
    Note over ABT: Назначение не найдено
    ABT->>ABT: hash = DeterministicHash(userId, expId)
    ABT->>ABT: isTreatment = (hash % 100) < 50
    ABT->>DB: INSERT ABTestAssignment
    ABT-->>User: Strategy = Popular (Контроль)

    User->>ARS: GetRecommendations(userId, Popular)
    ARS-->>User: Популярные товары

    Note over MET: После периода эксперимента...
    Admin->>MET: GetExperimentMetrics(experimentId)
    MET->>DB: Агрегация RecommendationEvents по группам
    MET->>MET: Расчёт CTR, Конверсии, AOV
    MET->>MET: z-тест для пропорций, t-тест для средних
    MET-->>Admin: ExperimentMetrics { лифты, p-значения }
```

---

## Поток Offline-оценки

```mermaid
flowchart TD
    A[Админ запускает оценку] --> B[Загрузка взаимодействий за период]
    B --> C[Временное разделение: 80% train / 20% test]
    C --> D[Для каждой стратегии...]

    D --> E1[Стратегия Popular]
    D --> E2[Коллаборативная фильтрация]
    D --> E3[Контентная]
    D --> E4[Гибридная адаптивная]

    E1 --> F[Для каждого тестового пользователя: сгенерировать Top-K рекомендаций]
    E2 --> F
    E3 --> F
    E4 --> F

    F --> G[Сравнить рекомендации с покупками из тестового набора]
    G --> H[Рассчитать Precision@K]
    G --> I[Рассчитать Recall@K]
    G --> J[Рассчитать NDCG@K]
    G --> K[Рассчитать Coverage]
    G --> L[Рассчитать MRR]

    H --> M[Вернуть StrategyOfflineMetrics для каждой стратегии]
    I --> M
    J --> M
    K --> M
    L --> M

    M --> N[Отобразить таблицу сравнения в админ UI]
```

---

## Слои Clean Architecture

```mermaid
graph LR
    subgraph Core["Core (Доменный слой)"]
        direction TB
        E[Сущности]
        I[Интерфейсы]
        S[Спецификации]
    end

    subgraph Infra["Infrastructure (Слой данных)"]
        direction TB
        D[DbContext + Миграции]
        SV[Реализации сервисов]
        C[Конфигурации сущностей]
    end

    subgraph Web["StorefrontRazor (Слой представления)"]
        direction TB
        P[Razor Pages]
        W[wwwroot / JS]
        PR[Program.cs / DI]
    end

    Web --> Core
    Web --> Infra
    Infra --> Core
```

**Правило зависимостей:** Внутренние слои ничего не знают о внешних слоях. Core не имеет зависимостей от Infrastructure или StorefrontRazor.

---

## Архитектура развёртывания (Docker)

```mermaid
graph LR
    subgraph Docker["Docker Compose"]
        SQL[SQL Server 2022<br/>Порт: 1433]
        Redis[Redis<br/>Порт: 6379]
    end

    subgraph Host["Хост-машина"]
        App[StorefrontRazor<br/>dotnet run<br/>Порт: 5001/5000]
    end

    subgraph Cloud["Облако Azure"]
        AOI[Azure OpenAI<br/>API эмбеддингов]
        CDN[Cloudinary CDN]
        SG[SendGrid SMTP]
    end

    App --> SQL
    App --> Redis
    App --> AOI
    App --> CDN
    App --> SG
```

---

## Используемые паттерны проектирования

| Паттерн | Где используется | Назначение |
|---------|-------|---------|
| **Clean Architecture** | Структура проекта | Разделение ответственности, тестируемость |
| **Repository / DbContext** | `StoreContext` | Абстракция доступа к данным |
| **Specification** | `Core/Specifications/` | Переиспользуемые, составные запросы |
| **Strategy** | `RecommendationStrategy` enum | Переключение между алгоритмами рекомендаций |
| **Dependency Injection** | `Program.cs` | Слабое связывание, тестируемость |
| **Observer** | `IntersectionObserver` (JS) | Отслеживание показов рекомендаций |
| **Deterministic Hashing** | `ABTestService` | Устойчивое назначение пользователя в группу |
| **Decorator** | Атрибут `[Cached]` | Прозрачное кэширование ответов |
