//
//  ClientDetailViewModel.swift
//  Employee
//
//  Created by Gleb Korotkov on 01.03.2026.
//

import SwiftUI
import Combine
import Foundation

@MainActor
final class ClientDetailViewModel: ObservableObject {
    @Published var accounts: [AccountDTO] = []
    @Published var credits: [CreditDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?

    func load(clientId: UUID) {
        Task {
            isLoading = true; errorMessage = nil
            async let accs  = AccountService.shared.getAccounts(clientId: clientId)
            async let crds  = CreditService.shared.getCredits(clientId: clientId)
            do {
                accounts = try await accs
                credits  = try await crds
            } catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }
}
