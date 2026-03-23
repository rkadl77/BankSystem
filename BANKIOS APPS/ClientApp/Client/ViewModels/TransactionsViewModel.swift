import Foundation
import Combine

@MainActor
final class TransactionsViewModel: ObservableObject {
    @Published var transactions: [TransactionDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?

    private let accountId: UUID
    private let ws = TransactionWebSocketService()

    init(accountId: UUID) { self.accountId = accountId }

    func load() {
        Task {
            isLoading = true; errorMessage = nil
            do { transactions = try await TransactionService.shared.getTransactions(accountId: accountId) }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }

    func startRealTime() {
        ws.connect(accountId: accountId) { [weak self] in
            self?.load()
        }
    }

    func stopRealTime() {
        ws.disconnect()
    }

    deinit { MainActor.assumeIsolated { ws.disconnect() } }
}
