import SwiftUI

struct DashboardView: View {
    @ObservedObject var authVM: AuthViewModel
    @StateObject private var accountsVM: AccountsViewModel
    @StateObject private var creditVM: CreditViewModel
    @StateObject private var settingsVM: SettingsViewModel

    init(authVM: AuthViewModel, user: UserDTO) {
        self.authVM = authVM
        _accountsVM = StateObject(wrappedValue: AccountsViewModel(userId: user.id))
        _creditVM   = StateObject(wrappedValue: CreditViewModel(clientId: user.id))
        _settingsVM = StateObject(wrappedValue: SettingsViewModel(userId: user.id))
    }

    var user: UserDTO { authVM.currentUser! }

    var body: some View {
        TabView {
            AccountsListView(vm: accountsVM, settingsVM: settingsVM)
                .tabItem { Label("Счета", systemImage: "creditcard.fill") }
                .onAppear { accountsVM.load() }

            CreditsView(creditVM: creditVM, accountsVM: accountsVM)
                .tabItem { Label("Кредиты", systemImage: "banknote.fill") }
                .onAppear { creditVM.load() }

            NavigationStack {
                CreditRatingView(clientId: user.id)
            }
            .tabItem { Label("Рейтинг", systemImage: "star.circle.fill") }

            ProfileView(user: user, settingsVM: settingsVM, accountsVM: accountsVM, onLogout: authVM.logout)
                .tabItem { Label("Профиль", systemImage: "person.fill") }
        }
        .tint(.bankAccent)
        .onAppear { settingsVM.load() }
        .preferredColorScheme(settingsVM.isDarkTheme ? .dark : .light)
    }
}
