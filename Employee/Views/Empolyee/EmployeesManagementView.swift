//
//  EmployeesManagementView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//

import SwiftUI

struct EmployeesManagementView: View {
    @ObservedObject var vm: UserManagementViewModel
    @State private var showAdd = false
    
    var body: some View {
        NavigationStack {
            List {
                ForEach(vm.employees) { emp in
                    HStack(spacing: 14) {
                        ZStack {
                            Circle()
                                .fill(emp.isBlocked ? Color.bankDanger.opacity(0.15) : Color.bankGold.opacity(0.15))
                                .frame(width: 44, height: 44)
                            Text(emp.initials)
                                .font(.subheadline.bold())
                                .foregroundColor(emp.isBlocked ? .bankDanger : .bankGold)
                        }
                        VStack(alignment: .leading, spacing: 2) {
                            Text(emp.fullName).font(.headline)
                            Text(emp.email).font(.caption).foregroundColor(.secondary)
                        }
                        Spacer()
                        Button {
                            vm.toggleBlock(emp)
                        } label: {
                            Image(systemName: emp.isBlocked ? "lock.open.fill" : "lock.fill")
                                .foregroundColor(emp.isBlocked ? .bankSuccess : .bankDanger)
                        }
                        .buttonStyle(.plain)
                    }
                    .padding(.vertical, 2)
                }
            }
            .navigationTitle("Сотрудники (\(vm.employees.count))")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button { showAdd = true } label: {
                        Image(systemName: "person.badge.plus")
                    }
                }
            }
            .sheet(isPresented: $showAdd) {
                AddUserSheet(role: .employee) { name, email, phone in
                    vm.createEmployee(fullName: name, email: email, phone: phone)
                    showAdd = false
                }
            }
        }
    }
}
