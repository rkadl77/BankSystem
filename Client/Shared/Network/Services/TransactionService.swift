//
//  TransactionService.swift
//  Client
//
//  Created by Gleb Korotkov on 01.03.2026.
//


import SwiftUI

final class TransactionService {
    static let shared = TransactionService()
    private let api = APIClient.shared

    func getTransactions(accountId: UUID) async throws -> [TransactionDTO] {
        try await api.get("\(API.core)/Transactions/account/\(accountId)")
    }
    @discardableResult
    func deposit(accountId: UUID, amount: Double, currency: String = "RUB",
                 description: String? = nil) async throws -> TransactionDTO {
        try await api.post("\(API.core)/Transactions/deposit",
                           body: CreateTransactionRequest(accountId: accountId, amount: amount,
                                                          currency: currency, type: "deposit",
                                                          description: description))
    }
    @discardableResult
    func withdraw(accountId: UUID, amount: Double, currency: String = "RUB",
                  description: String? = nil) async throws -> TransactionDTO {
        try await api.post("\(API.core)/Transactions/withdraw",
                           body: CreateTransactionRequest(accountId: accountId, amount: amount,
                                                          currency: currency, type: "withdrawal",
                                                          description: description))
    }
}
