import Foundation
import Combine

@MainActor
final class TransferViewModel: ObservableObject {
    @Published var toAccountIdText = ""
    @Published var amountText = ""
    @Published var descriptionText = ""
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var successMessage: String?

    func transfer(from account: AccountDTO, onSuccess: (() -> Void)? = nil) {
        let toIdStr = toAccountIdText.trimmingCharacters(in: .whitespaces)
        guard let toId = UUID(uuidString: toIdStr) else {
            errorMessage = "Некорректный ID счёта получателя"; return
        }
        guard let amount = Double(amountText.replacingOccurrences(of: ",", with: ".")),
              amount > 0 else {
            errorMessage = "Введите корректную сумму"; return
        }
        guard amount <= account.balance else {
            errorMessage = "Недостаточно средств на счёте"; return
        }

        Task {
            isLoading = true; errorMessage = nil
            do {
                let desc = descriptionText.trimmingCharacters(in: .whitespaces)
                try await TransactionService.shared.transfer(
                    fromAccountId: account.id,
                    toAccountId: toId,
                    amount: amount,
                    currency: account.currency,
                    description: desc.isEmpty ? nil : desc)
                successMessage = "Перевод \(String(format: "%.2f", amount)) \(account.currencySymbol) выполнен ✓"
                toAccountIdText = ""; amountText = ""; descriptionText = ""
                onSuccess?()
            }
            catch NetworkError.serverError(400, let msg) {
                errorMessage = msg.flatMap { $0.isEmpty ? nil : $0 } ?? "Недостаточно средств"
            }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }
}
