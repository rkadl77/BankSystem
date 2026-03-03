//
//  CreateClientSheet.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//


import SwiftUI

struct CreateClientSheet: View {
    @ObservedObject var vm: ClientsViewModel
    @Environment(\.dismiss) var dismiss
    @State private var firstName = ""
    @State private var lastName  = ""
    @State private var email     = ""
    @State private var phone     = ""

    var canCreate: Bool {
        !firstName.trimmingCharacters(in: .whitespaces).isEmpty &&
        !lastName.trimmingCharacters(in: .whitespaces).isEmpty &&
        email.contains("@") &&
        phone.count >= 5
    }

    var body: some View {
        NavigationStack {
            Form {
                Section("Личные данные") {
                    TextField("Имя",      text: $firstName)
                    TextField("Фамилия",  text: $lastName)
                }
                Section("Контакты") {
                    TextField("Email",    text: $email).textInputAutocapitalization(.never).keyboardType(.emailAddress)
                    TextField("Телефон",  text: $phone).keyboardType(.phonePad)
                }
                if let err = vm.errorMessage {
                    Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
                }
                Section {
                    Button("Создать клиента") {
                        vm.createClient(firstName: firstName, lastName: lastName,
                                        email: email, phone: phone)
                        dismiss()
                    }
                    .foregroundColor(.bankAccent).fontWeight(.semibold)
                    .disabled(!canCreate)
                }
            }
            .navigationTitle("Новый клиент")
            .toolbar { ToolbarItem(placement: .cancellationAction) { Button("Отмена") { dismiss() } } }
        }
    }
}
