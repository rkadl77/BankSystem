//
//  AllAccountsViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 25.03.2026.
//


import Foundation
import Combine

// MARK: — All Accounts

final class AllAccountsViewModel: ObservableObject {
    @Published var accounts: [Account] = []
    
    private let db = MockDataService.shared
    private var cancellables = Set<AnyCancellable>()
    
    init() {
        db.$accounts
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.load() }
            .store(in: &cancellables)
        load()
    }
    
    func load() { accounts = db.accounts }
    
    func ownerName(for account: Account) -> String {
        db.user(by: account.ownerId)?.fullName ?? "Неизвестно"
    }
    
    func transactions(for account: Account) -> [Transaction] {
        db.transactions(for: account.id)
    }
}
