//
//  EmployeeCreditRowView.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//


import SwiftUI

struct EmployeeCreditRowView: View {
    let credit: CreditDTO
    var body: some View {
        VStack(alignment: .leading, spacing: 6) {
            HStack {
                Text(credit.tariffName).font(.subheadline.weight(.medium))
                Spacer()
                Text(credit.statusLabel)
                    .font(.caption2.weight(.semibold))
                    .padding(.horizontal, 7).padding(.vertical, 3)
                    .background(credit.statusColor.opacity(0.15))
                    .foregroundColor(credit.statusColor).cornerRadius(5)
            }
            HStack {
                VStack(alignment: .leading, spacing: 1) {
                    Text("Выдано").font(.caption2).foregroundColor(.secondary)
                    Text(credit.formattedAmount).font(.caption.weight(.semibold))
                }
                Spacer()
                VStack(alignment: .trailing, spacing: 1) {
                    Text("Остаток").font(.caption2).foregroundColor(.secondary)
                    Text(credit.formattedRemaining).font(.caption.weight(.semibold))
                }
                Spacer()
                VStack(alignment: .trailing, spacing: 1) {
                    Text("Ставка").font(.caption2).foregroundColor(.secondary)
                    Text(credit.formattedRate).font(.caption.weight(.semibold))
                }
            }
            ProgressView(value: credit.progressFraction).tint(.bankAccent)
        }
        .padding(.vertical, 4)
    }
}
