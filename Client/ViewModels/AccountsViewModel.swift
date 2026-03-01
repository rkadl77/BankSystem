//
//  AccountsViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import Foundation
import Combine

@MainActor
final class AccountsViewModel: ObservableObject {
    @Published var accounts: [AccountDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var successMessage: String?

    let userId: UUID
    init(userId: UUID) { self.userId = userId }

    func load() {
        Task {
            isLoading = true; errorMessage = nil
            do { accounts = try await AccountService.shared.getAccounts(clientId: userId) }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }

    func openAccount(currency: String) {
        Task {
            do {
                let acc = try await AccountService.shared.createAccount(clientId: userId, currency: currency)
                accounts.append(acc)
                flash("Счёт \(currency) открыт ✓")
            } catch { errorMessage = error.localizedDescription }
        }
    }

    func closeAccount(_ account: AccountDTO) {
        Task {
            do {
                try await AccountService.shared.closeAccount(id: account.id)
                load(); flash("Счёт закрыт")
            } catch { errorMessage = error.localizedDescription }
        }
    }

    func deposit(to account: AccountDTO, amount: Double) {
        Task {
            do {
                try await TransactionService.shared.deposit(accountId: account.id, amount: amount,
                                                             currency: account.currency,
                                                             description: "Пополнение счёта")
                load(); flash("Пополнено \(String(format: "%.2f", amount)) \(account.currencySymbol)")
            } catch { errorMessage = error.localizedDescription }
        }
    }

    func withdraw(from account: AccountDTO, amount: Double) {
        Task {
            do {
                try await TransactionService.shared.withdraw(accountId: account.id, amount: amount,
                                                              currency: account.currency,
                                                              description: "Снятие наличных")
                load(); flash("Снято \(String(format: "%.2f", amount)) \(account.currencySymbol)")
            }
            catch NetworkError.serverError(400, _) { errorMessage = "Недостаточно средств" }
            catch { errorMessage = error.localizedDescription }
        }
    }

    private func flash(_ msg: String) {
        successMessage = msg
        Task { try? await Task.sleep(nanoseconds: 2_500_000_000); successMessage = nil }
    }
}
