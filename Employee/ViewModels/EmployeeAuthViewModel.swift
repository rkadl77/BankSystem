//
//  EmployeeAuthViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 25.03.2026.
//


import Foundation
import Combine

// MARK: — Auth

final class EmployeeAuthViewModel: ObservableObject {
    @Published var email = ""
    @Published var errorMessage: String?
    @Published var currentEmployee: User?
    @Published var isLoading = false
    
    private let db = MockDataService.shared
    
    var isLoggedIn: Bool { currentEmployee != nil }
    
    func login() {
        guard !email.isEmpty else { errorMessage = "Введите email"; return }
        isLoading = true
        errorMessage = nil
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.5) { [weak self] in
            guard let self else { return }
            self.isLoading = false
            if let user = self.db.login(email: self.email), user.role == .employee {
                self.currentEmployee = user
            } else {
                self.errorMessage = "Сотрудник не найден или нет доступа"
            }
        }
    }
    
    func logout() { currentEmployee = nil; email = "" }
}
