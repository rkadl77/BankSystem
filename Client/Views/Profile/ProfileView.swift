




import SwiftUI

struct ProfileView: View {
    let user: UserDTO
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
                            Text(user.role == "client" ? "Клиент" : "Сотрудник")
                                .font(.caption).foregroundColor(.secondary)
                        }
                    }.padding(.vertical, 6)
                }
                Section("Контакты") {
                    LabeledContent("Email",   value: user.email)
                    LabeledContent("Телефон", value: user.phone)
                    LabeledContent("С нами с", value: user.createdAt.shortFormatted)
                }
                Section {
                    Button("Выйти", role: .destructive, action: onLogout)
                }
            }
            .navigationTitle("Профиль")
        }
    }
}
