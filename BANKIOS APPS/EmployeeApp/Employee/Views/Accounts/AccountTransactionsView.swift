import SwiftUI

struct AccountTransactionsView: View {
    let account: AccountDTO
    @StateObject private var vm = AccountsMonitorViewModel()

    var body: some View {
        List {
            Section {
                HStack {
                    VStack(alignment: .leading, spacing: 4) {
                        Text(account.currency + " счёт").font(.headline)
                        Text(account.accountNumber).font(.caption.monospaced()).foregroundColor(.secondary)
                        Text("ID: \(account.id.uuidString)")
                            .font(.system(size: 8, design: .monospaced))
                            .foregroundColor(.secondary)
                            .lineLimit(1)
                    }
                    Spacer()
                    Text(account.formattedBalance).font(.title3.weight(.bold))
                }
                .padding(.vertical, 4)
            }

            Section("Транзакции (\(vm.transactions.count))") {
                if vm.isLoading {
                    ProgressView("Загрузка...").frame(maxWidth: .infinity)
                } else if vm.transactions.isEmpty {
                    Text("Нет транзакций").foregroundColor(.secondary)
                } else {
                    ForEach(vm.transactions) { tx in
                        TransactionRowView(tx: tx)
                    }
                }
            }

            if let err = vm.errorMessage {
                Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
            }
        }
        .navigationTitle("Транзакции")
        .navigationBarTitleDisplayMode(.inline)
        .onAppear {
            vm.loadTransactions(for: account.id)
            vm.startRealTime(for: account.id)
        }
        .onDisappear { vm.stopRealTime() }
        .refreshable { vm.loadTransactions(for: account.id) }
    }
}
