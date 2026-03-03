//
//  ClientsViewModel.swift
//  Employee
//
//  Created by Gleb Korotkov on 01.03.2026.
//

import SwiftUI
import Combine

@MainActor
final class ClientsViewModel: ObservableObject {
    @Published var clients: [UserDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var successMessage: String?
    @Published var searchText = ""

    var filtered: [UserDTO] {
        guard !searchText.isEmpty else { return clients }
        return clients.filter {
            $0.fullName.localizedCaseInsensitiveContains(searchText) ||
            $0.email.localizedCaseInsensitiveContains(searchText) ||
            $0.phone.contains(searchText)
        }
    }

    func load() {
        Task {
            isLoading = true; errorMessage = nil
            do {
                let all = try await UserService.shared.getAll()
                clients = all.filter { $0.role == "client" }
            } catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }

    func toggleBlock(_ user: UserDTO) {
        Task {
            do {
                let updated = try await UserService.shared.setActive(user: user, isActive: !user.isActive)
                if let i = clients.firstIndex(where: { $0.id == user.id }) { clients[i] = updated }
                flash(updated.isActive ? "Клиент разблокирован" : "Клиент заблокирован")
            } catch { errorMessage = error.localizedDescription }
        }
    }

    func createClient(firstName: String, lastName: String, email: String, phone: String) {
        Task {
            do {
                let u = try await UserService.shared.createUser(
                    firstName: firstName, lastName: lastName,
                    email: email, phone: phone, role: "client")
                clients.append(u)
                flash("Клиент \(u.fullName) создан ✓")
            } catch { errorMessage = error.localizedDescription }
        }
    }

    private func flash(_ msg: String) {
        successMessage = msg
        Task { try? await Task.sleep(nanoseconds: 2_500_000_000); successMessage = nil }
    }
}
