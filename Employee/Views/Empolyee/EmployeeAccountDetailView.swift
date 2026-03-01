//
//  EmployeeAccountDetailView.swift
//  Client
//
//  Created by Gleb Korotkov on 25.02.2026
//

import SwiftUI

struct EmployeeAccountDetailView: View {
    let account: Account
    private let db = MockDataService.shared
    
    var transactions: [Transaction] { db.transactions(for: account.id) }
    
    var body: some View {
        List {
            Section("Счёт") {
                LabeledContent("Номер", value: account.accountNumber)
                LabeledContent("Тип", value: account.type.rawValue)
                LabeledContent("Баланс", value: account.formattedBalance)
                LabeledContent("Статус", value: account.status.rawValue)
                LabeledContent("Открыт", value: account.openedAt.shortFormatted)
            }
            Section("История операций (\(transactions.count))") {
                if transactions.isEmpty {
                    Text("Нет операций").foregroundColor(.secondary)
                } else {
                    ForEach(transactions) { tx in TransactionRowView(tx: tx) }
                }
            }
        }
        .navigationTitle("Детали счёта")
    }
}

struct TransactionRowView: View {
    let tx: Transaction
    
    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                Circle()
                    .fill(tx.type.color.opacity(0.15))
                    .frame(width: 40, height: 40)
                Image(systemName: tx.iconName)
                    .foregroundColor(tx.type.color)
                    .font(.system(size: 18))
            }
            
            VStack(alignment: .leading, spacing: 2) {
                Text(tx.type.rawValue)
                    .font(.subheadline.weight(.medium))
                if !tx.description.isEmpty {
                    Text(tx.description)
                        .font(.caption)
                        .foregroundColor(.secondary)
                }
                Text(tx.createdAt.longFormatted)
                    .font(.caption2)
                    .foregroundColor(.secondary)
            }
            
            Spacer()
            
            VStack(alignment: .trailing, spacing: 2) {
                Text(tx.formattedAmount)
                    .font(.subheadline.weight(.semibold))
                    .foregroundColor(tx.type.color)
                Text("Баланс: \(String(format: "%.2f ₽", tx.balanceAfter))")
                    .font(.caption2)
                    .foregroundColor(.secondary)
            }
        }
        .padding(.vertical, 4)
    }
}
