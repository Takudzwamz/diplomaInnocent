# Упрощённые диаграммы — Mermaid Source

## 1. Общая архитектура системы

```mermaid
graph TB
    subgraph "Пользователь"
        A[🖥️ Браузер]
    end

    subgraph "Сервер приложения"
        B[ASP.NET Razor Pages<br/>Страницы + Логика]
        C[Рекомендательная<br/>система]
    end

    subgraph "Хранилище данных"
        D[(SQL Server<br/>Товары, заказы,<br/>взаимодействия)]
        E[(Redis<br/>Корзина, кэш)]
    end

    subgraph "Внешние сервисы"
        F[Azure OpenAI<br/>Генерация векторов]
        G[SendGrid<br/>Email-уведомления]
    end

    A -->|HTTP запросы| B
    B --> C
    C --> D
    B --> E
    C -->|Вектора товаров| F
    B -->|Письма| G

    style A fill:#e1f5fe
    style B fill:#fff3e0
    style C fill:#fce4ec
    style D fill:#e8f5e9
    style E fill:#e8f5e9
    style F fill:#f3e5f5
    style G fill:#f3e5f5
```

---

## 2. ER-диаграмма — таблицы рекомендательной системы

```mermaid
erDiagram
    Products {
        int Id PK
        string Name "Название товара"
        decimal Price "Цена"
        string Embedding "Вектор ИИ (1536 чисел)"
    }

    UserInteractions {
        int Id PK
        string UserId FK "Кто"
        int ProductId FK "Что"
        int Type "Тип действия"
        datetime Timestamp "Когда"
    }

    RecommendationEvents {
        int Id PK
        string UserId FK "Кому показали"
        int ProductId FK "Что рекомендовали"
        string Strategy "Какой алгоритм"
        int Position "Позиция 1-8"
        string EventType "Показ или Клик"
    }

    ABTestExperiments {
        int Id PK
        string Name "Название теста"
        string Control "Контрольная стратегия"
        string Treatment "Экспериментальная"
        int TrafficPercent "50/50"
        bool IsActive "Активен"
    }

    ABTestAssignments {
        int Id PK
        string UserId FK "Пользователь"
        int ExperimentId FK "Эксперимент"
        bool IsTreatment "В какой группе"
    }

    Products ||--o{ UserInteractions : "товар"
    Products ||--o{ RecommendationEvents : "рекомендован"
    ABTestExperiments ||--o{ ABTestAssignments : "содержит"
```

---

## 3. Алгоритм генерации рекомендаций (блок-схема)

```mermaid
flowchart TD
    A[Пользователь открывает страницу] --> B{Есть ли история<br/>взаимодействий?}
    
    B -->|Нет — холодный старт| C[Показать популярные<br/>товары за 30 дней]
    
    B -->|Да| D[Запустить гибридный<br/>алгоритм]
    
    D --> E[1. Коллаборативная<br/>фильтрация<br/>вес 40%]
    D --> F[2. Контентный<br/>анализ ИИ<br/>вес 35%]
    D --> G[3. Тренды<br/>за 7 дней<br/>вес 15%]
    D --> H[4. Категории<br/>пользователя<br/>вес 10%]
    
    E --> I[Суммировать баллы<br/>с весами]
    F --> I
    G --> I
    H --> I
    
    I --> J[Убрать товары,<br/>которые уже смотрел]
    J --> K[Выдать ТОП-8<br/>рекомендаций]
    C --> K

    style A fill:#e3f2fd
    style B fill:#fff9c4
    style C fill:#c8e6c9
    style D fill:#ffccbc
    style K fill:#c8e6c9
```

---

## 4. Формула гибридного алгоритма

```mermaid
graph LR
    subgraph "Компоненты"
        CF["Коллаборативная<br/>фильтрация<br/>(похожие пользователи)"]
        CB["Контентный анализ<br/>(похожие товары<br/>по ИИ-векторам)"]
        TR["Тренды<br/>(популярное<br/>за 7 дней)"]
        RC["Категории<br/>(недавние<br/>интересы)"]
    end

    subgraph "Веса"
        W1["× 0.40"]
        W2["× 0.35"]
        W3["× 0.15"]
        W4["× 0.10"]
    end

    subgraph "Результат"
        SUM["Σ Итоговый балл"]
        TOP["ТОП-8 товаров"]
    end

    CF --> W1 --> SUM
    CB --> W2 --> SUM
    TR --> W3 --> SUM
    RC --> W4 --> SUM
    SUM --> TOP

    style CF fill:#bbdefb
    style CB fill:#c8e6c9
    style TR fill:#fff9c4
    style RC fill:#ffccbc
    style SUM fill:#e1bee7
    style TOP fill:#f8bbd0
```

---

## 5. Процесс A/B тестирования

```mermaid
flowchart TD
    A[Новый пользователь<br/>заходит на сайт] --> B{Случайное<br/>распределение<br/>50/50}

    B -->|Группа A<br/>Контроль| C[Алгоритм: Popular<br/>Просто популярные товары]
    B -->|Группа B<br/>Эксперимент| D[Алгоритм: Adaptive<br/>Гибридная модель]

    C --> E[Записываем метрики:<br/>• Показы рекомендаций<br/>• Клики<br/>• Добавления в корзину<br/>• Покупки]
    D --> E

    E --> F[Сравниваем результаты<br/>двух групп]
    
    F --> G{Adaptive лучше?}
    G -->|Да ✓| H[Гипотеза подтверждена:<br/>гибридный алгоритм<br/>эффективнее]
    G -->|Нет ✗| I[Гипотеза опровергнута]

    style A fill:#e3f2fd
    style B fill:#fff9c4
    style C fill:#ffcdd2
    style D fill:#c8e6c9
    style H fill:#a5d6a7
    style I fill:#ef9a9a
```

---

## 6. Результаты — сравнение CTR

```mermaid
xychart-beta
    title "CTR рекомендаций: Контроль vs Эксперимент"
    x-axis ["Popular (Контроль)", "Adaptive (Эксперимент)"]
    y-axis "CTR, %" 0 --> 20
    bar [8, 15]
```

---

## 7. Воронка конверсии

```mermaid
graph TD
    subgraph "Контрольная группа (Popular)"
        A1["Показы: 100%"] --> B1["Клики: 8%"]
        B1 --> C1["В корзину: 1.2%"]
        C1 --> D1["Покупки: 0.3%"]
    end

    subgraph "Экспериментальная группа (Adaptive)"
        A2["Показы: 100%"] --> B2["Клики: 15%"]
        B2 --> C2["В корзину: 3.75%"]
        C2 --> D2["Покупки: 1.5%"]
    end

    style A1 fill:#ffcdd2
    style B1 fill:#ffcdd2
    style C1 fill:#ffcdd2
    style D1 fill:#ffcdd2
    style A2 fill:#c8e6c9
    style B2 fill:#c8e6c9
    style C2 fill:#c8e6c9
    style D2 fill:#c8e6c9
```

---

## 8. Диаграмма вариантов использования (Use Case)

```mermaid
graph LR
    subgraph "Покупатель"
        U1[👤 Покупатель]
    end

    subgraph "Администратор"
        U2[👨‍💼 Админ]
    end

    subgraph "Действия покупателя"
        A1[Просмотр каталога]
        A2[Получение рекомендаций]
        A3[Добавление в корзину]
        A4[Оформление заказа]
        A5[Написание отзыва]
    end

    subgraph "Действия администратора"
        B1[Управление товарами]
        B2[Запуск A/B тестов]
        B3[Просмотр метрик<br/>рекомендаций]
        B4[Управление заказами]
    end

    U1 --> A1
    U1 --> A2
    U1 --> A3
    U1 --> A4
    U1 --> A5
    U2 --> B1
    U2 --> B2
    U2 --> B3
    U2 --> B4

    style U1 fill:#e3f2fd
    style U2 fill:#fff3e0
```
