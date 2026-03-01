//
//  AccountsListView.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//

import SwiftUI

struct AccountsListView: View {
    @ObservedObject var vm: AccountsViewModel
    let user: User
    
    @State private var showOpenSheet = false
    @State private var selectedType: AccountType = .checking
    
    var activeAccounts: [Account] { vm.accounts.filter { $0.isActive } }
    var closedAccounts: [Account] { vm.accounts.filter { !$0.isActive } }
    
    var body: some View {
        NavigationStack {
            List {
                if let err = vm.errorMessage {
                    Section {
                        Label(err, systemImage: "exclamationmark.triangle.fill")
                            .foregroundColor(.bankDanger)
                    }
                }
                
                if vm.showSuccess {
                    Section {
                        Label(vm.successMessage, systemImage: "checkmark.circle.fill")
                            .foregroundColor(.bankSuccess)
                    }
                }
                
                Section("Активные счета (\(activeAccounts.count))") {
                    if activeAccounts.isEmpty {
                        Text("Нет активных счетов")
                            .foregroundColor(.secondary)
                    } else {
                        ForEach(activeAccounts) { account in
                            NavigationLink(destination: AccountDetailView(account: account, vm: vm)) {
                                AccountRowView(account: account)
                            }
                        }
                    }
                }
                
                if !closedAccounts.isEmpty {
                    Section("Закрытые счета") {
                        ForEach(closedAccounts) { account in
                            NavigationLink(destination: AccountDetailView(account: account, vm: vm)) {
                                AccountRowView(account: account)
                            }
                        }
                    }
                }
            }
            .navigationTitle("Счета")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button {
                        showOpenSheet = true
                    } label: {
                        Image(systemName: "plus")
                    }
                }
            }
            .sheet(isPresented: $showOpenSheet) {
                OpenAccountSheet(selectedType: $selectedType) {
                    vm.openAccount(type: selectedType)
                    showOpenSheet = false
                }
            }
        }
    }
}

private struct OpenAccountSheet: View {
    @Binding var selectedType: AccountType
    let onConfirm: () -> Void
    @Environment(\.dismiss) var dismiss
    
    var body: some View {
        NavigationStack {
            Form {
                Section("Тип счёта") {
                    Picker("Тип", selection: $selectedType) {
                        ForEach(AccountType.allCases, id: \.self) { t in
                            Text(t.rawValue).tag(t)
                        }
                    }
                    .pickerStyle(.segmented)
                }
                
                Section {
                    Button("Открыть счёт", action: onConfirm)
                        .foregroundColor(.bankAccent)
                }
            }
            .navigationTitle("Новый счёт")
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Отмена") { dismiss() }
                }
            }
        }
        .presentationDetents([.medium])
    }
}
