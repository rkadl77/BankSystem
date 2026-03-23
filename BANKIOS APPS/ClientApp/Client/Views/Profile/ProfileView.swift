import SwiftUI

struct ProfileView: View {
    let user: UserDTO
    @ObservedObject var settingsVM: SettingsViewModel
    @ObservedObject var accountsVM: AccountsViewModel
    let onLogout: () -> Void

    var body: some View {
        NavigationStack {
            List {
                Section {
                    HStack(spacing: 16) {
                        ZStack {
                            Circle().fill(Color.bankAccent.opacity(0.15)).frame(width: 60, height: 60)
                            Text(user.initials).font(.title2.bold()).foregroundColor(.bankAccent)
                        }
                        VStack(alignment: .leading) {
                            Text(user.fullName).font(.headline)
                            Text("Клиент").font(.caption).foregroundColor(.secondary)
                        }
                    }.padding(.vertical, 6)
                }

                Section("Контакты") {
                    LabeledContent("Email",   value: user.email)
                    LabeledContent("Телефон", value: user.phone)
                    LabeledContent("С нами с", value: user.createdAt.shortFormatted)
                }

                Section("Внешний вид") {
                    Toggle("Тёмная тема", isOn: Binding(
                        get: { settingsVM.isDarkTheme },
                        set: { settingsVM.setTheme($0) }
                    ))
                }

                Section {
                    NavigationLink("Настройки счетов") {
                        SettingsView(settingsVM: settingsVM, accountsVM: accountsVM)
                    }
                }

                Section {
                    Button("Выйти", role: .destructive, action: onLogout)
                }
            }
            .navigationTitle("Профиль")
        }
    }
}
