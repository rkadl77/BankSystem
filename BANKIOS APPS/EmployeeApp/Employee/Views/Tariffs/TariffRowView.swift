//
//  TariffRowView.swift
//  Employee
//
//  Created by Gleb Korotkov on 04.03.2026.
//

import SwiftUI

struct TariffRowView: View {
    let tariff: CreditTariffDTO
    @ObservedObject var vm: TariffsViewModel
    @State private var showConfirm = false

    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                RoundedRectangle(cornerRadius: 10)
                    .fill(tariff.isActive ? Color.bankGold.opacity(0.15) : Color.secondary.opacity(0.1))
                    .frame(width: 46, height: 46)
                Text(tariff.formattedRate)
                    .font(.caption.weight(.bold))
                    .foregroundColor(tariff.isActive ? .bankGold : .secondary)
            }
            VStack(alignment: .leading, spacing: 2) {
                Text(tariff.name).font(.headline)
                if !tariff.description.isEmpty {
                    Text(tariff.description).font(.caption).foregroundColor(.secondary).lineLimit(1)
                }
                Text("Создан: \(tariff.createdAt.shortFormatted)").font(.caption2).foregroundColor(.secondary)
            }
            Spacer()
            Button {
                showConfirm = true
            } label: {
                Image(systemName: tariff.isActive ? "pause.circle" : "play.circle")
                    .foregroundColor(tariff.isActive ? .bankDanger : .bankSuccess)
                    .font(.title3)
            }
            .buttonStyle(.plain)
        }
        .padding(.vertical, 4)
        .alert(tariff.isActive ? "Деактивировать тариф?" : "Активировать тариф?",
               isPresented: $showConfirm) {
            Button(tariff.isActive ? "Деактивировать" : "Активировать",
                   role: tariff.isActive ? .destructive : .none) {
                vm.toggleActive(tariff)
            }
            Button("Отмена", role: .cancel) {}
        }
    }
}
