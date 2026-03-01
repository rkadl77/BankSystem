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
            if authVM.isLoggedIn, let user = authVM.currentUser {
                DashboardView(authVM: authVM, user: user)
                    .transition(.asymmetric(insertion: .move(edge: .trailing),
                                            removal: .move(edge: .leading)))
            } else {
                LoginView(vm: authVM)
                    .transition(.opacity)
            }
        }
    }
}
