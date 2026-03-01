//
//  AccountsListView.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//

import SwiftUI

struct AccountsListView: View {
    @ObservedObject var vm: AccountsViewModel
    @State private var showOpenSheet = false
    @State private var currency = "RUB"

    var active: [AccountDTO] { vm.accounts.filter { $0.isActive } }
    var closed: [AccountDTO] { vm.accounts.filter { !$0.isActive } }

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
                Section("Активные счета (\(active.count))") {
                    if active.isEmpty && !vm.isLoading {
                        Text("Нет счетов. Откройте первый!").foregroundColor(.secondary)
                    }
                    ForEach(active) { acc in
                        NavigationLink(destination: AccountDetailView(account: acc, accountsVM: vm)) {
                            AccountRowView(account: acc)
                        }
                    }
                }
                if !closed.isEmpty {
                    Section("Закрытые") {
                        ForEach(closed) { acc in
                            NavigationLink(destination: AccountDetailView(account: acc, accountsVM: vm)) {
                                AccountRowView(account: acc)
                            }
                        }
                    }
                }
            }
            .navigationTitle("Мои счета")
            .toolbar { ToolbarItem(placement: .navigationBarTrailing) { Button { showOpenSheet = true } label: { Image(systemName: "plus") } } }
            .sheet(isPresented: $showOpenSheet) {
                OpenAccountSheet(currency: $currency) { vm.openAccount(currency: currency); showOpenSheet = false }
            }
            .refreshable { vm.load() }
        }
    }
}

