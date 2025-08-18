## Оглавление  
- [❓ О проекте](#о_проекте)
- [❓ О API](#о_api)
- [🚀 Как запустить](#как_запустить)  
- [🔑 Авторизация](#авторизация)
- [🧱 Структура проекта](#структура_проекта) 

<a name="о_проекте"></a> 
<h2>Обзор проекта</h2>
Микросервис для управления банковскими счетами с полным циклом операций - создание, редактирование, удаление счетов, а также обработка финансовых транзакций.

<h3>Стек технологий</h3>

Backend: ASP.NET Core

База данных: PostgreSQL + Entity Framework Core (ORM)

Сервис авторизации: Keycloak

Инфраструктура: Docker (контейнеризация)

Архитектурные подходы:

MediatR (реализация паттерна Mediator для CQRS)

Hangfire (фоновая обработка задач)

Логирование: Serilog (структурированное логирование)

Межсервисное взаимодействие: RabbitMQ (асинхронная коммуникация)

<h3>Тестирование</h3>

Проект включает комплекс тестов:
<p>Модульные тесты для проверки бизнес-логики</p>
<p>Интеграционные тесты для проверки взаимодействия компонентов</p>
<p>Тесты покрывают основные сценарии работы с API</p>

<a name="о_api"></a> 
<h2>Краткий обзор API</h2>
<p>Здесь представлены все запросы поддерживаемые API</p>
<img width="1609" height="849" alt="image" src="https://github.com/user-attachments/assets/a58b1844-bc0c-49ba-9ac8-8166f8764f13" />
<p>Полная информация о API представлена по адресу сервера ./swagger</p>

<a name="как_запустить"></a> 
<h2>Рекомендуемый способ запуска проекта</h2>
<p>1)!!! Скачайте архив БД и распакуйте по пути ./.containers/</p>
https://drive.google.com/file/d/1jMmgsa5U2zb8qO3q7jKeXonPl8cdSva-/view?usp=sharing
<p>Если предложит заменить - замените все файлы</p>

<p>2) Выберите в VS запускаемый проект docker-compose</p>
<img width="258" height="100" alt="image" src="https://github.com/user-attachments/assets/3417513a-6b28-4aa9-804c-7bf414c22658" />
<p>3) Запустите проект с отладкой или без</p>
<img width="152" height="28" alt="image" src="https://github.com/user-attachments/assets/91ffbfef-7ecc-49e2-926e-4585741208c0" />
<p>4) Вы также можете запустить проект командой </p> docker-compose up -d
<p>❗ Если контейнер API закрывается с ошибкой, попробуйте запустить все остальные контейнеры и через 5-10 секунд сам API</p>
<p>❗ API само почему-то запускается только при ошибке, откройте самостоятельно localhost:80/swagger</p>
<p>Прошу прощения за такой костыль с контейнерами, возникли проблемы</p>

<a name="авторизация"></a> 
<h2>Как авторизоваться</h2>
<p>При нажатии кнопки Authorize в Swagger введите client_id = client</p>
<img width="241" height="68" alt="image" src="https://github.com/user-attachments/assets/0a083da8-2569-4390-bdf5-0071beaa13b6" />
<p>После нажатия кнопки Authorize вас перенаправит на форму регистрации. Пройдите регистрацию или войдите в аккаунт</p>
<img width="547" height="577" alt="image" src="https://github.com/user-attachments/assets/70f1377b-6848-4268-9e58-6e1da6773353" />
<p>При успешном входе система вернет валидный JWT токен</p>

<a name="структура_проекта"></a> 
<h2>Описание структуры проекта</h2>
<img width="215" height="432" alt="image" src="https://github.com/user-attachments/assets/83125a37-183d-409e-a51e-125442e08d7f" />

<ul>
<li>/Features</li>
* Содержит всю логику связанную с конкретной сущностью: команды, запросы, dto, хендлеры, мапперы, контроллеры и т.д.

<li>/Infrastructure 
* Cервисы БД, Начисления процентов, RabbitMQ и сервис валют.

<li>/HealthCheck 
* Реализование HealthCheck.
  
<li>/MbResult 
* Реализование патерна MbResult - сущность хранит и результат операции и ошибку.
  
<li>/Middleware</li>
* Middleware-обработчик исключений
* Middleware-логированние запросов

<li>/PipelineBehavior</li>
* ValidationBehavior - внедряет валидацию в конвейер MediatR
</ul>

<h2>ER схема БД</h2>
<img width="735" height="811" alt="image" src="https://github.com/user-attachments/assets/16e1efd7-773d-4f71-a1ea-d3f2be188208" />
