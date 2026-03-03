//
//  EmployeeDashboardView.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//

import SwiftUI

struct EmployeeDashboardView: View {
    @ObservedObject var authVM: EmployeeAuthViewModel
    @StateObject private var clientsVM   = ClientsViewModel()
    @StateObject private var employeesVM = EmployeesViewModel()
    @StateObject private var tariffsVM   = TariffsViewModel()

    var employee: EmployeeDTO? { authVM.currentEmployee }
    var user: UserDTO { authVM.currentUser! }

    var body: some View {
        TabView {
            ClientsListView(vm: clientsVM)
                .tabItem { Label("Клиенты", systemImage: "person.2.fill") }
                .onAppear { clientsVM.load() }

            TariffsView(vm: tariffsVM)
                .tabItem { Label("Тарифы", systemImage: "percent") }
                .onAppear { tariffsVM.load() }

            EmployeesView(vm: employeesVM)
                .tabItem { Label("Сотрудники", systemImage: "briefcase.fill") }
                .onAppear { employeesVM.load() }

            EmployeeProfileView(user: user, employee: employee, onLogout: authVM.logout)
                .tabItem { Label("Профиль", systemImage: "person.fill") }
        }
        .tint(.bankGold)
    }
}
