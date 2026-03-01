//
//  EmployeeApp.swift
//  Employee
//
//  Created by Gleb Korotkov on 25.02.2026.
//

import SwiftUI

@main
struct EmployeeApp: App {
    @StateObject private var authVM = EmployeeAuthViewModel()
    
    var body: some Scene {
        WindowGroup {
            if authVM.isLoggedIn, let _ = authVM.currentEmployee {
                EmployeeDashboardView(authVM: authVM)
                    .transition(.asymmetric(insertion: .move(edge: .trailing),
                                            removal: .move(edge: .leading)))
            } else {
                EmployeeLoginView(vm: authVM)
                    .transition(.opacity)
            }
        }
    }
}
