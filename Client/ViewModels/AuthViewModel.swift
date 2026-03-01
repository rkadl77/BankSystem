//
//  AuthViewModel.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import Foundation
import Combine

@MainActor
final class AuthViewModel: ObservableObject {
    @Published var email = ""
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var currentUser: UserDTO?

    var isLoggedIn: Bool { currentUser != nil }

    func login() {
        let trimmed = email.trimmingCharacters(in: .whitespaces)
        guard !trimmed.isEmpty else { errorMessage = "Введите email"; return }
        Task {
            isLoading = true; errorMessage = nil
            do {
                let user = try await UserService.shared.getUserByEmail(trimmed)
                guard user.isActive  else { errorMessage = "Аккаунт заблокирован"; isLoading = false; return }
                guard user.role == "client" else { errorMessage = "Нет доступа как клиент"; isLoading = false; return }
                currentUser = user
            } catch NetworkError.serverError(404, _) { errorMessage = "Пользователь не найден" }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }

    func logout() { currentUser = nil; email = "" }
}
