import SwiftUI

struct TransferView: View {
    let account: AccountDTO
    @StateObject private var vm = TransferViewModel()
    @ObservedObject var accountsVM: AccountsViewModel
    @Environment(\.dismiss) var dismiss

    var body: some View {
        NavigationStack {
            Form {
                Section {
                    HStack {
                        VStack(alignment: .leading, spacing: 2) {
                            Text("Со счёта").font(.caption).foregroundColor(.secondary)
                            Text("\(account.currency) \(account.maskedNumber)").font(.headline)
                        }
                        Spacer()
                        Text(account.formattedBalance).font(.subheadline.weight(.semibold))
                    }
                }

                Section("Получатель") {
                    VStack(alignment: .leading, spacing: 4) {
                        Text("ID счёта получателя").font(.caption).foregroundColor(.secondary)
                        TextField("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx", text: $vm.toAccountIdText)
                            .font(.system(.body, design: .monospaced))
                            .autocorrectionDisabled()
                            .textInputAutocapitalization(.never)
                    }
                }

                let otherAccounts = accountsVM.accounts.filter { $0.id != account.id && $0.isActive }
                if !otherAccounts.isEmpty {
                    Section("Мои другие счета") {
                        ForEach(otherAccounts) { acc in
                            Button {
                                vm.toAccountIdText = acc.id.uuidString
                            } label: {
                                HStack {
                                    VStack(alignment: .leading, spacing: 2) {
                                        Text("\(acc.currency) \(acc.maskedNumber)").font(.subheadline)
                                        Text(acc.formattedBalance).font(.caption).foregroundColor(.secondary)
                                    }
                                    Spacer()
                                    if vm.toAccountIdText == acc.id.uuidString {
                                        Image(systemName: "checkmark").foregroundColor(.bankAccent)
                                    }
                                }
                            }
                            .foregroundColor(.primary)
                        }
                    }
                }

                Section("Сумма") {
                    HStack {
                        TextField("0.00", text: $vm.amountText)
                            .keyboardType(.decimalPad)
                        Text(account.currencySymbol).foregroundColor(.secondary)
                    }
                    if account.currency != "RUB" || true {
                        Text("При переводе между счетами в разных валютах конвертация происходит автоматически по текущему курсу")
                            .font(.caption).foregroundColor(.secondary)
                    }
                }

                Section("Описание (необязательно)") {
                    TextField("За что перевод...", text: $vm.descriptionText, axis: .vertical)
                        .lineLimit(1...3)
                }

                if let err = vm.errorMessage {
                    Section {
                        Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger)
                    }
                }
                if let ok = vm.successMessage {
                    Section {
                        Label(ok, systemImage: "checkmark.circle.fill").foregroundColor(.bankSuccess)
                    }
                }

                Section {
                    Button {
                        vm.transfer(from: account) {
                            accountsVM.load()
                            Task {
                                try? await Task.sleep(nanoseconds: 1_500_000_000)
                                await MainActor.run { dismiss() }
                            }
                        }
                    } label: {
                        HStack {
                            if vm.isLoading { ProgressView() }
                            else { Text("Перевести").fontWeight(.semibold) }
                        }
                        .frame(maxWidth: .infinity)
                    }
                    .foregroundColor(.bankAccent)
                    .disabled(vm.isLoading || vm.toAccountIdText.isEmpty || vm.amountText.isEmpty)
                }
            }
            .navigationTitle("Перевод")
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Отмена") { dismiss() }
                }
            }
        }
    }
}
