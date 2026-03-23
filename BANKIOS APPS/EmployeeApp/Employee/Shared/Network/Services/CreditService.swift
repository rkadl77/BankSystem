import Foundation

final class CreditService {
    static let shared = CreditService()
    private let api = APIClient.shared

    func getActiveTariffs() async throws -> [CreditTariffDTO] {
        try await api.get("\(API.credit)/CreditTariffs/active")
    }
    func getAllTariffs() async throws -> [CreditTariffDTO] {
        try await api.get("\(API.credit)/CreditTariffs")
    }
    func createTariff(name: String, interestRate: Double,
                      description: String) async throws -> CreditTariffDTO {
        try await api.post("\(API.credit)/CreditTariffs",
                           body: CreateCreditTariffRequest(name: name, interestRate: interestRate,
                                                            description: description))
    }
    func updateTariff(id: UUID, name: String, interestRate: Double,
                      description: String, isActive: Bool) async throws -> CreditTariffDTO {
        try await api.put("\(API.credit)/CreditTariffs/\(id)",
                          body: UpdateCreditTariffRequest(name: name, interestRate: interestRate,
                                                           description: description, isActive: isActive))
    }
    func getCredits(clientId: UUID) async throws -> [CreditDTO] {
        try await api.get("\(API.credit)/Credits/client/\(clientId)")
    }
    func getActiveCredits() async throws -> [CreditDTO] {
        try await api.get("\(API.credit)/Credits/active")
    }
    func getCreditDetails(id: UUID) async throws -> CreditDetailsDTO {
        try await api.get("\(API.credit)/Credits/\(id)")
    }
    func getTotalDebt(clientId: UUID) async throws -> Double {
        try await api.get("\(API.credit)/Credits/client/\(clientId)/total-debt")
    }
    func takeCredit(clientId: UUID, accountId: UUID,
                    tariffId: UUID, amount: Double) async throws -> CreditDTO {
        try await api.post("\(API.credit)/Credits",
                           body: CreateCreditRequest(clientId: clientId, accountId: accountId,
                                                      tariffId: tariffId, amount: amount))
    }
    func repayCredit(creditId: UUID, amount: Double) async throws {
        try await api.postVoid("\(API.credit)/Credits/repay",
                               body: RepayCreditRequest(creditId: creditId, amount: amount))
    }
    func getOverdueCredits(clientId: UUID) async throws -> [CreditDTO] {
        try await api.get("\(API.credit)/Credits/client/\(clientId)/overdue")
    }
    func getCreditRating(clientId: UUID) async throws -> CreditRatingDTO {
        try await api.get("\(API.credit)/Credits/client/\(clientId)/rating")
    }
}
