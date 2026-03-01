//
//  TransactionsViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import Foundation
import Combine

@MainActor
final class TransactionsViewModel: ObservableObject {
    @Published var transactions: [TransactionDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?

    private let accountId: UUID
    init(accountId: UUID) { self.accountId = accountId }

    func load() {
        Task {
            isLoading = true
            do { transactions = try await TransactionService.shared.getTransactions(accountId: accountId) }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }
}
