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
    
    init(authVM: AuthViewModel, user: User) {
        self.authVM = authVM
        _accountsVM = StateObject(wrappedValue: AccountsViewModel(userId: user.id))
        _creditVM   = StateObject(wrappedValue: CreditViewModel(clientId: user.id))
    }
    
    var user: User { authVM.currentUser! }
    
    var body: some View {
        TabView {
            AccountsListView(vm: accountsVM, user: user)
                .tabItem {
                    Label("Счета", systemImage: "creditcard.fill")
                }
            
            CreditsView(creditVM: creditVM,
                        accountsVM: accountsVM,
                        clientId: user.id)
                .tabItem {
                    Label("Кредиты", systemImage: "banknote.fill")
                }
            
            ProfileView(user: user, onLogout: authVM.logout)
                .tabItem {
                    Label("Профиль", systemImage: "person.fill")
                }
        }
        .tint(.bankAccent)
    }
}


//
//  ProfileView.swift
//  Client
//
//  Created by Gleb Korotkov on 28.02.2026.
//

import SwiftUI

private struct ProfileView: View {
    let user: User
    let onLogout: () -> Void
    
    var body: some View {
        NavigationStack {
            List {
                Section {
                    HStack(spacing: 16) {
                        ZStack {
                            Circle()
                                .fill(Color.bankAccent.opacity(0.15))
                                .frame(width: 60, height: 60)
                            Text(user.initials)
                                .font(.title2.bold())
                                .foregroundColor(.bankAccent)
                        }
                        VStack(alignment: .leading) {
                            Text(user.fullName)
                                .font(.headline)
                            Text(user.email)
                                .font(.caption)
                                .foregroundColor(.secondary)
                        }
                    }
                    .padding(.vertical, 6)
                }
                
                Section("Контакты") {
                    LabeledContent("Телефон", value: user.phone)
                    LabeledContent("Email", value: user.email)
                }
                
                Section {
                    Button("Выйти", role: .destructive, action: onLogout)
                }
            }
            .navigationTitle("Профиль")
        }
    }
}
