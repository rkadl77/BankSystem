# BankSystem
# Счета:
// Получить все счета клиента
GET    {API.core}/accounts/client/{clientId}

// Получить детали счета
GET    {API.core}/accounts/{accountId}

// Создать счет
POST   {API.core}/accounts
Body: {
  "clientId": "uuid",
  "currency": "RUB"  // или USD, EUR
}

// Закрыть счет
PUT    {API.core}/accounts/{accountId}/close

// Получить баланс
GET    {API.core}/accounts/{accountId}/balance

# Транзакции:
// История операций по счету
GET    {API.core}/transactions/account/{accountId}

// Пополнить счет
POST   {API.core}/transactions/deposit
Body: {
  "accountId": "uuid",
  "amount": 1000,
  "currency": "RUB",
  "description": "Пополнение"
}

// Снять со счета
POST   {API.core}/transactions/withdraw
Body: {
  "accountId": "uuid",
  "amount": 1000,
  "currency": "RUB",
  "description": "Снятие"
}

// Перевод между счетами
POST   {API.core}/transactions/transfer
Body: {
  "fromAccountId": "uuid",
  "toAccountId": "uuid",
  "amount": 1000,
  "currency": "RUB",
  "description": "Перевод"
}

# Пользователи
// Получить всех пользователей (для сотрудника)
GET     {API.users}/users

// Получить пользователя по ID
GET     {API.users}/users/{id}

// Получить пользователя по email
GET     {API.users}/users/email/{email}

// Проверить существование пользователя
GET     {API.users}/users/{id}/exists

// ПОЛНАЯ ИНФОРМАЦИЯ О КЛИЕНТЕ (счета + кредиты)
GET     {API.users}/users/{id}/details

// Создать пользователя
POST    {API.users}/users
Body: {
  "firstName": "Иван",
  "lastName": "Петров",
  "email": "ivan@mail.com",
  "phone": "+71234567890",
  "role": "client"  // client, employee, admin
}

// Обновить пользователя
PUT     {API.users}/users/{id}
Body: {
  "firstName": "Иван",
  "lastName": "Петров",
  "phone": "+71234567890",
  "role": "client",
  "isActive": true
}

# Сотрудники
// Получить всех сотрудников
GET     {API.users}/employees

// Получить сотрудника по ID
GET     {API.users}/employees/{id}

// Создать сотрудника
POST    {API.users}/employees
Body: {
  "firstName": "Петр",
  "lastName": "Сидоров",
  "email": "petr@bank.com",
  "phone": "+71112223344",
  "position": "Менеджер",
  "department": "Кредитный отдел"
}

// Уволить сотрудника
POST    {API.users}/employees/{id}/fire

# Тарифы
// Получить все тарифы
GET     {API.credit}/credittariffs

// Получить активные тарифы
GET     {API.credit}/credittariffs/active

// Создать тариф (только сотрудник)
POST    {API.credit}/credittariffs
Body: {
  "name": "Потребительский",
  "interestRate": 15.5,
  "description": "Кредит на любые цели"
}

# Кредиты
// Кредиты клиента
GET     {API.credit}/credits/client/{clientId}

// Детали кредита с историей платежей
GET     {API.credit}/credits/{creditId}

// Активные кредиты
GET     {API.credit}/credits/active

// Общая задолженность клиента
GET     {API.credit}/credits/client/{clientId}/total-debt

// Взять кредит
POST    {API.credit}/credits
Body: {
  "clientId": "uuid",
  "accountId": "uuid",     // счет для списаний
  "tariffId": "uuid",      // выбранный тариф
  "amount": 100000
}

// Погасить кредит
POST    {API.credit}/credits/repay
Body: {
  "creditId": "uuid",
  "amount": 10000
}
