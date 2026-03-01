//
//  AccountDetailView.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import SwiftUI

struct AccountDetailView: View {
    let account: AccountDTO
    @ObservedObject var accountsVM: AccountsViewModel
    @StateObject private var txVM: TransactionsViewModel
    @State private var depositText = ""
    @State private var withdrawText = ""
    @State private var showClose = false

    init(account: AccountDTO, accountsVM: AccountsViewModel) {
        self.account = account; self.accountsVM = accountsVM
        _txVM = StateObject(wrappedValue: TransactionsViewModel(accountId: account.id))
    }

    var body: some View {
        List {
            Section {
                ZStack {
                    RoundedRectangle(cornerRadius: 16)
                        .fill(LinearGradient(colors: [.bankPrimary, Color(red: 0.12, green: 0.25, blue: 0.50)],
                                             startPoint: .topLeading, endPoint: .bottomTrailing))
                    VStack(alignment: .leading, spacing: 10) {
                        HStack {
                            Text(account.currency).font(.caption.weight(.semibold)).foregroundColor(.white.opacity(0.7))
                            Spacer()
                            Text(account.isActive ? "Активен" : "Закрыт")
                                .font(.caption2.weight(.bold))
                                .padding(.horizontal, 8).padding(.vertical, 3)
                                .background((account.isActive ? Color.bankSuccess : Color.secondary).opacity(0.25))
                                .foregroundColor(account.isActive ? .bankSuccess : .secondary)
                                .cornerRadius(6)
                        }
                        Text(account.formattedBalance)
                            .font(.system(size: 28, weight: .bold, design: .rounded)).foregroundColor(.white)
                        Text(account.accountNumber).font(.caption.monospaced()).foregroundColor(.white.opacity(0.5))
                    }.padding(16)
                }
                .listRowInsets(EdgeInsets(top: 8, leading: 16, bottom: 8, trailing: 16))
            }

            if let err = accountsVM.errorMessage {
                Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
            }
            if let ok = accountsVM.successMessage {
                Section { Label(ok, systemImage: "checkmark.circle.fill").foregroundColor(.bankSuccess) }
            }

            if account.isActive {
                Section("Пополнить") {
                    HStack {
                        Image(systemName: "arrow.down.circle.fill").foregroundColor(.bankSuccess)
                        TextField("Сумма", text: $depositText).keyboardType(.decimalPad)
                        Button("OK") {
                            if let a = Double(depositText), a > 0 { accountsVM.deposit(to: account, amount: a); depositText = "" }
                        }.foregroundColor(.bankSuccess).fontWeight(.semibold).disabled(Double(depositText) == nil)
                    }
                }
                Section("Снять") {
                    HStack {
                        Image(systemName: "arrow.up.circle.fill").foregroundColor(.bankDanger)
                        TextField("Сумма", text: $withdrawText).keyboardType(.decimalPad)
                        Button("OK") {
                            if let a = Double(withdrawText), a > 0 { accountsVM.withdraw(from: account, amount: a); withdrawText = "" }
                        }.foregroundColor(.bankDanger).fontWeight(.semibold).disabled(Double(withdrawText) == nil)
                    }
                }
                Section {
                    Button("Закрыть счёт", role: .destructive) { showClose = true }
                }
            }

            Section("История операций (\(txVM.transactions.count))") {
                if txVM.isLoading { ProgressView().frame(maxWidth: .infinity) }
                else if txVM.transactions.isEmpty { Text("Нет операций").foregroundColor(.secondary) }
                else { ForEach(txVM.transactions) { TransactionRowView(tx: $0) } }
            }
        }
        .navigationTitle("Счёт")
        .navigationBarTitleDisplayMode(.inline)
        .onAppear { txVM.load() }
        .refreshable { txVM.load(); accountsVM.load() }
        .alert("Закрыть счёт?", isPresented: $showClose) {
            Button("Закрыть", role: .destructive) { accountsVM.closeAccount(account) }
            Button("Отмена", role: .cancel) {}
        } message: { Text("Баланс должен быть нулевым") }
    }
}
