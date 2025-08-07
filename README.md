При нажатии кнопки Authorize в Swagger введите client_id = client

<h2>Описание структуры проекта</h2>
<ul>
<li>/Features</li>
* Содержит всю логику связанную с конкретной сущностью: команды, запросы, dto, хендлеры, мапперы, контроллеры и т.д.

<li>/Infrastructure <p>* Заглушки сервисов аутентификации и валют.</p><ul><li>/Data</li>
* Заглушка БД "MyDataContext"
* Репозитории, реализующие CRUD операции, обращающиеся к "БД"</ul></li>

<li>/Middleware</li>
* Middleware-обработчик исключений

<li>/PipelineBehavior</li>
* ValidationBehavior - внедряет валидацию в конвейер MediatR
</ul>
