//
//  ClientRowView.swift
//  Employee
//
//  Created by Gleb Korotkov on 28.03.2026.
//


import SwiftUI

struct ClientRowView: View {
    let client: UserDTO
    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                Circle()
                    .fill(client.isActive ? Color.bankAccent.opacity(0.15) : Color.secondary.opacity(0.1))
                    .frame(width: 46, height: 46)
                Text(client.initials)
                    .font(.subheadline.bold())
                    .foregroundColor(client.isActive ? .bankAccent : .secondary)
            }
            VStack(alignment: .leading, spacing: 2) {
                Text(client.fullName).font(.headline)
                Text(client.email).font(.caption).foregroundColor(.secondary)
            }
            Spacer()
            if !client.isActive {
                Image(systemName: "lock.fill").foregroundColor(.bankDanger).font(.caption)
            }
        }
        .padding(.vertical, 4)
    }
}
