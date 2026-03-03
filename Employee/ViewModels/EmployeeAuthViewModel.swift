//
//  EmployeeAuthViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 28.03.2026.
//



import Foundation
import Combine

@MainActor
final class EmployeeAuthViewModel: ObservableObject {
    @Published var email = ""
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var currentUser: UserDTO?
    @Published var currentEmployee: EmployeeDTO?

    var isLoggedIn: Bool { currentUser != nil }

    func login() {
        let trimmed = email.trimmingCharacters(in: .whitespaces)
        guard !trimmed.isEmpty else { errorMessage = "Введите email"; return }
        Task {
            isLoading = true; errorMessage = nil
            do {
                let user = try await UserService.shared.getUserByEmail(trimmed)
                guard user.isActive else {
                    errorMessage = "Аккаунт заблокирован"; isLoading = false; return
                }
                guard user.role == "employee" || user.role == "admin" else {
                    errorMessage = "Нет прав сотрудника"; isLoading = false; return
                }
                // Ищем запись Employee по userId
                let employees = try await UserService.shared.getEmployees()
                currentEmployee = employees.first { $0.userId == user.id }
                currentUser = user
            } catch NetworkError.serverError(404, _) {
                errorMessage = "Сотрудник не найден"
            } catch {
                errorMessage = error.localizedDescription
            }
            isLoading = false
        }
    }

    func logout() { currentUser = nil; currentEmployee = nil; email = "" }
}
