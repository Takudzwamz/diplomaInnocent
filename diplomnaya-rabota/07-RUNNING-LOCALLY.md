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
    "DefaultConnection": "Server=localhost,1433;Database=Skinet;User Id=SA;Password=Password@1;TrustServerCertificate=True",
    "Redis": "localhost:6379"
  },
  "AzureOpenAI": {
    "Endpoint": "<ваш-endpoint>",
    "ApiKey": "<ваш-ключ>",
    "EmbeddingDeployment": "text-embedding-ada-002"
  }
}
```

> **Примечание:** Если нет доступа к Azure OpenAI, система будет работать без эмбеддингов (контентная фильтрация будет недоступна, но остальные стратегии работают).

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
- **HTTPS:** https://localhost:5001
- **HTTP:** http://localhost:5000

---

## Шаг 7: Начальные данные (Seeding)

При первом запуске автоматически создаются:
- 50 товаров с изображениями
- 20 тестовых пользователей
- ~15 000 взаимодействий за 30 дней
- 1 активный A/B эксперимент (Popular vs Adaptive)
- ~6 000 событий рекомендаций

### Тестовые аккаунты:

| Email | Пароль | Роль |
|-------|--------|------|
| admin@test.com | Pa$$w0rd | Администратор |
| user1@test.com | Pa$$w0rd | Покупатель |
| user2@test.com | Pa$$w0rd | Покупатель |

---

## Шаг 8: Проверка работоспособности

### Витрина (Storefront):
1. Откройте https://localhost:5001
2. Войдите как `user1@test.com`
3. Откройте любой товар — внизу должна быть секция «Рекомендации»

### Админ-панель:
1. Войдите как `admin@test.com`
2. Перейдите в раздел «Рекомендации» / «A/B тесты»
3. Просмотрите метрики эксперимента

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
