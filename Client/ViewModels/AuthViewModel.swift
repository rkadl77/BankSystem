//
//  AuthViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import Foundation
import Combine

final class AuthViewModel: ObservableObject {
    @Published var email: String = ""
    @Published var errorMessage: String?
    @Published var currentUser: User?
    @Published var isLoading: Bool = false
    
    private let db = MockDataService.shared
    
    var isLoggedIn: Bool { currentUser != nil }
    
    func login() {
        guard !email.isEmpty else {
            errorMessage = "Введите email"
            return
        }
        isLoading = true
        errorMessage = nil
        
        // Simulate network delay
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.6) { [weak self] in
            guard let self else { return }
            self.isLoading = false
            if let user = self.db.login(email: self.email), user.role == .client {
                self.currentUser = user
            } else {
                self.errorMessage = "Клиент не найден или заблокирован"
            }
        }
    }
    
    func logout() {
        currentUser = nil
        email = ""
    }
}
