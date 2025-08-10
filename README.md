<h2>О проекте</h2>
<p>Проект риеализует микросервис счетов. Позволяет выполнять CRUD операции над счетами и транзакциями, а также позволяет совершать переводы между счетами</p>
<p>Все операции защищены - неавторизованный пользователь не может получить доступ к операциям</p>
<p>Информация о счетах и их транзакциях хранится в БД PostgreSQL</p>
<p>Ежедневные начисления процентов на депозитные счета производится благодаря Cron-Job Hangfire</p>

<h2>Рекомендуемый способ запуска проекта</h2>
<p>1) Выберите в VS запускаемый проект docker-compose</p>
<img width="258" height="100" alt="image" src="https://github.com/user-attachments/assets/3417513a-6b28-4aa9-804c-7bf414c22658" />
<p>2) Запустите проект с отладкой или без</p>
<img width="152" height="28" alt="image" src="https://github.com/user-attachments/assets/91ffbfef-7ecc-49e2-926e-4585741208c0" />
<p>3) Вы также можете запустить проект командой </p> docker-compose up -d

<h2>Как авторизоваться</h2>
<p>При нажатии кнопки Authorize в Swagger введите client_id = client</p>
<img width="241" height="68" alt="image" src="https://github.com/user-attachments/assets/0a083da8-2569-4390-bdf5-0071beaa13b6" />
<p>После нажатия кнопки Authorize вас перенаправит на форму регистрации. Пройдите регистрацию или войдите в аккаунт</p>
<img width="547" height="577" alt="image" src="https://github.com/user-attachments/assets/70f1377b-6848-4268-9e58-6e1da6773353" />
<p>При успешном входе система вернет валидный JWT токен</p>


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
