//
//  EmployeeDetailView.swift
//  Employee
//
//  Created by Gleb Korotkov on 04.03.2026.
//


import SwiftUI

struct EmployeeDetailView: View {
    let employee: EmployeeDTO
    @ObservedObject var vm: EmployeesViewModel
    @State private var showFireConfirm = false
    @Environment(\.dismiss) var dismiss

    var body: some View {
        List {
            Section {
                HStack(spacing: 16) {
                    ZStack {
                        Circle()
                            .fill(employee.isActive ? Color.bankGold.opacity(0.15) : Color.secondary.opacity(0.1))
                            .frame(width: 60, height: 60)
                        Text(employee.initials).font(.title2.bold())
                            .foregroundColor(employee.isActive ? .bankGold : .secondary)
                    }
                    VStack(alignment: .leading, spacing: 4) {
                        Text(employee.fullName).font(.headline)
                        Text(employee.position).font(.subheadline).foregroundColor(.secondary)
                        HStack(spacing: 4) {
                            Circle()
                                .fill(employee.isActive ? Color.bankSuccess : Color.bankDanger)
                                .frame(width: 7, height: 7)
                            Text(employee.isActive ? "Работает" : "Уволен")
                                .font(.caption)
                                .foregroundColor(employee.isActive ? .bankSuccess : .bankDanger)
                        }
                    }
                }.padding(.vertical, 4)
            }

            Section("Данные") {
                LabeledContent("Email",      value: employee.email)
                LabeledContent("Должность",  value: employee.position)
                LabeledContent("Отдел",      value: employee.department)
                LabeledContent("Принят",     value: employee.hireDate.shortFormatted)
            }

            if let err = vm.errorMessage {
                Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
            }

            if employee.isActive {
                Section("Действия") {
                    Button("Уволить сотрудника", role: .destructive) { showFireConfirm = true }
                }
            }
        }
        .navigationTitle(employee.firstName)
        .navigationBarTitleDisplayMode(.inline)
        .alert("Уволить сотрудника?", isPresented: $showFireConfirm) {
            Button("Уволить", role: .destructive) { vm.fire(employee); dismiss() }
            Button("Отмена", role: .cancel) {}
        } message: {
            Text("\(employee.fullName) потеряет доступ к системе")
        }
    }
}
