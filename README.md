# PremierBank Testing

## Как запустить

**Требования:** .NET 8 SDK, Docker

### 1. Поднять базу данных

```bash
docker compose up -d
```

PostgreSQL запустится на порту **5433**.

| Параметр | Значение |
|----------|----------|
| Host | localhost |
| Port | 5433 |
| Database | premier_bank |
| Username | premier |
| Password | premier123 |

### 2. Применить миграции

```bash
dotnet ef database update \
  --project src/PremierBankTesting.Infrastructure \
  --startup-project src/PremierBankTesting.Api
```

> Если `dotnet ef` не установлен: `dotnet tool install --global dotnet-ef`

### 3. Запустить API

```bash
dotnet run --project src/PremierBankTesting.Api
```

### 4. Открыть Swagger UI

[http://localhost:5000/swagger](http://localhost:5000/swagger)

### 5. Запустить тесты

```bash
dotnet test
```

Интеграционные тесты используют InMemory-провайдер EF Core - Docker не нужен.

---

## API Endpoints

Все маршруты имеют префикс `/api/v1`.

| Метод | Путь | Описание |
|-------|------|----------|
| `POST` | `/api/v1/transactions/import` | Импортировать транзакции из банка |
| `GET` | `/api/v1/transactions` | Получить транзакции (фильтр + пагинация) |
| `POST` | `/api/v1/transactions/{id}/mark-as-processed` | Пометить транзакцию как обработанную |
| `GET` | `/api/v1/reports/users` | Сумма обработанных транзакций по пользователям за 30 дней |
| `GET` | `/api/v1/reports/types` | Группировка обработанных транзакций по типу операции |

### GET /api/v1/transactions - параметры

| Параметр | Тип | По умолчанию | Описание |
|----------|-----|-------------|----------|
| `isProcessed` | bool | `false` | Фильтр по статусу обработки |
| `page` | int | `1` | Номер страницы (от 1) |
| `pageSize` | int | `20` | Размер страницы (1-100) |

---

## Архитектура

Проект построен по принципам **Clean Architecture** с разделением на 4 слоя:

```
PremierBankTesting.Domain          <- сущности (User, Transaction), без зависимостей
PremierBankTesting.Application     <- бизнес-логика, интерфейсы, CQRS-хендлеры
PremierBankTesting.Infrastructure  <- EF Core, AppDbContext, BankApiClient
PremierBankTesting.Api             <- контроллеры, DI, Swagger
PremierBankTesting.Contracts       <- DTO, команды, запросы - общие контракты
```

Зависимости направлены строго внутрь: `Api -> Infrastructure -> Application -> Domain`.  
`Contracts` переиспользуется в нескольких слоях без создания циклических ссылок.

### Реализация

**CQRS через MediatR** - каждая операция инкапсулирована в отдельный `Command` или `Query` handler. Контроллеры остаются тонкими и не содержат логики; хендлеры легко тестируются изолированно.

**`IApplicationDbContext`** - интерфейс поверх `AppDbContext`, определённый в Application. Хендлеры работают с абстракцией, а не с конкретным EF Core-классом. Это позволяет подменить реализацию в тестах (InMemory-провайдер) без изменения бизнес-логики.

**`IBankApiClient`** определён в Application, реализован в Infrastructure.

**Доменный метод `MarkAsProcessed()`** вместо прямой установки поля извне.

**snake_case именование** через `UseSnakeCaseNamingConvention()`.

**Нормализация email** (`ToLowerInvariant()`) в `User.Create()` и при импорте - PostgreSQL чувствителен к регистру, поэтому `User@Mail.com` и `user@mail.com` без нормализации создали бы дублей.

**Централизованное управление пакетами** через `Directory.Packages.props` - версии NuGet-зависимостей объявлены в одном месте; в `.csproj` указываются только имена без версий.

### Найденный баг в исходном коде

В исходном `BankApiClient` **не реализовывал** интерфейс `IBankApiClient` - объявление класса было `public class BankApiClient` без `: IBankApiClient`.

---

## Что можно улучшить

- **Авторизация** - JWT Bearer для защиты эндпоинтов; сейчас любой клиент может вызвать импорт или пометить транзакцию
- **FluentValidation** - валидация входных данных через MediatR Pipeline Behaviour;
- **Структурированное логирование** (Serilog + Seq)
- **Настраиваемое окно отчёта** - сейчас период для отчёта по пользователям жёстко задан как 30 дней; можно принимать `from`/`to` через query string
