//
//  ClientDetailView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//


import SwiftUI

// MARK: — Client Detail

struct ClientDetailView: View {
    let client: User
    @ObservedObject var clientsVM: AllClientsViewModel
    
    private let db = MockDataService.shared
    
    var accounts: [Account] { db.accounts(for: client.id) }
    var credits: [Credit]   { db.credits(for: client.id) }
    
    var body: some View {
        List {
            Section("Информация") {
                LabeledContent("ФИО", value: client.fullName)
                LabeledContent("Email", value: client.email)
                LabeledContent("Телефон", value: client.phone)
                HStack {
                    Text("Статус")
                    Spacer()
                    Text(client.status.label)
                        .foregroundColor(client.status.color)
                        .fontWeight(.semibold)
                }
                LabeledContent("Дата регистрации", value: client.createdAt.shortFormatted)
            }
            
            Section("Счета (\(accounts.count))") {
                if accounts.isEmpty {
                    Text("Нет счетов").foregroundColor(.secondary)
                } else {
                    ForEach(accounts) { acc in
                        NavigationLink {
                            EmployeeAccountDetailView(account: acc)
                        } label: {
                            AccountRowView(account: acc)
                        }
                    }
                }
            }
            
            Section("Кредиты (\(credits.count))") {
                if credits.isEmpty {
                    Text("Нет кредитов").foregroundColor(.secondary)
                } else {
                    ForEach(credits) { credit in
                        EmployeeCreditRowView(credit: credit)
                    }
                }
            }
            
            Section {
                Button(client.isBlocked ? "Разблокировать" : "Заблокировать",
                       role: client.isBlocked ? nil : .destructive) {
                    clientsVM.toggleBlock(client)
                }
            }
        }
        .navigationTitle(client.fullName)
        .navigationBarTitleDisplayMode(.inline)
    }
}


private struct EmployeeCreditRowView: View {
    let credit: Credit
    
    var body: some View {
        VStack(alignment: .leading, spacing: 4) {
            HStack {
                Text("Кредит").font(.subheadline.weight(.medium))
                Spacer()
                Text(credit.status.rawValue)
                    .font(.caption2.weight(.semibold))
                    .foregroundColor(credit.status.color)
            }
            HStack {
                Text("Выдано: \(credit.formattedAmount)")
                    .font(.caption).foregroundColor(.secondary)
                Spacer()
                Text("Остаток: \(credit.formattedRemaining)")
                    .font(.caption).foregroundColor(.secondary)
            }
            HStack {
                Text("Ставка: \(String(format: "%.1f%%", credit.interestRate))")
                    .font(.caption2).foregroundColor(.secondary)
                Spacer()
                Text("\(credit.startDate.shortFormatted) → \(credit.endDate.shortFormatted)")
                    .font(.caption2).foregroundColor(.secondary)
            }
        }
        .padding(.vertical, 2)
    }
}

struct AccountRowView: View {
    let account: Account
    
    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                RoundedRectangle(cornerRadius: 10)
                    .fill(account.isActive ? Color.bankPrimary : Color.secondary.opacity(0.2))
                    .frame(width: 46, height: 46)
                Image(systemName: account.isActive ? "creditcard.fill" : "creditcard")
                    .foregroundColor(account.isActive ? .white : .secondary)
                    .font(.system(size: 20))
            }
            
            VStack(alignment: .leading, spacing: 2) {
                Text(account.type.rawValue)
                    .font(.headline)
                Text(account.maskedNumber)
                    .font(.caption.monospaced())
                    .foregroundColor(.secondary)
            }
            
            Spacer()
            
            VStack(alignment: .trailing, spacing: 2) {
                Text(account.formattedBalance)
                    .font(.subheadline.weight(.semibold))
                Text(account.status.rawValue)
                    .font(.caption2)
                    .foregroundColor(account.status.color)
            }
        }
        .padding(.vertical, 4)
    }
}
