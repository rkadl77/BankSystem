//
//  UserService.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//


import SwiftUI

final class UserService {
    static let shared = UserService()
    private let api = APIClient.shared

    func getAll() async throws -> [UserDTO] {
        try await api.get("\(API.users)/Users")
    }
    func getUser(id: UUID) async throws -> UserDTO {
        try await api.get("\(API.users)/Users/\(id)")
    }
    func getUserByEmail(_ email: String) async throws -> UserDTO {
        let enc = email.addingPercentEncoding(withAllowedCharacters: .urlPathAllowed) ?? email
        return try await api.get("\(API.users)/Users/email/\(enc)")
    }
    func createUser(firstName: String, lastName: String,
                    email: String, phone: String, role: String) async throws -> UserDTO {
        try await api.post("\(API.users)/Users",
                           body: CreateUserRequest(firstName: firstName, lastName: lastName,
                                                   email: email, phone: phone, role: role))
    }
    func updateUser(id: UUID, firstName: String, lastName: String, phone: String,
                    role: String, isActive: Bool) async throws -> UserDTO {
        try await api.put("\(API.users)/Users/\(id)",
                          body: UpdateUserRequest(firstName: firstName, lastName: lastName,
                                                  phone: phone, role: role, isActive: isActive))
    }
    func setActive(user: UserDTO, isActive: Bool) async throws -> UserDTO {
        try await updateUser(id: user.id, firstName: user.firstName, lastName: user.lastName,
                             phone: user.phone, role: user.role, isActive: isActive)
    }
    func getEmployees() async throws -> [EmployeeDTO] {
        try await api.get("\(API.users)/Employees")
    }
    func createEmployee(firstName: String, lastName: String, email: String, phone: String,
                        position: String, department: String) async throws -> EmployeeDTO {
        try await api.post("\(API.users)/Employees",
                           body: CreateEmployeeRequest(firstName: firstName, lastName: lastName,
                                                        email: email, phone: phone,
                                                        position: position, department: department))
    }
    func fireEmployee(id: UUID) async throws {
        try await api.postEmpty("\(API.users)/Employees/\(id)/fire")
    }
}
