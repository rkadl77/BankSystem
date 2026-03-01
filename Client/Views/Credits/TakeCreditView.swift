//
//  TakeCreditView.swift
//  Client
//
//  Created by Gleb Korotkov on 27.02.2026.
//


import SwiftUI

// MARK: — Take Credit Sheet

struct TakeCreditView: View {
    @ObservedObject var creditVM: CreditViewModel
    let accounts: [Account]
    @Environment(\.dismiss) var dismiss
    
    var body: some View {
        NavigationStack {
            Form {
                Section("Тариф") {
                    if creditVM.tariffs.isEmpty {
                        Text("Нет доступных тарифов")
                            .foregroundColor(.secondary)
                    } else {
                        Picker("Тариф", selection: $creditVM.selectedTariff) {
                            Text("Выберите тариф").tag(CreditTariff?.none)
                            ForEach(creditVM.tariffs) { t in
                                VStack(alignment: .leading) {
                                    Text(t.name)
                                    Text("\(t.formattedRate) годовых")
                                        .font(.caption)
                                        .foregroundColor(.secondary)
                                }
                                .tag(t as CreditTariff?)
                            }
                        }
                        
                        if let t = creditVM.selectedTariff {
                            LabeledContent("Ставка", value: t.formattedRate)
                            LabeledContent("Сумма от-до", value: "\(Int(t.minAmount)) — \(Int(t.maxAmount)) ₽")
                            LabeledContent("Срок (дней)", value: "\(t.minTermDays) — \(t.maxTermDays)")
                        }
                    }
                }
                
                Section("Счёт зачисления") {
                    Picker("Счёт", selection: $creditVM.selectedAccountId) {
                        Text("Выберите счёт").tag(UUID?.none)
                        ForEach(accounts) { acc in
                            Text("\(acc.type.rawValue) — \(acc.maskedNumber)").tag(acc.id as UUID?)
                        }
                    }
                }
                
                Section("Параметры") {
                    HStack {
                        Text("Сумма")
                        Spacer()
                        TextField("100 000", text: $creditVM.creditAmount)
                            .keyboardType(.numberPad)
                            .multilineTextAlignment(.trailing)
                        Text("₽").foregroundColor(.secondary)
                    }
                    HStack {
                        Text("Срок")
                        Spacer()
                        TextField("30", text: $creditVM.termDays)
                            .keyboardType(.numberPad)
                            .multilineTextAlignment(.trailing)
                        Text("дней").foregroundColor(.secondary)
                    }
                }
                
                if let err = creditVM.errorMessage {
                    Section {
                        Label(err, systemImage: "exclamationmark.circle.fill")
                            .foregroundColor(.bankDanger)
                    }
                }
                
                Section {
                    Button("Оформить кредит") {
                        if creditVM.takeCredit() { dismiss() }
                    }
                    .foregroundColor(.bankAccent)
                    .fontWeight(.semibold)
                }
            }
            .navigationTitle("Кредитная заявка")
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Отмена") { dismiss() }
                }
            }
        }
    }
}
