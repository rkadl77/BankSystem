//
//  TakeCreditView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//


import SwiftUI

struct TakeCreditView: View {
    @ObservedObject var creditVM: CreditViewModel
    let activeAccounts: [AccountDTO]
    @Environment(\.dismiss) var dismiss

    var body: some View {
        NavigationStack {
            Form {
                Section("Тариф") {
                    if creditVM.tariffs.isEmpty { Text("Нет доступных тарифов").foregroundColor(.secondary) }
                    else {
                        Picker("Тариф", selection: $creditVM.selectedTariff) {
                            Text("Выберите...").tag(CreditTariffDTO?.none)
                            ForEach(creditVM.tariffs) { t in
                                VStack(alignment: .leading) {
                                    Text(t.name)
                                    Text("\(t.formattedRate) годовых").font(.caption).foregroundColor(.secondary)
                                }.tag(t as CreditTariffDTO?)
                            }
                        }
                        if let t = creditVM.selectedTariff {
                            LabeledContent("Ставка", value: t.formattedRate)
                            if !t.description.isEmpty { Text(t.description).font(.caption).foregroundColor(.secondary) }
                        }
                    }
                }
                Section("Счёт зачисления") {
                    if activeAccounts.isEmpty { Text("Нет активных счетов").foregroundColor(.secondary) }
                    else {
                        Picker("Счёт", selection: $creditVM.selectedAccountId) {
                            Text("Выберите...").tag(UUID?.none)
                            ForEach(activeAccounts) { a in
                                Text("\(a.currency) — \(a.maskedNumber)").tag(a.id as UUID?)
                            }
                        }
                    }
                }
                Section("Сумма") {
                    HStack {
                        TextField("Например 100000", text: $creditVM.amountText).keyboardType(.decimalPad)
                        Text("₽").foregroundColor(.secondary)
                    }
                }
                Section("Срок") {
                    Picker("Срок", selection: $creditVM.selectedTermMonths) {
                        Text("1 месяц").tag(1)
                        Text("3 месяца").tag(3)
                        Text("6 месяцев").tag(6)
                        Text("12 месяцев").tag(12)
                        Text("24 месяца").tag(24)
                        Text("36 месяцев").tag(36)
                    }
                    .pickerStyle(.menu)
                }
                if let err = creditVM.errorMessage {
                    Section { Label(err, systemImage: "exclamationmark.circle.fill").foregroundColor(.bankDanger) }
                }
                Section {
                    Button {
                        if creditVM.submitTakeCredit() { dismiss() }
                    } label: {
                        if creditVM.isSubmitting { ProgressView().frame(maxWidth: .infinity) }
                        else { Text("Оформить кредит").fontWeight(.semibold).frame(maxWidth: .infinity) }
                    }
                    .foregroundColor(.bankAccent)
                    .disabled(creditVM.isSubmitting || creditVM.selectedTariff == nil
                              || creditVM.selectedAccountId == nil || Double(creditVM.amountText) == nil)
                }
            }
            .navigationTitle("Новый кредит")
            .toolbar { ToolbarItem(placement: .cancellationAction) { Button("Отмена") { dismiss() } } }
        }
    }
}
