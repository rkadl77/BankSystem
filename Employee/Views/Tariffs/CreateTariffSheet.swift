//
//  CreateTariffSheet.swift
//  Employee
//
//  Created by Gleb Korotkov on 04.03.2026.
//

import SwiftUI

struct CreateTariffSheet: View {
    @ObservedObject var vm: TariffsViewModel
    @Environment(\.dismiss) var dismiss

    var canCreate: Bool {
        !vm.newName.trimmingCharacters(in: .whitespaces).isEmpty &&
        Double(vm.newRate) != nil && Double(vm.newRate)! > 0
    }

    var body: some View {
        NavigationStack {
            Form {
                Section("Название тарифа") {
                    TextField("Например: Потребительский", text: $vm.newName)
                }
                Section("Процентная ставка") {
                    HStack {
                        TextField("15.5", text: $vm.newRate).keyboardType(.decimalPad)
                        Text("% годовых").foregroundColor(.secondary)
                    }
                }
                Section("Описание (необязательно)") {
                    TextField("Кредит на любые цели...", text: $vm.newDescription, axis: .vertical)
                        .lineLimit(3...5)
                }
                if let err = vm.errorMessage {
                    Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
                }
                Section {
                    Button("Создать тариф") { vm.create(); if vm.errorMessage == nil { dismiss() } }
                        .foregroundColor(.bankGold).fontWeight(.semibold)
                        .disabled(!canCreate)
                }
            }
            .navigationTitle("Новый тариф")
            .toolbar { ToolbarItem(placement: .cancellationAction) { Button("Отмена") { dismiss() } } }
        }
    }
}
