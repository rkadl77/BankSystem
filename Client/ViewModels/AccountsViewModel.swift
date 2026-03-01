//
//  AccountsViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import Foundation
import Combine

final class AccountsViewModel: ObservableObject {
    @Published var accounts: [Account] = []
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var showSuccess = false
    @Published var successMessage = ""
    
    private let db = MockDataService.shared
    private var cancellables = Set<AnyCancellable>()
    private let userId: UUID
    
    init(userId: UUID) {
        self.userId = userId
        observeDB()
        loadAccounts()
    }
    
    private func observeDB() {
        db.$accounts
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.loadAccounts() }
            .store(in: &cancellables)
    }
    
    func loadAccounts() {
        accounts = db.accounts(for: userId)
    }
    
    func openAccount(type: AccountType) {
        db.openAccount(for: userId, type: type)
        showSuccessMessage("Счёт открыт ✓")
    }
    
    func closeAccount(_ account: Account) {
        guard account.balance == 0 else {
            errorMessage = "Нельзя закрыть счёт с ненулевым балансом"
            return
        }
        db.closeAccount(id: account.id)
        showSuccessMessage("Счёт закрыт")
    }
    
    func deposit(to account: Account, amount: Double) {
        guard amount > 0 else { errorMessage = "Сумма должна быть больше 0"; return }
        db.deposit(accountId: account.id, amount: amount)
        showSuccessMessage("Пополнено на \(String(format: "%.2f", amount)) ₽")
    }
    
    func withdraw(from account: Account, amount: Double) {
        guard amount > 0 else { errorMessage = "Сумма должна быть больше 0"; return }
        let ok = db.withdraw(accountId: account.id, amount: amount)
        if ok {
            showSuccessMessage("Снято \(String(format: "%.2f", amount)) ₽")
        } else {
            errorMessage = "Недостаточно средств"
        }
    }
    
    private func showSuccessMessage(_ msg: String) {
        successMessage = msg
        showSuccess = true
        DispatchQueue.main.asyncAfter(deadline: .now() + 2) { [weak self] in
            self?.showSuccess = false
        }
    }
}
