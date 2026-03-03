//
//  TransactionRowView.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//

import SwiftUI

struct TransactionRowView: View {
    let tx: TransactionDTO
    
    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                Circle()
                    .fill(tx.typeColor.opacity(0.15))
                    .frame(width: 40, height: 40)
                Image(systemName: tx.iconName)
                    .foregroundColor(tx.typeColor)
                    .font(.system(size: 18))
            }
            
            VStack(alignment: .leading, spacing: 2) {
                Text(tx.type)
                    .font(.subheadline.weight(.medium))
                if !tx.description!.isEmpty {
                    Text(tx.description!)
                        .font(.caption)
                        .foregroundColor(.secondary)
                }
            }
            
            Spacer()
            
            VStack(alignment: .trailing, spacing: 2) {
                Text(tx.formattedAmount)
                    .font(.subheadline.weight(.semibold))
                    .foregroundColor(tx.typeColor)
                Text("Баланс: \(String(format: "%.2f ₽", tx.amount))")
                    .font(.caption2)
                    .foregroundColor(.secondary)
            }
        }
        .padding(.vertical, 4)
    }
}
