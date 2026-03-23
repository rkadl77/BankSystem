import SwiftUI

@main
struct EmployeeApp: App {
    @StateObject private var authVM = EmployeeAuthViewModel()

    var body: some Scene {
        WindowGroup {
            Group {
                if authVM.isLoggedIn {
                    EmployeeDashboardView(authVM: authVM)
                } else {
                    EmployeeLoginView(vm: authVM)
                }
            }
            .animation(.easeInOut(duration: 0.3), value: authVM.isLoggedIn)
        }
    }
}
