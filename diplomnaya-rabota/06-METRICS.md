# 06 — Метрики и статистические тесты

## Обзор

Система собирает два типа метрик:
1. **Online-метрики** — в реальном времени во время A/B эксперимента
2. **Offline-метрики** — ретроспективная оценка качества рекомендаций

---

## Часть 1: Online-метрики (A/B тестирование)

**Файл:** `Infrastructure/Services/RecommendationMetricsService.cs`

### Основные метрики:

| Метрика | Формула | Описание |
|---------|---------|----------|
| **CTR** (Click-Through Rate) | $\frac{\text{Clicks}}{\text{Impressions}}$ | Доля показов, приведших к клику |
| **CR** (Conversion Rate) | $\frac{\text{Purchases}}{\text{Clicks}}$ | Доля кликов, приведших к покупке |
| **AOV** (Average Order Value) | $\frac{\sum \text{OrderTotal}}{\text{OrderCount}}$ | Средний чек |

### Лифт (Lift):

$$
\text{Lift} = \frac{\text{Treatment} - \text{Control}}{\text{Control}} \times 100\%
$$

---

## Статистические тесты

### z-тест для пропорций (CTR, Конверсия)

Используется для сравнения долей (пропорций) между контрольной и экспериментальной группами.

**Гипотезы:**
- $H_0$: $p_1 = p_2$ (разницы нет)
- $H_1$: $p_1 \neq p_2$ (разница есть)

**Формула z-статистики:**

$$
z = \frac{\hat{p}_1 - \hat{p}_2}{\sqrt{\hat{p}(1-\hat{p})\left(\frac{1}{n_1} + \frac{1}{n_2}\right)}}
$$

где:
- $\hat{p}_1, \hat{p}_2$ — наблюдаемые пропорции в группах
- $\hat{p} = \frac{x_1 + x_2}{n_1 + n_2}$ — объединённая пропорция
- $n_1, n_2$ — размеры выборок

**Реализация:**

```csharp
private double CalculateZTestPValue(double p1, double p2, int n1, int n2)
{
    double pooledP = (p1 * n1 + p2 * n2) / (n1 + n2);
    double se = Math.Sqrt(pooledP * (1 - pooledP) * (1.0 / n1 + 1.0 / n2));
    
    if (se == 0) return 1.0;
    
    double z = Math.Abs(p1 - p2) / se;
    // Двусторонний тест
    return 2 * (1 - NormalCdf(z));
}
```

### t-тест Уэлча (Средний чек)

Используется для сравнения средних при неравных дисперсиях.

**Формула t-статистики:**

$$
t = \frac{\bar{x}_1 - \bar{x}_2}{\sqrt{\frac{s_1^2}{n_1} + \frac{s_2^2}{n_2}}}
$$

где:
- $\bar{x}_1, \bar{x}_2$ — средние в группах
- $s_1^2, s_2^2$ — дисперсии в группах
- $n_1, n_2$ — размеры выборок

**Степени свободы (Welch–Satterthwaite):**

$$
\nu = \frac{\left(\frac{s_1^2}{n_1} + \frac{s_2^2}{n_2}\right)^2}{\frac{\left(\frac{s_1^2}{n_1}\right)^2}{n_1-1} + \frac{\left(\frac{s_2^2}{n_2}\right)^2}{n_2-1}}
$$

**Реализация:**

```csharp
private double CalculateTTestPValue(
    double mean1, double std1, int n1,
    double mean2, double std2, int n2)
{
    double se = Math.Sqrt(std1 * std1 / n1 + std2 * std2 / n2);
    
    if (se == 0) return 1.0;
    
    double t = Math.Abs(mean1 - mean2) / se;
    // Аппроксимация p-value через нормальное распределение
    return 2 * (1 - NormalCdf(t));
}
```

### Аппроксимация нормальной CDF

Используется формула Абрамовица и Стегуна:

$$
\Phi(x) \approx 1 - \phi(x)(b_1 t + b_2 t^2 + b_3 t^3 + b_4 t^4 + b_5 t^5)
$$

где $t = \frac{1}{1 + 0.2316419 \cdot x}$, а коэффициенты:

| Коэффициент | Значение |
|-------------|----------|
| $b_1$ | 0.319381530 |
| $b_2$ | -0.356563782 |
| $b_3$ | 1.781477937 |
| $b_4$ | -1.821255978 |
| $b_5$ | 1.330274429 |

---

## Уровень значимости

$$\alpha = 0.05$$

**Интерпретация:**
- $p < 0.05$ → Результат **статистически значим** — разница между группами не случайна
- $p \geq 0.05$ → Результат **не значим** — недостаточно данных для вывода

---

## Часть 2: Offline-метрики

**Файл:** `Infrastructure/Services/OfflineMetricsService.cs`

### Методология оценки:

1. Загрузить все взаимодействия за период
2. **Временное разделение**: 80% обучающая / 20% тестовая (по дате)
3. Для каждой стратегии: сгенерировать Top-K рекомендаций на обучающих данных
4. Сравнить с покупками из тестового набора
5. Рассчитать метрики

### Precision@K (Точность):

$$
\text{Precision@K} = \frac{|\text{Рекомендовано} \cap \text{Релевантно}|}{K}
$$

«Какая доля из K рекомендаций оказалась релевантной?»

### Recall@K (Полнота):

$$
\text{Recall@K} = \frac{|\text{Рекомендовано} \cap \text{Релевантно}|}{|\text{Релевантно}|}
$$

«Какая доля всех релевантных товаров попала в рекомендации?»

### F1@K (F-мера):

$$
\text{F1@K} = 2 \cdot \frac{\text{Precision@K} \cdot \text{Recall@K}}{\text{Precision@K} + \text{Recall@K}}
$$

### NDCG@K (Normalized Discounted Cumulative Gain):

DCG учитывает позицию — релевантный товар на 1-й позиции ценнее, чем на 8-й:

$$
\text{DCG@K} = \sum_{i=1}^{K} \frac{\text{rel}_i}{\log_2(i + 1)}
$$

$$
\text{NDCG@K} = \frac{\text{DCG@K}}{\text{IDCG@K}}
$$

где $\text{IDCG@K}$ — идеальный DCG (все релевантные товары на первых позициях).

### MRR (Mean Reciprocal Rank):

$$
\text{MRR} = \frac{1}{|U|} \sum_{u=1}^{|U|} \frac{1}{\text{rank}_u}
$$

где $\text{rank}_u$ — позиция первого релевантного товара для пользователя $u$.

### Coverage (Покрытие каталога):

$$
\text{Coverage} = \frac{|\text{Уникальные рекомендованные товары}|}{|\text{Весь каталог}|}
$$

«Какую долю каталога система способна рекомендовать?»

---

## Сравнение стратегий (типичные значения)

| Метрика | Popular | CF | Content-Based | Adaptive |
|---------|---------|-----|---------------|----------|
| Precision@8 | 0.10 | 0.18 | 0.15 | 0.22 |
| Recall@8 | 0.08 | 0.14 | 0.12 | 0.18 |
| NDCG@8 | 0.12 | 0.20 | 0.17 | 0.25 |
| MRR | 0.15 | 0.28 | 0.22 | 0.32 |
| Coverage | 0.20 | 0.45 | 0.35 | 0.55 |

> **Примечание:** Значения приближённые, получены на синтетических сидированных данных (20 пользователей × 23 товара × 30 дней).

---

## Сбор данных

### Трекинг на фронтенде (JavaScript):

```javascript
// IntersectionObserver для показов
const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            fetch('/api/recommendations/track-impression', {
                method: 'POST',
                body: JSON.stringify({
                    productId: entry.target.dataset.productId,
                    strategy: entry.target.dataset.strategy,
                    position: entry.target.dataset.position
                })
            });
        }
    });
}, { threshold: 0.5 });
```

### Воронка событий:

```
Impression (Показ) → Click (Клик) → AddToCart (Корзина) → Purchase (Покупка)
```

Каждый переход между этапами — это метрика конверсии:
- **Impression → Click** = CTR
- **Click → Purchase** = Conversion Rate
