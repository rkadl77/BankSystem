//
//  EmployeesView.swift
//  Employee
//
//  Created by Gleb Korotkov on 04.03.2026.
//

import SwiftUI

struct EmployeesView: View {
    @ObservedObject var vm: EmployeesViewModel
    @State private var showCreate = false

    var body: some View {
        NavigationStack {
            List {
                if let err = vm.errorMessage {
                    Section { Label(err, systemImage: "exclamationmark.triangle.fill").foregroundColor(.bankDanger) }
                }
                if let ok = vm.successMessage {
                    Section { Label(ok, systemImage: "checkmark.circle.fill").foregroundColor(.bankSuccess) }
                }
                if vm.isLoading {
                    Section { ProgressView("Загрузка...").frame(maxWidth: .infinity) }
                }

                Section("Сотрудники (\(vm.employees.count))") {
                    if vm.employees.isEmpty && !vm.isLoading {
                        Text("Нет сотрудников").foregroundColor(.secondary)
                    }
                    ForEach(vm.employees) { emp in
                        NavigationLink(destination: EmployeeDetailView(employee: emp, vm: vm)) {
                            EmployeeRowView(employee: emp)
                        }
                    }
                }
            }
            .navigationTitle("Сотрудники")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button { showCreate = true } label: { Image(systemName: "person.badge.plus") }
                }
            }
            .sheet(isPresented: $showCreate) { CreateEmployeeSheet(vm: vm) }
            .refreshable { vm.load() }
        }
    }
}
