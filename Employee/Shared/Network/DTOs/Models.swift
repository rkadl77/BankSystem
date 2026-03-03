//
//  UserDTO.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//


import SwiftUI

struct UserDTO: Codable, Identifiable {
    let id: UUID
    let firstName: String
    let lastName: String
    let email: String
    let phone: String
    let role: String        // "client" | "employee" | "admin"
    let isActive: Bool
    let createdAt: Date

    var fullName: String { "\(firstName) \(lastName)" }
    var initials: String {
        (firstName.first.map(String.init) ?? "") +
        (lastName.first.map(String.init) ?? "")
    }
}

struct EmployeeDTO: Codable, Identifiable {
    let id: UUID
    let userId: UUID
    let firstName: String
    let lastName: String
    let email: String
    let position: String
    let department: String
    let hireDate: Date
    let isActive: Bool

    var fullName: String { "\(firstName) \(lastName)" }
    var initials: String {
        (firstName.first.map(String.init) ?? "") +
        (lastName.first.map(String.init) ?? "")
    }
}


struct AccountDTO: Codable, Identifiable {
    let id: UUID
    let accountNumber: String
    let balance: Double
    let currency: String
    let createdAt: Date
    let isActive: Bool

    var maskedNumber: String { "•••• \(accountNumber.suffix(4))" }
    var currencySymbol: String {
        switch currency.uppercased() {
        case "RUB": return "₽"
        case "USD": return "$"
        case "EUR": return "€"
        default:    return currency
        }
    }
    var formattedBalance: String {
        let f = NumberFormatter()
        f.numberStyle = .decimal
        f.minimumFractionDigits = 2
        f.maximumFractionDigits = 2
        return (f.string(from: NSNumber(value: balance)) ?? String(format: "%.2f", balance))
               + " " + currencySymbol
    }
}


struct TransactionDTO: Codable, Identifiable {
    let id: UUID
    let amount: Double
    let currency: String
    let type: String        // "deposit" | "withdrawal" | "transfer"
    let status: String
    let timestamp: Date
    let description: String?
    let relatedTransactionId: UUID?

    var isIncoming: Bool { type.lowercased() == "deposit" }
    var formattedAmount: String {
        String(format: "%@%.2f ₽", isIncoming ? "+" : "−", amount)
    }
    var typeLocalizedName: String {
        switch type.lowercased() {
        case "deposit":    return "Пополнение"
        case "withdrawal": return "Снятие"
        case "transfer":   return "Перевод"
        default:           return type
        }
    }
    var typeColor: Color {
        switch type.lowercased() {
        case "deposit":    return .bankSuccess
        case "withdrawal": return .bankDanger
        case "transfer":   return .bankAccent
        default:           return .secondary
        }
    }
    var iconName: String {
        switch type.lowercased() {
        case "deposit":    return "arrow.down.circle.fill"
        case "withdrawal": return "arrow.up.circle.fill"
        case "transfer":   return "arrow.left.arrow.right.circle.fill"
        default:           return "circle.fill"
        }
    }
}


struct CreditDTO: Codable, Identifiable {
    let id: UUID
    let clientId: UUID
    let accountId: UUID
    let tariffName: String
    let amount: Double
    let remainingAmount: Double
    let interestRate: Double
    let startDate: Date
    let endDate: Date?
    let status: String      // "active" | "closed" | "overdue"

    var progressFraction: Double {
        guard amount > 0 else { return 0 }
        return max(0, min(1, (amount - remainingAmount) / amount))
    }
    var formattedAmount:    String { String(format: "%.2f ₽", amount) }
    var formattedRemaining: String { String(format: "%.2f ₽", remainingAmount) }
    var formattedRate:      String { String(format: "%.1f%%", interestRate) }
    var statusColor: Color {
        switch status.lowercased() {
        case "active":  return .bankSuccess
        case "closed":  return .secondary
        case "overdue": return .bankDanger
        default:        return .secondary
        }
    }
    var statusLabel: String {
        switch status.lowercased() {
        case "active":  return "Активный"
        case "closed":  return "Закрыт"
        case "overdue": return "Просрочен"
        default:        return status
        }
    }
}

struct CreditDetailsDTO: Codable, Identifiable {
    let id: UUID
    let clientId: UUID
    let accountId: UUID
    let tariffName: String
    let amount: Double
    let remainingAmount: Double
    let interestRate: Double
    let startDate: Date
    let endDate: Date?
    let status: String
    let payments: [CreditPaymentDTO]
}

struct CreditPaymentDTO: Codable, Identifiable {
    let id: UUID
    let creditId: UUID
    let amount: Double
    let paymentDate: Date
    let status: String
}

struct CreditTariffDTO: Codable, Identifiable, Equatable {
    let id: UUID
    let name: String
    let interestRate: Double
    let description: String
    let createdAt: Date
    let isActive: Bool
    var formattedRate: String { String(format: "%.1f%%", interestRate) }
}


struct CreateAccountRequest: Encodable {
    let clientId: UUID
    let currency: String
}

struct CreateTransactionRequest: Encodable {
    let accountId: UUID
    let amount: Double
    let currency: String
    let type: String
    let description: String?
}

struct CreateCreditRequest: Encodable {
    let clientId: UUID
    let accountId: UUID
    let tariffId: UUID
    let amount: Double
}

struct RepayCreditRequest: Encodable {
    let creditId: UUID
    let amount: Double
}

struct CreateUserRequest: Encodable {
    let firstName: String
    let lastName: String
    let email: String
    let phone: String
    let role: String
}

struct UpdateUserRequest: Encodable {
    let firstName: String
    let lastName: String
    let phone: String
    let role: String
    let isActive: Bool
}

struct CreateEmployeeRequest: Encodable {
    let firstName: String
    let lastName: String
    let email: String
    let phone: String
    let position: String
    let department: String
}

struct CreateCreditTariffRequest: Encodable {
    let name: String
    let interestRate: Double
    let description: String
}

struct UpdateCreditTariffRequest: Encodable {
    let name: String
    let interestRate: Double
    let description: String
    let isActive: Bool
}
