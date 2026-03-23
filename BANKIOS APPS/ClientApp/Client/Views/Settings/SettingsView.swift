import SwiftUI

struct SettingsView: View {
    @ObservedObject var settingsVM: SettingsViewModel
    @ObservedObject var accountsVM: AccountsViewModel
    @Environment(\.colorScheme) var colorScheme

    var body: some View {
        NavigationStack {
            List {
                Section("Внешний вид") {
                    Toggle("Тёмная тема", isOn: Binding(
                        get: { settingsVM.isDarkTheme },
                        set: { settingsVM.setTheme($0) }
                    ))
                }

                Section("Скрытые счета") {
                    if accountsVM.accounts.isEmpty {
                        Text("Нет счетов").foregroundColor(.secondary)
                    }
                    ForEach(accountsVM.accounts) { acc in
                        HStack {
                            VStack(alignment: .leading, spacing: 2) {
                                Text("\(acc.currency) \(acc.maskedNumber)").font(.subheadline)
                                Text(acc.formattedBalance).font(.caption).foregroundColor(.secondary)
                            }
                            Spacer()
                            Toggle("", isOn: Binding(
                                get: { settingsVM.isHidden(acc.id) },
                                set: { _ in settingsVM.toggleAccountVisibility(acc.id) }
                            ))
                        }
                    }
                    Text("Скрытые счета не отображаются в списке, но продолжают работать.")
                        .font(.caption).foregroundColor(.secondary)
                }

                if let err = settingsVM.errorMessage {
                    Section {
                        Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger)
                    }
                }
            }
            .navigationTitle("Настройки")
            .onAppear { settingsVM.load() }
        }
    }
}
