//
//  TransactionsViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import Foundation
import Combine

final class TransactionsViewModel: ObservableObject {
    @Published var transactions: [Transaction] = []
    
    private let db = MockDataService.shared
    private var cancellables = Set<AnyCancellable>()
    let account: Account
    
    init(account: Account) {
        self.account = account
        observeDB()
        load()
    }
    
    private func observeDB() {
        db.$transactions
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.load() }
            .store(in: &cancellables)
    }
    
    func load() {
        transactions = db.transactions(for: account.id)
    }
}
