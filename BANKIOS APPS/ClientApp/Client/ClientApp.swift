//
//  ClientApp.swift
//  Client
//
//  Created by Gleb Korotkov on 28.02.2026.
//

import SwiftUI

@main
struct ClientApp: App {
    @StateObject private var authVM = AuthViewModel()
    var body: some Scene {
        WindowGroup {
            Group {
                if authVM.isLoggedIn, let user = authVM.currentUser {
                    DashboardView(authVM: authVM, user: user)
                } else {
                    LoginView(vm: authVM)
                }
            }
            .animation(.easeInOut(duration: 0.3), value: authVM.isLoggedIn)
        }
    }
}
