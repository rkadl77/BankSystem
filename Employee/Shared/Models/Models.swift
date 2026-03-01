//
//  AccountStatus.swift
//  Client
//
//  Created by Gleb Korotkov on 25.03.2026.
//



import Foundation

enum AccountStatus: String, Codable, CaseIterable {
    case active = "Активный"
    case closed = "Закрыт"
}

enum AccountType: String, Codable, CaseIterable {
    case checking = "Расчётный"
    case savings = "Накопительный"
    case credit = "Кредитный"
}

struct Account: Identifiable, Codable, Equatable {
    let id: UUID
    let ownerId: UUID
    var accountNumber: String
    var type: AccountType
    var balance: Double
    var currency: String
    var status: AccountStatus
    var openedAt: Date
    var closedAt: Date?
    
    init(
        id: UUID = UUID(),
        ownerId: UUID,
        accountNumber: String = Account.generateNumber(),
        type: AccountType = .checking,
        balance: Double = 0,
        currency: String = "RUB",
        status: AccountStatus = .active,
        openedAt: Date = Date(),
        closedAt: Date? = nil
    ) {
        self.id = id
        self.ownerId = ownerId
        self.accountNumber = accountNumber
        self.type = type
        self.balance = balance
        self.currency = currency
        self.status = status
        self.openedAt = openedAt
        self.closedAt = closedAt
    }
    
    static func generateNumber() -> String {
        let digits = (0..<20).map { _ in String(Int.random(in: 0...9)) }.joined()
        return digits
    }
    
    var maskedNumber: String {
        let last4 = String(accountNumber.suffix(4))
        return "•••• \(last4)"
    }
    
    var formattedBalance: String {
        let formatter = NumberFormatter()
        formatter.numberStyle = .currency
        formatter.currencyCode = currency
        formatter.locale = Locale(identifier: "ru_RU")
        return formatter.string(from: NSNumber(value: balance)) ?? "\(balance) ₽"
    }
    
    var isActive: Bool { status == .active }
}

// ============================================================
// FILE: Shared/Models/Credit.swift
// ============================================================

import Foundation

struct CreditTariff: Identifiable, Codable, Equatable {
    let id: UUID
    var name: String
    var interestRate: Double  // % per year
    var minAmount: Double
    var maxAmount: Double
    var minTermDays: Int
    var maxTermDays: Int
    var isActive: Bool
    var createdAt: Date
    var createdByEmployeeId: UUID
    
    init(
        id: UUID = UUID(),
        name: String,
        interestRate: Double,
        minAmount: Double = 10_000,
        maxAmount: Double = 1_000_000,
        minTermDays: Int = 30,
        maxTermDays: Int = 365,
        isActive: Bool = true,
        createdAt: Date = Date(),
        createdByEmployeeId: UUID
    ) {
        self.id = id
        self.name = name
        self.interestRate = interestRate
        self.minAmount = minAmount
        self.maxAmount = maxAmount
        self.minTermDays = minTermDays
        self.maxTermDays = maxTermDays
        self.isActive = isActive
        self.createdAt = createdAt
        self.createdByEmployeeId = createdByEmployeeId
    }
    
    var formattedRate: String { String(format: "%.1f%%", interestRate) }
}

enum CreditStatus: String, Codable, CaseIterable {
    case active = "Активный"
    case closed = "Закрыт"
    case overdue = "Просрочен"
}

struct Credit: Identifiable, Codable, Equatable {
    let id: UUID
    let clientId: UUID
    let accountId: UUID
    let tariffId: UUID
    var amount: Double
    var remainingAmount: Double
    var interestRate: Double
    var startDate: Date
    var endDate: Date
    var status: CreditStatus
    var nextPaymentDate: Date
    var dailyPayment: Double  // т.к. платёж раз в день
    
    init(
        id: UUID = UUID(),
        clientId: UUID,
        accountId: UUID,
        tariffId: UUID,
        amount: Double,
        interestRate: Double,
        termDays: Int,
        startDate: Date = Date()
    ) {
        self.id = id
        self.clientId = clientId
        self.accountId = accountId
        self.tariffId = tariffId
        self.amount = amount
        self.interestRate = interestRate
        self.startDate = startDate
        self.endDate = Calendar.current.date(byAdding: .day, value: termDays, to: startDate) ?? startDate
        self.status = .active
        
        let totalWithInterest = amount * (1 + interestRate / 100 * Double(termDays) / 365)
        self.remainingAmount = totalWithInterest
        self.dailyPayment = totalWithInterest / Double(termDays)
        self.nextPaymentDate = Calendar.current.date(byAdding: .day, value: 1, to: startDate) ?? startDate
    }
    
    var formattedAmount: String {
        String(format: "%.2f ₽", amount)
    }
    
    var formattedRemaining: String {
        String(format: "%.2f ₽", remainingAmount)
    }
    
    var progressFraction: Double {
        guard amount > 0 else { return 0 }
        let totalWithInterest = amount * (1 + interestRate / 100)
        let paid = totalWithInterest - remainingAmount
        return max(0, min(1, paid / totalWithInterest))
    }
}



enum TransactionType: String, Codable, CaseIterable {
    case deposit = "Пополнение"
    case withdrawal = "Снятие"
    case creditIssue = "Выдача кредита"
    case creditPayment = "Погашение кредита"
    case transfer = "Перевод"
}

struct Transaction: Identifiable, Codable, Equatable {
    let id: UUID
    let accountId: UUID
    var type: TransactionType
    var amount: Double
    var balanceAfter: Double
    var description: String
    var createdAt: Date
    
    init(
        id: UUID = UUID(),
        accountId: UUID,
        type: TransactionType,
        amount: Double,
        balanceAfter: Double,
        description: String = "",
        createdAt: Date = Date()
    ) {
        self.id = id
        self.accountId = accountId
        self.type = type
        self.amount = amount
        self.balanceAfter = balanceAfter
        self.description = description
        self.createdAt = createdAt
    }
    
    var isCredit: Bool {
        type == .deposit || type == .creditIssue
    }
    
    var sign: String { isCredit ? "+" : "−" }
    
    var formattedAmount: String {
        let formatter = NumberFormatter()
        formatter.numberStyle = .decimal
        formatter.minimumFractionDigits = 2
        formatter.maximumFractionDigits = 2
        let str = formatter.string(from: NSNumber(value: amount)) ?? String(format: "%.2f", amount)
        return "\(sign)\(str) ₽"
    }
    
    var iconName: String {
        switch type {
        case .deposit: return "arrow.down.circle.fill"
        case .withdrawal: return "arrow.up.circle.fill"
        case .creditIssue: return "banknote.fill"
        case .creditPayment: return "checkmark.circle.fill"
        case .transfer: return "arrow.left.arrow.right.circle.fill"
        }
    }
}



enum UserRole: String, Codable {
    case client
    case employee
}

enum UserStatus: String, Codable {
    case active
    case blocked
}

struct User: Identifiable, Codable, Equatable {
    let id: UUID
    var fullName: String
    var email: String
    var phone: String
    var role: UserRole
    var status: UserStatus
    var createdAt: Date
    
    init(
        id: UUID = UUID(),
        fullName: String,
        email: String,
        phone: String,
        role: UserRole,
        status: UserStatus = .active,
        createdAt: Date = Date()
    ) {
        self.id = id
        self.fullName = fullName
        self.email = email
        self.phone = phone
        self.role = role
        self.status = status
        self.createdAt = createdAt
    }
    
    var initials: String {
        let parts = fullName.split(separator: " ")
        return parts.prefix(2).compactMap { $0.first }.map(String.init).joined()
    }
    
    var isBlocked: Bool { status == .blocked }
}
