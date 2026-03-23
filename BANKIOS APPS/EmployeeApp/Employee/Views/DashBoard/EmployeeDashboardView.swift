import SwiftUI

struct EmployeeDashboardView: View {
    @ObservedObject var authVM: EmployeeAuthViewModel
    @StateObject private var clientsVM   = ClientsViewModel()
    @StateObject private var employeesVM = EmployeesViewModel()
    @StateObject private var tariffsVM   = TariffsViewModel()
    @StateObject private var settingsVM: SettingsViewModel

    init(authVM: EmployeeAuthViewModel) {
        self.authVM = authVM
        let userId = authVM.currentUser?.id ?? UUID()
        _settingsVM = StateObject(wrappedValue: SettingsViewModel(userId: userId))
    }

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

            EmployeeProfileView(user: user, employee: employee,
                                settingsVM: settingsVM, onLogout: authVM.logout)
                .tabItem { Label("Профиль", systemImage: "person.fill") }
        }
        .tint(.bankGold)
        .onAppear { settingsVM.load() }
        .preferredColorScheme(settingsVM.isDarkTheme ? .dark : .light)
    }
}
