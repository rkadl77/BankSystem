//
//  UserManagementViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 26.03.2026.
//


import Foundation
import Combine

// MARK: — User Management (employees)

final class UserManagementViewModel: ObservableObject {
    @Published var employees: [User] = []
    
    private let db = MockDataService.shared
    private var cancellables = Set<AnyCancellable>()
    
    init() {
        db.$users
            .receive(on: DispatchQueue.main)
            .sink { [weak self] _ in self?.load() }
            .store(in: &cancellables)
        load()
    }
    
    func load() { employees = db.employees() }
    
    func createEmployee(fullName: String, email: String, phone: String) {
        _ = db.createUser(fullName: fullName, email: email, phone: phone, role: .employee)
    }
    
    func toggleBlock(_ user: User) { db.toggleUserBlock(userId: user.id) }
}
