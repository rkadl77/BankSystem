//
//  CreateEmployeeSheet.swift
//  Employee
//
//  Created by Gleb Korotkov on 04.03.2026.
//

import SwiftUI

struct CreateEmployeeSheet: View {
    @ObservedObject var vm: EmployeesViewModel
    @Environment(\.dismiss) var dismiss
    @State private var firstName  = ""
    @State private var lastName   = ""
    @State private var email      = ""
    @State private var phone      = ""
    @State private var position   = ""
    @State private var department = ""

    var canCreate: Bool {
        !firstName.trimmingCharacters(in: .whitespaces).isEmpty &&
        !lastName.trimmingCharacters(in: .whitespaces).isEmpty &&
        email.contains("@") &&
        !position.trimmingCharacters(in: .whitespaces).isEmpty
    }

    var body: some View {
        NavigationStack {
            Form {
                Section("Личные данные") {
                    TextField("Имя",     text: $firstName)
                    TextField("Фамилия", text: $lastName)
                }
                Section("Контакты") {
                    TextField("Email",   text: $email).textInputAutocapitalization(.never).keyboardType(.emailAddress)
                    TextField("Телефон", text: $phone).keyboardType(.phonePad)
                }
                Section("Должность") {
                    TextField("Например: Менеджер", text: $position)
                    TextField("Отдел",              text: $department)
                }
                if let err = vm.errorMessage {
                    Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
                }
                Section {
                    Button("Добавить сотрудника") {
                        vm.create(firstName: firstName, lastName: lastName, email: email,
                                  phone: phone, position: position, department: department)
                        dismiss()
                    }
                    .foregroundColor(.bankGold).fontWeight(.semibold)
                    .disabled(!canCreate)
                }
            }
            .navigationTitle("Новый сотрудник")
            .toolbar { ToolbarItem(placement: .cancellationAction) { Button("Отмена") { dismiss() } } }
        }
    }
}
