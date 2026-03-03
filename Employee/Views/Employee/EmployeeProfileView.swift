//
//  File.swift
//  Employee
//
//  Created by Gleb Korotkov on 04.03.2026.
//

import SwiftUI

struct EmployeeProfileView: View {
    let user: UserDTO
    let employee: EmployeeDTO?
    let onLogout: () -> Void

    var body: some View {
        NavigationStack {
            List {
                Section {
                    HStack(spacing: 16) {
                        ZStack {
                            Circle().fill(Color.bankGold.opacity(0.15)).frame(width: 60, height: 60)
                            Text(user.initials).font(.title2.bold()).foregroundColor(.bankGold)
                        }
                        VStack(alignment: .leading, spacing: 4) {
                            Text(user.fullName).font(.headline)
                            Text(employee?.position ?? "Сотрудник")
                                .font(.subheadline).foregroundColor(.secondary)
                            Text(user.role == "admin" ? "Администратор" : "Сотрудник")
                                .font(.caption2)
                                .padding(.horizontal, 8).padding(.vertical, 2)
                                .background(Color.bankGold.opacity(0.15))
                                .foregroundColor(.bankGold).cornerRadius(4)
                        }
                    }.padding(.vertical, 4)
                }

                Section("Контакты") {
                    LabeledContent("Email",   value: user.email)
                    LabeledContent("Телефон", value: user.phone)
                }

                if let emp = employee {
                    Section("Должность") {
                        LabeledContent("Позиция", value: emp.position)
                        LabeledContent("Отдел",   value: emp.department)
                        LabeledContent("Принят",  value: emp.hireDate.shortFormatted)
                    }
                }

                Section {
                    Button("Выйти", role: .destructive, action: onLogout)
                }
            }
            .navigationTitle("Профиль")
        }
    }
}
