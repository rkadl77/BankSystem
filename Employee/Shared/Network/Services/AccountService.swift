//
//  AccountService.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//

import SwiftUI

final class AccountService {
    static let shared = AccountService()
    private let api = APIClient.shared

    func getAccounts(clientId: UUID) async throws -> [AccountDTO] {
        try await api.get("\(API.core)/Accounts/client/\(clientId)")
    }
    func getAccount(id: UUID) async throws -> AccountDTO {
        try await api.get("\(API.core)/Accounts/\(id)")
    }
    func createAccount(clientId: UUID, currency: String) async throws -> AccountDTO {
        try await api.post("\(API.core)/Accounts",
                           body: CreateAccountRequest(clientId: clientId, currency: currency))
    }
    func closeAccount(id: UUID) async throws {
        try await api.putVoid("\(API.core)/Accounts/\(id)/close")
    }
    func getTransactions(accountId: UUID) async throws -> [TransactionDTO] {
        try await api.get("\(API.core)/Accounts/\(accountId)/transactions")
    }
    func getAllAccounts() async throws -> [AccountDTO] {
        return []
    }
}
