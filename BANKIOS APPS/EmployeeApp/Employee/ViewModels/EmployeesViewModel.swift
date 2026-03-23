//
//  EmployeesViewModel.swift
//  Employee
//
//  Created by Gleb Korotkov on 01.03.2026.
//

import SwiftUI
import Combine

@MainActor
final class EmployeesViewModel: ObservableObject {
    @Published var employees: [EmployeeDTO] = []
    @Published var isLoading = false
    @Published var errorMessage: String?
    @Published var successMessage: String?

    func load() {
        Task {
            isLoading = true
            do { employees = try await UserService.shared.getEmployees() }
            catch { errorMessage = error.localizedDescription }
            isLoading = false
        }
    }

    func fire(_ employee: EmployeeDTO) {
        Task {
            do {
                try await UserService.shared.fireEmployee(id: employee.id)
                employees.removeAll { $0.id == employee.id }
                flash("Сотрудник уволен")
            } catch { errorMessage = error.localizedDescription }
        }
    }

    func create(firstName: String, lastName: String, email: String,
                phone: String, position: String, department: String) {
        Task {
            do {
                let e = try await UserService.shared.createEmployee(
                    firstName: firstName, lastName: lastName, email: email,
                    phone: phone, position: position, department: department)
                employees.append(e)
                flash("Сотрудник \(e.fullName) добавлен ✓")
            } catch { errorMessage = error.localizedDescription }
        }
    }

    private func flash(_ msg: String) {
        successMessage = msg
        Task { try? await Task.sleep(nanoseconds: 2_500_000_000); successMessage = nil }
    }
}
