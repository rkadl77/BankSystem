//
//  EmployeeProfileView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026
//

import SwiftUI
private struct EmployeeProfileView: View {
    let employee: User
    let onLogout: () -> Void
    
    var body: some View {
        NavigationStack {
            List {
                Section {
                    HStack(spacing: 16) {
                        ZStack {
                            Circle()
                                .fill(Color.bankGold.opacity(0.15))
                                .frame(width: 60, height: 60)
                            Text(employee.initials)
                                .font(.title2.bold())
                                .foregroundColor(.bankGold)
                        }
                        VStack(alignment: .leading) {
                            Text(employee.fullName).font(.headline)
                            Text("Сотрудник").font(.caption).foregroundColor(.secondary)
                        }
                    }
                    .padding(.vertical, 6)
                }
                Section("Контакты") {
                    LabeledContent("Email", value: employee.email)
                    LabeledContent("Телефон", value: employee.phone)
                }
                Section {
                    Button("Выйти", role: .destructive, action: onLogout)
                }
            }
            .navigationTitle("Профиль")
        }
    }
}
