//
//  EmployeeLoginView.swift
//  Client
//
//  Created by Gleb Korotkov on 26.03.2026.
//


import SwiftUI

struct EmployeeLoginView: View {
    @ObservedObject var vm: EmployeeAuthViewModel
    
    var body: some View {
        ZStack {
            LinearGradient(colors: [Color(red: 0.08, green: 0.08, blue: 0.12),
                                     Color(red: 0.15, green: 0.10, blue: 0.25)],
                           startPoint: .topLeading, endPoint: .bottomTrailing)
                .ignoresSafeArea()
            
            VStack(spacing: 0) {
                Spacer()
                
                VStack(spacing: 12) {
                    ZStack {
                        Circle()
                            .fill(Color.bankGold.opacity(0.15))
                            .frame(width: 90, height: 90)
                        Image(systemName: "person.badge.shield.checkmark.fill")
                            .font(.system(size: 36, weight: .semibold))
                            .foregroundColor(.bankGold)
                    }
                    Text("Банк — CRM")
                        .font(.system(size: 30, weight: .bold, design: .rounded))
                        .foregroundColor(.white)
                    Text("Портал сотрудника")
                        .font(.subheadline)
                        .foregroundColor(.white.opacity(0.5))
                }
                .padding(.bottom, 48)
                
                VStack(spacing: 20) {
                    Text("Вход для сотрудников")
                        .font(.title2.bold())
                        .frame(maxWidth: .infinity, alignment: .leading)
                    
                    VStack(alignment: .leading, spacing: 6) {
                        Text("Корпоративный email")
                            .font(.caption.weight(.semibold))
                            .foregroundColor(.secondary)
                        TextField("ivanova@bank.ru", text: $vm.email)
                            .textInputAutocapitalization(.never)
                            .keyboardType(.emailAddress)
                            .padding(12)
                            .background(Color.bankBackground)
                            .cornerRadius(10)
                    }
                    
                    if let err = vm.errorMessage {
                        HStack {
                            Image(systemName: "exclamationmark.circle.fill")
                            Text(err).font(.caption)
                        }
                        .foregroundColor(.bankDanger)
                    }
                    
                    Button(action: vm.login) {
                        HStack {
                            if vm.isLoading {
                                ProgressView().tint(.black)
                            } else {
                                Text("Войти").fontWeight(.semibold)
                            }
                        }
                        .frame(maxWidth: .infinity)
                        .padding(.vertical, 14)
                        .background(Color.bankGold)
                        .foregroundColor(.black)
                        .cornerRadius(12)
                    }
                    .disabled(vm.isLoading)
                    
                    VStack(spacing: 4) {
                        Text("Демо-доступ:").font(.caption2).foregroundColor(.secondary)
                        Text("ivanova@bank.ru")
                            .font(.caption.monospaced())
                            .foregroundColor(.bankGold)
                    }
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
