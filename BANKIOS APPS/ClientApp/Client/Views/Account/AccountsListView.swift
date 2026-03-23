import SwiftUI

struct AccountsListView: View {
    @ObservedObject var vm: AccountsViewModel
    @ObservedObject var settingsVM: SettingsViewModel
    @State private var showOpenSheet = false
    @State private var currency = "RUB"

    var visibleActive: [AccountDTO]  { vm.accounts.filter { $0.isActive  && !settingsVM.isHidden($0.id) } }
    var hiddenActive:  [AccountDTO]  { vm.accounts.filter { $0.isActive  &&  settingsVM.isHidden($0.id) } }
    var closed:        [AccountDTO]  { vm.accounts.filter { !$0.isActive } }

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

                Section("Активные счета (\(visibleActive.count))") {
                    if visibleActive.isEmpty && !vm.isLoading {
                        Text("Нет счетов. Откройте первый!").foregroundColor(.secondary)
                    }
                    ForEach(visibleActive) { acc in
                        NavigationLink(destination: AccountDetailView(account: acc,
                                                                      accountsVM: vm,
                                                                      settingsVM: settingsVM)) {
                            AccountRowView(account: acc)
                        }
                    }
                }

                if !hiddenActive.isEmpty {
                    Section("Скрытые (\(hiddenActive.count))") {
                        ForEach(hiddenActive) { acc in
                            NavigationLink(destination: AccountDetailView(account: acc,
                                                                          accountsVM: vm,
                                                                          settingsVM: settingsVM)) {
                                HStack {
                                    AccountRowView(account: acc)
                                    Image(systemName: "eye.slash").foregroundColor(.secondary).font(.caption)
                                }
                            }
                        }
                    }
                }

                if !closed.isEmpty {
                    Section("Закрытые") {
                        ForEach(closed) { acc in
                            NavigationLink(destination: AccountDetailView(account: acc,
                                                                          accountsVM: vm,
                                                                          settingsVM: settingsVM)) {
                                AccountRowView(account: acc)
                            }
                        }
                    }
                }
            }
            .navigationTitle("Мои счета")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button { showOpenSheet = true } label: { Image(systemName: "plus") }
                }
            }
            .sheet(isPresented: $showOpenSheet) {
                OpenAccountSheet(currency: $currency) {
                    vm.openAccount(currency: currency)
                    showOpenSheet = false
                }
            }
            .refreshable { vm.load() }
        }
    }
}
