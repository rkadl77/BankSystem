//
//  LoginView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//


import SwiftUI

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
                    Text("Личный кабинет").font(.subheadline).foregroundColor(.white.opacity(0.6))
                }
                .padding(.bottom, 48)
                VStack(spacing: 20) {
                    Text("Вход").font(.title2.bold()).frame(maxWidth: .infinity, alignment: .leading)
                    VStack(alignment: .leading, spacing: 6) {
                        Text("Email").font(.caption.weight(.semibold)).foregroundColor(.secondary)
                        TextField("email@mail.ru", text: $vm.email)
                            .textInputAutocapitalization(.never)
                            .keyboardType(.emailAddress)
                            .padding(12).background(Color.bankBackground).cornerRadius(10)
                    }
                    if let err = vm.errorMessage {
                        HStack { Image(systemName: "exclamationmark.circle.fill"); Text(err).font(.caption) }
                            .foregroundColor(.bankDanger)
                    }
                    Button(action: vm.login) {
                        HStack {
                            if vm.isLoading { ProgressView().tint(.white) }
                            else { Text("Войти").fontWeight(.semibold) }
                        }
                        .frame(maxWidth: .infinity).padding(.vertical, 14)
                        .background(Color.bankAccent).foregroundColor(.white).cornerRadius(12)
                    }
                    .disabled(vm.isLoading)
                }
                .padding(24).background(Color.bankSurface).cornerRadius(20).padding(.horizontal, 24)
                Spacer()
            }
        }
    }
}
