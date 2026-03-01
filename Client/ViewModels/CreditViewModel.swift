//
//  CreditViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import Foundation
import Combine

@MainActor
final class CreditViewModel: ObservableObject {
    @Published var credits: [CreditDTO] = []
    @Published var tariffs: [CreditTariffDTO] = []
    @Published var isLoading = false
    @Published var isSubmitting = false
    @Published var errorMessage: String?
    @Published var successMessage: String?

    // form
    @Published var selectedTariff: CreditTariffDTO?
    @Published var selectedAccountId: UUID?
    @Published var amountText = ""

    private let clientId: UUID
    init(clientId: UUID) { self.clientId = clientId }

    func load() {
        Task {
            isLoading = true; errorMessage = nil
            async let c = CreditService.shared.getCredits(clientId: clientId)
            async let t = CreditService.shared.getActiveTariffs()
            do { credits = try await c; tariffs = try await t }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }

    @discardableResult
    func submitTakeCredit() -> Bool {
        guard selectedTariff != nil, selectedAccountId != nil,
              let amount = Double(amountText), amount > 0
        else { errorMessage = "Заполните все поля"; return false }

        Task {
            isSubmitting = true; errorMessage = nil
            do {
                let c = try await CreditService.shared.takeCredit(
                    clientId: clientId, accountId: selectedAccountId!,
                    tariffId: selectedTariff!.id, amount: amount)
                credits.append(c)
                selectedTariff = nil; selectedAccountId = nil; amountText = ""
                successMessage = "Кредит \(c.formattedAmount) оформлен ✓"
            }
            catch NetworkError.serverError(400, let m) { errorMessage = m ?? "Ошибка оформления" }
            catch { errorMessage = error.localizedDescription }
            isSubmitting = false
        }
        return true
    }

    func repay(_ credit: CreditDTO, amount: Double) {
        Task {
            do {
                try await CreditService.shared.repayCredit(creditId: credit.id, amount: amount)
                load(); successMessage = "Платёж внесён ✓"
            }
            catch NetworkError.serverError(400, _) { errorMessage = "Недостаточно средств" }
            catch { errorMessage = error.localizedDescription }
        }
    }
}
