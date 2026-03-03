//
//  EmployeeLoginView.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//



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
                    Text("BankApp").font(.system(size: 32, weight: .bold, design: .rounded)).foregroundColor(.white)
                    Text("Панель сотрудника").font(.subheadline).foregroundColor(.white.opacity(0.5))
                }
                .padding(.bottom, 48)

                VStack(spacing: 20) {
                    Text("Вход для сотрудников")
                        .font(.title3.bold()).frame(maxWidth: .infinity, alignment: .leading)

                    VStack(alignment: .leading, spacing: 6) {
                        Text("Корпоративный email")
                            .font(.caption.weight(.semibold)).foregroundColor(.secondary)
                        TextField("user@bank.com", text: $vm.email)
                            .textInputAutocapitalization(.never)
                            .keyboardType(.emailAddress)
                            .padding(12)
                            .background(Color(UIColor.systemGroupedBackground))
                            .cornerRadius(10)
                    }

                    if let err = vm.errorMessage {
                        HStack {
                            Image(systemName: "exclamationmark.circle.fill")
                            Text(err).font(.caption)
                        }.foregroundColor(.bankDanger)
                    }

                    Button(action: vm.login) {
                        HStack {
                            if vm.isLoading { ProgressView().tint(.white) }
                            else { Text("Войти").fontWeight(.semibold) }
                        }
                        .frame(maxWidth: .infinity).padding(.vertical, 14)
                        .background(Color.bankGold).foregroundColor(.black).cornerRadius(12)
                    }
                    .disabled(vm.isLoading)
                }
                .padding(24)
                .background(Color(UIColor.secondarySystemGroupedBackground))
                .cornerRadius(20)
                .padding(.horizontal, 24)

                Spacer()
            }
        }
    }
}
