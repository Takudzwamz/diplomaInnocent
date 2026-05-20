# Руководство по оформлению приложений к дипломной работе
# Guide for Thesis Appendices (Приложения)

> **Что это:** Полный список того, что нужно включить в раздел «Приложения» дипломной работы.
> Каждый пункт содержит: (1) что копировать, (2) из какого файла, (3) готовый текст подписи.

---

## Структура приложений

| Приложение | Содержание | Файл-источник |
|---|---|---|
| А | Архитектурная диаграмма системы | `revised-diagrams-drawio/01_архитектура_системы.png` |
| Б | ER-диаграмма базы данных | `revised-diagrams-drawio/02_база_данных_ER.png` |
| В | Диаграмма вариантов использования | `revised-diagrams-drawio/08_варианты_использования.png` |
| Г | Диаграммы классов (5 частей) | `revised-diagrams-drawio/09а–09д_классы_*.png` |
| Д | Алгоритм гибридных рекомендаций | `revised-diagrams-drawio/03_алгоритм_рекомендаций.png` |
| Е | Формула гибридного алгоритма | `revised-diagrams-drawio/04_формула_гибрид.png` |
| Ж | Схема A/B тестирования | `revised-diagrams-drawio/05_AB_тестирование.png` |
| З | Результаты CTR и воронка конверсии | `revised-diagrams-drawio/06_*.png`, `07_*.png` |
| И | Фрагменты исходного кода | см. ниже |
| К | Описание API эндпоинтов | см. ниже |
| Л | Конфигурация базы данных | см. ниже |

---

# ═══════════════════════════════════════════════
# ПРИЛОЖЕНИЕ А — Архитектурные диаграммы
# ═══════════════════════════════════════════════

Вставить все диаграммы из папки `revised-diagrams-drawio/` с подписями:

### Подписи к рисункам (копируй как есть):

```
Рисунок А.1 — Архитектура системы электронной коммерции
Рисунок А.2 — ER-диаграмма базы данных
Рисунок А.3 — Диаграмма вариантов использования (Use Case)
Рисунок А.4 — Диаграмма классов: домен товаров
Рисунок А.5 — Диаграмма классов: домен заказов
Рисунок А.6 — Диаграмма классов: домен пользователя
Рисунок А.7 — Диаграмма классов: рекомендательная система и A/B тестирование
Рисунок А.8 — Диаграмма классов: купоны и управление контентом
Рисунок А.9 — Блок-схема алгоритма генерации рекомендаций
Рисунок А.10 — Формула гибридного алгоритма
Рисунок А.11 — Схема процесса A/B тестирования
Рисунок А.12 — Результаты сравнения CTR
Рисунок А.13 — Воронка конверсии рекомендаций
```

---

# ═══════════════════════════════════════════════
# ПРИЛОЖЕНИЕ Б — Фрагменты исходного кода
# ═══════════════════════════════════════════════

## Б.1 — Гибридный алгоритм адаптивных рекомендаций

> **Подпись:** Листинг Б.1 — Метод генерации адаптивных рекомендаций
> **Файл:** `Infrastructure/Services/AdaptiveRecommendationService.cs`
> **Метод:** `GetAdaptiveRecommendationsAsync`

Скопировать этот фрагмент в диплом:

```csharp
public async Task<List<Product>> GetAdaptiveRecommendationsAsync(
    string? userId, int count = 8)
{
    // 1. Получить историю взаимодействий пользователя
    var userTopProducts = !string.IsNullOrEmpty(userId)
        ? await _userInteractionService.GetUserTopProductsAsync(userId, 20)
        : new List<int>();

    // 2. Обработка холодного старта — новый пользователь без истории
    if (!userTopProducts.Any())
        return await GetPopularProductsAsync(count);

    // 3. Веса компонентов гибридного алгоритма
    const double collaborativeWeight = 0.40;  // Коллаборативная фильтрация
    const double contentWeight       = 0.35;  // Контентный анализ (ИИ)
    const double trendingWeight      = 0.15;  // Трендовые товары
    const double recencyWeight       = 0.10;  // Новизна в категориях

    // 4. Агрегация оценок из четырёх компонентов
    var productScores = new Dictionary<int, double>();
    
    // Коллаборативная фильтрация (40%)
    var collabScores = await GetCollaborativeRecommendationsInternalAsync(
        userId, count * 3);
    foreach (var (productId, score) in collabScores)
        productScores[productId] = score * collaborativeWeight;
    
    // Контентный анализ на основе эмбеддингов (35%)
    var contentRecs = await _aiRecommendationService
        .GetRecommendationsAsync(userTopProducts.First(), count * 2);
    for (int i = 0; i < contentRecs.Count; i++)
    {
        var score = (double)(contentRecs.Count - i) / contentRecs.Count;
        productScores.TryAdd(contentRecs[i].Id, 0);
        productScores[contentRecs[i].Id] += score * contentWeight;
    }

    // Трендовые товары за 30 дней (15%)
    // ... аналогичная агрегация ...

    // Бонус за новизну в категориях пользователя (10%)
    // ... аналогичная агрегация ...

    // 5. Фильтрация: исключить товары, с которыми пользователь взаимодействовал
    var userInteracted = await GetUserInteractedProductIds(userId);
    var filtered = productScores
        .Where(kvp => !userInteracted.Contains(kvp.Key))
        .OrderByDescending(kvp => kvp.Value)
        .Take(count)
        .Select(kvp => kvp.Key)
        .ToList();

    // 6. Загрузка полных данных товаров с изображениями
    return await LoadProductsWithDetails(filtered);
}
```

**Комментарий к листингу (вставить под листингом):**

> Метод `GetAdaptiveRecommendationsAsync` реализует гибридный алгоритм рекомендаций, объединяющий четыре компонента с заданными весами: коллаборативная фильтрация (0.40), контентный анализ на основе ИИ-эмбеддингов (0.35), трендовые товары (0.15) и бонус за новизну (0.10). При отсутствии истории взаимодействий применяется стратегия холодного старта — возврат популярных товаров. Итоговая оценка товара вычисляется как взвешенная сумма оценок компонентов. Товары, с которыми пользователь уже взаимодействовал, исключаются из результата.

---

## Б.2 — Коллаборативная фильтрация

> **Подпись:** Листинг Б.2 — Алгоритм коллаборативной фильтрации
> **Файл:** `Infrastructure/Services/AdaptiveRecommendationService.cs`
> **Метод:** `GetCollaborativeRecommendationsInternalAsync`

```csharp
private async Task<List<(int ProductId, double Score)>>
    GetCollaborativeRecommendationsInternalAsync(string userId, int count)
{
    // 1. Получить товары, с которыми взаимодействовал целевой пользователь
    var userProducts = await _unitOfWork.Repository<UserInteraction>()
        .GetQueryable()
        .Where(i => i.UserId == userId)
        .Select(i => i.ProductId)
        .Distinct()
        .ToListAsync();

    // 2. Найти похожих пользователей (по пересечению товаров)
    var similarUsers = await _unitOfWork.Repository<UserInteraction>()
        .GetQueryable()
        .Where(i => userProducts.Contains(i.ProductId) && i.UserId != userId)
        .GroupBy(i => i.UserId)
        .OrderByDescending(g => g.Select(i => i.ProductId).Distinct().Count())
        .Take(20)  // Топ-20 похожих пользователей
        .Select(g => g.Key)
        .ToListAsync();

    // 3. Получить товары похожих пользователей (покупки и корзина)
    var recommendations = await _unitOfWork.Repository<UserInteraction>()
        .GetQueryable()
        .Where(i => similarUsers.Contains(i.UserId)
            && !userProducts.Contains(i.ProductId)
            && (i.Type == InteractionType.Purchase
                || i.Type == InteractionType.AddToCart))
        .GroupBy(i => i.ProductId)
        .Select(g => new
        {
            ProductId = g.Key,
            Score = (double)g.Count() / similarUsers.Count
        })
        .OrderByDescending(x => x.Score)
        .Take(count)
        .ToListAsync();

    return recommendations.Select(r => (r.ProductId, r.Score)).ToList();
}
```

**Комментарий к листингу:**

> Алгоритм коллаборативной фильтрации реализует подход «пользователь-пользователь» (user-based). На первом шаге определяются товары целевого пользователя. Затем находятся 20 наиболее похожих пользователей — тех, кто взаимодействовал с наибольшим количеством тех же товаров. На третьем шаге извлекаются товары, которые похожие пользователи покупали или добавляли в корзину, но которых нет в истории целевого пользователя. Оценка релевантности вычисляется как отношение числа взаимодействий к числу похожих пользователей.

---

## Б.3 — Контентный анализ (косинусное сходство эмбеддингов)

> **Подпись:** Листинг Б.3 — Контентные рекомендации на основе ИИ-эмбеддингов
> **Файл:** `Infrastructure/Services/AIRecommendationService.cs`
> **Метод:** `GetRecommendationsAsync`

```csharp
public async Task<List<Product>> GetRecommendationsAsync(
    int productId, int count = 4)
{
    // 1. Получить эмбеддинг исходного товара
    var sourceProduct = await _unitOfWork.Repository<Product>()
        .GetQueryable()
        .Where(p => p.Id == productId && p.Embedding != null)
        .Select(p => new { p.Id, p.Embedding, p.Price,
                           p.ProductBrandId, p.ProductTypeId, p.CategoryId })
        .FirstOrDefaultAsync();

    var sourceEmbedding = JsonSerializer.Deserialize<float[]>(
        sourceProduct.Embedding);

    // 2. Предварительная фильтрация кандидатов (оптимизация)
    var candidates = await _unitOfWork.Repository<Product>()
        .GetQueryable()
        .Where(p =>
            (p.CategoryId == sourceProduct.CategoryId ||
             p.ProductBrandId == sourceProduct.ProductBrandId) &&
            p.Id != productId &&
            p.Embedding != null && p.Embedding != "")
        .Select(p => new { p.Id, p.Embedding, p.Price,
                           p.ProductBrandId, p.ProductTypeId })
        .Take(50)
        .ToListAsync();

    // 3. Вычисление косинусного сходства (параллельно)
    var scores = new ConcurrentBag<(int Id, double Score)>();
    Parallel.ForEach(candidates, candidate =>
    {
        var embedding = JsonSerializer.Deserialize<float[]>(
            candidate.Embedding);
        var similarity = CosineSimilarity(sourceEmbedding, embedding);

        // Дополнительные факторы
        if (candidate.ProductBrandId == sourceProduct.ProductBrandId)
            similarity += 0.1;
        if (candidate.ProductTypeId == sourceProduct.ProductTypeId)
            similarity += 0.1;

        scores.Add((candidate.Id, similarity));
    });

    // 4. Отбор лучших кандидатов
    var topIds = scores
        .OrderByDescending(s => s.Score)
        .Take(count)
        .Select(s => s.Id)
        .ToList();

    return await LoadFullProducts(topIds);
}

// Формула косинусного сходства
private static double CosineSimilarity(float[] a, float[] b)
{
    double dot = 0, normA = 0, normB = 0;
    for (int i = 0; i < a.Length; i++)
    {
        dot   += a[i] * b[i];
        normA += a[i] * a[i];
        normB += b[i] * b[i];
    }
    return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
}
```

**Комментарий к листингу:**

> Метод реализует контентные рекомендации на основе семантического сходства товаров. Каждый товар представлен вектором размерностью 1536 (embedding), сгенерированным сервисом Azure OpenAI. Для нахождения похожих товаров вычисляется косинусное сходство между векторами: $\cos(\theta) = \frac{\mathbf{A} \cdot \mathbf{B}}{|\mathbf{A}| \cdot |\mathbf{B}|}$. Для оптимизации применяется предварительная фильтрация по категории или бренду (сужение до 50 кандидатов) и параллельное вычисление сходства.

---

## Б.4 — Детерминированное назначение в группу A/B теста

> **Подпись:** Листинг Б.4 — Алгоритм назначения пользователя в группу A/B теста
> **Файл:** `Infrastructure/Services/ABTestService.cs`
> **Метод:** `GetOrAssignUserAsync`

```csharp
public async Task<ABTestAssignment> GetOrAssignUserAsync(
    string userId, int experimentId)
{
    // 1. Проверить существующее назначение
    var existing = await _unitOfWork.Repository<ABTestAssignment>()
        .GetQueryable()
        .FirstOrDefaultAsync(a =>
            a.UserId == userId && a.ExperimentId == experimentId);
    if (existing != null) return existing;

    // 2. Получить параметры эксперимента
    var experiment = await _unitOfWork.Repository<ABTestExperiment>()
        .GetByIdAsync(experimentId);

    // 3. Детерминированное хеширование для назначения группы
    var hash = GetDeterministicHash(userId, experimentId);
    var isTreatment = (hash % 100) < experiment.TreatmentPercentage;

    // 4. Сохранить назначение
    var assignment = new ABTestAssignment
    {
        ExperimentId = experimentId,
        UserId = userId,
        IsTreatment = isTreatment,
        AssignedAt = DateTime.UtcNow
    };
    _unitOfWork.Repository<ABTestAssignment>().Add(assignment);
    await _unitOfWork.Complete();

    return assignment;
}

/// <summary>
/// Детерминированный хеш: один и тот же пользователь всегда попадает
/// в одну и ту же группу для одного эксперимента.
/// </summary>
private static int GetDeterministicHash(string userId, int experimentId)
{
    var combined = $"{userId}:{experimentId}";
    unchecked
    {
        int hash = 17;
        foreach (char c in combined)
            hash = hash * 31 + c;
        return Math.Abs(hash);
    }
}
```

**Комментарий к листингу:**

> Метод реализует детерминированное назначение пользователя в контрольную или экспериментальную группу A/B теста. Хеш-функция на основе комбинации идентификатора пользователя и эксперимента гарантирует, что один и тот же пользователь всегда попадает в одну и ту же группу при повторных обращениях. Это обеспечивает консистентность эксперимента без необходимости обращения к базе данных при каждом запросе. Остаток от деления хеша на 100 сравнивается с процентом распределения (TreatmentPercentage, обычно 50%), определяя принадлежность к группе.

---

## Б.5 — Отслеживание взаимодействий и взвешенное скоринг

> **Подпись:** Листинг Б.5 — Система отслеживания и оценки взаимодействий
> **Файл:** `Infrastructure/Services/UserInteractionService.cs`

```csharp
// Отслеживание действия пользователя
public async Task TrackInteractionAsync(
    string userId, int productId, InteractionType type,
    string? sessionId = null, int? durationSeconds = null)
{
    var interaction = new UserInteraction
    {
        UserId = userId,
        ProductId = productId,
        Type = type,
        Timestamp = DateTime.UtcNow,
        SessionId = sessionId,
        DurationSeconds = durationSeconds
    };
    _unitOfWork.Repository<UserInteraction>().Add(interaction);
    await _unitOfWork.Complete();
}

// Получение топ товаров с взвешенной оценкой
public async Task<List<int>> GetUserTopProductsAsync(
    string userId, int count = 10)
{
    return await _unitOfWork.Repository<UserInteraction>()
        .GetQueryable()
        .Where(i => i.UserId == userId)
        .GroupBy(i => i.ProductId)
        .Select(g => new
        {
            ProductId = g.Key,
            Score = g.Sum(i => i.Type switch
            {
                InteractionType.Purchase            => 5,  // Покупка
                InteractionType.AddToCart            => 3,  // Корзина
                InteractionType.Click               => 2,  // Клик
                InteractionType.RecommendationClick  => 2,  // Клик по рек.
                InteractionType.Wishlist            => 2,  // Избранное
                _                                   => 1   // Просмотр и др.
            })
        })
        .OrderByDescending(x => x.Score)
        .Take(count)
        .Select(x => x.ProductId)
        .ToListAsync();
}
```

**Комментарий к листингу:**

> Система отслеживания записывает каждое значимое действие пользователя в базу данных. Для определения предпочтений пользователя используется взвешенная оценка: покупка имеет вес 5 (наибольшая значимость), добавление в корзину — 3, клики и избранное — 2, просмотры — 1. Метод `GetUserTopProductsAsync` группирует взаимодействия по товарам и возвращает товары с наибольшей суммарной оценкой, что позволяет определить реальные предпочтения пользователя.

---

## Б.6 — Генерация ИИ-эмбеддингов товаров

> **Подпись:** Листинг Б.6 — Генерация векторных представлений товаров
> **Файл:** `Infrastructure/Services/ProductEmbeddingService.cs`

```csharp
public async Task GenerateMissingEmbeddingsAsync()
{
    // 1. Найти товары без эмбеддингов
    var products = await _unitOfWork.Repository<Product>()
        .GetQueryable()
        .Include(p => p.ProductBrand)
        .Include(p => p.ProductType)
        .Include(p => p.Category)
        .Where(p => p.Embedding == null || p.Embedding == "")
        .ToListAsync();

    foreach (var product in products)
    {
        try
        {
            // 2. Создать текстовое описание для ИИ
            var description =
                $"{product.Name} {product.Description} " +
                $"Brand: {product.ProductBrand?.Name} " +
                $"Type: {product.ProductType?.Name} " +
                $"Category: {product.Category?.Name} " +
                $"Price: {product.Price:C}";

            // 3. Вызвать Azure OpenAI API (модель text-embedding-3-small)
            var embedding = await _openAIClient
                .GetEmbeddingAsync(description);

            // 4. Сохранить вектор (1536 чисел) в JSON
            product.Embedding = JsonSerializer.Serialize(embedding);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to generate embedding for product {Id}", product.Id);
        }
    }

    await _unitOfWork.Complete();
}
```

**Комментарий к листингу:**

> Метод `GenerateMissingEmbeddingsAsync` выполняет пакетную генерацию векторных представлений (эмбеддингов) для товаров, у которых они отсутствуют. Для каждого товара формируется текстовое описание, включающее название, описание, бренд, тип, категорию и цену. Это описание передаётся в API Azure OpenAI (модель text-embedding-3-small), который возвращает вектор размерностью 1536 чисел с плавающей точкой. Вектор сериализуется в JSON и сохраняется в поле Embedding таблицы Products. Обработка ошибок неблокирующая: при сбое генерации для одного товара процесс продолжается для остальных.

---

## Б.7 — Регистрация сервисов (Dependency Injection)

> **Подпись:** Листинг Б.7 — Конфигурация внедрения зависимостей
> **Файл:** `StorefrontRazor/Program.cs`

```csharp
// === Рекомендательная система ===

// ИИ-сервисы (Azure OpenAI)
builder.Services.AddSingleton<AzureOpenAIClientService>();
builder.Services.AddScoped<IAIRecommendationService, AIRecommendationService>();
builder.Services.AddScoped<IProductEmbeddingService, ProductEmbeddingService>();

// Адаптивные рекомендации
builder.Services.AddScoped<IUserInteractionService, UserInteractionService>();
builder.Services.AddScoped<IAdaptiveRecommendationService,
    AdaptiveRecommendationService>();
builder.Services.AddScoped<IABTestService, ABTestService>();

// Метрики и оценка
builder.Services.AddScoped<IRecommendationMetricsService,
    RecommendationMetricsService>();
builder.Services.AddScoped<IOfflineMetricsService, OfflineMetricsService>();

// === Инфраструктура ===

// База данных (SQL Server + EF Core)
builder.Services.AddDbContext<StoreContext>(opt =>
{
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOpt => sqlOpt.UseQuerySplittingBehavior(
            QuerySplittingBehavior.SplitQuery));
});

// Redis (корзина + защита данных)
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// Аутентификация (ASP.NET Identity)
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<StoreContext>();

// === Ограничение запросов (Rate Limiting) ===
builder.Services.AddRateLimiter(options =>
{
    // ИИ-чат: 10 запросов в минуту на IP
    options.AddPolicy("AiChat", context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 2
            }));
});
```

**Комментарий к листингу:**

> Фрагмент демонстрирует регистрацию сервисов рекомендательной системы в контейнере внедрения зависимостей (DI) ASP.NET Core. `AddSingleton` используется для клиента Azure OpenAI (один экземпляр на приложение), `AddScoped` — для сервисов, привязанных к HTTP-запросу. Паттерн «интерфейс → реализация» обеспечивает слабую связанность: `IAdaptiveRecommendationService` может быть заменён другой реализацией без изменения остального кода. Rate Limiting защищает ИИ-эндпоинт от злоупотреблений (10 запросов/мин).

---

# ═══════════════════════════════════════════════
# ПРИЛОЖЕНИЕ В — Описание API
# ═══════════════════════════════════════════════

## Таблица API эндпоинтов

> **Подпись:** Таблица В.1 — API эндпоинты платформы

| HTTP | Маршрут | Назначение | Аутент. |
|------|---------|------------|---------|
| **Рекомендательная система** | | | |
| POST | `/RecommendationTracking?handler=Track` | Запись взаимодействия пользователя | Да |
| POST | `/RecommendationTracking?handler=Click` | Запись клика по рекомендации | Да |
| POST | `/RecommendationTracking?handler=Impression` | Запись показа рекомендации | Да |
| **Товары** | | | |
| GET | `/Products` | Каталог с фильтрацией и пагинацией | Нет |
| GET | `/Products/Details/{id}` | Страница товара с отзывами и рекомендациями | Нет |
| GET | `/api/products/{id}` | API: данные товара (JSON) | Нет |
| **Корзина** | | | |
| POST | `/api/cart/add` | Добавить товар в корзину | Нет |
| POST | `/api/cart/update` | Обновить количество | Нет |
| GET | `/api/cart/count` | Количество товаров в корзине | Нет |
| GET | `/api/cart/context` | Полное содержимое корзины | Нет |
| **Заказы** | | | |
| GET | `/api/orders/context` | Список заказов пользователя | Да |
| **ИИ-чат** | | | |
| POST | `/api/ai/chat` | ИИ-ассистент (лимит: 10 зап./мин) | Нет |
| **Администрирование A/B тестов** | | | |
| GET | `/Admin/Recommendations` | Панель A/B тестирования и метрик | Админ |
| POST | `/Admin/Recommendations?handler=CreateExperiment` | Создание эксперимента | Админ |
| POST | `/Admin/Recommendations?handler=EndExperiment` | Завершение эксперимента | Админ |
| GET | `/Admin/Recommendations/Evaluation` | Офлайн-метрики оценки | Админ |

---

## Формат запросов и ответов

### Запись взаимодействия

> **Подпись:** Листинг В.1 — Пример запроса записи взаимодействия

```json
// Запрос: POST /RecommendationTracking?handler=Track
{
    "productId": 42,
    "type": 0,           // InteractionType.View
    "sessionId": "abc-123",
    "durationSeconds": 15
}

// Ответ:
{ "ok": true }
```

### Запись клика по рекомендации

> **Подпись:** Листинг В.2 — Пример запроса записи клика рекомендации

```json
// Запрос: POST /RecommendationTracking?handler=Click
{
    "productId": 17,
    "strategy": 4,       // RecommendationStrategy.Adaptive
    "position": 2,       // Позиция в списке рекомендаций
    "sourceProductId": 42 // С какой страницы товара
}

// Ответ:
{ "ok": true }
```

### Добавление в корзину

> **Подпись:** Листинг В.3 — Пример запроса добавления в корзину

```json
// Запрос: POST /api/cart/add
{
    "productId": 42,
    "quantity": 1
}

// Ответ:
{
    "success": true,
    "cartCount": 3
}
```

### ИИ-чат

> **Подпись:** Листинг В.4 — Пример запроса к ИИ-чату

```json
// Запрос: POST /api/ai/chat
{
    "message": "Какие кроссовки подойдут для бега?",
    "context": {
        "currentProduct": "Nike Air Max",
        "category": "Running"
    }
}

// Ответ:
{
    "success": true,
    "message": "Для бега рекомендую обратить внимание на..."
}
```

---

# ═══════════════════════════════════════════════
# ПРИЛОЖЕНИЕ Г — Конфигурация базы данных
# ═══════════════════════════════════════════════

## Индексы базы данных

> **Подпись:** Таблица Г.1 — Индексы для рекомендательной системы

| Таблица | Индекс | Столбцы | Тип |
|---------|--------|---------|-----|
| UserInteractions | IX_User_Time | UserId, Timestamp | Обычный |
| UserInteractions | IX_Product_Time | ProductId, Timestamp | Обычный |
| UserInteractions | IX_Session | SessionId | Обычный |
| ABTestExperiments | IX_IsActive | IsActive | Обычный |
| ABTestAssignments | IX_User_Experiment | UserId, ExperimentId | Уникальный |
| RecommendationEvents | IX_User_Time | UserId, Timestamp | Обычный |
| RecommendationEvents | IX_Experiment_Type | ExperimentId, EventType | Обычный |
| RecommendationEvents | IX_Strategy | Strategy | Обычный |

## Правила каскадного удаления

> **Подпись:** Таблица Г.2 — Поведение при удалении связанных записей

| Главная таблица | Зависимая таблица | Поведение | Обоснование |
|------|------|------|------|
| AppUser | UserInteraction | Cascade | Удаление пользователя удаляет его историю |
| AppUser | ABTestAssignment | Cascade | Удаление пользователя удаляет назначения |
| AppUser | RecommendationEvent | Cascade | Удаление пользователя удаляет события |
| Product | UserInteraction | Cascade | Удаление товара удаляет взаимодействия |
| Product | RecommendationEvent | **Restrict** | Запрет удаления товара с историей рекомендаций |
| ABTestExperiment | ABTestAssignment | Cascade | Удаление эксперимента удаляет назначения |
| ABTestExperiment | RecommendationEvent | **SetNull** | Завершение эксперимента сохраняет события |

---

# ═══════════════════════════════════════════════
# ПРИЛОЖЕНИЕ Д — Таблица технологий
# ═══════════════════════════════════════════════

> **Подпись:** Таблица Д.1 — Технологический стек платформы

| Компонент | Технология | Версия | Назначение |
|-----------|-----------|--------|------------|
| Backend | ASP.NET Core | 8.0 | Веб-сервер, API, бизнес-логика |
| ORM | Entity Framework Core | 8.0 | Объектно-реляционное отображение |
| СУБД | SQL Server | 2022 | Основное хранилище данных |
| Кэш | Redis | 7.x | Корзина, сессии, Data Protection |
| Аутентификация | ASP.NET Identity | 8.0 | Регистрация, JWT, роли |
| ИИ-эмбеддинги | Azure OpenAI | — | text-embedding-3-small (1536 dim) |
| ИИ-чат | Azure OpenAI | — | GPT-4o для ИИ-ассистента |
| Оплата | Stripe | — | Обработка платежей |
| Контейнеризация | Docker Compose | — | SQL Server + Redis |
| Frontend | Razor Pages + JS | — | Серверный рендеринг + AJAX |
| Rate Limiting | ASP.NET Rate Limiter | 8.0 | Защита API от злоупотреблений |

---

# Как оформлять приложения в Word

1. **Каждое приложение** начинается с новой страницы.
2. **Заголовок:** «ПРИЛОЖЕНИЕ А» (по центру, жирный, 14 пт).
3. **Подзаголовок:** описание содержания (под заголовком, 12 пт).
4. **Листинги кода:** шрифт Courier New или Consolas, 9-10 пт, с нумерацией строк.
5. **Подписи:** под каждым рисунком, таблицей или листингом.
6. **Ссылки в основном тексте:** «(см. Приложение А, рисунок А.1)» или «(см. Листинг Б.1)».
7. **Порядок:** Приложения нумеруются буквами: А, Б, В, Г, Д...
