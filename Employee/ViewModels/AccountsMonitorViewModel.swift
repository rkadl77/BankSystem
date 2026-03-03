//
//  AccountsMonitorViewModel.swift
//  Employee
//
//  Created by Gleb Korotkov on 01.03.2026.
//

import SwiftUI
import Foundation
import Combine

@MainActor
final class AccountsMonitorViewModel: ObservableObject {
    @Published var transactions: [TransactionDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?

    func loadTransactions(for accountId: UUID) {
        Task {
            isLoading = true
            do { transactions = try await TransactionService.shared.getTransactions(accountId: accountId) }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }
}
