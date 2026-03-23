import SwiftUI

struct EmployeeLoginView: View {
    @ObservedObject var vm: EmployeeAuthViewModel

    var body: some View {
        ZStack {
            LinearGradient(colors: [Color(red: 0.08, green: 0.08, blue: 0.12),
                                    Color(red: 0.12, green: 0.10, blue: 0.18)],
                           startPoint: .topLeading, endPoint: .bottomTrailing)
                .ignoresSafeArea()

            VStack(spacing: 0) {
                Spacer()

                VStack(spacing: 12) {
                    ZStack {
                        Circle().fill(Color.bankGold.opacity(0.15)).frame(width: 90, height: 90)
                        Image(systemName: "shield.checkered")
                            .font(.system(size: 38, weight: .semibold))
                            .foregroundColor(.bankGold)
                    }
                    Text("BankApp")
                        .font(.system(size: 32, weight: .bold, design: .rounded))
                        .foregroundColor(.white)
                    Text("Панель сотрудника")
                        .font(.subheadline).foregroundColor(.white.opacity(0.5))
                }
                .padding(.bottom, 48)

                VStack(spacing: 16) {
                    Text("Вход для сотрудников")
                        .font(.title3.bold())
                        .frame(maxWidth: .infinity, alignment: .leading)

                    Text("Вход выполняется на защищённой странице сервиса аутентификации.")
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
                                ProgressView().tint(.black)
                            } else {
                                Image(systemName: "arrow.right.circle.fill")
                                Text("Войти").fontWeight(.semibold)
                            }
                        }
                        .frame(maxWidth: .infinity).padding(.vertical, 14)
                        .background(Color.bankGold)
                        .foregroundColor(.black)
                        .cornerRadius(12)
                    }
                    .disabled(vm.isLoading)
                }
                .padding(24)
                .background(Color(.secondarySystemGroupedBackground))
                .cornerRadius(20)
                .padding(.horizontal, 24)

                VStack(spacing: 10) {
                    Button(action: vm.startWebRegister) {
                        Text("Регистрация")
                            .fontWeight(.medium)
                            .frame(maxWidth: .infinity).padding(.vertical, 13)
                            .background(Color.white.opacity(0.08))
                            .foregroundColor(.white)
                            .cornerRadius(12)
                            .overlay(RoundedRectangle(cornerRadius: 12).stroke(Color.white.opacity(0.2), lineWidth: 1))
                    }

                    Button(action: vm.startWebRegisterEmployee) {
                        Text("Регистрация сотрудника")
                            .fontWeight(.medium)
                            .frame(maxWidth: .infinity).padding(.vertical, 13)
                            .background(Color.bankGold.opacity(0.12))
                            .foregroundColor(.bankGold)
                            .cornerRadius(12)
                            .overlay(RoundedRectangle(cornerRadius: 12).stroke(Color.bankGold.opacity(0.4), lineWidth: 1))
                    }
                }
                .padding(.horizontal, 24)
                .padding(.top, 16)

                Spacer()
            }
        }
    }
}
