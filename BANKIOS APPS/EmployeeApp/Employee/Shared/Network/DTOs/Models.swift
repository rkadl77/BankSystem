import SwiftUI

// MARK: - User / Employee

struct UserDTO: Codable, Identifiable {
    let id: UUID
    let firstName: String
    let lastName: String
    let email: String
    let phone: String
    let role: String
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

// MARK: - Account

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

// MARK: - Transaction

struct TransactionDTO: Codable, Identifiable {
    let id: UUID
    let amount: Double
    let currency: String
    let type: String
    let status: String
    let timestamp: Date
    let description: String?
    let relatedTransactionId: UUID?
    let conversionRate: Double?
    let originalCurrency: String?
    let targetCurrency: String?

    var isIncoming: Bool { type.lowercased() == "deposit" }
    var formattedAmount: String {
        let sign = isIncoming ? "+" : "−"
        let sym  = currencySymbol(currency)
        return "\(sign)\(String(format: "%.2f", amount)) \(sym)"
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
    private func currencySymbol(_ code: String) -> String {
        switch code.uppercased() {
        case "RUB": return "₽"
        case "USD": return "$"
        case "EUR": return "€"
        default:    return code
        }
    }
}

// MARK: - Credit

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
    let status: String

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

// MARK: - Credit Rating

struct CreditRatingDTO: Codable {
    let clientId: UUID
    let rating: Int
    let description: String?
    let overdueCount: Int
    let totalCount: Int
    let onTimePercentage: Double

    var ratingLabel: String {
        switch rating {
        case 80...100: return "Отличный"
        case 60..<80:  return "Хороший"
        case 40..<60:  return "Удовлетворительный"
        case 20..<40:  return "Плохой"
        default:       return "Очень плохой"
        }
    }
    var ratingColor: Color {
        switch rating {
        case 75...100: return .bankSuccess
        case 50..<75:  return .bankAccent
        case 25..<50:  return .bankWarning
        default:       return .bankDanger
        }
    }
}

// MARK: - Settings

struct UserSettingsDTO: Codable, Identifiable {
    let id: UUID
    let userId: UUID
    let theme: String?
    let hiddenAccountIds: [UUID]?
    let updatedAt: Date
}

// MARK: - Request DTOs

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

struct TransferRequest: Encodable {
    let fromAccountId: UUID
    let toAccountId: UUID
    let amount: Double
    let currency: String
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

struct CreateSettingsRequest: Encodable {
    let theme: String?
    let hiddenAccountIds: [UUID]?
}

struct UpdateThemeRequest: Encodable {
    let theme: String
}

struct UpdateHiddenAccountsRequest: Encodable {
    let accountIds: [UUID]
}
