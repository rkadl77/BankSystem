//
//  File.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//


import SwiftUI

struct ClientDetailView: View {
    let client: UserDTO
    @ObservedObject var clientsVM: ClientsViewModel
    @StateObject private var detailVM = ClientDetailViewModel()
    @State private var showBlockConfirm = false

    var body: some View {
        List {
            Section {
                HStack(spacing: 16) {
                    ZStack {
                        Circle()
                            .fill(client.isActive ? Color.bankAccent.opacity(0.15) : Color.secondary.opacity(0.1))
                            .frame(width: 60, height: 60)
                        Text(client.initials).font(.title2.bold())
                            .foregroundColor(client.isActive ? .bankAccent : .secondary)
                    }
                    VStack(alignment: .leading, spacing: 4) {
                        Text(client.fullName).font(.headline)
                        HStack {
                            Circle()
                                .fill(client.isActive ? Color.bankSuccess : Color.bankDanger)
                                .frame(width: 8, height: 8)
                            Text(client.isActive ? "Активен" : "Заблокирован")
                                .font(.caption)
                                .foregroundColor(client.isActive ? .bankSuccess : .bankDanger)
                        }
                    }
                }
                .padding(.vertical, 4)
            }

            Section("Контакты") {
                LabeledContent("Email",    value: client.email)
                LabeledContent("Телефон",  value: client.phone)
                LabeledContent("С нами с", value: client.createdAt.shortFormatted)
            }

            if detailVM.isLoading {
                Section { ProgressView("Загрузка...").frame(maxWidth: .infinity) }
            } else {
                Section("Счета (\(detailVM.accounts.count))") {
                    if detailVM.accounts.isEmpty {
                        Text("Нет счетов").foregroundColor(.secondary)
                    } else {
                        ForEach(detailVM.accounts) { acc in
                            NavigationLink(destination: AccountTransactionsView(account: acc)) {
                                EmployeeAccountRowView(account: acc)
                            }
                        }
                    }
                }

                Section("Кредиты (\(detailVM.credits.count))") {
                    if detailVM.credits.isEmpty {
                        Text("Нет кредитов").foregroundColor(.secondary)
                    } else {
                        ForEach(detailVM.credits) { credit in
                            EmployeeCreditRowView(credit: credit)
                        }
                    }
                }
            }

            if let err = detailVM.errorMessage {
                Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
            }

            Section("Действия") {
                Button(client.isActive ? "Заблокировать клиента" : "Разблокировать клиента",
                       role: client.isActive ? .destructive : .none) {
                    showBlockConfirm = true
                }
                .foregroundColor(client.isActive ? .bankDanger : .bankSuccess)
            }
        }
        .navigationTitle(client.firstName)
        .navigationBarTitleDisplayMode(.inline)
        .onAppear { detailVM.load(clientId: client.id) }
        .refreshable { detailVM.load(clientId: client.id) }
        .alert(client.isActive ? "Заблокировать клиента?" : "Разблокировать?",
               isPresented: $showBlockConfirm) {
            Button(client.isActive ? "Заблокировать" : "Разблокировать",
                   role: client.isActive ? .destructive : .none) {
                clientsVM.toggleBlock(client)
            }
            Button("Отмена", role: .cancel) {}
        } message: {
            Text(client.isActive
                 ? "Клиент потеряет доступ к приложению"
                 : "Клиент снова сможет войти в приложение")
        }
    }
}
