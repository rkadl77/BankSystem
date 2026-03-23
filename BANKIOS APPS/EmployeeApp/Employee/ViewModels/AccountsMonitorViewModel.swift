import Foundation
import Combine

@MainActor
final class AccountsMonitorViewModel: ObservableObject {
    @Published var transactions: [TransactionDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?

    private var currentAccountId: UUID?
    private let ws = TransactionWebSocketService()

    func loadTransactions(for accountId: UUID) {
        currentAccountId = accountId
        Task {
            isLoading = true; errorMessage = nil
            do { transactions = try await TransactionService.shared.getTransactions(accountId: accountId) }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }

    func startRealTime(for accountId: UUID) {
        ws.connect(accountId: accountId) { [weak self] in
            self?.loadTransactions(for: accountId)
        }
    }

    func stopRealTime() { ws.disconnect() }

    deinit { MainActor.assumeIsolated { ws.disconnect() } }
}
