import SwiftUI
import Combine

struct LoginView: View {
    @ObservedObject var vm: AuthViewModel

    var body: some View {
        ZStack {
            LinearGradient(colors: [.bankPrimary, Color(red: 0.12, green: 0.22, blue: 0.45)],
                           startPoint: .topLeading, endPoint: .bottomTrailing)
                .ignoresSafeArea()

            VStack(spacing: 0) {
                Spacer()

                VStack(spacing: 12) {
                    ZStack {
                        Circle().fill(Color.bankAccent.opacity(0.15)).frame(width: 90, height: 90)
                        Image(systemName: "building.columns.fill")
                            .font(.system(size: 38, weight: .semibold))
                            .foregroundColor(.bankAccent)
                    }
                    Text("BankApp")
                        .font(.system(size: 32, weight: .bold, design: .rounded))
                        .foregroundColor(.white)
                    Text("Личный кабинет")
                        .font(.subheadline).foregroundColor(.white.opacity(0.6))
                }
                .padding(.bottom, 48)

                VStack(spacing: 16) {
                    Text("Добро пожаловать")
                        .font(.title2.bold())
                        .frame(maxWidth: .infinity, alignment: .leading)

                    Text("Вход и регистрация выполняются на защищённой странице сервиса аутентификации.")
                        .font(.subheadline).foregroundColor(.secondary)
                        .frame(maxWidth: .infinity, alignment: .leading)

                    if let err = vm.errorMessage {
                        HStack(spacing: 6) {
                            Image(systemName: "exclamationmark.circle.fill")
                            Text(err).font(.caption)
                        }
                        .foregroundColor(.bankDanger)
                        .frame(maxWidth: .infinity, alignment: .leading)
                    }

                    Button(action: vm.startWebLogin) {
                        HStack(spacing: 8) {
                            if vm.isLoading {
                                ProgressView().tint(.white)
                            } else {
                                Image(systemName: "arrow.right.circle.fill")
                                Text("Войти").fontWeight(.semibold)
                            }
                        }
                        .frame(maxWidth: .infinity).padding(.vertical, 14)
                        .background(Color.bankAccent)
                        .foregroundColor(.white)
                        .cornerRadius(12)
                    }
                    .disabled(vm.isLoading)

                    Button(action: vm.startWebRegister) {
                        HStack(spacing: 8) {
                            Image(systemName: "person.badge.plus")
                            Text("Зарегистрироваться").fontWeight(.medium)
                        }
                        .frame(maxWidth: .infinity).padding(.vertical, 14)
                        .background(Color.bankAccent.opacity(0.12))
                        .foregroundColor(.bankAccent)
                        .cornerRadius(12)
                    }
                    .disabled(vm.isLoading)
                }
                .padding(24)
                .background(Color.bankSurface)
                .cornerRadius(20)
                .padding(.horizontal, 24)

                Spacer()
            }
        }
    }
}
