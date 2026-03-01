//
//  AddUserSheet.swift
//  Client
//
//  Created by Gleb Korotkov on 25.03.2026.
//

import SwiftUI

struct AddUserSheet: View {
    let role: UserRole
    let onCreate: (String, String, String) -> Void
    
    @State private var fullName = ""
    @State private var email = ""
    @State private var phone = ""
    @Environment(\.dismiss) var dismiss
    
    var body: some View {
        NavigationStack {
            Form {
                Section("Данные \(role == .client ? "клиента" : "сотрудника")") {
                    HStack {
                        Text("ФИО")
                        Spacer()
                        TextField("Иванов Иван Иванович", text: $fullName)
                            .multilineTextAlignment(.trailing)
                    }
                    HStack {
                        Text("Email")
                        Spacer()
                        TextField("email@example.com", text: $email)
                            .keyboardType(.emailAddress)
                            .textInputAutocapitalization(.never)
                            .multilineTextAlignment(.trailing)
                    }
                    HStack {
                        Text("Телефон")
                        Spacer()
                        TextField("+7 900 000-00-00", text: $phone)
                            .keyboardType(.phonePad)
                            .multilineTextAlignment(.trailing)
                    }
                }
                
                Section {
                    Button("Создать") {
                        guard !fullName.isEmpty, !email.isEmpty else { return }
                        onCreate(fullName, email, phone)
                    }
                    .foregroundColor(role == .client ? .bankAccent : .bankGold)
                    .fontWeight(.semibold)
                }
            }
            .navigationTitle(role == .client ? "Новый клиент" : "Новый сотрудник")
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Отмена") { dismiss() }
                }
            }
        }
        .presentationDetents([.medium])
    }
}
