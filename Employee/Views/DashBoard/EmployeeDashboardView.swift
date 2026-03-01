//
//  EmployeeDashboardView.swift
//  Client
//
//  Created by Gleb Korotkov on 26.02.2026.
//



import SwiftUI

struct EmployeeDashboardView: View {
    @ObservedObject var authVM: EmployeeAuthViewModel
    
    private var employee: User { authVM.currentEmployee! }
    
    @StateObject private var clientsVM = AllClientsViewModel()
    @StateObject private var accountsVM = AllAccountsViewModel()
    @StateObject private var userMgmtVM = UserManagementViewModel()
    
    var body: some View {
        TabView {
            ClientsListView(vm: clientsVM)
                .tabItem { Label("Клиенты", systemImage: "person.2.fill") }
            
            AllAccountsView(vm: accountsVM)
                .tabItem { Label("Счета", systemImage: "creditcard.fill") }
            
            TariffsManagementView(
                vm: TariffViewModel(employeeId: employee.id)
            )
            .tabItem { Label("Тарифы", systemImage: "percent") }
            
            EmployeesManagementView(vm: userMgmtVM)
                .tabItem { Label("Сотрудники", systemImage: "person.badge.key.fill") }
            
            EmployeeProfileView(employee: employee, onLogout: authVM.logout)
                .tabItem { Label("Профиль", systemImage: "person.crop.circle.fill") }
        }
        .tint(.bankGold)
    }
}


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
