//
//  MockDataService.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import Foundation
import Combine

// MARK: — Singleton mock database

final class MockDataService: ObservableObject {
    static let shared = MockDataService()
    
    @Published var users: [User] = []
    @Published var accounts: [Account] = []
    @Published var transactions: [Transaction] = []
    @Published var credits: [Credit] = []
    @Published var tariffs: [CreditTariff] = []
    
    private init() {
        seed()
    }
    
    // MARK: — Seed
    
    private func seed() {
        // --- Employees ---
        let emp1 = User(id: UUID(uuidString: "00000000-0000-0000-0000-000000000001")!,
                        fullName: "Иванова Анна Сергеевна",
                        email: "ivanova@bank.ru",
                        phone: "+7 999 100-00-01",
                        role: .employee)
        
        // --- Clients ---
        let c1 = User(id: UUID(uuidString: "10000000-0000-0000-0000-000000000001")!,
                      fullName: "Петров Алексей Иванович",
                      email: "petrov@mail.ru",
                      phone: "+7 900 100-00-01",
                      role: .client)
        let c2 = User(id: UUID(uuidString: "10000000-0000-0000-0000-000000000002")!,
                      fullName: "Смирнова Дарья Олеговна",
                      email: "smirnova@mail.ru",
                      phone: "+7 900 100-00-02",
                      role: .client)
        let c3 = User(id: UUID(uuidString: "10000000-0000-0000-0000-000000000003")!,
                      fullName: "Козлов Михаил Дмитриевич",
                      email: "kozlov@mail.ru",
                      phone: "+7 900 100-00-03",
                      role: .client,
                      status: .blocked)
        
        users = [emp1, c1, c2, c3]
        
        // --- Tariffs ---
        let t1 = CreditTariff(id: UUID(uuidString: "20000000-0000-0000-0000-000000000001")!,
                               name: "Стандартный",
                               interestRate: 12.5,
                               createdByEmployeeId: emp1.id)
        let t2 = CreditTariff(id: UUID(uuidString: "20000000-0000-0000-0000-000000000002")!,
                               name: "Льготный",
                               interestRate: 7.0,
                               maxAmount: 500_000,
                               createdByEmployeeId: emp1.id)
        let t3 = CreditTariff(id: UUID(uuidString: "20000000-0000-0000-0000-000000000003")!,
                               name: "Экспресс",
                               interestRate: 20.0,
                               maxAmount: 100_000,
                               maxTermDays: 90,
                               createdByEmployeeId: emp1.id)
        tariffs = [t1, t2, t3]
        
        // --- Accounts for c1 ---
        var acc1 = Account(id: UUID(uuidString: "30000000-0000-0000-0000-000000000001")!,
                            ownerId: c1.id,
                            accountNumber: "40817810000000000001",
                            type: .checking,
                            balance: 45_250.75)
        var acc2 = Account(id: UUID(uuidString: "30000000-0000-0000-0000-000000000002")!,
                            ownerId: c1.id,
                            accountNumber: "40817810000000000002",
                            type: .savings,
                            balance: 120_000.00)
        
        // --- Accounts for c2 ---
        var acc3 = Account(id: UUID(uuidString: "30000000-0000-0000-0000-000000000003")!,
                            ownerId: c2.id,
                            accountNumber: "40817810000000000003",
                            type: .checking,
                            balance: 8_300.00)
        
        accounts = [acc1, acc2, acc3]
        
        // --- Transactions for acc1 ---
        let now = Date()
        let txs: [Transaction] = [
            Transaction(accountId: acc1.id, type: .deposit, amount: 50_000, balanceAfter: 50_000,
                        description: "Зачисление зарплаты",
                        createdAt: Calendar.current.date(byAdding: .day, value: -30, to: now)!),
            Transaction(accountId: acc1.id, type: .withdrawal, amount: 5_000, balanceAfter: 45_000,
                        description: "Снятие наличных",
                        createdAt: Calendar.current.date(byAdding: .day, value: -20, to: now)!),
            Transaction(accountId: acc1.id, type: .deposit, amount: 3_000, balanceAfter: 48_000,
                        description: "Возврат",
                        createdAt: Calendar.current.date(byAdding: .day, value: -10, to: now)!),
            Transaction(accountId: acc1.id, type: .withdrawal, amount: 2_749.25, balanceAfter: 45_250.75,
                        description: "Оплата подписки",
                        createdAt: Calendar.current.date(byAdding: .day, value: -3, to: now)!),
            
            // acc2
            Transaction(accountId: acc2.id, type: .deposit, amount: 120_000, balanceAfter: 120_000,
                        description: "Пополнение накоплений",
                        createdAt: Calendar.current.date(byAdding: .day, value: -60, to: now)!),
            
            // acc3
            Transaction(accountId: acc3.id, type: .deposit, amount: 20_000, balanceAfter: 20_000,
                        description: "Первоначальное пополнение",
                        createdAt: Calendar.current.date(byAdding: .day, value: -14, to: now)!),
            Transaction(accountId: acc3.id, type: .withdrawal, amount: 11_700, balanceAfter: 8_300,
                        description: "Снятие",
                        createdAt: Calendar.current.date(byAdding: .day, value: -5, to: now)!)
        ]
        transactions = txs
        
        // --- Credit for c1 ---
        let cr1 = Credit(clientId: c1.id,
                         accountId: acc1.id,
                         tariffId: t1.id,
                         amount: 100_000,
                         interestRate: 12.5,
                         termDays: 90,
                         startDate: Calendar.current.date(byAdding: .day, value: -15, to: now)!)
        credits = [cr1]
    }
    
    // MARK: — Helpers
    
    func accounts(for userId: UUID) -> [Account] {
        accounts.filter { $0.ownerId == userId }
    }
    
    func transactions(for accountId: UUID) -> [Transaction] {
        transactions.filter { $0.accountId == accountId }
            .sorted { $0.createdAt > $1.createdAt }
    }
    
    func credits(for clientId: UUID) -> [Credit] {
        credits.filter { $0.clientId == clientId }
    }
    
    func user(by id: UUID) -> User? {
        users.first { $0.id == id }
    }
    
    func clients() -> [User] {
        users.filter { $0.role == .client }
    }
    
    func employees() -> [User] {
        users.filter { $0.role == .employee }
    }
    
    func activeTariffs() -> [CreditTariff] {
        tariffs.filter { $0.isActive }
    }
    
    // MARK: — Mutations
    
    func openAccount(for userId: UUID, type: AccountType) {
        let acc = Account(ownerId: userId, type: type)
        accounts.append(acc)
    }
    
    func closeAccount(id: UUID) {
        guard let idx = accounts.firstIndex(where: { $0.id == id }) else { return }
        accounts[idx].status = .closed
        accounts[idx].closedAt = Date()
    }
    
    func deposit(accountId: UUID, amount: Double, description: String = "Пополнение") {
        guard let idx = accounts.firstIndex(where: { $0.id == accountId }) else { return }
        accounts[idx].balance += amount
        let tx = Transaction(accountId: accountId, type: .deposit, amount: amount,
                              balanceAfter: accounts[idx].balance, description: description)
        transactions.append(tx)
    }
    
    func withdraw(accountId: UUID, amount: Double, description: String = "Снятие") -> Bool {
        guard let idx = accounts.firstIndex(where: { $0.id == accountId }),
              accounts[idx].balance >= amount else { return false }
        accounts[idx].balance -= amount
        let tx = Transaction(accountId: accountId, type: .withdrawal, amount: amount,
                              balanceAfter: accounts[idx].balance, description: description)
        transactions.append(tx)
        return true
    }
    
    func takeCredit(clientId: UUID, accountId: UUID, tariffId: UUID,
                    amount: Double, termDays: Int) -> Bool {
        guard let tariff = tariffs.first(where: { $0.id == tariffId }),
              amount >= tariff.minAmount,
              amount <= tariff.maxAmount,
              termDays >= tariff.minTermDays,
              termDays <= tariff.maxTermDays else { return false }
        
        let credit = Credit(clientId: clientId, accountId: accountId,
                            tariffId: tariffId, amount: amount,
                            interestRate: tariff.interestRate, termDays: termDays)
        credits.append(credit)
        deposit(accountId: accountId, amount: amount, description: "Выдача кредита")
        return true
    }
    
    func repayCredit(creditId: UUID, accountId: UUID, amount: Double) -> Bool {
        guard let cIdx = credits.firstIndex(where: { $0.id == creditId }),
              let aIdx = accounts.firstIndex(where: { $0.id == accountId }),
              accounts[aIdx].balance >= amount else { return false }
        
        accounts[aIdx].balance -= amount
        credits[cIdx].remainingAmount = max(0, credits[cIdx].remainingAmount - amount)
        
        let tx = Transaction(accountId: accountId, type: .creditPayment, amount: amount,
                              balanceAfter: accounts[aIdx].balance,
                              description: "Погашение кредита")
        transactions.append(tx)
        
        if credits[cIdx].remainingAmount <= 0 {
            credits[cIdx].status = .closed
        }
        return true
    }
    
    func createTariff(name: String, rate: Double, employeeId: UUID) {
        let t = CreditTariff(name: name, interestRate: rate, createdByEmployeeId: employeeId)
        tariffs.append(t)
    }
    
    func createUser(fullName: String, email: String, phone: String, role: UserRole) -> User {
        let u = User(fullName: fullName, email: email, phone: phone, role: role)
        users.append(u)
        return u
    }
    
    func toggleUserBlock(userId: UUID) {
        guard let idx = users.firstIndex(where: { $0.id == userId }) else { return }
        users[idx].status = users[idx].status == .active ? .blocked : .active
    }
    
    func login(email: String) -> User? {
        users.first { $0.email.lowercased() == email.lowercased() && $0.status == .active }
    }
}
