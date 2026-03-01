//
//  TariffsManagementView.swift
//  Client
//
//  Created by Gleb Korotkov on 28.02.2026.
//


import SwiftUI

struct TariffsManagementView: View {
    @ObservedObject var vm: TariffViewModel
    @State private var showAddSheet = false
    
    var body: some View {
        NavigationStack {
            List {
                if let success = vm.successMessage {
                    Section {
                        Label(success, systemImage: "checkmark.circle.fill")
                            .foregroundColor(.bankSuccess)
                    }
                }
                
                Section("Тарифы (\(vm.tariffs.count))") {
                    ForEach(vm.tariffs) { tariff in
                        TariffRowView(tariff: tariff)
                    }
                }
            }
            .navigationTitle("Кредитные тарифы")
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button { showAddSheet = true } label: {
                        Image(systemName: "plus")
                    }
                }
            }
            .sheet(isPresented: $showAddSheet) {
                AddTariffSheet(vm: vm, onDone: { showAddSheet = false })
            }
        }
    }
}

private struct TariffRowView: View {
    let tariff: CreditTariff
    
    var body: some View {
        VStack(alignment: .leading, spacing: 6) {
            HStack {
                Text(tariff.name).font(.headline)
                Spacer()
                Text(tariff.formattedRate)
                    .font(.subheadline.weight(.bold))
                    .foregroundColor(.bankGold)
            }
            HStack {
                Text("Сумма: \(Int(tariff.minAmount)) – \(Int(tariff.maxAmount)) ₽")
                    .font(.caption).foregroundColor(.secondary)
                Spacer()
                Text("Срок: \(tariff.minTermDays)–\(tariff.maxTermDays) дн.")
                    .font(.caption).foregroundColor(.secondary)
            }
        }
        .padding(.vertical, 2)
    }
}

private struct AddTariffSheet: View {
    @ObservedObject var vm: TariffViewModel
    let onDone: () -> Void
    
    var body: some View {
        NavigationStack {
            Form {
                Section("Новый тариф") {
                    HStack {
                        Text("Название")
                        Spacer()
                        TextField("Стандартный", text: $vm.newName)
                            .multilineTextAlignment(.trailing)
                    }
                    HStack {
                        Text("Ставка, %")
                        Spacer()
                        TextField("12.5", text: $vm.newRate)
                            .keyboardType(.decimalPad)
                            .multilineTextAlignment(.trailing)
                    }
                }
                
                if let err = vm.errorMessage {
                    Section {
                        Label(err, systemImage: "exclamationmark.circle.fill")
                            .foregroundColor(.bankDanger)
                    }
                }
                
                Section {
                    Button("Создать тариф") {
                        vm.createTariff()
                        if vm.errorMessage == nil { onDone() }
                    }
                    .foregroundColor(.bankGold)
                    .fontWeight(.semibold)
                }
            }
            .navigationTitle("Новый тариф")
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Отмена") { onDone() }
                }
            }
        }
        .presentationDetents([.medium])
    }
}
