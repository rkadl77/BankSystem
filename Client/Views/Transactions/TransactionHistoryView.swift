//
//  TransactionHistoryView.swift
//  Client
//
//  Created by Gleb Korotkov on 28.02.2026.
//


import SwiftUI

struct TransactionHistoryView: View {
    @StateObject var vm: TransactionsViewModel

    var body: some View {
        List {
            if vm.isLoading {
                ProgressView("Загрузка...")
                    .frame(maxWidth: .infinity)
                    .padding()
            } else if vm.transactions.isEmpty {
                Text("Нет операций")
                    .foregroundColor(.secondary)
            } else {
                ForEach(vm.transactions) { tx in
                    TransactionRowView(tx: tx)
                }
            }
        }
        .navigationTitle("История операций")
        .onAppear { vm.load() }
    }
}

struct TransactionRowView: View {
    let tx: TransactionDTO
    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                Circle().fill(tx.typeColor.opacity(0.15)).frame(width: 40, height: 40)
                Image(systemName: tx.iconName).foregroundColor(tx.typeColor).font(.system(size: 18))
            }
            VStack(alignment: .leading, spacing: 2) {
                Text(tx.typeLocalizedName).font(.subheadline.weight(.medium))
                if let d = tx.description, !d.isEmpty { Text(d).font(.caption).foregroundColor(.secondary) }
                Text(tx.timestamp.longFormatted).font(.caption2).foregroundColor(.secondary)
            }
            Spacer()
            Text(tx.formattedAmount).font(.subheadline.weight(.semibold)).foregroundColor(tx.typeColor)
        }
        .padding(.vertical, 4)
    }
}
