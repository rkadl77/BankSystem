//
//  AccountRowView.swift
//  Client
//
//  Created by Gleb Korotkov on 24.02.2026.
//

import SwiftUI

struct AccountRowView: View {
    let account: AccountDTO
    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                RoundedRectangle(cornerRadius: 10)
                    .fill(account.isActive ? Color.bankPrimary : Color.secondary.opacity(0.2))
                    .frame(width: 46, height: 46)
                Image(systemName: account.isActive ? "creditcard.fill" : "creditcard")
                    .foregroundColor(account.isActive ? .white : .secondary).font(.system(size: 20))
            }
            VStack(alignment: .leading, spacing: 2) {
                Text(account.currency).font(.headline)
                Text(account.maskedNumber).font(.caption.monospaced()).foregroundColor(.secondary)
            }
            Spacer()
            Text(account.formattedBalance).font(.subheadline.weight(.semibold))
        }
        .padding(.vertical, 4)
    }
}
