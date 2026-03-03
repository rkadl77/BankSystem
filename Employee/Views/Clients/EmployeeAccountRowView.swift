//
//  EmployeeAccountRowView.swift
//  Employee
//
//  Created by Gleb Korotkov on 04.03.2026.
//


import SwiftUI

struct EmployeeAccountRowView: View {
    let account: AccountDTO
    var body: some View {
        HStack(spacing: 12) {
            ZStack {
                RoundedRectangle(cornerRadius: 8)
                    .fill(account.isActive ? Color.bankPrimary.opacity(0.9) : Color.secondary.opacity(0.2))
                    .frame(width: 38, height: 38)
                Image(systemName: account.isActive ? "creditcard.fill" : "creditcard")
                    .foregroundColor(account.isActive ? .white : .secondary)
                    .font(.system(size: 16))
            }
            VStack(alignment: .leading, spacing: 1) {
                Text(account.currency + " " + account.maskedNumber).font(.subheadline)
                Text(account.isActive ? "Активен" : "Закрыт")
                    .font(.caption2).foregroundColor(account.isActive ? .bankSuccess : .secondary)
            }
            Spacer()
            Text(account.formattedBalance).font(.subheadline.weight(.semibold))
        }
        .padding(.vertical, 2)
    }
}
