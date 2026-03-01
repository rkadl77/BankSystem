//
//  AccountDetailView.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//


import SwiftUI

struct AccountDetailView: View {
    let account: Account
    @ObservedObject var vm: AccountsViewModel

    @State private var depositAmountText = ""
    @State private var withdrawAmountText = ""
    @State private var showCloseConfirm = false
    @StateObject private var txVM: TransactionsViewModel

    init(account: Account, vm: AccountsViewModel) {
        self.account = account
        self.vm = vm
        _txVM = StateObject(wrappedValue: TransactionsViewModel(account: account))
    }

    var body: some View {
        List {

            // MARK: Balance Card
            Section {
                ZStack {
                    RoundedRectangle(cornerRadius: 16)
                        .fill(LinearGradient(
                            colors: [.bankPrimary, Color(red: 0.12, green: 0.25, blue: 0.50)],
                            startPoint: .topLeading,
                            endPoint: .bottomTrailing
                        ))
                    VStack(alignment: .leading, spacing: 8) {
                        HStack {
                            Text(account.type.rawValue)
                                .font(.caption.weight(.semibold))
                                .foregroundColor(.white.opacity(0.7))
                            Spacer()
                            Text(account.status.rawValue)
                                .font(.caption2.weight(.bold))
                                .padding(.horizontal, 8).padding(.vertical, 3)
                                .background(account.status.color.opacity(0.25))
                                .foregroundColor(account.status.color)
                                .cornerRadius(6)
                        }
                        Text(account.formattedBalance)
                            .font(.system(size: 28, weight: .bold, design: .rounded))
                            .foregroundColor(.white)
                        Text(account.accountNumber)
                            .font(.caption.monospaced())
                            .foregroundColor(.white.opacity(0.6))
                    }
                    .padding(16)
                }
                .listRowInsets(EdgeInsets(top: 8, leading: 16, bottom: 8, trailing: 16))
            }

            // MARK: Actions
            if account.isActive {
                Section("Пополнить счёт") {
                    HStack {
                        Image(systemName: "arrow.down.circle.fill")
                            .foregroundColor(.bankSuccess)
                        TextField("Сумма", text: $depositAmountText)
                            .keyboardType(.decimalPad)
                        Button("Пополнить") {
                            if let a = Double(depositAmountText) {
                                vm.deposit(to: account, amount: a)
                                depositAmountText = ""
                            }
                        }
                        .foregroundColor(.bankSuccess)
                        .fontWeight(.semibold)
                    }
                }

                Section("Снять со счёта") {
                    HStack {
                        Image(systemName: "arrow.up.circle.fill")
                            .foregroundColor(.bankDanger)
                        TextField("Сумма", text: $withdrawAmountText)
                            .keyboardType(.decimalPad)
                        Button("Снять") {
                            if let a = Double(withdrawAmountText) {
                                vm.withdraw(from: account, amount: a)
                                withdrawAmountText = ""
                            }
                        }
                        .foregroundColor(.bankDanger)
                        .fontWeight(.semibold)
                    }
                }

                Section {
                    Button("Закрыть счёт", role: .destructive) {
                        showCloseConfirm = true
                    }
                }
            }

            // MARK: Transaction History
            Section("История операций (\(txVM.transactions.count))") {
                if txVM.transactions.isEmpty {
                    Text("Нет операций")
                        .foregroundColor(.secondary)
                } else {
                    ForEach(txVM.transactions) { tx in
                        TransactionRowView(tx: tx)
                    }
                }
            }
        }
        .navigationTitle("Счёт")
        .navigationBarTitleDisplayMode(.inline)
        .alert("Закрыть счёт?", isPresented: $showCloseConfirm) {
            Button("Закрыть", role: .destructive) { vm.closeAccount(account) }
            Button("Отмена", role: .cancel) {}
        } message: {
            Text("Баланс счёта должен быть нулевым")
        }
        .overlay(alignment: .bottom) {
            if let err = vm.errorMessage {
                ToastView(message: err, isError: true).padding(.bottom, 16)
            } else if vm.showSuccess {
                ToastView(message: vm.successMessage, isError: false).padding(.bottom, 16)
            }
        }
    }
}
