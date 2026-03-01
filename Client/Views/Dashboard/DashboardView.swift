//
//  DashboardView.swift
//  Client
//
//  Created by Gleb Korotkov on 28.02.2026.
//


import SwiftUI

struct DashboardView: View {
    @ObservedObject var authVM: AuthViewModel
    @StateObject private var accountsVM: AccountsViewModel
    @StateObject private var creditVM: CreditViewModel

    init(authVM: AuthViewModel, user: UserDTO) {
        self.authVM = authVM
        _accountsVM = StateObject(wrappedValue: AccountsViewModel(userId: user.id))
        _creditVM   = StateObject(wrappedValue: CreditViewModel(clientId: user.id))
    }

    var user: UserDTO { authVM.currentUser! }

    var body: some View {
        TabView {
            AccountsListView(vm: accountsVM)
                .tabItem { Label("Счета", systemImage: "creditcard.fill") }
                .onAppear { accountsVM.load() }

            CreditsView(creditVM: creditVM, accountsVM: accountsVM)
                .tabItem { Label("Кредиты", systemImage: "banknote.fill") }
                .onAppear { creditVM.load() }

            ProfileView(user: user, onLogout: authVM.logout)
                .tabItem { Label("Профиль", systemImage: "person.fill") }
        }
        .tint(.bankAccent)
    }
}


