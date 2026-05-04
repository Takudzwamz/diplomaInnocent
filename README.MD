# Адаптивная система рекомендаций для интернет-магазина

> Магистерская диссертация: «Разработка адаптивной системы рекомендаций для интернет-магазина и оценка её влияния на эффективность»

## Описание проекта

Полнофункциональная платформа электронной коммерции с интегрированной адаптивной рекомендательной системой, механизмом A/B тестирования и комплексной оценкой качества рекомендаций.

### Ключевые возможности

- **4 стратегии рекомендаций:** Popular, Collaborative Filtering, Content-Based, Adaptive (гибридная)
- **Гибридная формула:** `Score = 0.40·CF + 0.35·CB + 0.15·Trending + 0.10·Recency`
- **A/B тестирование:** детерминированное назначение в группы с помощью хеш-функции
- **Решение холодного старта:** каскадная стратегия (0 → 1–2 → 3+ взаимодействий)
- **Offline-метрики:** Precision@K, Recall@K, NDCG@K, MRR, Coverage
- **Статистические тесты:** z-тест для пропорций, t-тест Уэлча для средних

## Технологический стек

| Компонент | Технология |
|-----------|-----------|
| Backend | .NET 10, C# 13, ASP.NET Core |
| Frontend | Razor Pages (SSR) + Bootstrap 5 + JavaScript |
| База данных | SQL Server 2022 (Docker) |
| Кэширование | Redis (Docker) |
| AI/Эмбеддинги | Azure OpenAI (text-embedding-ada-002, 1536 dim) |
| Реальное время | SignalR |
| Архитектура | Clean Architecture (Core → Infrastructure → StorefrontRazor) |

## Структура проекта

```
├── Core/                  # Доменные сущности, интерфейсы, спецификации
├── Infrastructure/        # EF Core, миграции, сервисы, конфигурация
├── StorefrontRazor/       # Веб-приложение (Razor Pages)
├── API/                   # REST API (для внешних интеграций)
├── diplomnaya-rabota/     # Документация к диссертации
│   ├── graphs/            # UML и графики (PNG)
│   └── *.md              # Главы документации
├── docker-compose.yml     # SQL Server + Redis
└── skinet.sln            # Solution файл
```

## Запуск проекта

### Предварительные требования

- [Docker](https://www.docker.com/) (для SQL Server и Redis)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git

### 1. Клонирование репозитория

```bash
git clone https://github.com/Takudzwamz/diplomaInnocent.git
cd diplomaInnocent
```

### 2. Запуск инфраструктуры (Docker)

```bash
docker compose up -d
```

Это запустит:
- **SQL Server 2022** на порту `1433` (SA / Password@1)
- **Redis** на порту `6379`

### 3. Настройка конфигурации

Создайте файл `StorefrontRazor/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=storefront;User Id=sa;Password=Password@1;TrustServerCertificate=True;",
    "Redis": "localhost:6379,abortConnect=False"
  },
  "AzureOpenAI": {
    "Enabled": false,
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "ApiKey": "YOUR_KEY_HERE",
    "EmbeddingDeployment": "text-embedding-ada-002",
    "ChatDeployment": "gpt-4o-mini"
  },
  "AllowedHosts": "*"
}
```

> **Примечание:** Для работы Content-Based рекомендаций необходимо настроить Azure OpenAI. Без него система будет использовать остальные стратегии.

### 4. Запуск приложения

```bash
cd StorefrontRazor
dotnet run
```

Приложение будет доступно по адресу: **http://localhost:5106**

### 5. Оплата — Cash On Delivery (наложенный платёж)

По умолчанию используется метод **«Оплата при получении»** (Cash On Delivery). При оформлении заказа:
- Оплата не требуется — заказ подтверждается мгновенно
- Не нужны внешние платёжные шлюзы (PayFast/Paystack)
- Не нужен ngrok или публичный URL
- Работает полностью на localhost

> **Для переключения на PayFast/Paystack** (онлайн-оплата): измените `Payment_ActiveGateway` в таблице `SiteSettings` на `"PayFast"` или `"Paystack"`. В этом случае понадобится ngrok для получения ITN-уведомлений.

### 6. Автоматическое заполнение данных (Seeding)

При первом запуске автоматически создаются:
- 23 товара с изображениями
- 20 пользователей (1 admin + 19 тестовых покупателей)
- **30 тестовых заказов** (COD, разные статусы доставки)
- 557 взаимодействий пользователей с товарами
- 1 A/B эксперимент (Popular vs Adaptive) с 20 назначениями
- 895 событий рекомендаций

Данные генерируются с фиксированным seed (`Random(42)`) для воспроизводимости.

### Учётные записи для входа

| Роль | Email | Пароль |
|------|-------|--------|
| Администратор | admin@test.com | Pa$$w0rd |
| Покупатель | anna@test.com | Pa$$w0rd |
| Покупатель | dmitriy@test.com | Pa$$w0rd |

(Все 19 тестовых покупателей используют пароль `Pa$$w0rd`)

## Документация

Полная документация находится в папке [`diplomnaya-rabota/`](diplomnaya-rabota/):

| Файл | Содержание |
|------|-----------|
| 01-TECH-STACK.md | Технологический стек |
| 02-ARCHITECTURE.md | Архитектура системы |
| 03-ALGORITHMS.md | Алгоритмы рекомендаций |
| 04-DATABASE.md | Структура базы данных |
| 05-AB-TESTING.md | Методология A/B тестирования |
| 06-METRICS.md | Offline-метрики качества |
| 07-RESULTS.md | Результаты экспериментов |
| 08-COLD-START.md | Решение проблемы холодного старта |
| 09-UML-DIAGRAMS.md | UML-диаграммы (10 шт.) |
| 10-GRAPHS.md | Графики и визуализации (11 шт.) |

## Результаты эксперимента

| Метрика | Control (Popular) | Treatment (Adaptive) | Лифт |
|---------|-------------------|----------------------|------|
| CTR | 8.1% | 16.0% | **+97%** |
| Конверсия → Cart | 15% | 25% | **+67%** |
| Конверсия → Purchase | 25% | 40% | **+60%** |
| NDCG@8 | 0.12 | 0.25 | **+108%** |
| Coverage | 20% | 55% | **+175%** |

Все ключевые метрики статистически значимы (p < 0.05).

## Лицензия

Учебный проект для магистерской диссертации.
