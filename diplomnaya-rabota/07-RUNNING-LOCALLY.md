# 07 — Запуск проекта локально

## Системные требования

| Компонент | Минимальная версия |
|-----------|--------------------|
| .NET SDK | 10.0 |
| Docker Desktop | 4.x (или Docker Engine + Compose) |
| Git | 2.x |
| Оперативная память | 8 ГБ (рекомендуется 16 ГБ) |
| ОС | Linux / Windows 10+ / macOS 12+ |

---

## Шаг 1: Клонирование репозитория

```bash
git clone <URL-репозитория>
cd StudentEcommproject
```

---

## Шаг 2: Запуск Docker-контейнеров

В корне проекта находится `docker-compose.yml` с SQL Server и Redis:

```bash
docker compose up -d
```

Это запустит:
- **SQL Server 2022** на порту `1433`
- **Redis** на порту `6379`

Проверить, что контейнеры запущены:
```bash
docker compose ps
```

---

## Шаг 3: Настройка конфигурации

### appsettings.Development.json

Файл `StorefrontRazor/appsettings.Development.json` должен содержать:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=storefront;User Id=SA;Password=Password@1;TrustServerCertificate=True",
    "Redis": "localhost:6379,abortConnect=False"
  },
  "AzureOpenAI": {
    "Enabled": false,
    "Endpoint": "<ваш-endpoint>",
    "ApiKey": "<ваш-ключ>",
    "EmbeddingDeployment": "text-embedding-ada-002"
  }
}
```

> **Примечание:** AI-эмбеддинги отключены по умолчанию (`AI_Enabled = false`). Система работает без Azure OpenAI — все стратегии, кроме контентной фильтрации, используют предзагруженные эмбеддинги из БД.

---

## Шаг 4: Восстановление пакетов и сборка

```bash
dotnet restore
dotnet build
```

Убедитесь, что сборка проходит без ошибок (допускаются предупреждения NuGet).

---

## Шаг 5: Применение миграций

Миграции применяются автоматически при запуске приложения. Если нужно применить вручную:

```bash
cd StorefrontRazor
dotnet ef database update --project ../Infrastructure
```

---

## Шаг 6: Запуск приложения

```bash
cd StorefrontRazor
dotnet run
```

Или с горячей перезагрузкой:
```bash
dotnet watch run
```

Приложение будет доступно по адресу:
- **HTTP:** http://localhost:5249

---

## Шаг 7: Оплата — Cash On Delivery (наложенный платёж)

По умолчанию используется метод **«Оплата при получении»**:
- Не требуется внешний платёжный шлюз
- Заказ подтверждается мгновенно после оформления
- Не нужен ngrok или публичный URL
- Работает полностью на localhost

> Для переключения на онлайн-оплату (PayFast/Paystack): Админ → Настройки → Оплата → изменить активный шлюз.

---

## Шаг 8: Начальные данные (Seeding)

При первом запуске автоматически создаются:
- 23 товара с изображениями (6 брендов, 6 категорий)
- 20 пользователей (1 admin + 19 покупателей с адресами доставки)
- **75 тестовых заказов** (COD) за 60 дней с полным циклом доставки
- 557 взаимодействий пользователей с товарами
- 1 активный A/B эксперимент (Popular vs Adaptive)
- 895 событий рекомендаций (показы, клики, корзина, покупки)
- 4 метода доставки (СДЭК-Экспресс,СДЭК-Стандарт, Почта России, Бесплатная)

> **Все данные восстанавливаются с нуля** — можно удалить БД (`docker compose down -v`) и запустить приложение заново.

### Тестовые аккаунты:

| Email | Пароль | Роль | Город |
|-------|--------|------|-------|
| admin@test.com | Pa$$w0rd | Администратор | — |
| anna@test.com | Pa$$w0rd | Покупатель | Москва |
| boris@test.com | Pa$$w0rd | Покупатель | Санкт-Петербург |
| vera@test.com | Pa$$w0rd | Покупатель | Казань |
| grigory@test.com | Pa$$w0rd | Покупатель | Новосибирск |
| darya@test.com | Pa$$w0rd | Покупатель | Екатеринбург |

*Все 19 покупателей используют пароль `Pa$$w0rd`, у каждого — уникальный адрес доставки (автозаполнение при оформлении заказа).*

---

## Шаг 9: Проверка работоспособности

### Витрина (Storefront):
1. Откройте http://localhost:5249
2. Войдите как `anna@test.com` / `Pa$$w0rd`
3. Откройте любой товар — внизу секция «Рекомендации»
4. Добавьте товар в корзину и оформите заказ — адрес заполнится автоматически
5. COD-заказ подтвердится мгновенно

### Админ-панель:
1. Войдите как `admin@test.com` / `Pa$$w0rd`
2. **Панель управления** — общие продажи, заказы, график за 7 дней
3. **Рекомендации** — метрики A/B эксперимента (CTR, показы, клики)
4. **Офлайн-оценка** — нажмите «Запустить офлайн-оценку» для полного отчёта
5. **Заказы** — 75 заказов с трекингом и статусами

> **Все данные доступны сразу после первого запуска** — ручное тестирование не требуется.

---

## Устранение неполадок

### Docker не запускается

```bash
# Проверить статус Docker
systemctl status docker  # Linux
# или
docker info
```

### Ошибка подключения к SQL Server

```bash
# Проверить, прослушивает ли порт
docker logs studentecommproject-sqlserver-1
# Или
ss -tlnp | grep 1433  # Linux
netstat -an | findstr 1433  # Windows
```

### Ошибка подключения к Redis

```bash
docker exec -it studentecommproject-redis-1 redis-cli ping
# Ответ должен быть: PONG
```

### База данных не создаётся

```bash
# Удалить и пересоздать
docker compose down -v  # ВНИМАНИЕ: удалит все данные!
docker compose up -d
cd StorefrontRazor && dotnet run
```

### Эмбеддинги не генерируются

Если в логах:
```
Azure OpenAI embedding generation failed: ...
```

Это нормально без ключа Azure OpenAI. Контентная фильтрация будет недоступна, но Popular, CF и частично Adaptive будут работать.

---

## Команды для разработки

| Действие | Команда |
|----------|---------|
| Сборка | `dotnet build` |
| Запуск | `cd StorefrontRazor && dotnet run` |
| Горячая перезагрузка | `cd StorefrontRazor && dotnet watch run` |
| Новая миграция | `dotnet ef migrations add <Имя> --project Infrastructure -s StorefrontRazor` |
| Применить миграции | `dotnet ef database update --project Infrastructure -s StorefrontRazor` |
| Сброс БД | `docker compose down -v && docker compose up -d` |
| Логи SQL Server | `docker logs studentecommproject-sqlserver-1` |
| Логи Redis | `docker logs studentecommproject-redis-1` |
