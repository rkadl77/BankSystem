//
//  AllClientsViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 25.03.2026.
//


import Foundation
import Combine

// MARK: — All Clients

final class AllClientsViewModel: ObservableObject {
    @Published var clients: [User] = []
    @Published var searchText = ""
    
    private let db = MockDataService.shared
    private var cancellables = Set<AnyCancellable>()
    
    init() {
        db.$users
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.load() }
            .store(in: &cancellables)
        load()
    }
    
    func load() { clients = db.clients() }
    
    var filtered: [User] {
        if searchText.isEmpty { return clients }
        return clients.filter {
            $0.fullName.localizedCaseInsensitiveContains(searchText) ||
            $0.email.localizedCaseInsensitiveContains(searchText)
        }
    }
    
    func toggleBlock(_ user: User) { db.toggleUserBlock(userId: user.id) }
    
    func createClient(fullName: String, email: String, phone: String) {
        _ = db.createUser(fullName: fullName, email: email, phone: phone, role: .client)
    }
}
