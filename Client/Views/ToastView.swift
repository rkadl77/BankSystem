//
//  ToastView.swift
//  Client
//
//  Created by Gleb Korotkov on 28.02.2026.
//


import SwiftUI

struct ToastView: View {
    let message: String
    let isError: Bool

    var body: some View {
        HStack(spacing: 8) {
            Image(systemName: isError ? "exclamationmark.circle.fill" : "checkmark.circle.fill")
            Text(message)
                .font(.subheadline.weight(.medium))
        }
        .padding(.horizontal, 20)
        .padding(.vertical, 12)
        .background(isError ? Color.bankDanger : Color.bankSuccess)
        .foregroundColor(.white)
        .cornerRadius(30)
        .shadow(radius: 8)
        .transition(.move(edge: .bottom).combined(with: .opacity))
        .animation(.spring(), value: message)
    }
}
